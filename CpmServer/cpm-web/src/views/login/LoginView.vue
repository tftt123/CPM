<template>
  <div class="login-page">
    <div class="login-left">
      <div class="brand-section">
        <div class="brand-header">
          <img src="/spxlogo/logo.png" alt="SIP" class="brand-logo" />
          <h1 class="brand-title">{{ t('login.title') }}</h1>
        </div>
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
      <div v-if="false" class="login-footer">
        <span>{{ t('login.copyright') }}</span>
      </div>
    </div>

    <div class="login-right">
      <div class="login-card">
        <div class="login-header">
          <h2 class="login-title">{{ t('login.welcome') }}</h2>
          <p class="login-desc">{{ t('login.desc') }}</p>
        </div>

        <el-form :model="form" @submit.prevent="handleLogin" class="login-form">
          <!-- Username -->
          <el-form-item>
            <label class="form-label">{{ t('login.username') }}</label>
            <el-input
              v-model="form.username"
              :placeholder="t('login.usernamePlaceholder')"
              size="large"
              :prefix-icon="User"
            />
          </el-form-item>

          <!-- Password -->
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

          <!-- Login Type (dropdown) -->
          <el-form-item>
            <label class="form-label">{{ t('login.loginType') }}</label>
            <el-select
              v-model="loginMode"
              :placeholder="t('login.loginTypePlaceholder')"
              size="large"
              style="width: 100%"
              @change="onLoginModeChange"
            >
              <el-option :label="t('login.qadLogin')" value="qad" />
              <el-option :label="t('login.localLogin')" value="local" />
            </el-select>
          </el-form-item>

          <!-- Unified Domain field -->
          <el-form-item>
            <label class="form-label">{{ t('login.domain') }}</label>
            <GcSelect
              v-model="domainValue"
              domain="LOGIN_DOMAIN"
              :placeholder="t('login.domainPlaceholder')"
              style="width: 100%"
              clearable
              :fallback="[
                { code: 'NT01', label: 'NT01', tagType: 'info' },
                { code: 'MY01', label: 'MY01', tagType: 'info' }
              ]"
            />
          </el-form-item>

          <div class="form-options">
            <el-checkbox v-model="rememberMe">{{ t('login.rememberMe') }}</el-checkbox>
            <el-link v-if="false" type="primary" :underline="false">{{ t('login.forgotPassword') }}</el-link>
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
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { authApi } from '@/api/auth'
import { useUserStore } from '@/stores/user'
import { useI18n } from '@/composables/useI18n'
import GcSelect from '@/components/GcSelect.vue'
import { User, Lock, Check, MapLocation, ArrowDown, CircleCheck } from '@element-plus/icons-vue'

const router = useRouter()
const userStore = useUserStore()
const { t, locale, setLocale } = useI18n()

const loading = ref(false)
const rememberMe = ref(false)
const loginMode = ref<'local' | 'qad'>('local')
const form = ref({ username: '', password: '', domain: '' })
const selectedSite = ref('')

const domainValue = computed({
  get() {
    return loginMode.value === 'qad' ? form.value.domain : selectedSite.value
  },
  set(val: string) {
    if (loginMode.value === 'qad') {
      form.value.domain = val
    } else {
      selectedSite.value = val
    }
  }
})

const currentLanguageLabel = computed(() => locale.value === 'zh' ? 'CH' : 'EN')

const handleSwitchLanguage = (lang: 'zh' | 'en') => {
  if (lang !== locale.value) {
    setLocale(lang)
  }
}

const loadRemembered = () => {
  const saved = localStorage.getItem('login_remember')
  if (saved) {
    try {
      const data = JSON.parse(saved)
      form.value.username = data.username || ''
      loginMode.value = data.loginMode || 'local'
      if (loginMode.value === 'qad') {
        form.value.domain = data.domain || ''
      } else {
        selectedSite.value = data.site || ''
      }
      rememberMe.value = true
    } catch {
      // ignore parse error
    }
  }
}

