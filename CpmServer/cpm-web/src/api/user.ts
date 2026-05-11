/* eslint-disable @typescript-eslint/no-explicit-any */
import request from './request'

export const userApi = {
  list: (params: any) => request.get('/user/list', { params }),
  getById: (id: number) => request.get(`/user/${id}`),
  create: (data: any) => request.post('/user', data),
  update: (id: number, data: any) => request.put(`/user/${id}`, data),
  delete: (id: number) => request.delete(`/user/${id}`),
  resetPassword: (id: number, newPassword: string) => request.post(`/user/${id}/reset-password`, { newPassword })
}

export const profileApi = {
  getProfile: () => request.get('/user/profile'),
  updateProfile: (data: any) => request.put('/user/profile', data),
  changePassword: (data: any) => request.post('/user/change-password', data)
}
