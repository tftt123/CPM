<template>
  <div class="login-page">
    <div class="login-left">
      <div class="brand-section">
        <el-icon size="48" color="#90D0FE"><Cloudy /></el-icon>
        <h1 class="brand-title">{{ t('login.title') }}</h1>
        <p class="brand-subtitle">{{ t('login.subtitle') }}</p>
      </div>
      <div class="feature-list">
        <div class="feature-item">
          <el-icon size="20" color="#90D0FE"><Check /></el-icon>
          <span>{{ t('login.feature1') }}</span>
        </div>
        <div class="feature-item">
          <el-icon size="20" color="#90D0FE"><Check /></el-icon>
          <span>{{ t('login.feature2') }}</span>
        </div>
        <div class="feature-item">
          <el-icon size="20" color="#90D0FE"><Check /></el-icon>
          <span>{{ t('login.feature3') }}</span>
        </div>
      </div>
      <div class="login-footer">
        <span>{{ t('login.copyright') }}</span>
      </div>
    </div>

    <div class="login-right">
      <div class="login-card">
        <div class="login-header">
          <h2 class="login-title">{{ t('login.welcome') }}</h2>
          <p class="login-desc">{{ t('login.desc') }}</p>
        </div>

        <!-- Login Mode Toggle -->
        <div class="login-mode-toggle">
          <el-radio-group v-model="loginMode" size="default" fill="#1890ff">
            <el-radio-button label="local">{{ t('login.localLogin') }}</el-radio-button>
            <el-radio-button label="qad">{{ t('login.qadLogin') }}</el-radio-button>
          </el-radio-group>
        </div>

        <el-form :model="form" @submit.prevent="handleLogin" class="login-form">
          <el-form-item>
            <label class="form-label">{{ t('login.username') }}</label>
            <el-input
              v-model="form.username"
              :placeholder="t('login.usernamePlaceholder')"
              size="large"
              :prefix-icon="User"
            />
          </el-form-item>

          <el-form-item>
            <label class="form-label">{{ t('login.password') }}</label>
            <el-input
              v-model="form.password"
              type="password"
              :placeholder="t('login.passwordPlaceholder')"
              size="large"
              :prefix-icon="Lock"
              show-password
              @keyup.enter="handleLogin"
            />
          </el-form-item>

          <!-- QAD Login: Domain -->
          <el-form-item v-if="loginMode === 'qad'">
            <label class="form-label">{{ t('login.domain') }}</label>
            <el-input
              v-model="form.domain"
              :placeholder="t('login.domainPlaceholder')"
              size="large"
              :prefix-icon="OfficeBuilding"
            />
          </el-form-item>

          <!-- Local Login: Site -->
          <el-form-item v-if="loginMode === 'local'">
            <label class="form-label">{{ t('common.site') }}</label>
            <el-select
              v-model="selectedSite"
              :placeholder="t('login.sitePlaceholder')"
              size="large"
              style="width: 100%"
              clearable
            >
              <el-option
                v-for="site in siteList"
                :key="site"
                :label="site"
                :value="site"
              />
            </el-select>
          </el-form-item>

          <div class="form-options">
            <el-checkbox v-model="rememberMe">{{ t('login.rememberMe') }}</el-checkbox>
            <el-link type="primary" :underline="false">{{ t('login.forgotPassword') }}</el-link>
          </div>

          <el-form-item class="submit-item">
            <el-button
              type="primary"
              size="large"
              class="login-btn"
              :loading="loading"
              @click="handleLogin"
            >
              {{ t('login.loginBtn') }}
            </el-button>
          </el-form-item>
        </el-form>

        <!-- Language selector -->
        <div class="language-selector">
          <el-dropdown @command="handleSwitchLanguage" trigger="click">
            <div class="lang-trigger">
              <el-icon size="16"><MapLocation /></el-icon>
              <span>{{ currentLanguageLabel }}</span>
              <el-icon size="12"><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="zh" :class="{ 'is-active': locale === 'zh' }">
                  <el-icon v-if="locale === 'zh'"><CircleCheck /></el-icon>
                  <span v-else style="display:inline-block;width:16px"></span>
                  简体中文
                </el-dropdown-item>
                <el-dropdown-item command="en" :class="{ 'is-active': locale === 'en' }">
                  <el-icon v-if="locale === 'en'"><CircleCheck /></el-icon>
                  <span v-else style="display:inline-block;width:16px"></span>
                  English
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { authApi } from '@/api/auth'
import { useUserStore } from '@/stores/user'
import { useI18n } from '@/composables/useI18n'
import { User, Lock, Cloudy, Check, MapLocation, ArrowDown, CircleCheck, OfficeBuilding } from '@element-plus/icons-vue'

const router = useRouter()
const userStore = useUserStore()
const { t, locale, setLocale } = useI18n()

const loading = ref(false)
const rememberMe = ref(false)
const siteList = ref<string[]>([])
const loginMode = ref<'local' | 'qad'>('local')
const form = ref({ username: '', password: '', domain: '' })
const selectedSite = ref('')

const currentLanguageLabel = computed(() => locale.value === 'zh' ? 'CH' : 'EN')

