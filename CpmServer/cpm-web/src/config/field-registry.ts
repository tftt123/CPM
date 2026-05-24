// ========== 模块定义 ==========
export interface ModuleDef {
  code: string
  i18nKey: string
}

export const Modules: ModuleDef[] = [
  { code: 'Quotation', i18nKey: 'nav.quotation' },
  { code: 'Opportunity', i18nKey: 'nav.opportunity' },
  { code: 'Customer', i18nKey: 'nav.customer' },
  { code: 'Product', i18nKey: 'nav.product' },
  { code: 'Mfg', i18nKey: 'nav.mfgProcess' },
  { code: 'PM', i18nKey: 'nav.productTrace' },
  { code: 'Approval', i18nKey: 'nav.approval' },
  { code: 'System', i18nKey: 'common.systemSettings' },
]

// ========== 页面定义 ==========
export interface PageDef {
  code: string
  i18nKey: string
}

export const Pages: Record<string, PageDef[]> = {
  Quotation: [
    { code: 'QuotationForm', i18nKey: 'quotation.quotationDetail' },
    { code: 'QuotationList', i18nKey: 'common.list' },
    { code: 'QuotationDetail', i18nKey: 'quotation.quotationDetail' },
  ],
  Opportunity: [
    { code: 'OpportunityForm', i18nKey: 'opportunity.pageTitle' },
    { code: 'OpportunityList', i18nKey: 'common.list' },
  ],
  Customer: [
    { code: 'CustomerForm', i18nKey: 'customer.pageTitle' },
    { code: 'CustomerList', i18nKey: 'common.list' },
  ],
  Product: [
    { code: 'ProductForm', i18nKey: 'product.pageTitle' },
    { code: 'ProductList', i18nKey: 'common.list' },
  ],
  Mfg: [
    { code: 'MfgProcessManage', i18nKey: 'mfg.title' },
  ],
  PM: [
    { code: 'ProductTraceList', i18nKey: 'pmTrace.pageTitle' },
    { code: 'ProductTraceDetail', i18nKey: 'pmTrace.pageTitle' },
    { code: 'ActualCycleTimeManage', i18nKey: 'pmTrace.actualCycleTimeTitle' },
  ],
  Approval: [
    { code: 'ApprovalConfig', i18nKey: 'nav.approvalConfig' },
  ],
  System: [
    { code: 'UserForm', i18nKey: 'system.userListTitle' },
    { code: 'UserList', i18nKey: 'common.list' },
    { code: 'RoleList', i18nKey: 'system.roleListTitle' },
  ],
}

// ========== 字段模板 ==========
export interface FieldTemplate {
  code: string
  i18nKey: string
  dataType: string
  defaultRequired: boolean
}

