<template>
  <div class="gc-manage">
    <div class="gc-layout">
      <!-- Left: Domain List -->
      <div class="gc-sidebar">
        <div class="gc-sidebar-header">
          <span class="gc-sidebar-title">{{ t('gc.domainLabel') }}</span>
        </div>
        <div class="gc-domain-search">
          <el-input
            v-model="domainKeyword"
            :placeholder="t('common.pleaseInput')"
            clearable
            size="small"
            :prefix-icon="Search"
          />
        </div>
        <el-menu
          :default-active="currentDomain"
          class="gc-domain-menu"
          @select="handleDomainSelect"
        >
          <el-menu-item index="__all__">
            <span>{{ t('gc.allDomains') }}</span>
          </el-menu-item>
          <el-menu-item
            v-for="d in filteredDomains"
            :key="d.domain"
            :index="d.domain"
          >
            <span>{{ d.domain }}</span>
            <el-tag size="small" type="info" style="margin-left: auto;">{{ d.count }}</el-tag>
          </el-menu-item>
        </el-menu>
        <el-empty v-if="filteredDomains.length === 0" :description="t('gc.emptyTip')" />
      </div>

      <!-- Right: Code Table -->
      <div class="gc-content">
        <div class="toolbar">
          <el-select v-model="searchDomain" :placeholder="t('gc.domainLabel')" clearable style="width: 180px" @change="handleSearchDomainChange">
            <el-option :label="t('gc.allDomains')" value="__all__" />
            <el-option v-for="d in domains" :key="d.domain" :label="d.domain" :value="d.domain" />
          </el-select>
          <el-input v-model="codeKeyword" :placeholder="t('common.pleaseInput')" clearable style="width: 220px; margin-left: 12px;" @keyup.enter="handleCodeSearch" />
          <el-button type="primary" :icon="Search" style="margin-left: 12px;" @click="handleCodeSearch">{{ t('common.search') }}</el-button>
          <el-button :icon="RefreshRight" @click="handleReset">{{ t('common.reset') }}</el-button>

          <el-button v-if="domains.length === 0" type="warning" :loading="initing" @click="handleInit" style="margin-left: auto;">
            {{ t('gc.initDefault') }}
          </el-button>
          <template v-else>
            <el-button type="primary" :icon="Plus" style="margin-left: auto;" @click="handleAdd" :disabled="!currentDomain || currentDomain === '__all__'">
              {{ t('gc.add') }}
            </el-button>
            <el-button type="primary" :loading="saving" :icon="Check" @click="handleSave" style="margin-left: 12px;" :disabled="!currentDomain || currentDomain === '__all__'">
              {{ t('gc.saveAll') }}
            </el-button>
          </template>
        </div>

        <div v-if="currentDomain === '__all__'" class="gc-all-hint">
          <el-alert :title="t('gc.allDomains')" type="info" :closable="false" />
        </div>

        <el-table v-else-if="filteredCodes.length > 0" border :data="filteredCodes" v-loading="loading" stripe size="small">
          <el-table-column type="index" label="#" width="45" align="center" />
          <el-table-column :label="t('gc.code')" width="140">
            <template #default="{ row }">
              <el-input v-model="row.code" size="small" :disabled="row.id > 0" />
            </template>
          </el-table-column>
          <el-table-column :label="t('gc.label')" min-width="140">
            <template #default="{ row }">
              <el-input v-model="row.label" size="small" :placeholder="getFallbackLabel(row, 'zh')" />
            </template>
          </el-table-column>
          <el-table-column :label="t('gc.labelEn')" min-width="140">
            <template #default="{ row }">
              <el-input v-model="row.labelEn" size="small" :placeholder="getFallbackLabel(row, 'en')" />
            </template>
          </el-table-column>
          <el-table-column :label="t('gc.tagType')" width="110">
            <template #default="{ row }">
              <el-select v-model="row.tagType" size="small" clearable style="width: 95px">
                <el-option label="info" value="info" />
                <el-option label="success" value="success" />
                <el-option label="warning" value="warning" />
                <el-option label="danger" value="danger" />
                <el-option label="primary" value="primary" />
              </el-select>
            </template>
          </el-table-column>
          <el-table-column :label="t('gc.sortOrder')" width="80" align="center">
            <template #default="{ row }">
              <el-input-number v-model="row.sortOrder" size="small" :min="0" :max="999" :controls="false" style="width: 55px" />
            </template>
          </el-table-column>
          <el-table-column :label="t('gc.isActive')" width="70" align="center">
            <template #default="{ row }">
              <el-switch v-model="row.isActive" size="small" />
            </template>
          </el-table-column>
          <el-table-column :label="t('common.action')" width="70" align="center" fixed="right">
            <template #default="{ $index }">
              <el-button link type="danger" size="small" :icon="Delete" @click="handleRemove($index)">
                {{ t('gc.delete') }}
              </el-button>
            </template>
          </el-table-column>
        </el-table>

        <el-empty v-else :description="t('gc.emptyTip')" />
      </div>
    </div>

    <!-- Add/Edit Dialog -->
    <el-dialog
      :title="isEdit ? t('gc.editTitle') : t('gc.addTitle')"
      v-model="dialogVisible"
      width="480px"
      :close-on-click-modal="false"
    >
      <el-form :model="form" label-width="100px" ref="formRef" :rules="rules">
        <el-form-item :label="t('gc.domain')" prop="domain">
          <el-input v-model="form.domain" :disabled="isEdit" />
        </el-form-item>
        <el-form-item :label="t('gc.code')" prop="code">
          <el-input v-model="form.code" :disabled="isEdit" />
        </el-form-item>
        <el-form-item :label="t('gc.label')">
          <el-input v-model="form.label" />
        </el-form-item>
        <el-form-item :label="t('gc.labelEn')">
          <el-input v-model="form.labelEn" />
        </el-form-item>
        <el-form-item :label="t('gc.tagType')">
          <el-select v-model="form.tagType" clearable style="width: 100%">
            <el-option label="info" value="info" />
            <el-option label="success" value="success" />
            <el-option label="warning" value="warning" />
            <el-option label="danger" value="danger" />
            <el-option label="primary" value="primary" />
          </el-select>
        </el-form-item>
        <el-form-item :label="t('gc.sortOrder')">
          <el-input-number v-model="form.sortOrder" :min="0" :max="999" />
        </el-form-item>
        <el-form-item :label="t('gc.isActive')">
          <el-switch v-model="form.isActive" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">{{ t('common.cancel') }}</el-button>
        <el-button type="primary" @click="handleSubmit">{{ t('common.confirm') }}</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useI18n } from '@/composables/useI18n'
