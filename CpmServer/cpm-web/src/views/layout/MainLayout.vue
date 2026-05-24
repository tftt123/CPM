<template>
  <el-container style="height: 100vh; background: var(--slds-bg-page);">
    <!-- 椤堕儴 Global Header -->
    <el-header class="global-header">
      <div class="header-left">
        <div class="app-logo">
          <img src="/spxlogo/SPINDEX_HRZ_FA.png" class="app-logo-img" alt="logo" />
        </div>
        <div class="app-divider"></div>

        <el-dropdown @command="handleSwitchApp" trigger="click">
          <div class="current-app">
            <el-icon size="16"><Grid /></el-icon>
            <span>{{ currentAppLabel }}</span>
            <el-icon class="dropdown-arrow"><ArrowDown /></el-icon>
          </div>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="cpm" :class="{ 'is-active': currentApp === 'cpm' }">
                <el-icon v-if="currentApp === 'cpm'"><CircleCheck /></el-icon>
                <span v-else style="display:inline-block;width:16px"></span>
                {{ t('app.cpm') }}
              </el-dropdown-item>
              <el-dropdown-item command="sp" disabled>
                <el-icon><Lock /></el-icon>
                {{ t('app.sp') }} ({{ t('app.comingSoon') }})
              </el-dropdown-item>
              <el-dropdown-item command="mes" disabled>
                <el-icon><Lock /></el-icon>
                {{ t('app.mes') }} ({{ t('app.comingSoon') }})
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
      <div class="header-center">
        <el-input
          v-model="searchQuery"
          :placeholder="t('layout.searchPlaceholder')"
          class="global-search"
          :prefix-icon="Search"
          size="default"
        />
      </div>

      <div class="header-right">
        <!-- Workspace 鍒囨崲鍣?-->
        <div class="workspace-selector" v-if="userStore.siteList.length > 0">
          <el-dropdown @command="handleSwitchSite" trigger="click">
            <div class="workspace-trigger">
              <el-icon size="16"><OfficeBuilding /></el-icon>
              <span class="workspace-code">{{ userStore.currentSite }}</span>
              <el-icon class="dropdown-arrow"><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item
                  v-for="site in userStore.siteList"
                  :key="site"
                  :command="site"
                  :class="{ 'is-active': site === userStore.currentSite }"
                >
                  <el-icon v-if="site === userStore.currentSite"><CircleCheck /></el-icon>
                  <span v-else style="display:inline-block;width:16px"></span>
                  {{ site }}
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>

        <div class="header-divider"></div>

        <!-- Language 鍒囨崲鍣?-->
        <div class="language-selector">
          <el-dropdown @command="handleSwitchLanguage" trigger="click">
            <div class="lang-trigger">
              <el-icon size="14"><MapLocation /></el-icon>
              <span class="lang-code">{{ currentLangLabel }}</span>
              <el-icon class="dropdown-arrow"><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="zh" :class="{ 'is-active': locale === 'zh' }">
                  <el-icon v-if="locale === 'zh'"><CircleCheck /></el-icon>
                  <span v-else style="display:inline-block;width:16px"></span>
                  简体中文                </el-dropdown-item>
                <el-dropdown-item command="en" :class="{ 'is-active': locale === 'en' }">
                  <el-icon v-if="locale === 'en'"><CircleCheck /></el-icon>
                  <span v-else style="display:inline-block;width:16px"></span>
                  English
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>

        <div class="header-divider"></div>

        <el-tooltip :content="t('common.notification')">
          <el-badge :value="3" class="notification-badge">
            <el-button circle :icon="Bell" class="header-icon-btn" />
          </el-badge>
        </el-tooltip>
        <el-dropdown @command="handleUserCommand">
          <div class="user-info">
            <el-avatar :size="32" :src="userStore.userInfo?.avatarUrl || ''" :icon="UserFilled" class="user-avatar" />
            <div class="user-details">
              <span class="user-name">{{ userStore.userInfo?.realName || userStore.userInfo?.username || t('common.user') }}</span>
              <span class="user-role">{{ userStore.userInfo?.roles?.[0] || '' }}</span>
            </div>
            <el-icon class="dropdown-arrow"><ArrowDown /></el-icon>
          </div>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="profile">
                <el-icon><User /></el-icon>{{ t('common.personalInfo') }}
              </el-dropdown-item>
              <el-dropdown-item command="settings" v-if="canViewSettings">
                <el-icon><Setting /></el-icon>{{ t('common.systemSettings') }}
              </el-dropdown-item>
              <el-dropdown-item divided command="logout">
                <el-icon><SwitchButton /></el-icon>{{ t('common.logout') }}
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </el-header>

    <el-container style="flex: 1; overflow: hidden;">
      <!-- 左侧导航栏 -->
      <el-aside width="240px" class="sidebar">
        <!-- Dynamic Navigation -->
        <template v-if="groupedModules.length > 0">
          <div class="nav-section" v-for="module in groupedModules" :key="module.moduleCode">
            <div class="nav-section-title">{{ module.moduleLabel }}</div>
            <div class="nav-menu">
              <div
                v-for="item in module.items"
                :key="item.navCode"
                class="nav-item"
                :class="{ 'is-active': isActiveRoute(item.routePath) }"
                @click="router.push(item.routePath)"
              >
                <el-icon size="18">
                  <component :is="getIconComponent(item.iconName)" />
                </el-icon>
                <span>{{ resolveNavLabel(item) }}</span>
              </div>
            </div>
          </div>
        </template>

        <!-- Fallback: hardcoded navigation when dynamic config is unavailable -->
        <template v-else>
          <div class="nav-section">
            <div class="nav-section-title">{{ t('nav.home') }}</div>
            <div class="nav-menu">
              <div class="nav-item" :class="{ 'is-active': isActiveRoute('/home') }" @click="router.push('/home')">
                <el-icon size="18"><HomeFilled /></el-icon>
                <span>{{ t('nav.home') }}</span>
              </div>
            </div>
          </div>

          <div class="nav-section">
            <div class="nav-section-title">{{ t('nav.rfq') }}</div>
            <div class="nav-menu">
              <div class="nav-item" :class="{ 'is-active': isActiveRoute('/customer') }" @click="router.push('/customer')">
                <el-icon size="18"><UserFilled /></el-icon>
                <span>{{ t('nav.customer') }}</span>
              </div>
              <div class="nav-item" :class="{ 'is-active': isActiveRoute('/product') }" @click="router.push('/product')">
                <el-icon size="18"><Box /></el-icon>
                <span>{{ t('nav.product') }}</span>
              </div>
              <div class="nav-item" :class="{ 'is-active': isActiveRoute('/opportunity') }" @click="router.push('/opportunity')">
                <el-icon size="18"><FolderOpened /></el-icon>
                <span>{{ t('nav.opportunity') }}</span>
              </div>
              <div class="nav-item" :class="{ 'is-active': isActiveRoute('/quotation/list') }" @click="router.push('/quotation/list')">
                <el-icon size="18"><Document /></el-icon>
                <span>{{ t('nav.quotation') }}</span>
              </div>
              <div class="nav-item" :class="{ 'is-active': isActiveRoute('/mfg/process') }" @click="router.push('/mfg/process')">
                <el-icon size="18"><Setting /></el-icon>
                <span>{{ t('nav.mfgProcess') }}</span>
              </div>
            </div>
          </div>

          <div class="nav-section">
            <div class="nav-section-title">{{ t('nav.pm') }}</div>
            <div class="nav-menu">
              <div class="nav-item" :class="{ 'is-active': isActiveRoute('/pm/actual-cycle-time') }" @click="router.push('/pm/actual-cycle-time')">
                <el-icon size="18"><Timer /></el-icon>
                <span>{{ t('nav.actualCycleTime') }}</span>
              </div>
              <div class="nav-item" :class="{ 'is-active': isActiveRoute('/pm/trace') }" @click="router.push('/pm/trace')">
                <el-icon size="18"><TrendCharts /></el-icon>
                <span>{{ t('nav.productTrace') }}</span>
              </div>
            </div>
          </div>

          <div class="nav-section">
            <div class="nav-section-title">{{ t('nav.approval') }}</div>
            <div class="nav-menu">
              <div class="nav-item" :class="{ 'is-active': isActiveRoute('/approval/center') }" @click="router.push('/approval/center')">
                <el-icon size="18"><CircleCheck /></el-icon>
                <span>{{ t('nav.approvalCenter') }}</span>
              </div>
            </div>
          </div>

          <div class="nav-section">
            <div class="nav-section-title">{{ t('nav.salesReport') }}</div>
            <div class="nav-menu">
              <div class="nav-item" :class="{ 'is-active': isActiveRoute('/report/analysis') }" @click="router.push('/report/analysis')">
                <el-icon size="18"><DataAnalysis /></el-icon>
                <span>{{ t('nav.dataAnalysis') }}</span>
              </div>
            </div>
          </div>
        </template>
      </el-aside>

      <!-- 涓诲唴瀹瑰尯鍩?-->
      <el-main class="main-content">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { useI18n } from '@/composables/useI18n'
