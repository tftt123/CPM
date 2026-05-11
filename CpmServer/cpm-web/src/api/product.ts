/* eslint-disable @typescript-eslint/no-explicit-any */
import request from './request'

export const productApi = {
  list: (params: any) => request.get('/product/list', { params }),
  add: (data: any) => request.post('/product', data),
  update: (id: number, data: any) => request.put(`/product/${id}`, data),
  delete: (id: number) => request.delete(`/product/${id}`)
}
