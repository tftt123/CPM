/* eslint-disable @typescript-eslint/no-explicit-any */
import request from './request'

export const customerApi = {
  list: (params: any) => request.get('/customer/list', { params }),
  getById: (id: number) => request.get(`/customer/${id}`),
  add: (data: any) => request.post('/customer', data),
  update: (id: number, data: any) => request.put(`/customer/${id}`, data),
  delete: (id: number) => request.delete(`/customer/${id}`)
}
