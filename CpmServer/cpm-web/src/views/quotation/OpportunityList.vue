<template>
  <div class="page-container">
    <div class="page-header-section">
      <el-page-header @back="$router.push('/home')">
        <template #content>
          <div class="page-header-content">
            <h1 class="page-title">{{ t('opportunity.pageTitle') }}</h1>
            <span class="page-subtitle">{{ t('opportunity.title') }}</span>
          </div>
        </template>
        <template #extra>
          <el-button type="primary" :icon="Plus" @click="handleAdd">
            {{ t('common.add') }}
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
        <div class="stat-value" style="color: var(--slds-warning);">{{ stageCount.NEW || 0 }}</div>
        <div class="stat-label">{{ t('common.new') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value" style="color: var(--slds-brand-primary);">{{ stageCount.PROPOSAL || 0 }}</div>
        <div class="stat-label">{{ t('common.proposal') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-value" style="color: var(--slds-success);">{{ stageCount.CLOSED || 0 }}</div>
        <div class="stat-label">{{ t('common.closed') }}</div>
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
        <el-form-item :label="t('opportunity.stage')">
          <GcSelect
            v-model="query.stage"
            domain="OPP_STAGE"
            :placeholder="t('common.all')"
            clearable
            style="width: 140px"
            :fallback="stageFallback"
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
        <el-table-column v-if="isVisible('opportunityNo')" prop="opportunityNo" :label="t('opportunity.opportunityNo')" width="200">
          <template #default="{ row }">
            <span class="code-link">{{ row.opportunityNo }}</span>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('title')" prop="title" :label="t('opportunity.title')" min-width="180">
          <template #default="{ row }">
            <div class="opportunity-title-cell">
              <span class="opportunity-title">{{ row.title }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('customerName')" prop="customerName" :label="t('quotation.customer')" width="330">
          <template #default="{ row }">
            <div class="customer-cell">
              <el-avatar :size="24" :icon="UserFilled" class="customer-avatar-small" />
              <span>{{ row.customerName }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('expectedAmount')" prop="expectedAmount" :label="t('opportunity.expectedAmount')" width="130" align="right">
          <template #default="{ row }">
            <span class="amount">{{ row.expectedAmount ? '¥' + row.expectedAmount.toLocaleString() : '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('stage')" prop="stage" :label="t('opportunity.stage')" width="150">
          <template #default="{ row }">
            <el-tag :type="getStageType(row.stage)" size="small" effect="light">
              {{ getStageLabel(row.stage) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('quoteDeadline')" prop="quoteDeadline" :label="t('opportunity.quoteDeadline')" width="130">
          <template #default="{ row }">
            <span class="date-text">{{ row.quoteDeadline ? formatDate(row.quoteDeadline) : '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column v-if="isVisible('ownerName')" prop="ownerName" :label="t('opportunity.owner')" min-width="100">
          <template #default="{ row }">
            <div class="owner-cell">
              <el-icon size="14"><User /></el-icon>
              <span>{{ row.ownerName }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column :label="t('common.action')" min-width="200" align="right" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" size="small" :icon="Edit" @click="handleEdit(row)">
              {{ t('common.edit') }}
            </el-button>
            <el-button link type="success" size="small" :icon="Document" @click="handleCreateQuotation(row)">
              {{ t('common.create') }}
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

    <OpportunityForm
      v-model:visible="dialogVisible"
      :data="editData"
      @success="loadData"
    />

    <QuotationForm
      v-model:visible="quotationDialogVisible"
      :opportunity="selectedOpportunity"
      @success="$router.push('/quotation/list')"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { opportunityApi } from '@/api/quotation'
import OpportunityForm from './OpportunityForm.vue'
import QuotationForm from './QuotationForm.vue'
import { useI18n } from '@/composables/useI18n'
import { useFieldControl } from '@/composables/useFieldControl'
import { useGeneralizedCode, type GcOption } from '@/composables/useGeneralizedCode'
import GcSelect from '@/components/GcSelect.vue'
import {
  Plus,
  Search,
  RefreshRight,
  Edit,
  Delete,
  UserFilled,
  User,
  Document
} from '@element-plus/icons-vue'

const { t } = useI18n()
const { isVisible } = useFieldControl('Opportunity', 'OpportunityList')
const { getOptions } = useGeneralizedCode()
const loading = ref(false)
const dialogVisible = ref(false)
const quotationDialogVisible = ref(false)
const editData = ref<any>(null)
const selectedOpportunity = ref<any>(null)
const tableData = ref<any[]>([])
const total = ref(0)
const stageCount = reactive<Record<string, number>>({})
const query = ref<{ pageNum: number; pageSize: number; keyword: string; stage: string | null }>({ pageNum: 1, pageSize: 10, keyword: '', stage: '' })
const stageOptions = ref<GcOption[]>([])

const stageFallback = [
  { code: 'NEW', label: t('common.new'), tagType: 'info' },
  { code: 'QUALIFIED', label: t('common.qualified'), tagType: 'warning' },
  { code: 'PROPOSAL', label: t('common.proposal'), tagType: 'warning' },
  { code: 'NEGOTIATION', label: t('common.negotiation'), tagType: 'warning' },
  { code: 'CLOSED', label: t('common.closed'), tagType: 'success' },
  { code: 'LOST', label: t('common.lost'), tagType: 'danger' }
]

const loadData = async () => {
  loading.value = true
  try {
    const res = await opportunityApi.list(query.value)
    tableData.value = res.data.list
    total.value = res.data.total
    tableData.value.forEach((item: any) => {
      stageCount[item.stage] = (stageCount[item.stage] || 0) + 1
    })
  } finally {
    loading.value = false
  }
}

const handleSearch = () => { query.value.pageNum = 1; loadData() }
const handleReset = () => { query.value = { pageNum: 1, pageSize: 10, keyword: '', stage: '' }; loadData() }

const getStageOption = (stage: string) => stageOptions.value.find(o => o.value === stage)

const getStageType = (stage: string) => getStageOption(stage)?.tagType || 'info'

const getStageLabel = (stage: string) => getStageOption(stage)?.label || stage
const handleAdd = () => { editData.value = null; dialogVisible.value = true }
const handleEdit = (row: any) => { editData.value = row; dialogVisible.value = true }

const handleCreateQuotation = (row: any) => {
  selectedOpportunity.value = row
  quotationDialogVisible.value = true
}

const handleDelete = async (row: any) => {
  await ElMessageBox.confirm(t('message.deleteConfirm'), t('common.tip'), { type: 'warning' })
  await opportunityApi.delete(row.id)
  ElMessage.success(t('message.deleteSuccess'))
  loadData()
}

const formatDate = (date: string) => {
  return new Date(date).toLocaleDateString()
}

onMounted(async () => {
  stageOptions.value = await getOptions('OPP_STAGE', stageFallback)
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
}

.opportunity-title-cell {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
}

.opportunity-title {
  font-weight: 600;
  color: var(--slds-text-primary);
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

.owner-cell {
  display: flex;
  align-items: center;
  gap: 4px;
  color: var(--slds-text-secondary);
  font-size: 13px;
}

.date-text {
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