import { useNavigationConfig } from '@/composables/useNavigationConfig'
import {
  HomeFilled,
  UserFilled,
  Box,
  Grid,
  Search,
  Bell,
  User,
  ArrowDown,
  Setting,
  SwitchButton,
  TrendCharts,
  DataAnalysis,
  Document,
  FolderOpened,
  OfficeBuilding,
  CircleCheck,
  MapLocation,
  Timer,
  Lock
} from '@element-plus/icons-vue'
import { authApi } from '@/api/auth'
import { ElMessage } from 'element-plus'

const router = useRouter()
const route = useRoute()
const userStore = useUserStore()
const { t, locale, setLocale } = useI18n()
const { loadNavigationConfig, groupedModules, resolveNavLabel, getIconComponent } = useNavigationConfig()
const searchQuery = ref('')

const isActiveRoute = (path: string) => route.path === path || route.path.startsWith(path + '/')

onMounted(() => {
  loadNavigationConfig()
  // 空闲时预加载所有页面组件，消除首次切换的懒加载延迟
  requestIdleCallback?.(() => {
    import('@/views/HomeView.vue').catch(() => {})
    import('@/views/customer/CustomerList.vue').catch(() => {})
    import('@/views/product/ProductList.vue').catch(() => {})
    import('@/views/quotation/OpportunityList.vue').catch(() => {})
    import('@/views/quotation/QuotationList.vue').catch(() => {})
    import('@/views/quotation/QuotationDetail.vue').catch(() => {})
    import('@/views/mfg/MfgProcessManage.vue').catch(() => {})
    import('@/views/pm/ProductTraceList.vue').catch(() => {})
    import('@/views/pm/ProductTraceDetail.vue').catch(() => {})
    import('@/views/pm/ActualCycleTimeManage.vue').catch(() => {})
    import('@/views/approval/ApprovalCenter.vue').catch(() => {})
    import('@/views/system/SystemSettings.vue').catch(() => {})
    import('@/views/profile/ProfileView.vue').catch(() => {})
  })
})


