import request from './request'
import type { ApiResponse } from './pmProjectTrace'

export interface SequenceRule {
  id?: number
  moduleType: string
  moduleName?: string
  prefix: string
  currentSequence: number
  sequenceLength: number
  resetRule: number
  lastResetDate?: string
  lastGeneratedNo?: string
  site?: string
  createdAt: string
  updatedAt: string
}

export interface SequenceRuleCreateDto {
  moduleType: string
  moduleName?: string
  prefix: string
  sequenceLength: number
  resetRule: number
}

export function getSequenceRules(): Promise<ApiResponse<SequenceRule[]>> {
  return request.get('/SequenceRule')
}

export function getSequenceRule(id: number): Promise<ApiResponse<SequenceRule>> {
  return request.get(`/SequenceRule/${id}`)
}

export function createSequenceRule(data: SequenceRuleCreateDto): Promise<ApiResponse<number>> {
  return request.post('/SequenceRule', data)
}

export function updateSequenceRule(id: number, data: SequenceRuleCreateDto): Promise<ApiResponse<void>> {
  return request.put(`/SequenceRule/${id}`, data)
}

export function deleteSequenceRule(id: number): Promise<ApiResponse<void>> {
  return request.delete(`/SequenceRule/${id}`)
}

export function generateSequence(moduleType: string): Promise<ApiResponse<string>> {
  return request.post('/SequenceRule/generate', { moduleType })
}
