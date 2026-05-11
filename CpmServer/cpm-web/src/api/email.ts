/* eslint-disable @typescript-eslint/no-explicit-any */
import request from './request'

export const emailApi = {
  getConfig: () => request.get('/email/config'),
  saveConfig: (data: any) => request.post('/email/config', data),
  sendTest: (data: any) => request.post('/email/test', data)
}