// Current App / Subsystem
const currentApp = ref(localStorage.getItem('current_app') || 'cpm')
const currentAppLabel = computed(() => {
  const map: Record<string, string> = {
    cpm: t('app.cpm'),
    sp: t('app.sp'),
    mes: t('app.mes'),
  }
  return map[currentApp.value] || currentApp.value
})
const currentLangLabel = computed(() => locale.value === 'zh' ? 'CH' : 'EN')

const canViewSettings = computed(() => userStore.hasPermission('settings.view'))

const handleUserCommand = (command: string) => {
  switch (command) {
    case 'logout':
      userStore.logout()
      router.push('/login')
      break
    case 'profile':
      router.push('/profile')
      break
    case 'settings':
      router.push('/system/settings')
      break
  }
}

const handleSwitchSite = async (site: string) => {
  if (site === userStore.currentSite) return
  try {
    const res = await authApi.switchSite(site)
    userStore.setToken(res.data.token)
    userStore.setUserInfo({
      username: res.data.username,
      realName: res.data.realName,
      site: res.data.site,
      roles: res.data.roles,
      permissions: res.data.permissions || []
    })
    ElMessage.success(t('message.switchSiteSuccess', { site }))
    router.replace('/home')
  } catch {
    ElMessage.error(t('common.failed'))
  }
}

const handleSwitchLanguage = (lang: 'zh' | 'en') => {
  if (lang === locale.value) return
  setLocale(lang)
}

const handleSwitchApp = (command: string) => {
  if (command === currentApp.value) return
  if (command === 'cpm') {
    currentApp.value = command
    localStorage.setItem('current_app', command)
    router.push('/home')
  } else {
    ElMessage.info(t('app.comingSoon'))
  }
}
</script>

<style scoped>
/* Global Header */
.global-header {
  background: var(--slds-bg-header);
  border-bottom: 1px solid var(--slds-border-color);
  box-shadow: var(--slds-shadow-header);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 var(--slds-spacing-lg);
  height: 60px !important;
  z-index: 100;
}

.header-left {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-md);
}



.app-logo {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
}

.app-logo-img {
  height: 30px;
  width: auto;
  object-fit: contain;
  display: block;
}

.app-name {
  font-size: calc(var(--slds-font-size-lg) + 2px);
  font-weight: 900;
  color: var(--slds-brand-primary);
  letter-spacing: -0.3px;
  -webkit-text-stroke: 0.3px currentColor;
}

.app-divider {
  width: 1px;
  height: 24px;
  background: var(--slds-border-color);
}

.current-app {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
  color: var(--slds-text-secondary);
  font-size: var(--slds-font-size-md);
  font-weight: 500;
  cursor: pointer;
  padding: var(--slds-spacing-sm) var(--slds-spacing-md);
  border-radius: var(--slds-border-radius);
  border: 1px solid transparent;
  transition: background 0.2s;
}

.current-app:hover {
  background: var(--slds-bg-hover);
  border-color: var(--slds-border-color-light);
}

:deep(.el-dropdown-menu__item.is-active) {
  color: var(--slds-brand-primary);
  font-weight: 600;
}

