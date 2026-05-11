<template>
  <div class="page-container">
    <div class="page-header-section">
      <el-page-header @back="$router.push('/home')">
        <template #content>
          <div class="page-header-content">
            <h1 class="page-title">{{ t('profile.title') }}</h1>
            <span class="page-subtitle">{{ t('profile.subtitle') }}</span>
          </div>
        </template>
      </el-page-header>
    </div>

    <el-row :gutter="24">
      <!-- 左侧：头像和基本信息 -->
      <el-col :xs="24" :md="8">
        <div class="content-card profile-card">
          <div class="avatar-section">
            <el-avatar :size="100" :src="form.avatarUrl || ''" :icon="UserFilled" class="profile-avatar" />
            <el-upload
              class="avatar-uploader"
              action="#"
              :auto-upload="false"
              :show-file-list="false"
              :on-change="handleAvatarChange"
              accept="image/*"
            >
              <el-button type="primary" size="small">{{ t('profile.uploadAvatar') }}</el-button>
            </el-upload>
          </div>
          <div class="profile-info">
            <div class="info-item">
              <span class="info-label">{{ t('profile.username') }}</span>
              <span class="info-value">{{ form.username }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">{{ t('profile.site') }}</span>
              <span class="info-value">{{ form.site || '-' }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">{{ t('profile.roles') }}</span>
              <div class="info-value">
                <el-tag v-for="role in form.roles" :key="role" size="small" class="role-tag">{{ role }}</el-tag>
              </div>
            </div>
          </div>
        </div>
      </el-col>

      <!-- 右侧：编辑表单 -->
      <el-col :xs="24" :md="16">
        <el-tabs v-model="activeTab" type="border-card" class="settings-tabs">
          <!-- 基本信息 -->
          <el-tab-pane :label="t('profile.basicInfo')" name="basic">
            <el-form :model="form" label-width="120px" class="slds-form">
              <el-form-item :label="t('profile.realName')">
                <el-input v-model="form.realName" :placeholder="t('common.pleaseInput')" />
              </el-form-item>
              <el-form-item :label="t('profile.email')">
                <el-input v-model="form.email" :placeholder="t('common.pleaseInput')" />
              </el-form-item>
              <el-form-item :label="t('profile.phone')">
                <el-input v-model="form.phone" :placeholder="t('common.pleaseInput')" />
              </el-form-item>
              <el-form-item>
                <el-button type="primary" :loading="saving" @click="handleSave">
                  {{ t('profile.save') }}
                </el-button>
              </el-form-item>
            </el-form>
          </el-tab-pane>

          <!-- 修改密码 -->
          <el-tab-pane :label="t('profile.changePassword')" name="password">
            <el-form :model="pwdForm" :rules="pwdRules" ref="pwdFormRef" label-width="160px" class="slds-form">
              <el-form-item :label="t('profile.currentPassword')" prop="currentPassword">
                <el-input v-model="pwdForm.currentPassword" type="password" show-password :placeholder="t('common.pleaseInput')" />
              </el-form-item>
              <el-form-item :label="t('profile.newPassword')" prop="newPassword">
                <el-input v-model="pwdForm.newPassword" type="password" show-password :placeholder="t('common.pleaseInput')" />
              </el-form-item>
              <el-form-item :label="t('profile.confirmPassword')" prop="confirmPassword">
                <el-input v-model="pwdForm.confirmPassword" type="password" show-password :placeholder="t('common.pleaseInput')" />
              </el-form-item>
              <el-form-item>
                <el-button type="primary" :loading="changingPwd" @click="handleChangePassword">
                  {{ t('profile.changePassword') }}
                </el-button>
              </el-form-item>
            </el-form>
          </el-tab-pane>
        </el-tabs>
      </el-col>
    </el-row>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { UserFilled } from '@element-plus/icons-vue'
import { useI18n } from '@/composables/useI18n'
import { profileApi } from '@/api/user'
import { useUserStore } from '@/stores/user'

const { t } = useI18n()
const userStore = useUserStore()

const activeTab = ref('basic')
const saving = ref(false)
const changingPwd = ref(false)
const pwdFormRef = ref()

const form = ref({
  username: '',
  realName: '',
  email: '',
  phone: '',
  site: '',
  avatarUrl: '',
  roles: [] as string[]
})

const pwdForm = ref({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const pwdRules = {
  currentPassword: [{ required: true, message: t('validation.required', { field: t('profile.currentPassword') }), trigger: 'blur' }],
  newPassword: [
    { required: true, message: t('validation.required', { field: t('profile.newPassword') }), trigger: 'blur' },
    { min: 6, message: t('system.passwordMinLength'), trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, message: t('validation.required', { field: t('profile.confirmPassword') }), trigger: 'blur' },
    {
      validator: (_: any, value: string, callback: any) => {
        if (value !== pwdForm.value.newPassword) {
          callback(new Error(t('profile.passwordMismatch')))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ]
}

const loadProfile = async () => {
  try {
    const res = await profileApi.getProfile()
    if (res.data) {
      const data = res.data
      form.value = {
        username: data.username || '',
        realName: data.realName || '',
        email: data.email || '',
        phone: data.phone || '',
        site: data.site || '',
        avatarUrl: data.avatarUrl || '',
        roles: data.roles || []
      }
    }
  } catch {
    const info = userStore.userInfo
    if (info) {
      form.value.username = info.username || ''
      form.value.realName = info.realName || ''
      form.value.site = info.site || ''
      form.value.roles = info.roles || []
    }
  }
}

const handleAvatarChange = (file: any) => {
  const reader = new FileReader()
  reader.onload = (e) => {
    form.value.avatarUrl = e.target?.result as string
  }
  reader.readAsDataURL(file.raw)
}

const handleSave = async () => {
  saving.value = true
  try {
    await profileApi.updateProfile({
      realName: form.value.realName,
      email: form.value.email,
      phone: form.value.phone,
      avatarUrl: form.value.avatarUrl
    })
    if (userStore.userInfo) {
      userStore.userInfo.realName = form.value.realName
      userStore.userInfo.avatarUrl = form.value.avatarUrl
      localStorage.setItem('userInfo', JSON.stringify(userStore.userInfo))
    }
    ElMessage.success(t('profile.saveSuccess'))
  } catch {
    ElMessage.error(t('common.failed'))
  } finally {
    saving.value = false
  }
}

const handleChangePassword = async () => {
  await pwdFormRef.value.validate()
  changingPwd.value = true
  try {
    await profileApi.changePassword({
      currentPassword: pwdForm.value.currentPassword,
      newPassword: pwdForm.value.newPassword
    })
    ElMessage.success(t('profile.passwordChanged'))
    pwdForm.value = { currentPassword: '', newPassword: '', confirmPassword: '' }
  } catch {
    ElMessage.error(t('profile.passwordChangeFailed'))
  } finally {
    changingPwd.value = false
  }
}

onMounted(() => {
  loadProfile()
})
</script>

<style scoped>
.page-container {
  padding: var(--slds-spacing-lg);
}

.page-header-section {
  background: var(--slds-bg-card);
  border-bottom: 1px solid var(--slds-border-color);
  padding: var(--slds-spacing-lg);
  margin: calc(-1 * var(--slds-spacing-lg));
  margin-bottom: var(--slds-spacing-lg);
}

.page-header-content {
  display: flex;
  flex-direction: column;
}

.page-title {
  font-size: var(--slds-font-size-xl);
  font-weight: 700;
  color: var(--slds-text-primary);
  margin: 0;
}

.page-subtitle {
  font-size: var(--slds-font-size-sm);
  color: var(--slds-text-secondary);
  margin-top: var(--slds-spacing-xs);
}

.content-card {
  background: var(--slds-bg-card);
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
  box-shadow: var(--slds-shadow-card);
  overflow: hidden;
}

.profile-card {
  padding: var(--slds-spacing-xl);
  text-align: center;
}

.avatar-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--slds-spacing-md);
  margin-bottom: var(--slds-spacing-xl);
}

.profile-avatar {
  background: var(--slds-brand-primary);
  color: white;
  font-size: 40px;
}

.avatar-uploader {
  display: flex;
  justify-content: center;
}

.profile-info {
  text-align: left;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--slds-spacing-sm) 0;
  border-bottom: 1px solid var(--slds-border-color-light);
}

.info-item:last-child {
  border-bottom: none;
}

.info-label {
  font-size: 13px;
  color: var(--slds-text-secondary);
  font-weight: 500;
}

.info-value {
  font-size: 13px;
  color: var(--slds-text-primary);
  font-weight: 600;
  display: flex;
  gap: 4px;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.role-tag {
  margin: 0;
}

.settings-tabs {
  background: var(--slds-bg-card);
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
  box-shadow: var(--slds-shadow-card);
  overflow: hidden;
}

.settings-tabs :deep(.el-tabs__header) {
  margin: 0;
  background: #FAFBFC;
  border-bottom: 1px solid var(--slds-border-color-light);
}

.settings-tabs :deep(.el-tabs__content) {
  padding: var(--slds-spacing-lg);
}

.slds-form :deep(.el-form-item__label) {
  font-size: 13px;
  font-weight: 600;
  color: var(--slds-text-secondary);
}
</style>
