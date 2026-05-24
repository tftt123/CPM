<template>
  <div class="field-control-config">
    <div class="toolbar">
      <el-select v-model="selectedModule" :placeholder="t('common.pleaseSelect')" style="width: 160px" @change="onModuleChange">
        <el-option v-for="m in modules" :key="m" :label="m" :value="m" />
      </el-select>
      <el-select v-model="selectedPage" :placeholder="t('common.pleaseSelect')" style="width: 180px; margin-left: 12px;" @change="onPageChange" :disabled="!selectedModule">
        <el-option v-for="p in pages" :key="p" :label="p" :value="p" />
      </el-select>
      <el-button type="primary" :loading="saving" :disabled="!selectedPage" @click="handleSave" style="margin-left: 12px;">
        {{ t('common.save') }}
      </el-button>
    </div>

    <el-table v-if="fields.length > 0" border :data="fields" v-loading="loading" stripe>
      <el-table-column type="index" label="#" width="50" align="center" />
      <el-table-column prop="fieldCode" :label="t('system.fieldCode')" width="140" />
      <el-table-column :label="t('system.fieldName')" min-width="160">
        <template #default="{ row }">
          {{ getFieldLabel(row.fieldCode) }}
        </template>
      </el-table-column>
      <el-table-column :label="t('common.status')" width="100" align="center">
        <template #default="{ row }">
          <el-switch v-model="row.isVisible" />
        </template>
      </el-table-column>
      <el-table-column :label="t('common.required')" width="100" align="center">
        <template #default="{ row }">
          <el-switch v-model="row.isRequired" />
        </template>
      </el-table-column>
    </el-table>

    <el-empty v-else :description="t('common.noData')" />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useI18n } from '@/composables/useI18n'
import { getModules, getPages, getFieldControls, updateFieldControls, type FieldControl } from '@/api/fieldControl'
import { getFieldTemplateList } from '@/config/field-registry'

const { t } = useI18n()
const loading = ref(false)
const saving = ref(false)
const modules = ref<string[]>([])
const pages = ref<string[]>([])
const selectedModule = ref('')
const selectedPage = ref('')
const fields = ref<FieldControl[]>([])

const loadModules = async () => {
  const res = await getModules()
  modules.value = res.data || []
  if (modules.value.length > 0) {
    selectedModule.value = modules.value[0]
    await loadPages()
  }
}

const loadPages = async () => {
  if (!selectedModule.value) return
  const res = await getPages(selectedModule.value)
  pages.value = res.data || []
  if (pages.value.length > 0) {
    selectedPage.value = pages.value[0]
    await loadFields()
  } else {
    selectedPage.value = ''
    fields.value = []
  }
}

const loadFields = async () => {
  if (!selectedModule.value || !selectedPage.value) return
  loading.value = true
  try {
    const res = await getFieldControls(selectedModule.value, selectedPage.value)
    fields.value = res.data || []
  } finally {
    loading.value = false
  }
}

const onModuleChange = async () => {
  selectedPage.value = ''
  fields.value = []
  await loadPages()
}

const onPageChange = async () => {
  await loadFields()
}

const getFieldLabel = (fieldCode: string) => {
  const templates = getFieldTemplateList(selectedModule.value, selectedPage.value)
  const template = templates.find(f => f.code === fieldCode)
  if (template) {
    return t(template.i18nKey)
  }
  return fieldCode
}

const handleSave = async () => {
  saving.value = true
  try {
    await updateFieldControls(fields.value)
    ElMessage.success(t('message.saveSuccess'))
  } catch {
    ElMessage.error(t('common.failed'))
  } finally {
    saving.value = false
  }
}

onMounted(loadModules)
</script>

<style scoped>
.field-control-config {
  padding: 0;
}
.toolbar {
  display: flex;
  align-items: center;
  margin-bottom: 16px;
}
</style>
