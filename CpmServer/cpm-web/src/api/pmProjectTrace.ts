import request from './request'

export interface PmProjectTraceStep {
  id?: number
  stepOrder: number
  processName: string
  personInCharge?: string
  cycleTime?: number
  settingDays?: number
  estimatedHours?: number
  remarks?: string
  planDurationDays?: number
  planStartDate?: string
  planEndDate?: string
  actualStartDate?: string
  actualForecastStartDate?: string
  actualDurationDays?: number
  actualPlanDurationDays?: number
  actualEndDate?: string
}

export interface PmProjectTrace {
  id?: number
  quotationId?: number
  customerId: number
  customerName: string
  productId?: number
  productCode: string
  productName?: string
  plannedQty?: number
  projectStartDate?: string
  displayWeeks?: number
  status: number
  createdAt?: string
  updatedAt?: string
  steps: PmProjectTraceStep[]
}

export function getTraceList(params: {
  keyword?: string
  status?: number
  page?: number
  pageSize?: number
}) {
  return request.get('/PmProjectTrace', { params })
}

export function getTraceDetail(id: number) {
  return request.get(`/PmProjectTrace/${id}`)
}

export function createTrace(data: PmProjectTrace) {
  return request.post('/PmProjectTrace', data)
}

export function updateTrace(id: number, data: PmProjectTrace) {
  return request.put(`/PmProjectTrace/${id}`, data)
}

export function deleteTrace(id: number) {
  return request.delete(`/PmProjectTrace/${id}`)
}
