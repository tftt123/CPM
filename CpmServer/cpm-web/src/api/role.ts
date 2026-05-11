/* eslint-disable @typescript-eslint/no-explicit-any */
import request from './request'

export const roleApi = {
  list: (params?: any) => request.get('/role/list', { params }),
  getById: (id: number) => request.get(`/role/${id}`),
  create: (data: any) => request.post('/role', data),
  update: (id: number, data: any) => request.put(`/role/${id}`, data),
  delete: (id: number) => request.delete(`/role/${id}`)
}
