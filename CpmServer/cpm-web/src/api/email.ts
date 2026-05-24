/* eslint-disable @typescript-eslint/no-explicit-any */
import request from './request'

export interface EmailTemplate {
  id: number
  templateCode: string
  templateName: string
  subject: string
  body: string
  isSystem: boolean
  isActive: boolean
}

export const emailApi = {
  getConfig: () => request.get('/email/config'),
  saveConfig: (data: any) => request.post('/email/config', data),
  sendTest: (data: any) => request.post('/email/test', data),
  getTemplates: () => request.get('/email/templates'),
  updateTemplate: (id: number, data: any) => request.put(`/email/templates/${id}`, data)
}
