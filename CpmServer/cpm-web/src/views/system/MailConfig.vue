<template>
  <div>
    <div class="page-header-content" style="margin-bottom: 24px;">
      <h1 class="page-title">{{ t('mail.title') }}</h1>
      <span class="page-subtitle">{{ t('mail.subtitle') }}</span>
    </div>

    <div class="content-card">
      <el-form
        :model="form"
        label-width="140px"
        :rules="rules"
        ref="formRef"
        class="slds-form"
        style="max-width: 600px;"
      >
        <el-form-item :label="t('mail.smtpServer')" prop="smtpServer">
          <el-input
            v-model="form.smtpServer"
            :placeholder="t('common.pleaseInput')"
          />
        </el-form-item>

        <el-form-item :label="t('mail.smtpPort')" prop="smtpPort">
          <el-input-number
            v-model="form.smtpPort"
            :min="1"
            :max="65535"
            controls-position="right"
            style="width: 100%;"
          />
        </el-form-item>

        <el-form-item :label="t('mail.smtpUsername')" prop="smtpUsername">
          <el-input
            v-model="form.smtpUsername"
            :placeholder="t('common.pleaseInput')"
          />
        </el-form-item>

        <el-form-item :label="t('mail.smtpPassword')" prop="smtpPassword">
          <el-input
            v-model="form.smtpPassword"
            type="password"
            show-password
            :placeholder="t('common.pleaseInput')"
          />
        </el-form-item>

        <el-form-item :label="t('mail.fromAddress')" prop="fromAddress">
          <el-input
            v-model="form.fromAddress"
            :placeholder="t('common.pleaseInput')"
          />
        </el-form-item>

        <el-form-item :label="t('mail.fromName')">
          <el-input
            v-model="form.fromName"
            :placeholder="t('common.pleaseInput')"
          />
        </el-form-item>

        <el-form-item :label="t('mail.enableSsl')">
          <el-checkbox v-model="form.enableSsl">
            {{ form.enableSsl ? t('common.yes') : t('common.no') }}
          </el-checkbox>
        </el-form-item>

        <el-form-item>
          <el-button type="primary" :icon="Check" :loading="saving" @click="handleSave">
            {{ t('common.save') }}
          </el-button>
        </el-form-item>
      </el-form>
    </div>

    <!-- Test Email Section -->
    <div class="content-card" style="margin-top: var(--slds-spacing-lg);">
      <div class="card-header" style="margin-bottom: var(--slds-spacing-md);">
        <h3 class="card-title">{{ t('mail.testEmail') }}</h3>
      </div>
      <el-form
        :model="testForm"
        label-width="140px"
        :rules="testRules"
        ref="testFormRef"
        class="slds-form"
        style="max-width: 600px;"
      >
        <el-form-item :label="t('mail.testTo')" prop="toAddress">
          <el-input
            v-model="testForm.toAddress"
            :placeholder="t('common.pleaseInput')"
          />
        </el-form-item>

        <el-form-item :label="t('mail.testSubject')" prop="subject">
          <el-input
            v-model="testForm.subject"
            :placeholder="t('common.pleaseInput')"
          />
        </el-form-item>

        <el-form-item :label="t('mail.testBody')" prop="body">
          <el-input
            v-model="testForm.body"
            type="textarea"
            :rows="4"
            :placeholder="t('common.pleaseInput')"
          />
        </el-form-item>

        <el-form-item>
          <el-button type="success" :icon="Promotion" :loading="testing" @click="handleTest">
            {{ t('mail.testEmail') }}
          </el-button>
        </el-form-item>
      </el-form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { emailApi } from '@/api/email'
import { useI18n } from '@/composables/useI18n'
import { Check, Promotion } from '@element-plus/icons-vue'

const { t } = useI18n()
const formRef = ref()
const testFormRef = ref()
const saving = ref(false)
const testing = ref(false)

const form = ref({
  id: null as number | null,
  smtpServer: 'smtp.qiye.aliyun.com',
  smtpPort: 465,
  smtpUsername: '',
  smtpPassword: 'VIofMFXhpgPoWCFV',
  fromAddress: '',
  fromName: 'CPM System',
  enableSsl: true,
  isActive: true
})

const testForm = ref({
  toAddress: '',
  subject: 'CPM Test Email',
  body: 'This is a test email from CPM System.'
})

const rules = {
  smtpServer: [{ required: true, message: t('validation.required', { field: t('mail.smtpServer') }), trigger: 'blur' }],
  smtpPort: [{ required: true, message: t('validation.required', { field: t('mail.smtpPort') }), trigger: 'blur' }],
  smtpUsername: [{ required: true, message: t('validation.required', { field: t('mail.smtpUsername') }), trigger: 'blur' }],
  smtpPassword: [{ required: true, message: t('validation.required', { field: t('mail.smtpPassword') }), trigger: 'blur' }],
  fromAddress: [{ required: true, message: t('validation.required', { field: t('mail.fromAddress') }), trigger: 'blur' }]
}

const testRules = {
  toAddress: [
    { required: true, message: t('validation.required', { field: t('mail.testTo') }), trigger: 'blur' },
    { type: 'email', message: 'Invalid email format', trigger: 'blur' }
  ],
  subject: [{ required: true, message: t('validation.required', { field: t('mail.testSubject') }), trigger: 'blur' }],
  body: [{ required: true, message: t('validation.required', { field: t('mail.testBody') }), trigger: 'blur' }]
}

const loadConfig = async () => {
  try {
    const res = await emailApi.getConfig()
    if (res.data) {
      form.value = { ...form.value, ...res.data }
    }
  } catch (e) {
    console.log('Load mail config failed:', e)
  }
}

const handleSave = async () => {
  await formRef.value.validate()
  saving.value = true
  try {
    await emailApi.saveConfig(form.value)
    ElMessage.success(t('message.saveSuccess'))
    loadConfig()
  } finally {
    saving.value = false
  }
}

const handleTest = async () => {
  await testFormRef.value.validate()
  testing.value = true
  try {
    await emailApi.sendTest({
      toAddress: testForm.value.toAddress,
      subject: testForm.value.subject,
      body: testForm.value.body
    })
    ElMessage.success(t('mail.testSuccess'))
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || t('mail.testFailed'))
  } finally {
    testing.value = false
  }
}

onMounted(loadConfig)
</script>

<style scoped>
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
  padding: var(--slds-spacing-lg);
}

.card-title {
  font-size: var(--slds-font-size-md);
  font-weight: 600;
  color: var(--slds-text-primary);
  margin: 0;
}

.slds-form :deep(.el-form-item__label) {
  font-size: 13px;
  font-weight: 600;
  color: var(--slds-text-secondary);
}

/* 统一表单控件高度为 32px */
.slds-form :deep(.el-input__wrapper) {
  height: 32px !important;
  min-height: 32px !important;
  padding: 1px 11px !important;
}

.slds-form :deep(.el-input__inner) {
  height: 30px !important;
  line-height: 30px !important;
}

/* el-input-number 高度同步 */
.slds-form :deep(.el-input-number .el-input__wrapper) {
  height: 32px !important;
  min-height: 32px !important;
  padding: 1px 11px !important;
}

.slds-form :deep(.el-input-number .el-input__inner) {
  height: 30px !important;
  line-height: 30px !important;
}
</style>
