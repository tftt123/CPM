<template>
  <div>
    <div class="toolbar">
      <el-select v-model="query.category" :placeholder="t('common.pleaseSelect')" clearable style="width: 160px" @change="handleSearch">
        <el-option v-for="c in categories" :key="c" :label="c" :value="c" />
      </el-select>
      <el-input v-model="query.keyword" :placeholder="t('common.pleaseInput')" clearable style="width: 260px; margin-left: 12px;" @keyup.enter="handleSearch" />
      <el-button type="primary" :icon="Search" style="margin-left: 12px;" @click="handleSearch">{{ t('common.search') }}</el-button>
      <el-button :icon="RefreshRight" @click="handleReset">{{ t('common.reset') }}</el-button>
      <el-button type="primary" :icon="Plus" style="margin-left: auto;" @click="handleAdd">{{ t('common.create') }}</el-button>
    </div>

    <el-table :data="filteredData" v-loading="loading" stripe border size="small">
      <el-table-column type="index" label="#" width="50" align="center" />
      <el-table-column prop="messageKey" :label="t('system.fieldCode')" min-width="180">
        <template #default="{ row }">
          <div class="key-cell">
            <span>{{ row.messageKey }}</span>
            <el-tag v-if="row.source === 'static'" size="small" type="info">静态</el-tag>
            <el-tag v-else size="small" type="success">DB</el-tag>
          </div>
        </template>
      </el-table-column>
      <el-table-column prop="category" :label="t('system.fieldName')" width="120" />
      <el-table-column prop="zhValue" :label="'中文 (zh)'" min-width="160">
        <template #default="{ row }">
          <span class="truncate">{{ row.zhValue || '-' }}</span>
        </template>
      </el-table-column>
      <el-table-column prop="enValue" :label="'English (en)'" min-width="160">
        <template #default="{ row }">
          <span class="truncate">{{ row.enValue || '-' }}</span>
        </template>
      </el-table-column>
      <el-table-column prop="isActive" :label="t('common.status')" width="90" align="center">
        <template #default="{ row }">
          <el-tag :type="row.isActive ? 'success' : 'info'" size="small">
            {{ row.isActive ? t('common.active') : t('common.inactive') }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column :label="t('common.action')" width="140" align="center" fixed="right">
        <template #default="{ row }">
          <template v-if="row.source === 'db'">
            <el-button link type="primary" size="small" :icon="Edit" @click="handleEdit(row)">
              {{ t('common.edit') }}
            </el-button>
            <el-button link type="danger" size="small" :icon="Delete" @click="handleDelete(row)">
              {{ t('common.delete') }}
            </el-button>
          </template>
          <template v-else>
            <el-button link type="warning" size="small" :icon="Download" @click="handleImport(row)">
              导入DB
            </el-button>
          </template>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog
      :title="isEdit ? t('common.edit') : t('common.create')"
      v-model="dialogVisible"
      width="520px"
      :close-on-click-modal="false"
    >
      <el-form :model="form" label-width="100px" ref="formRef" :rules="rules">
        <el-form-item :label="t('system.fieldCode')" prop="messageKey">
          <el-input v-model="form.messageKey" placeholder="e.g. common.required" />
        </el-form-item>
        <el-form-item :label="t('system.fieldName')" prop="category">
          <el-select v-model="form.category" style="width: 100%" filterable allow-create>
            <el-option v-for="c in categories" :key="c" :label="c" :value="c" />
          </el-select>
        </el-form-item>
        <el-form-item label="中文 (zh)">
          <el-input v-model="form.zhValue" type="textarea" :rows="2" placeholder="中文翻译" />
        </el-form-item>
        <el-form-item label="English (en)">
          <el-input v-model="form.enValue" type="textarea" :rows="2" placeholder="English translation" />
        </el-form-item>
        <el-form-item :label="t('common.status')">
          <el-switch v-model="form.isActive" />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogVisible = false">{{ t('common.cancel') }}</el-button>
          <el-button type="primary" :loading="submitting" @click="handleSubmit">
            {{ isEdit ? t('common.save') : t('common.create') }}
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useI18n } from '@/composables/useI18n'
import { getI18nMessages, createI18nMessage, updateI18nMessage, deleteI18nMessage, type I18nMessage } from '@/api/i18nMessage'
import { Plus, Edit, Delete, Search, RefreshRight, Download } from '@element-plus/icons-vue'
import zhMessages from '@/locales/zh'
import enMessages from '@/locales/en'

const { t } = useI18n()
const loading = ref(false)
const dbData = ref<I18nMessage[]>([])
const dialogVisible = ref(false)
const submitting = ref(false)
const editId = ref<number | null>(null)

const isEdit = computed(() => !!editId.value)

const categories = [
  'common', 'login', 'uiControl', 'layout', 'nav',
  'customer', 'product', 'opportunity', 'quotation',
  'system', 'fieldControl', 'approval', 'validation',
  'mfg', 'pmTrace', 'siteSetup', 'sequenceRule',
  'emailTemplate', 'alertRecipient', 'mail', 'profile', 'message'
]

const query = ref({
  category: '',
  keyword: ''
})

const form = ref({
  messageKey: '',
  category: '',
  zhValue: '',
  enValue: '',
  isActive: true
})

const rules = {
  messageKey: [{ required: true, message: t('validation.required', { field: t('system.fieldCode') }), trigger: 'blur' }],
  category: [{ required: true, message: t('validation.required', { field: t('system.fieldName') }), trigger: 'blur' }]
}

