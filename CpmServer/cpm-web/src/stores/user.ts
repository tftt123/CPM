/* eslint-disable @typescript-eslint/no-explicit-any */

import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useUserStore = defineStore('user', () => {
  const token = ref(localStorage.getItem('token') || '')
  const userInfo = ref<any>(null)
  const roles = ref<string[]>([])
  const permissions = ref<string[]>([])
  const siteList = ref<string[]>([])

  const currentSite = computed(() => userInfo.value?.site || '')

  const restoreUserInfo = () => {
    try {
      const stored = localStorage.getItem('userInfo')
      if (stored) {
        const info = JSON.parse(stored)
        userInfo.value = info
        roles.value = info.roles || []
        permissions.value = info.permissions || []
      }
    } catch {
      userInfo.value = null
      roles.value = []
      permissions.value = []
    }
    const storedSites = localStorage.getItem('siteList')
    if (storedSites) {
      try { siteList.value = JSON.parse(storedSites) } catch { siteList.value = [] }
    }
  }

  const setToken = (t: string) => {
    token.value = t
    localStorage.setItem('token', t)
  }

  const setUserInfo = (info: any) => {
    userInfo.value = info
    roles.value = info.roles || []
    permissions.value = info.permissions || []
    localStorage.setItem('userInfo', JSON.stringify(info))
  }

  const hasPermission = (code: string): boolean => {
    if (roles.value.some(r => r.toUpperCase() === 'ADMIN')) return true
    return permissions.value.includes(code)
  }

  const hasAnyPermission = (codes: string[]): boolean => {
    if (roles.value.some(r => r.toUpperCase() === 'ADMIN')) return true
    return codes.some(c => permissions.value.includes(c))
  }

  const setSiteList = (sites: string[]) => {
    siteList.value = sites
    localStorage.setItem('siteList', JSON.stringify(sites))
  }

  const switchSite = (site: string) => {
    if (userInfo.value) {
      userInfo.value.site = site
      localStorage.setItem('userInfo', JSON.stringify(userInfo.value))
    }
  }

  const logout = () => {
    token.value = ''
    userInfo.value = null
    roles.value = []
    permissions.value = []
    siteList.value = []
    localStorage.removeItem('token')
    localStorage.removeItem('userInfo')
    localStorage.removeItem('siteList')
  }

  // 初始化时恢复用户信息
  restoreUserInfo()

  return { token, userInfo, roles, permissions, siteList, currentSite, setToken, setUserInfo, setSiteList, switchSite, logout, hasPermission, hasAnyPermission }
})
