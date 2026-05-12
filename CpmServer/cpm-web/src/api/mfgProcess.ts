import request from './request'

export interface MfgProcessCreate {
  category: string
  processCode?: string
  processName: string
  description?: string
  stdTimeMin?: number
  costRate?: number
  sortOrder: number
}

export interface MfgSubCategoryCreate {
  processId: number
  subCategoryCode?: string
  subCategoryName: string
  description?: string
  toleranceGrade?: string
  sortOrder: number
  equipments: MfgEquipmentCreate[]
}

export interface MfgEquipmentCreate {
  subCategoryId: number
  equipmentCode?: string
  equipmentName: string
  description?: string
  model?: string
  spec?: string
  manufacturer?: string
  owner?: string
  costRate?: number
}

export interface MfgProcessRecord {
  equipmentId: number
  category: string
  processName: string
  subCategoryName: string
  equipmentCode?: string
  equipmentName: string
  description?: string
  model?: string
  spec?: string
  manufacturer?: string
  owner?: string
  costRate?: number
  isActive: boolean
  processId: number
  subCategoryId: number
}

export interface MfgCascadeOption {
  id: number
  label: string
  owner?: string
}

export const mfgProcessApi = {
  // Cascade options
  getCategoryList: () => request.get('/MfgProcess/categories'),
  getProcessOptions: (category?: string) => request.get('/MfgProcess/process-options', { params: { category } }),
  getSubCategoryOptions: (processId: number) => request.get('/MfgProcess/subcategory-options', { params: { processId } }),

  // Flat records
  getFlatRecords: (params?: { category?: string; keyword?: string }) => request.get('/MfgProcess/records', { params }),

  // Process
  getProcesses: (category?: string) => request.get('/MfgProcess/processes', { params: { category } }),
  createProcess: (data: MfgProcessCreate) => request.post('/MfgProcess/processes', data),
  updateProcess: (id: number, data: MfgProcessCreate) => request.put(`/MfgProcess/processes/${id}`, data),
  deleteProcess: (id: number) => request.delete(`/MfgProcess/processes/${id}`),

  // SubCategory
  getSubCategories: (processId?: number) => request.get('/MfgProcess/subcategories', { params: { processId } }),
  createSubCategory: (data: MfgSubCategoryCreate) => request.post('/MfgProcess/subcategories', data),
  updateSubCategory: (id: number, data: MfgSubCategoryCreate) => request.put(`/MfgProcess/subcategories/${id}`, data),
  deleteSubCategory: (id: number) => request.delete(`/MfgProcess/subcategories/${id}`),

  // Equipment
  getEquipments: (subCategoryId?: number) => request.get('/MfgProcess/equipments', { params: { subCategoryId } }),
  createEquipment: (data: MfgEquipmentCreate) => request.post('/MfgProcess/equipments', data),
  updateEquipment: (id: number, data: MfgEquipmentCreate) => request.put(`/MfgProcess/equipments/${id}`, data),
  deleteEquipment: (id: number) => request.delete(`/MfgProcess/equipments/${id}`),
}
