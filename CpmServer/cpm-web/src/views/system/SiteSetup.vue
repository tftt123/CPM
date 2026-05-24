<template>
  <div class="site-setup">
    <el-form :model="form" label-width="120px" class="setup-form">
      <el-form-item :label="t('siteSetup.site')">
        <el-input v-model="form.site" disabled style="max-width: 300px;" />
        <span class="form-tip">{{ t('siteSetup.siteTip') }}</span>
      </el-form-item>

      <el-form-item :label="t('siteSetup.siteCode')">
        <el-input
          v-model="form.siteCode"
          :placeholder="t('siteSetup.siteCodePlaceholder')"
          style="max-width: 300px;"
          maxlength="20"
          show-word-limit
        />
        <span class="form-tip">{{ t('siteSetup.siteCodeTip') }}</span>
      </el-form-item>

      <el-form-item :label="t('siteSetup.currency')">
        <GcSelect
          v-model="form.currency"
          domain="CURRENCY"
          style="max-width: 300px;"
          :fallback="[
            { code: 'CNY', label: 'CNY', tagType: 'info' },
            { code: 'USD', label: 'USD', tagType: 'info' },
            { code: 'MYN', label: 'MYN', tagType: 'info' },
            { code: 'SGD', label: 'SGD', tagType: 'info' },
            { code: 'EUR', label: 'EUR', tagType: 'info' },
            { code: 'JPY', label: 'JPY', tagType: 'info' },
            { code: 'HKD', label: 'HKD', tagType: 'info' }
          ]"
        />
      </el-form-item>

      <el-form-item :label="t('siteSetup.description')">
        <el-input
          v-model="form.description"
          type="textarea"
          :rows="3"
          :placeholder="t('siteSetup.descriptionPlaceholder')"
          style="max-width: 500px;"
        />
      </el-form-item>

      <el-form-item>
        <el-button type="primary" @click="handleSave">
          {{ t('common.save') }}
        </el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useI18n } from '@/composables/useI18n'
import { getSiteSettings, saveSiteSettings } from '@/api/siteSettings'
import type { SiteSettings } from '@/api/siteSettings'
import { useUserStore } from '@/stores/user'
import GcSelect from '@/components/GcSelect.vue'

const { t } = useI18n()
const userStore = useUserStore()

const form = ref<SiteSettings>({
  site: userStore.currentSite || '',
  siteCode: '',
  currency: 'CNY',
  description: '',
})

async function loadSettings() {
  try {
    const res = await getSiteSettings()
    if (res.data) {
      form.value = {
        site: res.data.site || userStore.currentSite || '',
        siteCode: res.data.siteCode || '',
        currency: res.data.currency || 'CNY',
        description: res.data.description || '',
      }
    } else {
      form.value.site = userStore.currentSite || ''
    }
  } catch {
    form.value.site = userStore.currentSite || ''
  }
}

async function handleSave() {
  if (!form.value.currency) {
    ElMessage.warning(t('siteSetup.currencyRequired'))
    return
  }
  try {
    await saveSiteSettings(form.value)
    ElMessage.success(t('common.saveSuccess'))
  } catch {
    ElMessage.error(t('common.saveFailed'))
  }
}

onMounted(loadSettings)
</script>

<style scoped>
.site-setup {
  padding: var(--cpm-space-4);
}

.setup-form :deep(.el-form-item__label) {
  font-weight: 600;
}

.form-tip {
  margin-left: var(--cpm-space-3);
  color: var(--cpm-text-secondary);
  font-size: var(--cpm-text-small);
}
</style>
