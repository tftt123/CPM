import { storeToRefs } from 'pinia'
import { useLocaleStore } from '@/stores/locale'

export function useI18n() {
  const localeStore = useLocaleStore()
  const { currentLocale } = storeToRefs(localeStore)
  return {
    locale: currentLocale,
    setLocale: localeStore.setLocale,
    t: localeStore.t
  }
}