const saveRemembered = () => {
  if (rememberMe.value) {
    localStorage.setItem('login_remember', JSON.stringify({
      username: form.value.username,
      site: selectedSite.value,
      domain: form.value.domain,
      loginMode: loginMode.value,
    }))
  } else {
    localStorage.removeItem('login_remember')
  }
}

const onLoginModeChange = () => {
  domainValue.value = ''
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
      roles: res.data.roles,
      permissions: res.data.permissions || []
    })
    const sitesRes = await authApi.getMySites()
    userStore.setSiteList(sitesRes.data || [res.data.site])
    saveRemembered()
    ElMessage.success(t('login.loginSuccess'))
    router.push('/')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadRemembered()
})
</script>

<style scoped>
.login-page {
  display: flex;
  height: 100vh;
  width: 100vw;
}

.login-left {
  flex: 1;
  background: var(--cpm-bg-sidebar);
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  padding: 80px;
  position: relative;
}

.brand-section {
  margin-bottom: 48px;
  text-align: center;
}

.brand-header {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0;
  margin-bottom: 8px;
}

.brand-logo {
  width: 80px;
  height: 48px;
  object-fit: contain;
}

.brand-title {
  color: #fff;
  font-size: 36px;
  font-weight: 700;
  margin: 0;
  letter-spacing: 1.5px;
  transform: translateX(-5px);
}

.brand-subtitle {
  color: rgba(255, 255, 255, 0.65);
  font-size: 18px;
  font-weight: 400;
  text-align: center;
}

.feature-list {
  display: flex;
  flex-direction: column;
  align-items: center;
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
  background: var(--cpm-bg-page);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
}

.login-card {
  background: var(--cpm-bg-card);
  border-radius: var(--cpm-radius-md);
  box-shadow: var(--cpm-shadow-sm);
  padding: 48px;
  width: 100%;
  max-width: 440px;
}

.login-header {
  text-align: center;
  margin-bottom: 24px;
}

.login-title {
  font-size: var(--cpm-text-display);
  font-weight: 700;
  color: var(--cpm-text-primary);
  margin: 0 0 8px;
  letter-spacing: -0.5px;
  line-height: 1.2;
}

.login-desc {
  font-size: var(--cpm-text-body);
  color: var(--cpm-text-secondary);
  margin: 0;
}

.form-label {
  display: block;
  font-size: var(--cpm-text-small);
  font-weight: 600;
  color: var(--cpm-text-secondary);
  margin-bottom: 6px;
  letter-spacing: 0.3px;
}

.login-form :deep(.el-input__wrapper) {
  padding: 4px 12px;
  min-height: 44px;
}

.login-form :deep(.el-input__inner) {
  font-size: 15px;
}

.login-form :deep(.el-select .el-input__wrapper) {
  min-height: 44px;
  height: 44px;
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 8px 0 24px;
}

.form-options :deep(.el-checkbox__label) {
  font-size: 13px;
  color: var(--cpm-text-secondary);
}

.submit-item {
  margin-bottom: 0 !important;
}

.login-btn {
  width: 100%;
  height: 48px;
  font-size: 16px;
  font-weight: 600;
  border-radius: var(--cpm-radius-sm);
}

.language-selector {
  display: flex;
  justify-content: center;
  margin-top: 24px;
  padding-top: 24px;
  border-top: 1px solid var(--cpm-border-light);
}

.lang-trigger {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 14px;
  border-radius: var(--cpm-radius-sm);
  background: transparent;
  border: 1px solid var(--cpm-border);
  cursor: pointer;
  transition: all 0.2s;
  font-size: 13px;
  color: var(--cpm-text-secondary);
}

.lang-trigger:hover {
  background: var(--cpm-bg-hover);
  border-color: var(--cpm-border-light);
  color: var(--cpm-text-primary);
}

:deep(.el-dropdown-menu__item.is-active) {
  color: var(--cpm-brand-accent);
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