export const FieldTemplates: Record<string, FieldTemplate[]> = {
  // === Quotation (existing) ===
  'Quotation.QuotationForm': [
    { code: 'productName', i18nKey: 'quotation.productName', dataType: 'select', defaultRequired: true },
    { code: 'qty', i18nKey: 'quotation.qty', dataType: 'number', defaultRequired: true },
    { code: 'processType', i18nKey: 'quotation.processType', dataType: 'select', defaultRequired: true },
    { code: 'equipmentType', i18nKey: 'quotation.equipmentType', dataType: 'select', defaultRequired: true },
    { code: 'equipment', i18nKey: 'quotation.equipment', dataType: 'select', defaultRequired: true },
    { code: 'cycleTime', i18nKey: 'quotation.cycleTime', dataType: 'number', defaultRequired: true },
    { code: 'hourlyRate', i18nKey: 'quotation.hourlyRate', dataType: 'number', defaultRequired: true },
    { code: 'packaging', i18nKey: 'quotation.packaging', dataType: 'number', defaultRequired: false },
    { code: 'transport', i18nKey: 'quotation.transport', dataType: 'number', defaultRequired: false },
    { code: 'lineAmount', i18nKey: 'quotation.lineAmount', dataType: 'number', defaultRequired: false },
  ],
  'Quotation.QuotationList': [
    { code: 'quotationNo', i18nKey: 'quotation.quotationNo', dataType: 'text', defaultRequired: false },
    { code: 'customer', i18nKey: 'quotation.customer', dataType: 'text', defaultRequired: false },
    { code: 'totalAmount', i18nKey: 'quotation.totalAmount', dataType: 'number', defaultRequired: false },
    { code: 'status', i18nKey: 'common.status', dataType: 'text', defaultRequired: false },
  ],
  'Quotation.QuotationDetail': [
    { code: 'productName', i18nKey: 'quotation.productName', dataType: 'select', defaultRequired: false },
    { code: 'processType', i18nKey: 'quotation.processType', dataType: 'select', defaultRequired: false },
    { code: 'equipmentType', i18nKey: 'quotation.equipmentType', dataType: 'select', defaultRequired: false },
    { code: 'equipment', i18nKey: 'quotation.equipment', dataType: 'select', defaultRequired: false },
    { code: 'cycleTime', i18nKey: 'quotation.cycleTime', dataType: 'number', defaultRequired: false },
    { code: 'hourlyRate', i18nKey: 'quotation.hourlyRate', dataType: 'number', defaultRequired: false },
  ],

  // === Opportunity ===
  'Opportunity.OpportunityForm': [
    { code: 'title', i18nKey: 'opportunity.title', dataType: 'text', defaultRequired: true },
    { code: 'customerId', i18nKey: 'quotation.customer', dataType: 'select', defaultRequired: true },
    { code: 'expectedAmount', i18nKey: 'opportunity.expectedAmount', dataType: 'number', defaultRequired: false },
    { code: 'quoteDeadline', i18nKey: 'opportunity.quoteDeadline', dataType: 'date', defaultRequired: false },
  ],
  'Opportunity.OpportunityList': [
    { code: 'opportunityNo', i18nKey: 'opportunity.opportunityNo', dataType: 'text', defaultRequired: false },
    { code: 'title', i18nKey: 'opportunity.title', dataType: 'text', defaultRequired: false },
    { code: 'customerName', i18nKey: 'quotation.customer', dataType: 'text', defaultRequired: false },
    { code: 'expectedAmount', i18nKey: 'opportunity.expectedAmount', dataType: 'number', defaultRequired: false },
    { code: 'stage', i18nKey: 'opportunity.stage', dataType: 'text', defaultRequired: false },
    { code: 'quoteDeadline', i18nKey: 'opportunity.quoteDeadline', dataType: 'date', defaultRequired: false },
    { code: 'ownerName', i18nKey: 'opportunity.owner', dataType: 'text', defaultRequired: false },
  ],

  // === Customer ===
  'Customer.CustomerForm': [
    { code: 'customerCode', i18nKey: 'customer.customerCode', dataType: 'text', defaultRequired: true },
    { code: 'customerName', i18nKey: 'customer.customerName', dataType: 'text', defaultRequired: true },
    { code: 'industry', i18nKey: 'customer.industry', dataType: 'select', defaultRequired: false },
    { code: 'currency', i18nKey: 'customer.currency', dataType: 'text', defaultRequired: false },
    { code: 'contactName', i18nKey: 'customer.contactName', dataType: 'text', defaultRequired: false },
  ],
  'Customer.CustomerList': [
    { code: 'customerCode', i18nKey: 'customer.customerCode', dataType: 'text', defaultRequired: false },
    { code: 'customerName', i18nKey: 'customer.customerName', dataType: 'text', defaultRequired: false },
    { code: 'industry', i18nKey: 'customer.industry', dataType: 'text', defaultRequired: false },
    { code: 'currency', i18nKey: 'customer.currency', dataType: 'text', defaultRequired: false },
    { code: 'contactName', i18nKey: 'customer.contactName', dataType: 'text', defaultRequired: false },
  ],

  // === Product ===
  'Product.ProductForm': [
    { code: 'productCode', i18nKey: 'product.productCode', dataType: 'text', defaultRequired: true },
    { code: 'productName', i18nKey: 'product.productName', dataType: 'text', defaultRequired: true },
    { code: 'material', i18nKey: 'product.material', dataType: 'text', defaultRequired: false },
    { code: 'surfaceTreatment', i18nKey: 'product.surfaceTreatment', dataType: 'text', defaultRequired: false },
  ],
  'Product.ProductList': [
    { code: 'productCode', i18nKey: 'product.productCode', dataType: 'text', defaultRequired: false },
    { code: 'productName', i18nKey: 'product.productName', dataType: 'text', defaultRequired: false },
    { code: 'material', i18nKey: 'product.material', dataType: 'text', defaultRequired: false },
    { code: 'surfaceTreatment', i18nKey: 'product.surfaceTreatment', dataType: 'text', defaultRequired: false },
  ],

  // === Mfg ===
  'Mfg.MfgProcessManage': [
    { code: 'processName', i18nKey: 'mfg.process', dataType: 'select', defaultRequired: true },
    { code: 'subCategoryName', i18nKey: 'mfg.subCategory', dataType: 'select', defaultRequired: true },
    { code: 'equipmentName', i18nKey: 'mfg.equipment', dataType: 'text', defaultRequired: true },
    { code: 'costRate', i18nKey: 'mfg.costRate', dataType: 'number', defaultRequired: false },
    { code: 'owner', i18nKey: 'mfg.owner', dataType: 'text', defaultRequired: false },
    { code: 'description', i18nKey: 'mfg.description', dataType: 'text', defaultRequired: false },
    { code: 'isActive', i18nKey: 'common.status', dataType: 'switch', defaultRequired: false },
  ],

  // === PM ===
  'PM.ProductTraceList': [
    { code: 'customerName', i18nKey: 'pmTrace.customer', dataType: 'text', defaultRequired: false },
    { code: 'productCode', i18nKey: 'pmTrace.partNo', dataType: 'text', defaultRequired: false },
    { code: 'productName', i18nKey: 'pmTrace.productName', dataType: 'text', defaultRequired: false },
    { code: 'plannedQty', i18nKey: 'pmTrace.plannedQty', dataType: 'number', defaultRequired: false },
    { code: 'projectStartDate', i18nKey: 'pmTrace.startDate', dataType: 'date', defaultRequired: false },
    { code: 'status', i18nKey: 'common.status', dataType: 'text', defaultRequired: false },
  ],
  'PM.ProductTraceDetail': [
    { code: 'plannedQty', i18nKey: 'pmTrace.plannedQty', dataType: 'number', defaultRequired: false },
    { code: 'projectStartDate', i18nKey: 'pmTrace.startDate', dataType: 'date', defaultRequired: false },
    { code: 'displayWeeks', i18nKey: 'pmTrace.displayWeeks', dataType: 'number', defaultRequired: false },
    { code: 'status', i18nKey: 'common.status', dataType: 'text', defaultRequired: false },
    // Route summary table columns
    { code: 'processName', i18nKey: 'pmTrace.process', dataType: 'text', defaultRequired: false },
    { code: 'equipment', i18nKey: 'mfg.equipment', dataType: 'text', defaultRequired: false },
    { code: 'personInCharge', i18nKey: 'pmTrace.person', dataType: 'text', defaultRequired: false },
    { code: 'cycleTime', i18nKey: 'pmTrace.cycleTime', dataType: 'number', defaultRequired: false },
    { code: 'actualCycleTime', i18nKey: 'pmTrace.actualCycleTime', dataType: 'number', defaultRequired: false },
    { code: 'settingDays', i18nKey: 'pmTrace.settingDays', dataType: 'number', defaultRequired: false },
    { code: 'estimatedHours', i18nKey: 'pmTrace.estimatedHours', dataType: 'number', defaultRequired: false },
    { code: 'remarks', i18nKey: 'pmTrace.remarks', dataType: 'text', defaultRequired: false },
  ],
  'PM.ActualCycleTimeManage': [
    { code: 'quotationNo', i18nKey: 'quotation.quotationNo', dataType: 'text', defaultRequired: false },
    { code: 'customerName', i18nKey: 'pmTrace.customer', dataType: 'text', defaultRequired: false },
    { code: 'productCode', i18nKey: 'pmTrace.partNo', dataType: 'text', defaultRequired: false },
    { code: 'productName', i18nKey: 'pmTrace.productName', dataType: 'text', defaultRequired: false },
    { code: 'processName', i18nKey: 'pmTrace.process', dataType: 'text', defaultRequired: false },
    { code: 'personInCharge', i18nKey: 'pmTrace.person', dataType: 'text', defaultRequired: false },
    { code: 'cycleTime', i18nKey: 'pmTrace.cycleTime', dataType: 'number', defaultRequired: false },
    { code: 'latestActualCycleTime', i18nKey: 'pmTrace.latestActualCycleTime', dataType: 'number', defaultRequired: false },
    { code: 'latestRecordDate', i18nKey: 'pmTrace.latestRecordDate', dataType: 'date', defaultRequired: false },
    { code: 'pendingRequestCount', i18nKey: 'pmTrace.pendingApproval', dataType: 'number', defaultRequired: false },
  ],

  // === Approval ===
  'Approval.ApprovalConfig': [
    { code: 'templateName', i18nKey: 'approval.templateName', dataType: 'text', defaultRequired: true },
    { code: 'moduleType', i18nKey: 'approval.moduleType', dataType: 'select', defaultRequired: true },
    { code: 'isDefault', i18nKey: 'approval.isDefault', dataType: 'switch', defaultRequired: false },
    { code: 'isActive', i18nKey: 'common.status', dataType: 'switch', defaultRequired: false },
    { code: 'description', i18nKey: 'mfg.description', dataType: 'textarea', defaultRequired: false },
    // Table columns
    { code: 'templateNameCol', i18nKey: 'approval.templateName', dataType: 'text', defaultRequired: false },
    { code: 'moduleTypeCol', i18nKey: 'approval.moduleType', dataType: 'text', defaultRequired: false },
    { code: 'stepsCol', i18nKey: 'approval.steps', dataType: 'text', defaultRequired: false },
    { code: 'isDefaultCol', i18nKey: 'approval.isDefault', dataType: 'text', defaultRequired: false },
    { code: 'isActiveCol', i18nKey: 'common.status', dataType: 'text', defaultRequired: false },
  ],

  // === System ===
  'System.UserForm': [
    { code: 'username', i18nKey: 'login.username', dataType: 'text', defaultRequired: true },
    { code: 'password', i18nKey: 'login.password', dataType: 'password', defaultRequired: true },
    { code: 'realName', i18nKey: 'profile.realName', dataType: 'text', defaultRequired: true },
    { code: 'email', i18nKey: 'common.email', dataType: 'text', defaultRequired: false },
    { code: 'phone', i18nKey: 'common.phone', dataType: 'text', defaultRequired: false },
    { code: 'site', i18nKey: 'common.site', dataType: 'text', defaultRequired: false },
    { code: 'roleIds', i18nKey: 'common.role', dataType: 'select', defaultRequired: false },
    { code: 'isActive', i18nKey: 'common.status', dataType: 'switch', defaultRequired: false },
  ],
  'System.UserList': [
    { code: 'email', i18nKey: 'common.email', dataType: 'text', defaultRequired: false },
    { code: 'phone', i18nKey: 'common.phone', dataType: 'text', defaultRequired: false },
    { code: 'site', i18nKey: 'common.site', dataType: 'text', defaultRequired: false },
    { code: 'role', i18nKey: 'common.role', dataType: 'text', defaultRequired: false },
    { code: 'isActive', i18nKey: 'common.status', dataType: 'text', defaultRequired: false },
  ],
  'System.RoleList': [
    { code: 'roleCode', i18nKey: 'approval.templateCode', dataType: 'text', defaultRequired: false },
    { code: 'roleName', i18nKey: 'approval.templateName', dataType: 'text', defaultRequired: false },
    { code: 'site', i18nKey: 'common.site', dataType: 'text', defaultRequired: false },
  ],
}

// 辅助函数
export const getModuleList = () => Modules
export const getPageList = (moduleCode: string) => Pages[moduleCode] || []
export const getFieldTemplateList = (moduleCode: string, pageCode: string) =>
  FieldTemplates[`${moduleCode}.${pageCode}`] || []
