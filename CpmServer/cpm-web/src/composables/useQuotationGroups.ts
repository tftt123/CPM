/**
 * 报价单产品分组工具
 * 将扁平的报价明细按产品分组，解决 isProcessRow 展示层耦合问题
 */

export interface QuotationItem {
  id?: number
  productId?: number
  productName?: string
  qty: number
  isProcessRow: boolean
  processType?: string
  equipmentType?: string
  equipment?: string
  cycleTime?: number
  hourlyRate?: number
  cost?: number
  lineAmount?: number
  unitPrice?: number
  [key: string]: any
}

export interface ProductGroup {
  /** 产品ID */
  productId?: number
  /** 产品名称 */
  productName: string
  /** 数量 */
  qty: number
  /** 主行原始数据 */
  masterRow: QuotationItem
  /** 所有工艺行（主行 + 子工艺行） */
  processes: QuotationItem[]
  /** 工艺成本小计 */
  processCost: number
  /** 包装费 */
  packagingCost: number
  /** 运输费 */
  transportCost: number
  /** 行金额小计 */
  lineAmount: number
}

/**
 * 将扁平的报价明细按产品分组
 *
 * 优先使用 isProcessRow 标记（精确模式）。
 * 如果数据中所有行的 isProcessRow 都是 false（历史数据），
 * 则退化为按 productId 连续分组（兼容模式）。
 */
export function groupQuotationItems(items: QuotationItem[]): ProductGroup[] {
  if (items.length === 0) return []

  // 检测数据中是否存在 isProcessRow=true 的行
  const hasExplicitFlags = items.some(i => i.isProcessRow)

  return hasExplicitFlags
    ? groupByExplicitFlag(items)
    : groupByProductId(items)
}

/** 精确模式：isProcessRow=false 为新组起点 */
function groupByExplicitFlag(items: QuotationItem[]): ProductGroup[] {
  const groups: ProductGroup[] = []
  let current: ProductGroup | null = null

  for (const item of items) {
    if (!item.isProcessRow) {
      current = createGroup(item)
      groups.push(current)
    } else if (current) {
      appendToGroup(current, item)
    } else {
      // 孤立工艺行容错
      groups.push(createGroup(item))
    }
  }

  return groups
}

/** 兼容模式：连续相同的 productId 归为一组 */
function groupByProductId(items: QuotationItem[]): ProductGroup[] {
  const groups: ProductGroup[] = []
  let current: ProductGroup | null = null

  for (const item of items) {
    if (!current || current.productId !== item.productId) {
      current = createGroup(item)
      groups.push(current)
    } else {
      appendToGroup(current, item)
    }
  }

  return groups
}

function createGroup(item: QuotationItem): ProductGroup {
  return {
    productId: item.productId,
    productName: item.productName || '未选择产品',
    qty: item.qty || 1,
    masterRow: item,
    processes: [item],
    processCost: item.cost || 0,
    packagingCost: item.packagingCost || 0,
    transportCost: item.transportCost || 0,
    lineAmount: item.lineAmount || 0,
  }
}

function appendToGroup(group: ProductGroup, item: QuotationItem): void {
  group.processes.push(item)
  group.processCost += item.cost || 0
  group.packagingCost += item.packagingCost || 0
  group.transportCost += item.transportCost || 0
  group.lineAmount += item.lineAmount || 0
}

/**
 * 将分组结构转回扁平数组（保存时）
 */
export function flattenQuotationGroups(groups: ProductGroup[]): QuotationItem[] {
  const result: QuotationItem[] = []
  for (const group of groups) {
    for (let i = 0; i < group.processes.length; i++) {
      const src = group.processes[i]
      const item: QuotationItem = {
        ...src,
        isProcessRow: i > 0,
        productId: group.productId,
        productName: group.productName,
        qty: group.qty,
      }
      result.push(item)
    }
  }
  return result
}

/**
 * 计算分组汇总信息
 */
export function calcGroupSummary(groups: ProductGroup[]) {
  return {
    totalLineAmount: groups.reduce((sum, g) => sum + g.lineAmount, 0),
    totalProcessCost: groups.reduce((sum, g) => sum + g.processCost, 0),
    productCount: groups.length,
    totalProcessCount: groups.reduce((sum, g) => sum + g.processes.length, 0),
  }
}
