<template>
  <div class="page-container">
    <div v-if="!embedded" class="page-header-section">
      <el-page-header @back="$router.push('/home')">
        <template #content>
          <div class="page-header-content">
            <h1 class="page-title">{{ t('system.roleListTitle') }}</h1>
            <span class="page-subtitle">{{ t('system.roleListSubtitle') }}</span>
          </div>
        </template>
        <template #extra>
          <el-button type="primary" :icon="Plus" @click="handleAdd">
            {{ t('common.add') }}
          </el-button>
        </template>
      </el-page-header>
    </div>
    <div v-else class="embedded-header">
      <el-button type="primary" :icon="Plus" @click="handleAdd">
        {{ t('common.add') }}
      </el-button>
    </div>

    <div class="content-card" style="padding: 0;">
      <el-table :data="tableData" v-loading="loading" stripe style="width: 100%">
        <el-table-column type="index" label="#" width="50" align="center" />
        <el-table-column prop="roleCode" :label="t('approval.templateCode')" width="140">
          <template #default="{ row }">
            <span class="code-link">{{ row.roleCode }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="roleName" :label="t('approval.templateName')" min-width="160" />
        <el-table-column prop="site" :label="t('common.site')" min-width="100">
          <template #default="{ row }">
            <span class="text-secondary">{{ row.site || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column :label="t('common.action')" min-width="150" align="right" fixed="right">
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
    </div>

    <!-- Role Form Dialog -->
    <el-dialog
      :title="isEdit ? t('common.edit') : t('common.create')"
      v-model="dialogVisible"
      width="520px"
      :close-on-click-modal="false"
      class="slds-dialog"
      @closed="handleClose"
    >
      <div class="dialog-body">
        <el-form :model="form" label-width="100px" :rules="rules" ref="formRef" class="slds-form">
          <el-form-item :label="t('common.code')" prop="roleCode">
            <el-input v-model="form.roleCode" :placeholder="t('common.pleaseInput')" :disabled="isEdit" />
          </el-form-item>

          <el-form-item :label="t('common.name')" prop="roleName">
            <el-input v-model="form.roleName" :placeholder="t('common.pleaseInput')" />
          </el-form-item>

          <el-form-item :label="t('common.site')">
            <el-input v-model="form.site" :placeholder="t('common.pleaseInput')" />
          </el-form-item>
        </el-form>
      </div>
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
import { roleApi } from '@/api/role'
import { useI18n } from '@/composables/useI18n'
import { Plus, Edit, Delete } from '@element-plus/icons-vue'

const props = defineProps<{ embedded?: boolean }>()
const { t } = useI18n()

const loading = ref(false)
const dialogVisible = ref(false)
const submitting = ref(false)
const tableData = ref<any[]>([])
const editId = ref<number | null>(null)
const formRef = ref()

const isEdit = computed(() => !!editId.value)

const form = ref({
  roleCode: '',
  roleName: '',
  site: ''
})

const rules = {
  roleCode: [{ required: true, message: t('validation.required', { field: t('common.code') }), trigger: 'blur' }],
  roleName: [{ required: true, message: t('validation.required', { field: t('common.name') }), trigger: 'blur' }]
}

const loadData = async () => {
  loading.value = true
  try {
    const res = await roleApi.list()
    tableData.value = res.data || []
  } finally {
    loading.value = false
  }
}

const handleAdd = () => {
  editId.value = null
  form.value = { roleCode: '', roleName: '', site: '' }
  dialogVisible.value = true
}

const handleEdit = (row: any) => {
  editId.value = row.id
  form.value = {
    roleCode: row.roleCode,
    roleName: row.roleName,
    site: row.site || ''
  }
  dialogVisible.value = true
}

const handleDelete = async (row: any) => {
  await ElMessageBox.confirm(t('message.deleteConfirm'), t('common.tip'), { type: 'warning' })
  await roleApi.delete(row.id)
  ElMessage.success(t('message.deleteSuccess'))
  loadData()
}

const handleSubmit = async () => {
  await formRef.value.validate()
  submitting.value = true
  try {
    if (isEdit.value && editId.value) {
      await roleApi.update(editId.value, { roleName: form.value.roleName, site: form.value.site })
      ElMessage.success(t('message.updateSuccess'))
    } else {
      await roleApi.create(form.value)
      ElMessage.success(t('message.createSuccess'))
    }
    dialogVisible.value = false
    loadData()
  } finally {
    submitting.value = false
  }
}

const handleClose = () => {
  formRef.value?.resetFields()
  editId.value = null
}

onMounted(loadData)
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

.embedded-header {
  display: flex;
  justify-content: flex-end;
  margin-bottom: var(--slds-spacing-md);
}

.content-card {
  background: var(--slds-bg-card);
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
  box-shadow: var(--slds-shadow-card);
  overflow: hidden;
}

.code-link {
  color: var(--slds-brand-primary);
  font-weight: 600;
  font-family: 'SF Mono', Monaco, monospace;
  font-size: 13px;
}

.text-secondary {
  color: var(--slds-text-secondary);
  font-size: 13px;
}

.dialog-body {
  padding: 24px 8px;
}

:deep(.slds-dialog .el-dialog__header) {
  border-bottom: 1px solid var(--slds-border-color-light);
  padding: 16px 24px;
  margin-right: 0;
}

:deep(.slds-dialog .el-dialog__title) {
  font-size: 18px;
  font-weight: 700;
  color: var(--slds-text-primary);
}

.slds-form :deep(.el-form-item__label) {
  font-size: 13px;
  font-weight: 600;
  color: var(--slds-text-secondary);
}

.slds-form :deep(.el-input__wrapper) {
  height: 32px !important;
  min-height: 32px !important;
  padding: 1px 11px !important;
}

.slds-form :deep(.el-input__inner) {
  height: 30px !important;
  line-height: 30px !important;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>
