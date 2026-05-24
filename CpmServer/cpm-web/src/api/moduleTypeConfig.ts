import request from './request'

export interface ModuleTypeConfig {
  id: number
  moduleType: string
  moduleName?: string
  description?: string
  isActive: boolean
  createdAt?: string
}

export function getModuleTypeList(all?: boolean) {
  return request.get('/ModuleTypeConfig', { params: { all } })
}

export function getSystemModuleTypes() {
  return request.get('/ModuleTypeConfig/system-types')
}

export function createModuleType(data: ModuleTypeConfig) {
  return request.post('/ModuleTypeConfig', data)
}

export function updateModuleType(id: number, data: ModuleTypeConfig) {
  return request.put(`/ModuleTypeConfig/${id}`, data)
}

export function deleteModuleType(id: number) {
  return request.delete(`/ModuleTypeConfig/${id}`)
}
