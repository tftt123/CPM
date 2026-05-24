import axios from 'axios'
import { ElMessage } from 'element-plus'

const request = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  timeout: 30000
})

// 请求拦截器：加 token + app code
request.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = 'Bearer ' + token
  }
  const appCode = localStorage.getItem('current_app') || 'cpm'
  config.headers['X-App-Code'] = appCode
  return config
})

// 响应拦截器
request.interceptors.response.use(
  response => {
    const data = response.data
    if (data.code !== 200) {
      ElMessage.error(data.message || '请求失败')
      return Promise.reject(data)
    }
    return data
  },
  error => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('userInfo')
      window.location.href = '/login'
    } else if (error.response?.status === 403) {
      ElMessage.error('没有权限执行此操作')
    } else if (error.response?.status === 500) {
      const serverMsg = error.response.data?.message || error.response.data || '服务器内部错误'
      ElMessage.error(typeof serverMsg === 'string' ? serverMsg : JSON.stringify(serverMsg))
    } else {
      ElMessage.error(error.response?.data?.message || error.message || '网络错误')
    }
    return Promise.reject(error)
  }
)

export default request
