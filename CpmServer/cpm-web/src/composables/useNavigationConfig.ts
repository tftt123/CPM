import { ref, computed, type Component } from 'vue'
import { useLocaleStore } from '@/stores/locale'
import { navigationConfigApi, type NavigationConfig } from '@/api/navigationConfig'
import {
  HomeFilled,
  UserFilled,
  Box,
  FolderOpened,
  Document,
  Setting,
  Timer,
  TrendCharts,
  CircleCheck,
  DataAnalysis,
  House,
  User,
  DocumentChecked,
  List,
  Edit,
  Delete,
  Search,
  Plus,
  Minus,
  Link,
  Message,
  Bell,
  Calendar,
  Clock,
  Money,
  Wallet,
  Shop,
  Goods,
  Grid,
  Menu,
  OfficeBuilding,
  MapLocation,
  Compass,
  StarFilled,
  FirstAidKit,
  Aim,
  Cpu,
  Monitor,
  Tools,
  Service,
  Warning,
  InfoFilled,
  QuestionFilled
} from '@element-plus/icons-vue'

const iconMap: Record<string, Component> = {
  HomeFilled,
  UserFilled,
  Box,
  FolderOpened,
  Document,
  Setting,
  Timer,
  TrendCharts,
  CircleCheck,
  DataAnalysis,
  House,
  User,
  DocumentChecked,
  List,
  Edit,
  Delete,
  Search,
  Plus,
  Minus,
  Link,
  Message,
  Bell,
  Calendar,
  Clock,
  Money,
  Wallet,
  Shop,
  Goods,
  Grid,
  Menu,
  OfficeBuilding,
  MapLocation,
  Compass,
  StarFilled,
  FirstAidKit,
  Aim,
  Cpu,
  Monitor,
  Tools,
  Service,
  Warning,
  InfoFilled,
  QuestionFilled
}

// Hardcoded default navigation structure (fallback when DB is empty)
const defaultNavConfig: NavigationConfig[] = [
  { id: 0, navCode: 'home', moduleCode: 'home', moduleLabel: null, moduleLabelEn: null, navLabel: null, navLabelEn: null, routePath: '/home', iconName: 'HomeFilled', sortOrder: 0, isVisible: true, isActive: true },
  { id: 0, navCode: 'customer', moduleCode: 'rfq', moduleLabel: null, moduleLabelEn: null, navLabel: null, navLabelEn: null, routePath: '/customer', iconName: 'UserFilled', sortOrder: 0, isVisible: true, isActive: true },
  { id: 0, navCode: 'product', moduleCode: 'rfq', moduleLabel: null, moduleLabelEn: null, navLabel: null, navLabelEn: null, routePath: '/product', iconName: 'Box', sortOrder: 1, isVisible: true, isActive: true },
  { id: 0, navCode: 'opportunity', moduleCode: 'rfq', moduleLabel: null, moduleLabelEn: null, navLabel: null, navLabelEn: null, routePath: '/opportunity', iconName: 'FolderOpened', sortOrder: 2, isVisible: true, isActive: true },
  { id: 0, navCode: 'quotation', moduleCode: 'rfq', moduleLabel: null, moduleLabelEn: null, navLabel: null, navLabelEn: null, routePath: '/quotation/list', iconName: 'Document', sortOrder: 3, isVisible: true, isActive: true },
  { id: 0, navCode: 'mfgProcess', moduleCode: 'rfq', moduleLabel: null, moduleLabelEn: null, navLabel: null, navLabelEn: null, routePath: '/mfg/process', iconName: 'Setting', sortOrder: 4, isVisible: true, isActive: true },
  { id: 0, navCode: 'actualCycleTime', moduleCode: 'pm', moduleLabel: null, moduleLabelEn: null, navLabel: null, navLabelEn: null, routePath: '/pm/actual-cycle-time', iconName: 'Timer', sortOrder: 0, isVisible: true, isActive: true },
  { id: 0, navCode: 'productTrace', moduleCode: 'pm', moduleLabel: null, moduleLabelEn: null, navLabel: null, navLabelEn: null, routePath: '/pm/trace', iconName: 'TrendCharts', sortOrder: 1, isVisible: true, isActive: true },
  { id: 0, navCode: 'approvalCenter', moduleCode: 'approval', moduleLabel: null, moduleLabelEn: null, navLabel: null, navLabelEn: null, routePath: '/approval/center', iconName: 'CircleCheck', sortOrder: 0, isVisible: true, isActive: true },
  { id: 0, navCode: 'dataAnalysis', moduleCode: 'salesReport', moduleLabel: null, moduleLabelEn: null, navLabel: null, navLabelEn: null, routePath: '/report/analysis', iconName: 'DataAnalysis', sortOrder: 0, isVisible: true, isActive: true },
]

