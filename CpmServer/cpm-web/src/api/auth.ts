import request from './request'

export const authApi = {
  login: (data: { username: string; password: string; site?: string }) =>
    request.post('/auth/login', data),
  qadLogin: (data: { username: string; password: string; domain: string }) =>
    request.post('/auth/qad-login', data),
  getMySites: () => request.get('/auth/my-sites'),
  getSites: () => request.get('/auth/sites'),
  switchSite: (site: string) => request.post('/auth/switch-site', { site })
}