import { useLocaleStore } from '@/stores/locale'
import { generalizedCodeApi, type GeneralizedCode } from '@/api/generalizedCode'
import { Plus, Check, Delete, Search, RefreshRight } from '@element-plus/icons-vue'

const { t } = useI18n()
const localeStore = useLocaleStore()

const loading = ref(false)
const saving = ref(false)
const initing = ref(false)
const domains = ref<{ domain: string; count: number }[]>([])
const currentDomain = ref('__all__')
const searchDomain = ref('__all__')
const codes = ref<GeneralizedCode[]>([])
const domainKeyword = ref('')
const codeKeyword = ref('')

const filteredDomains = computed(() => {
  if (!domainKeyword.value.trim()) return domains.value
  const kw = domainKeyword.value.trim().toLowerCase()
  return domains.value.filter(d => d.domain.toLowerCase().includes(kw))
})

const filteredCodes = computed(() => {
  if (!codeKeyword.value.trim()) return codes.value
  const kw = codeKeyword.value.trim().toLowerCase()
  return codes.value.filter(c =>
    (c.code || '').toLowerCase().includes(kw) ||
    (c.label || '').toLowerCase().includes(kw) ||
    (c.labelEn || '').toLowerCase().includes(kw)
  )
})

const handleCodeSearch = () => {
  // computed property auto-updates
}
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref<any>(null)

const form = ref<GeneralizedCode>({
  id: 0,
  domain: '',
  code: '',
  label: '',
  labelEn: '',
  sortOrder: 0,
  isActive: true,
  tagType: 'info',
  attributes: null
})

const rules = {
  domain: [{ required: true, message: '请输入代码域', trigger: 'blur' }],
  code: [{ required: true, message: '请输入代码值', trigger: 'blur' }]
}

// Load domain list
const loadDomains = async () => {
  try {
    const res = await generalizedCodeApi.getDomains()
    domains.value = res.data || []
  } catch {
    domains.value = []
  }
}

// Load codes for selected domain
const loadCodes = async () => {
  if (currentDomain.value === '__all__') {
    codes.value = []
    return
  }
  loading.value = true
  try {
    const res = await generalizedCodeApi.getByDomain(currentDomain.value)
    codes.value = res.data || []
  } finally {
    loading.value = false
  }
}

