import request from '@/api/request'

export interface I18nMessage {
  id: number
  messageKey: string
  category: string
  zhValue?: string
  enValue?: string
  isActive: boolean
  site?: string
  createdAt: string
  updatedAt: string
}

export function getI18nMessages(category?: string, keyword?: string) {
  const params: Record<string, string> = {}
  if (category) params.category = category
  if (keyword) params.keyword = keyword
  return request.get<I18nMessage[]>('/I18nMessage', { params })
}

export function getActiveI18nMessages() {
  return request.get<Record<string, Record<string, string>>>('/I18nMessage/active')
}

export function createI18nMessage(data: Partial<I18nMessage>) {
  return request.post<I18nMessage>('/I18nMessage', data)
}

export function updateI18nMessage(id: number, data: Partial<I18nMessage>) {
  return request.put(`/I18nMessage/${id}`, data)
}

export function deleteI18nMessage(id: number) {
  return request.delete(`/I18nMessage/${id}`)
}
