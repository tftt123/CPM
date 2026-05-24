import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import zh from '@/locales/zh'
import en from '@/locales/en'
import { getActiveI18nMessages } from '@/api/i18nMessage'

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

function setNestedValue(obj: any, path: string, value: string) {
  const keys = path.split('.')
  let current = obj
  for (let i = 0; i < keys.length - 1; i++) {
    const key = keys[i]
    if (!current[key] || typeof current[key] !== 'object') {
      current[key] = {}
    }
    current = current[key]
  }
  current[keys[keys.length - 1]] = value
}

function interpolate(template: string, params?: Record<string, any>): string {
  if (!params) return template
  return template.replace(/\{(\w+)\}/g, (_, key) => {
    return params[key] !== undefined ? String(params[key]) : `{${key}}`
  })
}

export const useLocaleStore = defineStore('locale', () => {
  const currentLocale = ref<Locale>((localStorage.getItem('locale') as Locale) || 'zh')
  const dbMessagesLoaded = ref(false)

  // Deep clone static messages so we can override with DB translations
  const mergedMessages = ref<Record<Locale, any>>({
    zh: JSON.parse(JSON.stringify(messages.zh)),
    en: JSON.parse(JSON.stringify(messages.en))
  })

  const localeMessages = computed(() => mergedMessages.value[currentLocale.value])

  async function loadDbMessages() {
    try {
      const res = await getActiveI18nMessages()
      const dbData = res.data || {}
      for (const [key, localeMap] of Object.entries(dbData)) {
        if (localeMap.zh) {
          setNestedValue(mergedMessages.value.zh, key, localeMap.zh)
        }
        if (localeMap.en) {
          setNestedValue(mergedMessages.value.en, key, localeMap.en)
        }
      }
      dbMessagesLoaded.value = true
    } catch {
      // If API fails, static messages still work
      dbMessagesLoaded.value = true
    }
  }

  function t(key: string, params?: Record<string, any>): string {
    const message = getNestedValue(localeMessages.value, key)
    if (message) return interpolate(message, params)
    // fallback to zh
    const fallback = getNestedValue(mergedMessages.value.zh, key)
    if (fallback) return interpolate(fallback, params)
    return key
  }

  function setLocale(locale: Locale) {
    currentLocale.value = locale
    localStorage.setItem('locale', locale)
  }

  function getMessage(key: string, locale: Locale): string {
    const msg = getNestedValue(mergedMessages.value[locale], key)
    if (msg) return interpolate(msg)
    const fallback = getNestedValue(mergedMessages.value.zh, key)
    if (fallback) return interpolate(fallback)
    return key
  }

  return { currentLocale, setLocale, t, loadDbMessages, dbMessagesLoaded, getMessage }
})