const handleSwitchLanguage = (lang: 'zh' | 'en') => {
  if (lang !== locale.value) {
    setLocale(lang)
    window.location.reload()
  }
}

const loadSites = async () => {
  try {
    const res = await authApi.getSites()
    siteList.value = res.data || []
  } catch {
    siteList.value = []
  }
}

const handleLogin = async () => {
  if (!form.value.username || !form.value.password) {
    ElMessage.warning(t('login.inputRequired'))
    return
  }

  loading.value = true
  try {
    let res
    if (loginMode.value === 'qad') {
      if (!form.value.domain) {
        ElMessage.warning(t('login.domainPlaceholder'))
        loading.value = false
        return
      }
      res = await authApi.qadLogin({
        username: form.value.username,
        password: form.value.password,
        domain: form.value.domain
      })
    } else {
      if (!selectedSite.value) {
        ElMessage.warning(t('login.selectSite'))
        loading.value = false
        return
      }
      res = await authApi.login({
        username: form.value.username,
        password: form.value.password,
        site: selectedSite.value
      })
    }

    userStore.setToken(res.data.token)
    userStore.setUserInfo({
      username: res.data.username,
      realName: res.data.realName,
      site: res.data.site,
      roles: res.data.roles
    })
    const sitesRes = await authApi.getMySites()
    userStore.setSiteList(sitesRes.data || [res.data.site])
    ElMessage.success(t('login.loginSuccess'))
    router.push('/')
  } finally {
    loading.value = false
  }
}

loadSites()
</script>

<style scoped>
.login-page {
  display: flex;
  height: 100vh;
  width: 100vw;
}

.login-left {
  flex: 1;
  background: var(--slds-bg-sidebar);
  display: flex;
  flex-direction: column;
  justify-content: center;
  padding: 80px;
  position: relative;
}

.brand-section {
  margin-bottom: 48px;
}

.brand-title {
  color: #fff;
  font-size: 36px;
  font-weight: 700;
  margin: 24px 0 8px;
  letter-spacing: -0.5px;
}

.brand-subtitle {
  color: rgba(255, 255, 255, 0.65);
  font-size: 18px;
  font-weight: 400;
}

.feature-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.feature-item {
  display: flex;
  align-items: center;
  gap: 12px;
  color: rgba(255, 255, 255, 0.85);
  font-size: 16px;
  font-weight: 500;
}

.login-footer {
  position: absolute;
  bottom: 32px;
  left: 80px;
  color: rgba(255, 255, 255, 0.4);
  font-size: 12px;
}

.login-right {
  flex: 1;
  background: var(--slds-bg-page);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
}

.login-card {
  background: var(--slds-bg-card);
  border-radius: var(--slds-border-radius);
  box-shadow: var(--slds-shadow-card);
  padding: 48px;
  width: 100%;
  max-width: 440px;
}

.login-mode-toggle {
  display: flex;
  justify-content: center;
  margin-bottom: 24px;
}

.login-mode-toggle :deep(.el-radio-group) {
  width: 100%;
}

.login-mode-toggle :deep(.el-radio-button) {
  flex: 1;
}

.login-mode-toggle :deep(.el-radio-button__inner) {
  width: 100%;
}

.login-header {
  text-align: center;
  margin-bottom: 24px;
}

.login-title {
  font-size: 28px;
  font-weight: 700;
  color: var(--slds-text-primary);
  margin: 0 0 8px;
}

.login-desc {
  font-size: 14px;
  color: var(--slds-text-secondary);
  margin: 0;
}

.form-label {
  display: block;
  font-size: 12px;
  font-weight: 600;
  color: var(--slds-text-secondary);
  margin-bottom: 6px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.login-form :deep(.el-input__wrapper) {
  padding: 4px 12px;
  min-height: 44px;
}

.login-form :deep(.el-input__inner) {
  font-size: 15px;
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 8px 0 24px;
}

.form-options :deep(.el-checkbox__label) {
  font-size: 13px;
  color: var(--slds-text-secondary);
}

.submit-item {
  margin-bottom: 0 !important;
}

.login-btn {
  width: 100%;
  height: 48px;
  font-size: 16px;
  font-weight: 600;
  border-radius: var(--slds-border-radius);
}

.language-selector {
  display: flex;
  justify-content: center;
  margin-top: 24px;
  padding-top: 24px;
  border-top: 1px solid var(--slds-border-color-light);
}

.lang-trigger {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 14px;
  border-radius: var(--slds-border-radius);
  background: transparent;
  border: 1px solid var(--slds-border-color);
  cursor: pointer;
  transition: all 0.2s;
  font-size: 13px;
  color: var(--slds-text-secondary);
}

.lang-trigger:hover {
  background: var(--slds-bg-hover);
  border-color: var(--slds-border-color-light);
  color: var(--slds-text-primary);
}

:deep(.el-dropdown-menu__item.is-active) {
  color: var(--slds-brand-primary);
  font-weight: 600;
}

@media (max-width: 960px) {
  .login-left {
    display: none;
  }

  .login-right {
    flex: 1;
  }
}
</style>
