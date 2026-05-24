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
              <el-dropdown-item command="settings" v-if="isAdmin">
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
      <!-- 宸︿晶瀵艰埅鏍?-->
      <el-aside width="240px" class="sidebar">
        <!-- Dynamic Navigation -->
        <template v-if="groupedModules.length > 0">
          <div class="nav-section" v-for="module in groupedModules" :key="module.moduleCode">
            <div class="nav-section-title">{{ module.moduleLabel }}</div>
            <el-menu
              router
              :default-active="$route.path"
              class="nav-menu"
              :collapse="false"
              :collapse-transition="false"
            >
              <el-menu-item
                v-for="item in module.items"
                :key="item.navCode"
                :index="item.routePath"
                class="nav-item"
              >
                <el-icon size="18">
                  <component :is="getIconComponent(item.iconName)" />
                </el-icon>
                <span>{{ resolveNavLabel(item) }}</span>
              </el-menu-item>
            </el-menu>
          </div>
        </template>

        <!-- Fallback: hardcoded navigation when dynamic config is unavailable -->
        <template v-else>
          <div class="nav-section">
            <div class="nav-section-title">{{ t('nav.home') }}</div>
            <el-menu
              router
              :default-active="$route.path"
              class="nav-menu"
              :collapse="false"
              :collapse-transition="false"
            >
              <el-menu-item index="/home" class="nav-item">
                <el-icon size="18"><HomeFilled /></el-icon>
                <span>{{ t('nav.home') }}</span>
              </el-menu-item>
            </el-menu>
          </div>

          <div class="nav-section">
            <div class="nav-section-title">{{ t('nav.rfq') }}</div>
            <el-menu
              router
              :default-active="$route.path"
              class="nav-menu"
              :collapse="false"
              :collapse-transition="false"
            >
              <el-menu-item index="/customer" class="nav-item">
                <el-icon size="18"><UserFilled /></el-icon>
                <span>{{ t('nav.customer') }}</span>
              </el-menu-item>
              <el-menu-item index="/product" class="nav-item">
                <el-icon size="18"><Box /></el-icon>
                <span>{{ t('nav.product') }}</span>
              </el-menu-item>
              <el-menu-item index="/opportunity" class="nav-item">
                <el-icon size="18"><FolderOpened /></el-icon>
                <span>{{ t('nav.opportunity') }}</span>
              </el-menu-item>
              <el-menu-item index="/quotation/list" class="nav-item">
                <el-icon size="18"><Document /></el-icon>
                <span>{{ t('nav.quotation') }}</span>
              </el-menu-item>
              <el-menu-item index="/mfg/process" class="nav-item">
                <el-icon size="18"><Setting /></el-icon>
                <span>{{ t('nav.mfgProcess') }}</span>
              </el-menu-item>
            </el-menu>
          </div>

          <div class="nav-section">
            <div class="nav-section-title">{{ t('nav.pm') }}</div>
            <el-menu
              router
              :default-active="$route.path"
              class="nav-menu"
              :collapse="false"
              :collapse-transition="false"
            >
              <el-menu-item index="/pm/actual-cycle-time" class="nav-item">
                <el-icon size="18"><Timer /></el-icon>
                <span>{{ t('nav.actualCycleTime') }}</span>
              </el-menu-item>
              <el-menu-item index="/pm/trace" class="nav-item">
                <el-icon size="18"><TrendCharts /></el-icon>
                <span>{{ t('nav.productTrace') }}</span>
              </el-menu-item>
            </el-menu>
          </div>

          <div class="nav-section">
            <div class="nav-section-title">{{ t('nav.approval') }}</div>
            <el-menu
              router
              :default-active="$route.path"
              class="nav-menu"
              :collapse="false"
              :collapse-transition="false"
            >
              <el-menu-item index="/approval/center" class="nav-item">
                <el-icon size="18"><CircleCheck /></el-icon>
                <span>{{ t('nav.approvalCenter') }}</span>
              </el-menu-item>
            </el-menu>
          </div>

          <div class="nav-section">
            <div class="nav-section-title">{{ t('nav.salesReport') }}</div>
            <el-menu class="nav-menu">
              <el-menu-item index="/report/analysis" class="nav-item">
                <el-icon size="18"><DataAnalysis /></el-icon>
                <span>{{ t('nav.dataAnalysis') }}</span>
              </el-menu-item>
            </el-menu>
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
import { useRouter } from 'vue-router'
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
const userStore = useUserStore()
const { t, locale, setLocale } = useI18n()
const { loadNavigationConfig, groupedModules, resolveNavLabel, getIconComponent } = useNavigationConfig()
const searchQuery = ref('')

onMounted(() => {
  loadNavigationConfig()
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

const isAdmin = computed(() => userStore.roles.some(r => r.toUpperCase() === 'ADMIN'))

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
    userStore.switchSite(site)
    ElMessage.success(t('message.switchSiteSuccess', { site }))
    window.location.reload()
  } catch {
    ElMessage.error(t('common.failed'))
  }
}

const handleSwitchLanguage = (lang: 'zh' | 'en') => {
  if (lang === locale.value) return
  setLocale(lang)
  window.location.reload()
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
  color: rgba(255, 255, 255, 0.85) !important;
  margin: 2px var(--slds-spacing-sm);
  border-radius: var(--slds-border-radius);
  height: 44px;
  line-height: 44px;
  font-size: var(--slds-font-size-md);
  font-weight: 500;
  transition: all 0.2s;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.08) !important;
  color: #fff !important;
}

.nav-item.is-active {
  background: rgba(255, 255, 255, 0.12) !important;
  color: #fff !important;
  border-left: 3px solid #90D0FE;
}

.nav-item :deep(.el-icon) {
  margin-right: var(--slds-spacing-sm);
  color: inherit;
}

.nav-item :deep(.el-menu-tooltip__trigger) {
  justify-content: flex-start !important;
  padding-left: var(--slds-spacing-lg) !important;
}

/* Main Content */
.main-content {
  background: var(--slds-bg-page);
  padding: var(--slds-spacing-lg);
  overflow-y: auto;
}

:deep(.el-menu-item) {
  padding-left: var(--slds-spacing-lg) !important;
}
</style>

