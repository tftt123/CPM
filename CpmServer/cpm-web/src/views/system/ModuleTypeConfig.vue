<template>
  <div class="module-type-config">
    <div class="toolbar">
      <el-button type="primary" :icon="Plus" @click="handleAdd">{{ t('common.create') }}</el-button>
    </div>

    <el-table border :data="tableData" v-loading="loading" stripe>
      <el-table-column type="index" label="#" width="50" align="center" />
      <el-table-column prop="moduleType" :label="t('system.moduleType')" width="160" />
      <el-table-column prop="moduleName" :label="t('system.moduleName')" width="160" />
      <el-table-column prop="description" :label="t('common.detail')" min-width="200">
        <template #default="{ row }">
          {{ row.description || '-' }}
        </template>
      </el-table-column>
      <el-table-column prop="isActive" :label="t('common.status')" min-width="80" align="center">
        <template #default="{ row }">
          <el-switch v-model="row.isActive" disabled size="small" />
        </template>
      </el-table-column>
      <el-table-column :label="t('common.action')" min-width="150" align="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" :icon="Edit" @click="handleEdit(row)">
            {{ t('common.edit') }}
          </el-button>
          <el-button link type="danger" size="small" :icon="Delete" @click="handleDelete(row)">
            {{ t('common.delete') }}
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog
      :title="isEdit ? t('common.edit') : t('common.create')"
      v-model="dialogVisible"
      width="500px"
      :close-on-click-modal="false"
    >
      <el-form :model="form" label-width="100px" :rules="rules" ref="formRef">
        <el-form-item :label="t('system.moduleType')" prop="moduleType">
          <el-select v-model="form.moduleType" :placeholder="t('common.pleaseSelect')" style="width: 100%" :disabled="isEdit">
            <el-option v-for="type in availableTypes" :key="type" :label="type" :value="type" />
          </el-select>
        </el-form-item>
        <el-form-item :label="t('system.moduleName')">
          <el-input v-model="form.moduleName" :placeholder="t('common.pleaseInput')" />
        </el-form-item>
        <el-form-item :label="t('common.detail')">
          <el-input v-model="form.description" type="textarea" :rows="2" :placeholder="t('common.pleaseInput')" />
        </el-form-item>
        <el-form-item :label="t('common.status')">
          <el-switch v-model="form.isActive" />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogVisible = false">{{ t('common.cancel') }}</el-button>
          <el-button type="primary" :loading="submitting" @click="handleSubmit">{{ t('common.save') }}</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Edit, Delete } from '@element-plus/icons-vue'
import { useI18n } from '@/composables/useI18n'
import {
  getModuleTypeList,
  getSystemModuleTypes,
  createModuleType,
  updateModuleType,
  deleteModuleType,
} from '@/api/moduleTypeConfig'
import type { ModuleTypeConfig } from '@/api/moduleTypeConfig'

const { t } = useI18n()

const loading = ref(false)
const dialogVisible = ref(false)
const submitting = ref(false)
const tableData = ref<ModuleTypeConfig[]>([])
const systemTypeList = ref<string[]>([])
const editId = ref<number | null>(null)
const formRef = ref()

const isEdit = computed(() => !!editId.value)

const availableTypes = computed(() => {
  const configured = new Set(tableData.value.map((x) => x.moduleType))
  return systemTypeList.value.filter((type) => {
    if (isEdit.value && form.value.moduleType === type) return true
    return !configured.has(type)
  })
})

const form = ref<ModuleTypeConfig>({
  id: 0,
  moduleType: '',
  moduleName: '',
  description: '',
  isActive: true,
})

const rules = {
  moduleType: [{ required: true, message: t('validation.required', { field: t('system.moduleType') }), trigger: 'blur' }],
}

const loadSystemTypes = async () => {
  const res = await getSystemModuleTypes()
  systemTypeList.value = res.data
}

const loadData = async () => {
  loading.value = true
  try {
    const res = await getModuleTypeList(true)
    tableData.value = res.data
  } finally {
    loading.value = false
  }
}

const handleAdd = () => {
  editId.value = null
  form.value = {
    id: 0,
    moduleType: '',
    moduleName: '',
    description: '',
    isActive: true,
  }
  dialogVisible.value = true
}

const handleEdit = (row: ModuleTypeConfig) => {
  editId.value = row.id
  form.value = { ...row }
  dialogVisible.value = true
}

const handleDelete = async (row: ModuleTypeConfig) => {
  await ElMessageBox.confirm(t('message.deleteConfirm'), t('common.tip'), { type: 'warning' })
  await deleteModuleType(row.id)
  ElMessage.success(t('message.deleteSuccess'))
  loadData()
}

const handleSubmit = async () => {
  await formRef.value.validate()
  submitting.value = true
  try {
    if (isEdit.value && editId.value) {
      await updateModuleType(editId.value, form.value)
      ElMessage.success(t('message.updateSuccess'))
    } else {
      await createModuleType(form.value)
      ElMessage.success(t('message.createSuccess'))
    }
    dialogVisible.value = false
    loadData()
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  loadData()
  loadSystemTypes()
})
</script>

<style scoped>
.module-type-config {
  padding: 16px 0;
}
.toolbar {
  margin-bottom: 16px;
}
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>
