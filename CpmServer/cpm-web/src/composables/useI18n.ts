import { storeToRefs } from 'pinia'
import { useLocaleStore, type Locale } from '@/stores/locale'

/**
 * Pattern: locale changes dispatch a CustomEvent 'locale-changed'.
 * Components that need to react to locale changes (e.g. re-fetch localized data)
 * can listen via:
 *   window.addEventListener('locale-changed', (e) => { ... })
 * or use a composable that wraps this event.
 */
export function useI18n() {
  const localeStore = useLocaleStore()
  const { currentLocale } = storeToRefs(localeStore)

  function setLocale(locale: Locale) {
    currentLocale.value = locale
    localStorage.setItem('locale', locale)
    window.dispatchEvent(new CustomEvent('locale-changed', { detail: { locale } }))
  }

  return {
    locale: currentLocale,
    setLocale,
    t: localeStore.t
  }
}
