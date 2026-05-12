import request from './request'

export interface ApiResponse<T> {
  code: number
  message: string
  data: T
}

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

export interface TraceListResult {
  total: number
  list: PmProjectTrace[]
}

export function getTraceList(params: {
  keyword?: string
  status?: number
  page?: number
  pageSize?: number
}): Promise<ApiResponse<TraceListResult>> {
  return request.get('/PmProjectTrace', { params })
}

export function getTraceDetail(id: number): Promise<ApiResponse<PmProjectTrace>> {
  return request.get(`/PmProjectTrace/${id}`)
}

export function createTrace(data: PmProjectTrace): Promise<ApiResponse<number>> {
  return request.post('/PmProjectTrace', data)
}

export function updateTrace(id: number, data: PmProjectTrace): Promise<ApiResponse<unknown>> {
  return request.put(`/PmProjectTrace/${id}`, data)
}

export function deleteTrace(id: number): Promise<ApiResponse<unknown>> {
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

export function getAllStepsActualCycleTime(params: { keyword?: string }): Promise<ApiResponse<PmProjectTraceStepCycleTimeItem[]>> {
  return request.get('/PmProjectTrace/steps/actual-cycle-time', { params })
}

export function submitCycleTimeChangeRequest(data: {
  stepId: number
  traceId: number
  changes: PmStepCycleTimeChangeDetail[]
}): Promise<ApiResponse<number>> {
  return request.post('/PmProjectTrace/steps/actual-cycle-time/change-request', data)
}
