import { ref, computed, type Component } from 'vue'
import { useLocaleStore } from '@/stores/locale'
import { generalizedCodeApi, type GeneralizedCode } from '@/api/generalizedCode'

export interface GcOption {
  value: string
  label: string
  tagType: string
  raw: GeneralizedCode | null
}

const codeCache = ref<Record<string, GeneralizedCode[]>>({})
const domainList = ref<{ domain: string; count: number }[]>([])
const loadedDomains = ref<Set<string>>(new Set())

export function useGeneralizedCode() {
  const localeStore = useLocaleStore()

  async function loadDomain(domain: string): Promise<GeneralizedCode[]> {
    if (loadedDomains.value.has(domain)) {
      return codeCache.value[domain] || []
    }
    try {
      const res = await generalizedCodeApi.getByDomain(domain)
      const list = (res.data || [])
        .filter(c => c.isActive)
        .sort((a, b) => a.sortOrder - b.sortOrder)
      codeCache.value[domain] = list
      loadedDomains.value.add(domain)
      return list
    } catch {
      return []
    }
  }

  async function loadDomains(): Promise<{ domain: string; count: number }[]> {
    try {
      const res = await generalizedCodeApi.getDomains()
      domainList.value = res.data || []
      return domainList.value
    } catch {
      return []
    }
  }

  function getLabel(item: GeneralizedCode): string {
    const locale = localeStore.currentLocale
    if (locale === 'en' && item.labelEn) return item.labelEn
    if (locale === 'zh' && item.label) return item.label
    // i18n fallback: gc.{domain}.{code}
    const key = `gc.${item.domain}.${item.code}`
    const translated = localeStore.t(key)
    if (translated !== key) return translated
    return item.code
  }

  function getTagType(item: GeneralizedCode): string {
    return item.tagType || 'info'
  }

  async function getOptions(
    domain: string,
    fallback?: { code: string; label: string; tagType?: string }[]
  ): Promise<GcOption[]> {
    const list = await loadDomain(domain)
    if (list.length > 0) {
      return list.map(item => ({
        value: item.code,
        label: getLabel(item),
        tagType: getTagType(item),
        raw: item
      }))
    }
    // Fallback to hardcoded
    return (fallback || []).map(f => ({
      value: f.code,
      label: f.label,
      tagType: f.tagType || 'info',
      raw: null
    }))
  }

  function invalidateCache(domain?: string) {
    if (domain) {
      delete codeCache.value[domain]
      loadedDomains.value.delete(domain)
    } else {
      codeCache.value = {}
      loadedDomains.value.clear()
      domainList.value = []
    }
  }

  return {
    loadDomain,
    loadDomains,
    getLabel,
    getTagType,
    getOptions,
    invalidateCache,
    codeCache,
    domainList
  }
}
