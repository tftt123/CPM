import request from '@/api/request'

export interface GeneralizedCode {
  id: number
  domain: string
  code: string
  label: string | null
  labelEn: string | null
  sortOrder: number
  isActive: boolean
  tagType: string | null
  attributes: string | null
  createdAt?: string
  updatedAt?: string
}

export interface DomainSummary {
  domain: string
  count: number
}

export interface GeneralizedCodeBatchItem {
  code: string
  label: string | null
  labelEn: string | null
  sortOrder: number
  isActive: boolean
  tagType: string | null
  attributes: string | null
}

export const generalizedCodeApi = {
  getByDomain: (domain: string) =>
    request.get<GeneralizedCode[]>(`/GeneralizedCode?domain=${encodeURIComponent(domain)}`),
  getDomains: () =>
    request.get<DomainSummary[]>('/GeneralizedCode/domains'),
  batchUpdate: (domain: string, items: GeneralizedCodeBatchItem[]) =>
    request.post('/GeneralizedCode/batch', { domain, items }),
  init: () =>
    request.post('/GeneralizedCode/init'),
  validate: (domain: string, code: string) =>
    request.get<boolean>(`/GeneralizedCode/validate?domain=${encodeURIComponent(domain)}&code=${encodeURIComponent(code)}`)
}
