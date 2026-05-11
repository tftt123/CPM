import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import zh from '@/locales/zh'
import en from '@/locales/en'

export type Locale = 'zh' | 'en'

const messages: Record<Locale, any> = { zh, en }

function getNestedValue(obj: any, path: string): string | undefined {
  const keys = path.split('.')
  let value = obj
  for (const key of keys) {
    if (value == null) return undefined
    value = value[key]
  }
  return typeof value === 'string' ? value : undefined
}

function interpolate(template: string, params?: Record<string, any>): string {
  if (!params) return template
  return template.replace(/\{(\w+)\}/g, (_, key) => {
    return params[key] !== undefined ? String(params[key]) : `{${key}}`
  })
}

export const useLocaleStore = defineStore('locale', () => {
  const currentLocale = ref<Locale>((localStorage.getItem('locale') as Locale) || 'zh')

  const localeMessages = computed(() => messages[currentLocale.value])

  function t(key: string, params?: Record<string, any>): string {
    const message = getNestedValue(localeMessages.value, key)
    if (message) return interpolate(message, params)
    // fallback to zh
    const fallback = getNestedValue(messages.zh, key)
    if (fallback) return interpolate(fallback, params)
    return key
  }

  function setLocale(locale: Locale) {
    currentLocale.value = locale
    localStorage.setItem('locale', locale)
  }

  return { currentLocale, setLocale, t }
})
