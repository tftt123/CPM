import request from './request'

export interface PmProjectTraceStepActualCycleTime {
  id?: number
  projectTraceStepId?: number
  recordDate: string
  actualCycleTime?: number
  remarks?: string
  status?: number
}

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
  actualCycleTimes: PmProjectTraceStepActualCycleTime[]
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

export interface PmProjectTraceStepCycleTimeItem {
  traceId: number
  customerName: string
  productCode: string
  productName?: string
  stepId: number
  stepOrder: number
  processName: string
  personInCharge?: string
  cycleTime?: number
  latestActualCycleTime?: number
  latestRecordDate?: string
  pendingRequestCount: number
}

export interface PmStepCycleTimeChangeDetail {
  changeType: number
  targetRecordId?: number
  recordDate: string
  actualCycleTime?: number
  remarks?: string
}

export function getAllStepsActualCycleTime(params: { keyword?: string }) {
  return request.get('/PmProjectTrace/steps/actual-cycle-time', { params })
}

export function submitCycleTimeChangeRequest(data: {
  stepId: number
  traceId: number
  changes: PmStepCycleTimeChangeDetail[]
}) {
  return request.post('/PmProjectTrace/steps/actual-cycle-time/change-request', data)
}
