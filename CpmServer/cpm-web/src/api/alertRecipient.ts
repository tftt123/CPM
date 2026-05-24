import request from './request'

export interface AlertRecipient {
  id: number
  alertType: string
  recipientType: string
  recipientValue: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export const alertRecipientApi = {
  getList: () => request.get('/AlertRecipient'),
  create: (data: Omit<AlertRecipient, 'id' | 'createdAt' | 'updatedAt'>) => request.post('/AlertRecipient', data),
  update: (id: number, data: Omit<AlertRecipient, 'id' | 'createdAt' | 'updatedAt'>) => request.put(`/AlertRecipient/${id}`, data),
  delete: (id: number) => request.delete(`/AlertRecipient/${id}`)
}