// Flatten nested translation object into dot-notation map
function flattenMessages(obj: any, prefix = ''): Record<string, string> {
  const result: Record<string, string> = {}
  for (const [key, value] of Object.entries(obj)) {
    const fullKey = prefix ? `${prefix}.${key}` : key
    if (typeof value === 'string') {
      result[fullKey] = value
    } else if (typeof value === 'object' && value !== null) {
      Object.assign(result, flattenMessages(value, fullKey))
    }
  }
  return result
}

const zhFlat = flattenMessages(zhMessages)
const enFlat = flattenMessages(enMessages)

interface MergedMessage {
  id: number
  messageKey: string
  category: string
  zhValue?: string
  enValue?: string
  isActive: boolean
  source: 'static' | 'db'
}

const mergedData = computed<MergedMessage[]>(() => {
  const allKeys = new Set([
    ...Object.keys(zhFlat),
    ...Object.keys(enFlat),
    ...dbData.value.map(d => d.messageKey)
  ])

  const dbKeyMap = new Map(dbData.value.map(d => [d.messageKey, d]))

  const list: MergedMessage[] = []
  for (const key of allKeys) {
    const category = key.split('.')[0] || ''
    const dbItem = dbKeyMap.get(key)
    if (dbItem) {
      list.push({
        id: dbItem.id,
        messageKey: key,
        category: dbItem.category || category,
        zhValue: dbItem.zhValue || undefined,
        enValue: dbItem.enValue || undefined,
        isActive: dbItem.isActive,
        source: 'db'
      })
    } else {
      list.push({
        id: -1,
        messageKey: key,
        category,
        zhValue: zhFlat[key] || undefined,
        enValue: enFlat[key] || undefined,
        isActive: true,
        source: 'static'
      })
    }
  }

  return list.sort((a, b) => a.messageKey.localeCompare(b.messageKey))
})

const filteredData = computed(() => {
  let list = mergedData.value
  if (query.value.category) {
    list = list.filter(m => m.category === query.value.category)
  }
  if (query.value.keyword) {
    const kw = query.value.keyword.toLowerCase()
    list = list.filter(m =>
      m.messageKey.toLowerCase().includes(kw) ||
      (m.zhValue && m.zhValue.toLowerCase().includes(kw)) ||
      (m.enValue && m.enValue.toLowerCase().includes(kw))
    )
  }
  return list
})

const loadDbData = async () => {
  loading.value = true
  try {
    const res = await getI18nMessages(
      query.value.category || undefined,
      query.value.keyword || undefined
    )
    dbData.value = res.data || []
  } catch (e) {
    console.error(e)
    ElMessage.error(t('common.failed'))
  } finally {
    loading.value = false
  }
}

const handleSearch = () => { loadDbData() }
const handleReset = () => {
  query.value = { category: '', keyword: '' }
  loadDbData()
}

const handleAdd = () => {
  editId.value = null
  form.value = {
    messageKey: '',
    category: '',
    zhValue: '',
    enValue: '',
    isActive: true
  }
  dialogVisible.value = true
}

const handleEdit = (row: MergedMessage) => {
  editId.value = row.id
  form.value = {
    messageKey: row.messageKey,
    category: row.category,
    zhValue: row.zhValue || '',
    enValue: row.enValue || '',
    isActive: row.isActive
  }
  dialogVisible.value = true
}

const handleDelete = async (row: MergedMessage) => {
  try {
    await ElMessageBox.confirm(t('message.deleteConfirm'), t('common.tip'), { type: 'warning' })
    await deleteI18nMessage(row.id)
    ElMessage.success(t('message.deleteSuccess'))
    loadDbData()
  } catch (e: any) {
    if (e !== 'cancel') {
      console.error(e)
      ElMessage.error(t('common.failed'))
    }
  }
}

const handleImport = async (row: MergedMessage) => {
  try {
    await createI18nMessage({
      messageKey: row.messageKey,
      category: row.category,
      zhValue: row.zhValue || null,
      enValue: row.enValue || null,
      isActive: true
    })
    ElMessage.success(`'${row.messageKey}' 已导入数据库`)
    loadDbData()
  } catch (e) {
    console.error(e)
    ElMessage.error(t('common.failed'))
  }
}

const handleSubmit = async () => {
  if (!form.value.messageKey.trim()) {
    ElMessage.warning(t('validation.required', { field: t('system.fieldCode') }))
    return
  }
  if (!form.value.category.trim()) {
    ElMessage.warning(t('validation.required', { field: t('system.fieldName') }))
    return
  }

  submitting.value = true
  try {
    const payload = {
      messageKey: form.value.messageKey.trim(),
      category: form.value.category.trim(),
      zhValue: form.value.zhValue.trim() || null,
      enValue: form.value.enValue.trim() || null,
      isActive: form.value.isActive
    }
    if (isEdit.value && editId.value) {
      await updateI18nMessage(editId.value, payload)
      ElMessage.success(t('message.updateSuccess'))
    } else {
      await createI18nMessage(payload)
      ElMessage.success(t('message.createSuccess'))
    }
    dialogVisible.value = false
    loadDbData()
  } catch (e) {
    console.error(e)
    ElMessage.error(t('common.failed'))
  } finally {
    submitting.value = false
  }
}

onMounted(loadDbData)
</script>

<style scoped>
.toolbar {
  display: flex;
  align-items: center;
  margin-bottom: 16px;
}
.truncate {
  display: inline-block;
  max-width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
.key-cell {
  display: flex;
  align-items: center;
  gap: 8px;
}
</style>
