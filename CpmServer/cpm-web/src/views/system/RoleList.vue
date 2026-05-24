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
      <el-table border :data="tableData" v-loading="loading" stripe style="width: 100%">
        <el-table-column type="index" label="#" width="50" align="center" />
        <el-table-column v-if="isVisible('roleCode')" prop="roleCode" :label="t('approval.templateCode')" width="140">
          <template #default="{ row }">
            <span class="code-link">{{ row.roleCode }}</span>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('roleName')" prop="roleName" :label="t('approval.templateName')" min-width="160" />
        <el-table-column v-if="isVisible('site')" prop="site" :label="t('common.site')" min-width="100">
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
      width="640px"
      :close-on-click-modal="false"
      class="slds-dialog"
      @closed="handleClose"
    >
      <div class="dialog-body">
        <el-form :model="form" label-width="100px" :rules="rules" ref="formRef" class="slds-form">
          <el-form-item v-if="isVisible('roleCode')" :label="t('common.code')" prop="roleCode">
            <el-input v-model="form.roleCode" :placeholder="t('common.pleaseInput')" :disabled="isEdit" />
          </el-form-item>

          <el-form-item v-if="isVisible('roleName')" :label="t('common.name')" prop="roleName">
            <el-input v-model="form.roleName" :placeholder="t('common.pleaseInput')" />
          </el-form-item>

          <el-form-item v-if="isVisible('site')" :label="t('common.site')">
            <el-input v-model="form.site" :placeholder="t('common.pleaseInput')" />
          </el-form-item>
        </el-form>

        <div v-if="isEdit" v-loading="permLoading" class="permission-section">
          <div class="permission-header">{{ t('permission.title') }}</div>
          <el-checkbox-group v-model="selectedPermissions" class="permission-group">
            <div v-for="(perms, module) in permissionGroups" :key="module" class="permission-module">
              <div class="permission-module-title">{{ getModuleLabel(module) }}</div>
              <div class="permission-items">
                <el-checkbox v-for="perm in perms" :key="perm" :label="perm" :value="perm" border size="small">
                  {{ getPermissionLabel(perm) }}
                </el-checkbox>
              </div>
            </div>
          </el-checkbox-group>
        </div>
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
import { useFieldControl } from '@/composables/useFieldControl'
import { Plus, Edit, Delete } from '@element-plus/icons-vue'

const props = defineProps<{ embedded?: boolean }>()
const { t } = useI18n()
const { isVisible, load: loadFieldConfig } = useFieldControl('System', 'RoleList')

const loading = ref(false)
const dialogVisible = ref(false)
const submitting = ref(false)
const tableData = ref<any[]>([])
const editId = ref<number | null>(null)
const formRef = ref()
const allPermissions = ref<string[]>([])
const selectedPermissions = ref<string[]>([])
const permLoading = ref(false)

const isEdit = computed(() => !!editId.value)

const form = ref({
  roleCode: '',
  roleName: '',
  site: ''
})

const permissionGroups = computed(() => {
  const groups: Record<string, string[]> = {}
  allPermissions.value.forEach(p => {
    const module = p.split('.')[0]
    if (!groups[module]) groups[module] = []
    groups[module].push(p)
  })
  return groups
})

const getModuleLabel = (module: string) => {
  const map: Record<string, string> = {
    system: t('permission.system_manage'),
    users: t('permission.users_manage'),
    customers: t('permission.customers_manage'),
    products: t('permission.products_manage'),
    mfg: t('permission.mfg_manage'),
    quotations: t('permission.quotations_manage'),
    pm: t('permission.pm_manage'),
    approval: t('permission.approval_templates_manage'),
    gc: t('permission.gc_manage'),
    nav: t('permission.nav_manage'),
    fieldcontrol: t('permission.fieldcontrol_manage'),
    i18n: t('permission.i18n_manage'),
    email: t('permission.email_templates_manage'),
    alerts: t('permission.alerts_manage')
  }
  return map[module] || module.toUpperCase()
}

const getPermissionLabel = (code: string) => {
  const key = code.replace(/\./g, '_')
  const translated = t(`permission.${key}`)
  return translated !== `permission.${key}` ? translated : code
}

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

const handleEdit = async (row: any) => {
  editId.value = row.id
  form.value = {
    roleCode: row.roleCode,
    roleName: row.roleName,
    site: row.site || ''
  }
  selectedPermissions.value = []
  permLoading.value = true
  try {
    const [allRes, roleRes] = await Promise.all([
      roleApi.getAllPermissions(),
      roleApi.getById(row.id)
    ])
    allPermissions.value = allRes.data || []
    selectedPermissions.value = roleRes.data?.permissions || []
  } finally {
    permLoading.value = false
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
      await roleApi.assignPermissions(editId.value, selectedPermissions.value)
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

onMounted(async () => {
  await Promise.all([loadData(), loadFieldConfig()])
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

.permission-section {
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid var(--slds-border-color-light);
}

.permission-header {
  font-size: 14px;
  font-weight: 700;
  color: var(--slds-text-primary);
  margin-bottom: 12px;
}

.permission-module {
  margin-bottom: 12px;
}

.permission-module-title {
  font-size: 12px;
  font-weight: 600;
  color: var(--slds-brand-primary);
  margin-bottom: 8px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.permission-items {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.permission-items :deep(.el-checkbox) {
  margin-right: 0;
}
</style>
