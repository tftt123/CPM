import request from './request'
import type { ApiResponse } from './pmProjectTrace'

export interface SiteSettings {
  id?: number
  site: string
  siteCode?: string
  currency: string
  description?: string
}

export function getSiteSettings(): Promise<ApiResponse<SiteSettings>> {
  return request.get('/SiteSettings')
}

export function saveSiteSettings(data: SiteSettings): Promise<ApiResponse<void>> {
  return request.post('/SiteSettings', data)
}
