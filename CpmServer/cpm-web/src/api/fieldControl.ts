import request from '@/api/request'

export interface FieldControl {
  id: number
  moduleCode: string
  pageCode: string
  fieldCode: string
  isVisible: boolean
  isRequired: boolean
  sortOrder: number
  site?: string
  createdAt?: string
  updatedAt?: string
}

export interface FieldInitItem {
  fieldCode: string
  defaultRequired: boolean
}

export function getModules() {
  return request.get<string[]>('/FieldControl/modules')
}

export function getPages(moduleCode: string) {
  return request.get<string[]>(`/FieldControl/pages?moduleCode=${moduleCode}`)
}

export function getFieldControls(moduleCode: string, pageCode: string) {
  return request.get<FieldControl[]>(`/FieldControl?moduleCode=${moduleCode}&pageCode=${pageCode}`)
}

export function initFieldControls(moduleCode: string, pageCode: string, fields: FieldInitItem[]) {
  return request.post<FieldControl[]>('/FieldControl/init', {
    moduleCode,
    pageCode,
    fields
  })
}

export function updateFieldControls(fields: FieldControl[]) {
  return request.put('/FieldControl/batch', fields)
}