.header-center {
  flex: 1;
  max-width: 480px;
  margin: 0 var(--slds-spacing-xl);
}

.global-search :deep(.el-input__wrapper) {
  border-radius: 20px !important;
  background: #F3F3F3;
  box-shadow: none !important;
  padding: 0 16px;
}

.global-search :deep(.el-input__inner) {
  background: transparent;
}

.header-right {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
}

.header-divider {
  width: 1px;
  height: 24px;
  background: var(--slds-border-color);
  margin: 0 var(--slds-spacing-xs);
}

/* Workspace 鍒囨崲鍣?- 鍙充笂瑙?*/
.workspace-selector {
  display: flex;
  align-items: center;
}

.workspace-trigger {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: var(--slds-spacing-sm) var(--slds-spacing-md);
  border-radius: var(--slds-border-radius);
  background: transparent;
  border: 1px solid var(--slds-border-color);
  cursor: pointer;
  transition: all 0.2s;
}

.workspace-trigger:hover {
  background: var(--slds-bg-hover);
  border-color: var(--slds-border-color-light);
}

.workspace-code {
  font-size: 13px;
  font-weight: 600;
  color: var(--slds-text-primary);
  letter-spacing: 0.5px;
  flex: 0 0 auto;
  text-align: center;
}

.workspace-trigger .el-icon:first-child,
.workspace-trigger .dropdown-arrow {
  width: 16px;
  text-align: center;
  flex-shrink: 0;
}

/* Language 鍒囨崲鍣?*/
.language-selector {
  display: flex;
  align-items: center;
}

.lang-trigger {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  padding: var(--slds-spacing-sm) var(--slds-spacing-md);
  border-radius: var(--slds-border-radius);
  background: transparent;
  border: 1px solid var(--slds-border-color);
  cursor: pointer;
  transition: all 0.2s;
}

.lang-trigger:hover {
  background: var(--slds-bg-hover);
  border-color: var(--slds-border-color-light);
}

.lang-code {
  font-size: 12px;
  font-weight: 600;
  color: var(--slds-text-primary);
  min-width: 18px;
  text-align: center;
  flex: 0 0 auto;
}

.lang-trigger .el-icon:first-child,
.lang-trigger .dropdown-arrow {
  width: 14px;
  text-align: center;
  flex-shrink: 0;
}

.notification-badge :deep(.el-badge__content) {
  background: var(--slds-error);
  border: none;
}

.header-icon-btn {
  border: none;
  background: transparent;
  color: var(--slds-text-secondary);
  transition: all 0.2s;
}

.header-icon-btn:hover {
  background: var(--slds-bg-hover);
  color: var(--slds-text-primary);
}

.user-info {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
  cursor: pointer;
  padding: var(--slds-spacing-xs) var(--slds-spacing-sm);
  border-radius: var(--slds-border-radius);
  transition: background 0.2s;
}

.user-info:hover {
  background: var(--slds-bg-hover);
}

.user-avatar {
  background: var(--slds-brand-primary);
  color: white;
}

.user-details {
  display: flex;
  flex-direction: column;
  line-height: 1.3;
}

.user-name {
  font-size: var(--slds-font-size-sm);
  font-weight: 600;
  color: var(--slds-text-primary);
}

.user-role {
  font-size: 11px;
  color: var(--slds-text-secondary);
}

.dropdown-arrow {
  color: var(--slds-text-secondary);
  font-size: 12px;
}

/* Sidebar */
.sidebar {
  background: var(--slds-bg-sidebar);
  display: flex;
  flex-direction: column;
  padding: var(--slds-spacing-md) 0;
}

.nav-section {
  margin-bottom: var(--slds-spacing-md);
}

.nav-section-title {
  color: rgba(255, 255, 255, 0.5);
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.5px;
  padding: var(--slds-spacing-sm) var(--slds-spacing-lg);
}

.nav-menu {
  background: transparent;
  border: none;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
  color: rgba(255, 255, 255, 0.85);
  margin: 2px var(--slds-spacing-sm);
  border-radius: var(--slds-border-radius);
  height: 44px;
  line-height: 44px;
  font-size: var(--slds-font-size-md);
  font-weight: 500;
  transition: all 0.2s;
  padding-left: var(--slds-spacing-lg);
  cursor: pointer;
  user-select: none;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.08);
  color: #fff;
}

.nav-item.is-active {
  background: rgba(255, 255, 255, 0.12);
  color: #fff;
  border-left: 3px solid #90D0FE;
}

.nav-item .el-icon {
  color: inherit;
  flex-shrink: 0;
}

/* Main Content */
.main-content {
  background: var(--slds-bg-page);
  padding: var(--slds-spacing-lg);
  overflow-y: auto;
}
</style>

