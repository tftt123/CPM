<template>
  <div class="page-container">
    <div class="page-header">
      <div>
        <h2>{{ t('pmTrace.actualCycleTimeTitle') }}</h2>
        <p class="subtitle">{{ t('pmTrace.actualCycleTimeSubtitle') }}</p>
      </div>
    </div>

    <el-card class="search-card">
      <el-form :inline="true" :model="searchForm">
        <el-form-item :label="t('common.keyword')">
          <el-input
            v-model="searchForm.keyword"
            :placeholder="t('pmTrace.searchPlaceholder')"
            clearable
            @keyup.enter="loadData"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadData">
            <el-icon><Search /></el-icon>
            {{ t('common.search') }}
          </el-button>
          <el-button @click="handleReset">
            {{ t('common.reset') }}
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-table :data="tableData" v-loading="loading" stripe :span-method="objectSpanMethod">
      <el-table-column prop="quotationNo" :label="t('quotation.quotationNo')" min-width="140">
        <template #default="{ row }">
          <span v-if="isFirstRowOfTrace(row)">{{ row.quotationNo }}</span>
        </template>
      </el-table-column>
      <el-table-column prop="customerName" :label="t('pmTrace.customer')" min-width="140">
        <template #default="{ row }">
          <span v-if="isFirstRowOfTrace(row)">{{ row.customerName }}</span>
        </template>
      </el-table-column>
      <el-table-column prop="productCode" :label="t('pmTrace.partNo')" min-width="140">
        <template #default="{ row }">
          <span v-if="isFirstRowOfTrace(row)">{{ row.productCode }}</span>
        </template>
      </el-table-column>
      <el-table-column prop="productName" :label="t('pmTrace.productName')" min-width="160">
        <template #default="{ row }">
          <span v-if="isFirstRowOfTrace(row)">{{ row.productName }}</span>
        </template>
      </el-table-column>

      <el-table-column prop="stepOrder" :label="t('pmTrace.process')" width="240" align="left">
        <template #default="{ row }">
          {{ row.stepOrder }}.{{ row.processName }}
        </template>
      </el-table-column>
      <el-table-column prop="personInCharge" :label="t('pmTrace.person')" width="120">
        <template #default="{ row }">
          {{ getPersonInCharge(row) }}
        </template>
      </el-table-column>
      <el-table-column prop="cycleTime" :label="t('pmTrace.cycleTime')" width="120" align="right">
        <template #default="{ row }">
          {{ row.cycleTime ?? '-' }}
        </template>
      </el-table-column>
      <el-table-column prop="latestActualCycleTime" :label="t('pmTrace.latestActualCycleTime')" width="150" align="right">
        <template #default="{ row }">
          <span :class="{ 'text-warning': row.latestActualCycleTime && row.cycleTime && row.latestActualCycleTime > row.cycleTime }">
            {{ row.latestActualCycleTime ?? '-' }}
          </span>
        </template>
      </el-table-column>
      <el-table-column prop="latestRecordDate" :label="t('pmTrace.latestRecordDate')" width="140">
        <template #default="{ row }">
          {{ formatDate(row.latestRecordDate) }}
        </template>
      </el-table-column>
      <el-table-column :label="t('pmTrace.pendingApproval')" width="100" align="center">
        <template #default="{ row }">
          <el-tag v-if="row.pendingRequestCount > 0" type="warning" size="small">{{ row.pendingRequestCount }}</el-tag>
          <span v-else>-</span>
        </template>
      </el-table-column>
      <el-table-column :label="t('common.action')" width="120" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" @click="openCycleTimeDialog(row)">
            {{ t('common.edit') }}
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 实际节拍维护弹窗 -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="700px"
      :close-on-click-modal="false"
    >
      <el-table :data="dialogRecords" border stripe size="small">
        <el-table-column :label="t('pmTrace.recordDate')" width="130">
          <template #default="{ row }">
            <span v-if="!row.editable" class="text-muted">{{ row.recordDate }}</span>
            <el-date-picker
              v-else
              v-model="row.recordDate"
              type="date"
              value-format="YYYY-MM-DD"
              size="small"
              style="width: 115px"
            />
          </template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.actualCycleTime')" width="110" align="center">
          <template #default="{ row }">
            <span v-if="!row.editable" class="text-muted">{{ row.actualCycleTime }}</span>
            <el-input-number
              v-else
              v-model="row.actualCycleTime"
              :min="0"
              :precision="4"
              size="small"
              controls-position="right"
              style="width: 95px"
            />
          </template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.remarks')" min-width="120">
          <template #default="{ row }">
            <span v-if="!row.editable" class="text-muted">{{ row.remarks }}</span>
            <el-input v-else v-model="row.remarks" size="small" />
          </template>
        </el-table-column>
        <el-table-column :label="t('common.status')" width="80" align="center">
          <template #default="{ row }">
            <el-tag :type="statusTagType(row.status)" size="small">{{ statusLabel(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column :label="t('common.action')" width="100" align="center">
          <template #default="{ row, $index }">
            <div v-if="row.status === 0">
              <el-button v-if="!row.editable" link type="primary" size="small" @click="startEdit(row)">
                {{ t('common.edit') }}
              </el-button>
              <el-button v-else link type="success" size="small" @click="confirmEdit(row)">
                {{ t('common.confirm') }}
              </el-button>
              <el-button link type="danger" size="small" @click="markDelete(row, $index)">
                {{ t('common.delete') }}
              </el-button>
            </div>
            <span v-else class="text-muted">-</span>
          </template>
        </el-table-column>
      </el-table>
      <div style="margin-top: 12px">
        <el-button type="primary" size="small" @click="addCycleTime">
          <el-icon><Plus /></el-icon>
          {{ t('pmTrace.addCycleTime') }}
        </el-button>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button size="small" @click="dialogVisible = false">{{ t('common.cancel') }}</el-button>
          <el-button type="primary" size="small" :loading="saving" @click="submitApproval">{{ t('pmTrace.submitApproval') }}</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { Search, Plus } from '@element-plus/icons-vue'
import { useI18n } from '@/composables/useI18n'
import {
  getAllStepsActualCycleTime,
  getTraceDetail,
  submitCycleTimeChangeRequest,
} from '@/api/pmProjectTrace'
import type {
  PmProjectTraceStepCycleTimeItem,
  PmProjectTraceStepActualCycleTime,
  PmStepCycleTimeChangeDetail,
} from '@/api/pmProjectTrace'
import { mfgProcessApi, type MfgCascadeOption } from '@/api/mfgProcess'

const { t } = useI18n()

const processOptions = ref<MfgCascadeOption[]>([])

const loading = ref(false)
const saving = ref(false)
const tableData = ref<PmProjectTraceStepCycleTimeItem[]>([])
const searchForm = reactive({
  keyword: '',
})

// 合并
const traceSpanMap = ref<Map<number, { startRow: number; rowCount: number }>>(new Map())

const processOptionMap = computed(() =>
  new Map(processOptions.value.map(p => [p.label, p.owner]))
)

function getPersonInCharge(row: PmProjectTraceStepCycleTimeItem): string {
  if (row.personInCharge) return row.personInCharge
  return processOptionMap.value.get(row.processName) || '-'
}

// 弹窗
const dialogVisible = ref(false)
const dialogTitle = ref('')
const editingStepId = ref(0)
const editingTraceId = ref(0)

interface DialogRecord extends PmProjectTraceStepActualCycleTime {
  editable: boolean
  originalStatus?: number
  isModified?: boolean
}

const dialogRecords = ref<DialogRecord[]>([])

// 原始值快照，用于检测修改
const originalSnapshot = ref<Map<number, { recordDate: string; actualCycleTime?: number; remarks?: string }>>(new Map())

function formatDate(date?: string) {
  if (!date) return '-'
  return new Date(date).toLocaleDateString()
}

function formatDateISO(d: Date): string {
  const year = d.getFullYear()
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function statusLabel(status?: number): string {
  switch (status) {
    case 0: return t('pmTrace.statusActive')
    case 1: return t('pmTrace.statusExpired')
    case 2: return t('pmTrace.statusInvalidated')
    default: return '-'
  }
}

function statusTagType(status?: number): string {
  switch (status) {
    case 0: return 'success'
    case 1: return 'info'
    case 2: return 'danger'
    default: return ''
  }
}

function computeSpans(data: PmProjectTraceStepCycleTimeItem[]) {
  const map = new Map<number, { startRow: number; rowCount: number }>()
  for (let i = 0; i < data.length; i++) {
    const item = data[i]
    if (!item) continue
    const traceId = item.traceId
    if (!map.has(traceId)) {
      map.set(traceId, { startRow: i, rowCount: 1 })
    } else {
      const info = map.get(traceId)!
      info.rowCount++
    }
  }
  return map
}

function objectSpanMethod({ rowIndex, columnIndex }: any) {
  if (columnIndex >= 3) return { rowspan: 1, colspan: 1 }
  const row = tableData.value[rowIndex]
  if (!row) return { rowspan: 1, colspan: 1 }
  const info = traceSpanMap.value.get(row.traceId)
  if (!info) return { rowspan: 1, colspan: 1 }
  if (rowIndex === info.startRow) {
    return { rowspan: info.rowCount, colspan: 1 }
  }
  return { rowspan: 0, colspan: 0 }
}

function isFirstRowOfTrace(row: PmProjectTraceStepCycleTimeItem) {
  const idx = tableData.value.indexOf(row)
  if (idx <= 0) return true
  const prev = tableData.value[idx - 1]
  if (!prev) return true
  return prev.traceId !== row.traceId
}

async function loadData() {
  loading.value = true
  try {
    const res = await getAllStepsActualCycleTime({
      keyword: searchForm.keyword || undefined,
    })
    tableData.value = res.data
    traceSpanMap.value = computeSpans(tableData.value)
  } catch (e) {
    console.error(e)
    ElMessage.error(t('common.failed'))
  } finally {
    loading.value = false
  }
}

function handleReset() {
  searchForm.keyword = ''
  loadData()
}

async function openCycleTimeDialog(row: PmProjectTraceStepCycleTimeItem) {
  editingStepId.value = row.stepId
  editingTraceId.value = row.traceId
  dialogTitle.value = `${t('pmTrace.actualCycleTime')} - ${row.processName}`
  dialogVisible.value = true
  dialogRecords.value = []

  try {
    const res = await getTraceDetail(row.traceId)
    const trace = res.data
    const step = trace.steps?.find((s: any) => s.id === row.stepId)
    if (step) {
      dialogRecords.value = (step.actualCycleTimes || []).map((a: any) => ({
        id: a.id,
        projectTraceStepId: a.projectTraceStepId,
        recordDate: a.recordDate,
        actualCycleTime: a.actualCycleTime,
        remarks: a.remarks,
        status: a.status ?? 0,
        originalStatus: a.status ?? 0,
        editable: false,
        isModified: false,
      }))
      // 保存原始值快照
      originalSnapshot.value = new Map()
      dialogRecords.value.forEach(r => {
        if (r.id) {
          originalSnapshot.value.set(r.id, {
            recordDate: r.recordDate,
            actualCycleTime: r.actualCycleTime,
            remarks: r.remarks,
          })
        }
      })
    }
  } catch (e) {
    console.error(e)
    ElMessage.error(t('common.failed'))
  }
}

function addCycleTime() {
  dialogRecords.value.push({
    id: undefined,
    projectTraceStepId: editingStepId.value,
    recordDate: formatDateISO(new Date()),
    actualCycleTime: undefined,
    remarks: '',
    status: 0,
    editable: true,
  })
}

function startEdit(row: DialogRecord) {
  row.editable = true
}

function confirmEdit(row: DialogRecord) {
  row.editable = false
  if (row.id && originalSnapshot.value.has(row.id)) {
    const orig = originalSnapshot.value.get(row.id)!
    row.isModified =
      row.recordDate !== orig.recordDate ||
      row.actualCycleTime !== orig.actualCycleTime ||
      row.remarks !== orig.remarks
  }
}

function markDelete(row: DialogRecord, index: number) {
  if (row.id) {
    row.status = 2
    row.editable = false
  } else {
    dialogRecords.value.splice(index, 1)
  }
}

function isRecordModified(row: DialogRecord): boolean {
  if (!row.id || !originalSnapshot.value.has(row.id)) return false
  const orig = originalSnapshot.value.get(row.id)!
  return (
    row.recordDate !== orig.recordDate ||
    row.actualCycleTime !== orig.actualCycleTime ||
    row.remarks !== orig.remarks
  )
}

async function submitApproval() {
  const changes: PmStepCycleTimeChangeDetail[] = []

  for (const row of dialogRecords.value) {
    if (!row.id) {
      // 新增记录
      changes.push({
        changeType: 0,
        recordDate: row.recordDate,
        actualCycleTime: row.actualCycleTime,
        remarks: row.remarks,
      })
    } else if (row.status === 2 && row.originalStatus !== 2) {
      // 删除
      changes.push({
        changeType: 2,
        targetRecordId: row.id,
        recordDate: row.recordDate,
        actualCycleTime: row.actualCycleTime,
        remarks: row.remarks,
      })
    } else if (row.id && row.status === 0 && (row.editable || isRecordModified(row))) {
      // 修改：用户点了编辑（editable=true）或值有变化，均提交审批
      changes.push({
        changeType: 1,
        targetRecordId: row.id,
        recordDate: row.recordDate,
        actualCycleTime: row.actualCycleTime,
        remarks: row.remarks,
      })
    }
  }

  if (changes.length === 0) {
    ElMessage.warning(t('pmTrace.noChanges'))
    return
  }

  saving.value = true
  try {
    await submitCycleTimeChangeRequest({
      stepId: editingStepId.value,
      traceId: editingTraceId.value,
      changes,
    })
    ElMessage.success(t('pmTrace.submitApprovalSuccess'))
    dialogVisible.value = false
    loadData()
  } catch (e) {
    console.error(e)
    ElMessage.error(t('common.failed'))
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  const res = await mfgProcessApi.getProcessOptions()
  processOptions.value = res.data || []
  await loadData()
})
</script>

<style scoped>
.page-container {
  padding: 20px;
}
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}
.page-header h2 {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
}
.subtitle {
  margin: 4px 0 0;
  color: var(--slds-text-secondary);
  font-size: 13px;
}
.search-card {
  margin-bottom: 16px;
}
.text-warning {
  color: var(--el-color-danger);
  font-weight: 600;
}
.text-muted {
  color: var(--el-text-color-secondary);
}

/* 弹窗表格控件高度对齐报价单 */
:deep(.el-dialog .el-table--small .el-input__wrapper),
:deep(.el-dialog .el-table--small .el-select .el-input__wrapper),
:deep(.el-dialog .el-table--small .el-date-editor.el-input__wrapper),
:deep(.el-dialog .el-table--small .el-input-number .el-input__wrapper) {
  min-height: 28px !important;
  height: 28px !important;
  padding: 0 8px !important;
}
:deep(.el-dialog .el-table--small .el-input__inner),
:deep(.el-dialog .el-table--small .el-select .el-input__inner),
:deep(.el-dialog .el-table--small .el-date-editor .el-input__inner),
:deep(.el-dialog .el-table--small .el-input-number .el-input__inner) {
  height: 26px !important;
  line-height: 26px !important;
}
:deep(.el-dialog .el-table--small .el-input-number .el-input-number__decrease),
:deep(.el-dialog .el-table--small .el-input-number .el-input-number__increase) {
  height: 26px !important;
  line-height: 26px !important;
  top: 1px;
}
:deep(.el-dialog .el-table--small td) {
  padding: 4px 0 !important;
}
</style>
