import request from '@/api/request'

export interface NavigationConfig {
  id: number
  navCode: string
  moduleCode: string
  moduleLabel: string | null
  moduleLabelEn: string | null
  navLabel: string | null
  navLabelEn: string | null
  routePath: string
  iconName: string | null
  sortOrder: number
  isVisible: boolean
  isActive: boolean
  createdAt?: string
  updatedAt?: string
}

export const navigationConfigApi = {
  getList: () => request.get<NavigationConfig[]>('/NavigationConfig'),
  batchUpdate: (data: NavigationConfig[]) => request.post('/NavigationConfig/batch', data),
  init: () => request.post<NavigationConfig[]>('/NavigationConfig/init')
}
