<template>
  <div class="page-container">
    <div class="page-header-section">
      <el-page-header @back="$router.push('/home')">
        <template #content>
          <div class="page-header-content">
            <h1 class="page-title">{{ t('quotation.pageTitle') }}</h1>
            <span class="page-subtitle">{{ t('quotation.quotationDetail') }}</span>
          </div>
        </template>
        <template #extra>
          <el-button v-if="userStore.hasPermission('quotations.manage')" type="primary" :icon="Plus" @click="handleAdd">
            {{ t('common.create') }}
          </el-button>
        </template>
      </el-page-header>
    </div>

    <div class="stats-row">
      <div class="stat-card">
        <div class="stat-value">{{ total }}</div>
        <div class="stat-label">{{ t('common.total') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value" style="color: var(--slds-text-secondary);">{{ statusCount[0] || 0 }}</div>
        <div class="stat-label">{{ t('quotation.draft') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value" style="color: var(--slds-warning);">{{ (statusCount[1] || 0) + (statusCount[2] || 0) }}</div>
        <div class="stat-label">{{ t('quotation.pendingReview') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value" style="color: var(--slds-success);">{{ statusCount[3] || 0 }}</div>
        <div class="stat-label">{{ t('quotation.issued') }}</div>
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
        <el-form-item :label="t('common.status')">
          <GcSelect
            v-model="query.status"
            domain="QUO_STATUS"
            :placeholder="t('common.all')"
            clearable
            style="width: 140px"
            :fallback="statusFallback"
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
        <el-table-column v-if="isVisible('quotationNo')" prop="quotationNo" :label="t('quotation.quotationNo')" width="200">
          <template #default="{ row }">
            <span class="code-link" @click="$router.push(`/quotation/detail/${row.id}`)">
              {{ row.quotationNo }}
            </span>
          </template>
        </el-table-column>
        <el-table-column prop="opportunityTitle" :label="t('opportunity.title')" min-width="160">
          <template #default="{ row }">
            <span class="text-secondary">{{ row.opportunityTitle }}</span>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('customer')" prop="customerName" :label="t('quotation.customer')" width="330">
          <template #default="{ row }">
            <div class="customer-cell">
              <el-avatar :size="24" :icon="UserFilled" class="customer-avatar-small" />
              <span>{{ row.customerName }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('totalAmount')" prop="totalAmount" :label="t('quotation.totalAmount')" width="130" align="right">
          <template #default="{ row }">
            <span class="amount">{{ row.totalAmount ? '¥' + row.totalAmount.toLocaleString() : '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('status')" prop="status" :label="t('common.status')" min-width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)" size="small" effect="light">
              {{ getStatusLabel(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="currentStepName" :label="t('approval.stepName')" width="150">
          <template #default="{ row }">
            <span class="step-name">{{ row.currentStepName || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="createdByName" :label="t('quotation.requestor')" min-width="100">
          <template #default="{ row }">
            <span class="text-secondary">{{ row.createdByName }}</span>
          </template>
        </el-table-column>
        <el-table-column :label="t('common.action')" min-width="200" align="right" fixed="right">
          <template #default="{ row }">
            <el-button link type="info" size="small" :icon="View" @click="$router.push(`/quotation/detail/${row.id}`)">
              {{ t('common.detail') }}
            </el-button>
            <el-button link type="primary" size="small" :icon="Edit" @click="handleEdit(row)" v-if="row.status === 0">
              {{ t('common.edit') }}
            </el-button>
            <el-button link type="danger" size="small" :icon="Delete" @click="handleDelete(row)" v-if="row.status === 0">
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

    <QuotationForm
      v-model:visible="dialogVisible"
      :data="editData"
      @success="loadData"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { quotationApi } from '@/api/quotation'
import QuotationForm from './QuotationForm.vue'
import { useI18n } from '@/composables/useI18n'
import { useFieldControl } from '@/composables/useFieldControl'
import { useUserStore } from '@/stores/user'
import { useGeneralizedCode, type GcOption } from '@/composables/useGeneralizedCode'
import GcSelect from '@/components/GcSelect.vue'
import {
  Plus,
  Search,
  RefreshRight,
  Delete,
  UserFilled,
  View,
  Edit
} from '@element-plus/icons-vue'

const { t } = useI18n()
const userStore = useUserStore()
const { isVisible, load: loadFieldConfig } = useFieldControl('Quotation', 'QuotationList')
const { getOptions } = useGeneralizedCode()
const loading = ref(false)
const dialogVisible = ref(false)
const tableData = ref<any[]>([])
const total = ref(0)
const statusCount = reactive<Record<number, number>>({})
const query = ref({ pageNum: 1, pageSize: 10, keyword: '', status: undefined as number | undefined })
const statusOptions = ref<GcOption[]>([])

const statusFallback = [
  { code: '0', label: t('quotation.draft'), tagType: 'info' },
  { code: '1', label: t('quotation.pendingReview'), tagType: 'warning' },
  { code: '2', label: t('quotation.pendingApproval'), tagType: 'warning' },
  { code: '3', label: t('quotation.issued'), tagType: 'success' },
  { code: '9', label: t('quotation.completed'), tagType: 'success' }
]

const loadData = async () => {
  loading.value = true
  try {
    await loadFieldConfig()
    const res = await quotationApi.list(query.value)
    tableData.value = res.data.list
    total.value = res.data.total
    // 閲嶇疆鐘舵€佺粺璁?    Object.keys(statusCount).forEach(key => delete statusCount[Number(key)])
    tableData.value.forEach((item: any) => {
      statusCount[item.status] = (statusCount[item.status] || 0) + 1
    })
  } finally {
    loading.value = false
  }
}

const editData = ref<any>(null)
const handleSearch = () => { query.value.pageNum = 1; loadData() }
const handleReset = () => { query.value = { pageNum: 1, pageSize: 10, keyword: '', status: undefined }; loadData() }
const handleAdd = () => { editData.value = null; dialogVisible.value = true }
const handleEdit = (row: any) => { editData.value = row; dialogVisible.value = true }

const handleDelete = async (row: any) => {
  await ElMessageBox.confirm(t('message.deleteConfirm'), t('common.tip'), { type: 'warning' })
  await quotationApi.delete(row.id)
  ElMessage.success(t('message.deleteSuccess'))
  loadData()
}

const getStatusOption = (status: number) => statusOptions.value.find(o => o.value === String(status))

function getStatusType(status: number) {
  return getStatusOption(status)?.tagType || 'info'
}

function getStatusLabel(status: number) {
  return getStatusOption(status)?.label || t('common.noData')
}

onMounted(async () => {
  statusOptions.value = await getOptions('QUO_STATUS', statusFallback)
  loadData()
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

.stats-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
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

.code-link {
  color: var(--slds-brand-primary);
  font-weight: 600;
  font-family: 'SF Mono', Monaco, monospace;
  font-size: 13px;
  cursor: pointer;
}

.code-link:hover {
  text-decoration: underline;
}

.customer-cell {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
}

.customer-avatar-small {
  background: var(--slds-brand-primary-light);
  color: var(--slds-brand-primary-dark);
}

.amount {
  font-weight: 600;
  color: var(--slds-text-primary);
}

.step-name {
  color: var(--slds-brand-primary);
  font-size: 12px;
  font-weight: 500;
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
</style>