export interface NavModule {
  moduleCode: string
  moduleLabel: string
  items: NavigationConfig[]
}

const navConfig = ref<NavigationConfig[]>([])
const loaded = ref(false)
const loading = ref(false)

export function useNavigationConfig() {
  const localeStore = useLocaleStore()

  async function loadNavigationConfig() {
    if (loaded.value) return
    loading.value = true
    try {
      const res = await navigationConfigApi.getList()
      const data = res.data || []
      if (data.length > 0) {
        navConfig.value = data
      } else {
        navConfig.value = [...defaultNavConfig]
      }
      loaded.value = true
    } catch {
      // Fallback to hardcoded defaults on API failure
      navConfig.value = [...defaultNavConfig]
      loaded.value = true
    } finally {
      loading.value = false
    }
  }

  function resolveModuleLabel(item: NavigationConfig): string {
    const locale = localeStore.currentLocale
    if (locale === 'en' && item.moduleLabelEn) return item.moduleLabelEn
    if (locale === 'zh' && item.moduleLabel) return item.moduleLabel
    // Fallback to i18n
    const i18nKeyMap: Record<string, string> = {
      home: 'nav.home',
      rfq: 'nav.rfq',
      pm: 'nav.pm',
      approval: 'nav.approval',
      salesReport: 'nav.salesReport'
    }
    return localeStore.t(i18nKeyMap[item.moduleCode] || item.moduleCode)
  }

  function resolveNavLabel(item: NavigationConfig): string {
    const locale = localeStore.currentLocale
    if (locale === 'en' && item.navLabelEn) return item.navLabelEn
    if (locale === 'zh' && item.navLabel) return item.navLabel
    // Fallback to i18n
    const i18nKey = `nav.${item.navCode}`
    const translated = localeStore.t(i18nKey)
    if (translated !== i18nKey) return translated
    return item.navCode
  }

  function getIconComponent(iconName: string | null): Component {
    if (!iconName) return Document
    return iconMap[iconName] || Document
  }

  const groupedModules = computed<NavModule[]>(() => {
    const visible = navConfig.value.filter(n => n.isVisible && n.isActive)
    const groups: Record<string, NavigationConfig[]> = {}

    for (const item of visible) {
      if (!groups[item.moduleCode]) {
        groups[item.moduleCode] = []
      }
      groups[item.moduleCode].push(item)
    }

    // Sort items within each module by sortOrder
    for (const code of Object.keys(groups)) {
      groups[code].sort((a, b) => a.sortOrder - b.sortOrder)
    }

    // Define module display order
    const moduleOrder = ['home', 'rfq', 'pm', 'approval', 'salesReport']
    const result: NavModule[] = []

    for (const code of moduleOrder) {
      if (groups[code]) {
        const firstItem = groups[code][0]
        result.push({
          moduleCode: code,
          moduleLabel: resolveModuleLabel(firstItem),
          items: groups[code]
        })
      }
    }

    // Append any unknown modules at the end
    for (const code of Object.keys(groups)) {
      if (!moduleOrder.includes(code)) {
        const firstItem = groups[code][0]
        result.push({
          moduleCode: code,
          moduleLabel: resolveModuleLabel(firstItem),
          items: groups[code]
        })
      }
    }

    return result
  })

  return {
    navConfig,
    loaded,
    loading,
    loadNavigationConfig,
    resolveModuleLabel,
    resolveNavLabel,
    getIconComponent,
    groupedModules
  }
}