const handleDomainSelect = (domain: string) => {
  currentDomain.value = domain
  searchDomain.value = domain
  loadCodes()
}

const handleSearchDomainChange = (domain: string) => {
  currentDomain.value = domain || '__all__'
  loadCodes()
}

const handleReset = () => {
  searchDomain.value = '__all__'
  currentDomain.value = '__all__'
  codeKeyword.value = ''
  domainKeyword.value = ''
  loadCodes()
}

function getFallbackLabel(row: GeneralizedCode, locale: 'zh' | 'en'): string {
  const key = `gc.${row.domain}.${row.code}`
  const msg = localeStore.getMessage(key, locale)
  if (msg !== key) return msg
  return row.code
}

const handleAdd = () => {
  isEdit.value = false
  form.value = {
    id: 0,
    domain: currentDomain.value,
    code: '',
    label: '',
    labelEn: '',
    sortOrder: 0,
    isActive: true,
    tagType: 'info',
    attributes: null
  }
  dialogVisible.value = true
}

const handleSubmit = async () => {
  if (!formRef.value) return
  await formRef.value.validate((valid: boolean) => {
    if (!valid) return

    const exists = codes.value.find(c => c.code === form.value.code)
    if (!isEdit.value && exists) {
      ElMessage.error('代码值已存在')
      return
    }

    if (isEdit.value && exists) {
      const idx = codes.value.findIndex(c => c.code === form.value.code)
      if (idx >= 0) {
        codes.value[idx] = { ...form.value }
      }
    } else {
      codes.value.push({ ...form.value })
    }

    dialogVisible.value = false
    ElMessage.success(isEdit.value ? t('message.updateSuccess') : t('message.createSuccess'))
  })
}

const handleRemove = async (index: number) => {
  try {
    await ElMessageBox.confirm(t('gc.deleteConfirm'), t('common.tip'), {
      confirmButtonText: t('common.confirm'),
      cancelButtonText: t('common.cancel'),
      type: 'warning'
    })
  } catch {
    return
  }
  codes.value.splice(index, 1)
}

const handleInit = async () => {
  try {
    await ElMessageBox.confirm(t('gc.initConfirm'), t('common.tip'), {
      confirmButtonText: t('common.confirm'),
      cancelButtonText: t('common.cancel'),
      type: 'warning'
    })
  } catch {
    return
  }

  initing.value = true
  try {
    await generalizedCodeApi.init()
    await loadDomains()
    ElMessage.success(t('message.createSuccess'))
  } catch {
    ElMessage.error(t('common.failed'))
  } finally {
    initing.value = false
  }
}

const handleSave = async () => {
  if (currentDomain.value === '__all__') return
  saving.value = true
  try {
    const items = codes.value.map(c => ({
      code: c.code,
      label: c.label,
      labelEn: c.labelEn,
      sortOrder: c.sortOrder,
      isActive: c.isActive,
      tagType: c.tagType,
      attributes: c.attributes
    }))
    await generalizedCodeApi.batchUpdate(currentDomain.value, items)
    await loadDomains()
    ElMessage.success(t('message.saveSuccess'))
  } catch {
    ElMessage.error(t('common.failed'))
  } finally {
    saving.value = false
  }
}

watch(() => currentDomain.value, () => {
  loadCodes()
})

onMounted(loadDomains)
</script>

<style scoped>
.gc-manage {
  padding: 0;
}
.gc-layout {
  display: flex;
  gap: 16px;
  min-height: 500px;
}
.gc-sidebar {
  width: 240px;
  flex-shrink: 0;
  border: 1px solid var(--el-border-color-light);
  border-radius: var(--el-border-radius-base);
  background: var(--el-bg-color);
}
.gc-sidebar-header {
  padding: 12px 16px;
  border-bottom: 1px solid var(--el-border-color-light);
  font-weight: 600;
  font-size: 14px;
}
.gc-domain-search {
  padding: 8px 12px;
  border-bottom: 1px solid var(--el-border-color-light);
}
.gc-domain-menu {
  border-right: none;
}
.gc-content {
  flex: 1;
  min-width: 0;
}
.toolbar {
  display: flex;
  align-items: center;
  margin-bottom: 16px;
}
.gc-all-hint {
  margin-top: 40px;
}
</style>
