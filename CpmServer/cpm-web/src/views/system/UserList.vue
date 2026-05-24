<template>
  <div class="page-container">
    <div class="page-header-section" v-if="!embedded">
      <el-page-header @back="$router.push('/home')">
        <template #content>
          <div class="page-header-content">
            <h1 class="page-title">{{ t('system.userListTitle') }}</h1>
            <span class="page-subtitle">{{ t('system.userListSubtitle') }}</span>
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

    <div class="stats-row">
      <div class="stat-card">
        <div class="stat-value">{{ total }}</div>
        <div class="stat-label">{{ t('system.totalUsers') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value" style="color: var(--slds-success);">{{ activeCount }}</div>
        <div class="stat-label">{{ t('system.activeUsers') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value" style="color: var(--slds-text-secondary);">{{ inactiveCount }}</div>
        <div class="stat-label">{{ t('system.inactiveUsers') }}</div>
      </div>
    </div>

    <div class="search-area">
      <el-form :model="query" inline class="search-form">
        <el-form-item>
          <el-input
            v-model="query.keyword"
            :placeholder="t('common.pleaseInput')"
            clearable
            style="width: 280px"
            :prefix-icon="Search"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :icon="Search" @click="handleSearch">{{ t('common.search') }}</el-button>
          <el-button :icon="RefreshRight" @click="handleReset">{{ t('common.reset') }}</el-button>
        </el-form-item>
      </el-form>
    </div>

    <div class="content-card" style="padding: 0;">
      <el-table border :data="tableData" v-loading="loading" stripe style="width: 100%">
        <el-table-column type="index" label="#" width="50" align="center" />
        <el-table-column :label="t('common.user')" min-width="180">
          <template #default="{ row }">
            <div class="user-info-cell">
              <el-avatar :size="32" :icon="UserFilled" class="user-avatar-small" />
              <div class="user-meta">
                <div class="user-real-name">{{ row.realName || row.username }}</div>
                <div class="user-username">{{ row.username }}</div>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('email')" prop="email" :label="t('common.email')" width="180">
          <template #default="{ row }">
            <span class="text-secondary">{{ row.email || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('phone')" prop="phone" :label="t('common.phone')" width="130">
          <template #default="{ row }">
            <span class="text-secondary">{{ row.phone || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('site')" prop="site" :label="t('common.site')" min-width="100">
          <template #default="{ row }">
            <span class="text-secondary">{{ row.site || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('role')" :label="t('common.role')" min-width="150">
          <template #default="{ row }">
            <div class="role-tags">
              <el-tag v-for="role in row.roles" :key="role" size="small" class="role-tag">
                {{ role }}
              </el-tag>
              <span v-if="!row.roles?.length" class="text-secondary">{{ t('common.noData') }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('isActive')" prop="isActive" :label="t('common.status')" min-width="80" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'info'" size="small">
              {{ row.isActive ? t('common.active') : t('common.inactive') }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column :label="t('common.action')" min-width="220" align="right" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" size="small" :icon="Edit" @click="handleEdit(row)">
              {{ t('common.edit') }}
            </el-button>
            <el-button link type="warning" size="small" :icon="Key" @click="handleResetPassword(row)">
              {{ t('system.resetPassword') }}
            </el-button>
            <el-button link type="danger" size="small" :icon="Delete" @click="handleDelete(row)">
              {{ t('common.delete') }}
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="table-footer">
        <el-pagination
          v-model:current-page="query.pageNum"
          v-model:page-size="query.pageSize"
          :total="total"
          layout="prev, pager, next, sizes"
          :page-sizes="[10, 20, 50]"
          @change="loadData"
        />
        <span class="table-info">{{ t('common.total') }} {{ total }}</span>
      </div>
    </div>

    <UserForm
      v-model:visible="dialogVisible"
      :data="editData"
      @success="loadData"
    />

    <!-- Reset Password Dialog -->
    <el-dialog
      :title="t('system.resetPassword')"
      v-model="pwdDialogVisible"
      width="400px"
      class="slds-dialog"
    >
      <div class="dialog-body">
        <el-form :model="pwdForm" label-width="100px" ref="pwdFormRef" :rules="pwdRules" class="slds-form">
          <el-form-item :label="t('system.newPassword')" prop="newPassword">
            <el-input v-model="pwdForm.newPassword" type="password" show-password :placeholder="t('common.pleaseInput')" />
          </el-form-item>
          <el-form-item :label="t('system.confirmPassword')" prop="confirmPassword">
            <el-input v-model="pwdForm.confirmPassword" type="password" show-password :placeholder="t('common.pleaseInput')" />
          </el-form-item>
        </el-form>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="pwdDialogVisible = false">{{ t('common.cancel') }}</el-button>
          <el-button type="primary" :loading="pwdLoading" @click="confirmResetPassword">{{ t('common.confirm') }}</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { userApi } from '@/api/user'
import UserForm from './UserForm.vue'
import { useI18n } from '@/composables/useI18n'
import { useFieldControl } from '@/composables/useFieldControl'
import {
  Plus,
  Search,
  RefreshRight,
  Edit,
  Delete,
  UserFilled,
  Key
} from '@element-plus/icons-vue'

const props = defineProps<{ embedded?: boolean }>()
const { t } = useI18n()
const { isVisible } = useFieldControl('System', 'UserList')

const loading = ref(false)
const dialogVisible = ref(false)
const editData = ref<any>(null)
const tableData = ref<any[]>([])
const total = ref(0)
const query = ref({ pageNum: 1, pageSize: 10, keyword: '' })

const activeCount = computed(() => tableData.value.filter((u: any) => u.isActive).length)
const inactiveCount = computed(() => tableData.value.filter((u: any) => !u.isActive).length)

const loadData = async () => {
  loading.value = true
  try {
    const res = await userApi.list(query.value)
    tableData.value = res.data.list
    total.value = res.data.total
  } finally {
    loading.value = false
  }
}

const handleSearch = () => { query.value.pageNum = 1; loadData() }
const handleReset = () => { query.value = { pageNum: 1, pageSize: 10, keyword: '' }; loadData() }
const handleAdd = () => { editData.value = null; dialogVisible.value = true }
const handleEdit = (row: any) => { editData.value = row; dialogVisible.value = true }

const handleDelete = async (row: any) => {
  await ElMessageBox.confirm(t('message.deleteConfirm'), t('common.tip'), { type: 'warning' })
  await userApi.delete(row.id)
  ElMessage.success(t('message.deleteSuccess'))
  loadData()
}

// Reset password
const pwdDialogVisible = ref(false)
const pwdLoading = ref(false)
const pwdFormRef = ref()
const currentUserId = ref<number | null>(null)
const pwdForm = ref({ newPassword: '', confirmPassword: '' })
const pwdRules = {
  newPassword: [
    { required: true, message: t('validation.required', { field: t('system.newPassword') }), trigger: 'blur' },
    { min: 6, message: t('validation.minLength', { field: t('system.newPassword'), min: 6 }), trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, message: t('validation.required', { field: t('system.confirmPassword') }), trigger: 'blur' },
    {
      validator: (_rule: any, value: string, callback: Function) => {
        if (value !== pwdForm.value.newPassword) {
          callback(new Error(t('system.passwordMismatch')))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ]
}

const handleResetPassword = (row: any) => {
  currentUserId.value = row.id
  pwdForm.value = { newPassword: '', confirmPassword: '' }
  pwdDialogVisible.value = true
}

const confirmResetPassword = async () => {
  await pwdFormRef.value.validate()
  if (!currentUserId.value) return
  pwdLoading.value = true
  try {
    await userApi.resetPassword(currentUserId.value, pwdForm.value.newPassword)
    ElMessage.success(t('message.updateSuccess'))
    pwdDialogVisible.value = false
  } finally {
    pwdLoading.value = false
  }
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

.stats-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--slds-spacing-md);
  margin-bottom: var(--slds-spacing-md);
}

.stat-card {
  background: var(--slds-bg-card);
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
  box-shadow: var(--slds-shadow-card);
  padding: var(--slds-spacing-lg);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.stat-value {
  font-size: 32px;
  font-weight: 700;
  color: var(--slds-brand-primary);
  line-height: 1;
}

.stat-label {
  font-size: var(--slds-font-size-sm);
  color: var(--slds-text-secondary);
  margin-top: var(--slds-spacing-sm);
  letter-spacing: 0.3px;
}

.search-area {
  background: var(--slds-bg-card);
  padding: var(--slds-spacing-md) var(--slds-spacing-lg);
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
  margin-bottom: var(--slds-spacing-md);
}

.search-form :deep(.el-form-item) {
  margin-bottom: 0;
  margin-right: var(--slds-spacing-md);
}

.search-form :deep(.el-form-item__label) {
  font-size: var(--slds-font-size-sm);
  font-weight: 500;
  color: var(--slds-text-secondary);
}

.content-card {
  background: var(--slds-bg-card);
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
  box-shadow: var(--slds-shadow-card);
  overflow: hidden;
}

.user-info-cell {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
}

.user-avatar-small {
  background: var(--slds-brand-primary-light);
  color: var(--slds-brand-primary-dark);
}

.user-meta {
  display: flex;
  flex-direction: column;
}

.user-real-name {
  font-weight: 600;
  color: var(--slds-text-primary);
  font-size: 14px;
}

.user-username {
  font-size: 12px;
  color: var(--slds-text-secondary);
}

.role-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.role-tag {
  margin: 0;
}

.text-secondary {
  color: var(--slds-text-secondary);
  font-size: 13px;
}

.table-footer {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: var(--slds-spacing-lg);
  padding: var(--slds-spacing-md) var(--slds-spacing-lg);
  border-top: 1px solid var(--slds-border-color-light);
}

.table-info {
  font-size: var(--slds-font-size-sm);
  color: var(--slds-text-secondary);
}

.dialog-body {
  padding: 24px 8px;
}

.embedded-header {
  display: flex;
  justify-content: flex-end;
  margin-bottom: var(--slds-spacing-md);
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
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
</style>
