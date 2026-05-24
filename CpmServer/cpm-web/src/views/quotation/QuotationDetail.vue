<template>
  <div class="page-container">
    <div class="page-header-section">
      <el-page-header @back="$router.push('/quotation/list')">
        <template #content>
          <div class="page-header-content">
            <h1 class="page-title">{{ t('quotation.quotationDetail') }}</h1>
            <span class="page-subtitle">{{ detail?.quotationNo }} - {{ detail?.customerName }}</span>
          </div>
        </template>
        <template #extra>
          <el-button type="primary" :icon="Promotion" @click="handleSubmit" v-if="detail?.status === 0">
            {{ t('quotation.submitApproval') }}
          </el-button>
          <el-button type="success" :icon="CircleCheck" @click="showApproveDialog('APPROVE')" v-if="canApprove">
            {{ t('quotation.approve') }}
          </el-button>
          <el-button type="danger" :icon="CircleClose" @click="showApproveDialog('REJECT')" v-if="canReject">
            {{ t('quotation.reject') }}
          </el-button>
          <el-button
            link
            type="primary"
            size="small"
            :icon="View"
            @click="showForecast"
            :loading="forecastLoading"
          >
            {{ t('quotation.forecast') || '审批预测' }}
          </el-button>
        </template>
      </el-page-header>
    </div>

    <div class="detail-grid" v-loading="loading">
      <!-- Left: Quotation Info + Product Groups -->
      <div class="detail-left">
        <div class="content-card info-card">
          <div class="card-header">
            <el-icon size="18"><Document /></el-icon>
            <span>{{ detail?.opportunityTitle || t('common.detail') }}</span>
          </div>
          <div class="info-grid">
            <div class="info-item">
              <div class="info-label">{{ t('quotation.quotationNo') }}</div>
              <div class="info-value code">{{ detail?.quotationNo }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">{{ t('quotation.customer') }}</div>
              <div class="info-value">{{ detail?.customerName }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">{{ t('customer.currency') }}</div>
              <div class="info-value">{{ detail?.customerCurrency || '-' }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">{{ t('common.status') }}</div>
              <div class="info-value">
                <el-tag :type="getStatusType(detail?.status)" size="small">
                  {{ getStatusLabel(detail?.status) }}
                </el-tag>
              </div>
            </div>
            <div class="info-item">
              <div class="info-label">{{ t('common.user') }}</div>
              <div class="info-value">{{ detail?.createdByName }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">{{ t('common.create') }}</div>
              <div class="info-value">{{ detail?.createdAt ? formatDate(detail.createdAt) : '-' }}</div>
            </div>
          </div>
        </div>

        <!-- Product Groups: grouped card style -->
        <div class="content-card items-card">
          <div class="card-header">
            <el-icon size="18"><Tickets /></el-icon>
            <span>{{ t('quotation.items') }}</span>
          </div>

          <div class="product-groups" v-if="productGroups.length > 0">
            <div
              v-for="(group, groupIndex) in productGroups"
              :key="group.productId || groupIndex"
              class="product-group"
            >
              <!-- Group Header: Product Name + Qty -->
              <div class="group-header">
                <div class="group-product">
                  <el-icon size="16"><Box /></el-icon>
                  <span v-if="isVisible('productName')" class="group-product-name">{{ group.productName }}</span>
                  <el-tag size="small" type="info">x{{ group.qty }}</el-tag>
                </div>
                <div class="group-summary">
                  <span class="group-cost">{{ t('quotation.packaging') }}: {{ formatCurrency(group.packagingCost) }}</span>
                  <span class="group-cost">{{ t('quotation.transport') }}: {{ formatCurrency(group.transportCost) }}</span>
                  <span class="group-cost">{{ t('quotation.processingFee') }}: {{ formatCurrency(group.processCost) }}</span>
                  <span class="group-amount">{{ formatCurrency(group.lineAmount) }}</span>
                </div>
              </div>

              <!-- Process Table -->
              <div class="group-table">
                <div class="group-table-header">
                  <span v-if="isVisible('processType')" class="th">{{ t('quotation.processType') }}</span>
                  <span v-if="isVisible('equipmentType')" class="th">{{ t('quotation.equipmentType') }}</span>
                  <span v-if="isVisible('equipment')" class="th">{{ t('quotation.equipment') }}</span>
                  <span v-if="isVisible('cycleTime')" class="th" align="right">{{ t('quotation.cycleTime') }}</span>
                  <span v-if="isVisible('hourlyRate')" class="th" align="right">{{ t('quotation.hourlyRate') }}</span>
                  <span class="th" align="right">{{ t('quotation.processingFee') }}</span>
                  <span class="th" align="right">{{ t('quotation.lineAmount') }}</span>
                </div>
                <div
                  v-for="(process, pIdx) in group.processes"
                  :key="process.id || pIdx"
                  class="group-table-row"
                  :class="{ 'is-master': pIdx === 0 }"
                >
                  <span v-if="isVisible('processType')" class="td">{{ process.processType || '-' }}</span>
                  <span v-if="isVisible('equipmentType')" class="td">{{ process.equipmentType || '-' }}</span>
                  <span v-if="isVisible('equipment')" class="td">{{ process.equipment || '-' }}</span>
                  <span v-if="isVisible('cycleTime')" class="td" align="right">{{ process.cycleTime != null ? Math.round(process.cycleTime) : '-' }}</span>
                  <span v-if="isVisible('hourlyRate')" class="td" align="right">{{ process.hourlyRate?.toFixed(2) || '-' }}</span>
                  <span class="td cost" align="right">{{ formatCurrency(process.cost || 0) }}</span>
                  <span class="td amount" align="right">{{ formatCurrency(process.lineAmount || 0) }}</span>
                </div>
              </div>
            </div>
          </div>
          <el-empty v-else :description="t('common.noData')" />

          <!-- Grand Total -->
          <div class="items-total" v-if="productGroups.length > 0">
            <div class="total-final">
              <span>{{ t('quotation.totalAmount') }}: </span>
              <span class="total-amount">{{ formatCurrency(detail?.totalAmount || 0) }}</span>
            </div>
          </div>
        </div>

        <!-- Attachments -->
        <div class="content-card" v-if="detail?.fileIds?.length">
          <div class="card-header">
            <el-icon size="18"><Paperclip /></el-icon>
            <span>{{ t('quotation.attachments') }}</span>
          </div>
          <div class="attachments-list">
            <div v-for="fileId in detail.fileIds" :key="fileId" class="attachment-item">
              <el-icon size="16"><Document /></el-icon>
              <span class="attachment-name">{{ fileId }}</span>
            </div>
          </div>
        </div>

        <!-- Approval Flow -->
        <!-- Approval Records -->
        <div class="content-card approval-card" v-if="approvalRecords.length > 0">
          <div class="card-header">
            <el-icon size="18"><List /></el-icon>
            <span>{{ t('quotation.approvalRecords') }}</span>
          </div>
          <div class="record-list">
            <div v-for="record in approvalRecords" :key="record.id" class="record-item">
              <div class="record-step">{{ record.stepName }}</div>
              <div class="record-approver">{{ record.approverName }}</div>
              <div class="record-comment" v-if="record.comment">{{ record.comment }}</div>
              <div class="record-time">{{ formatDateTime(record.createdAt) }}</div>
              <el-tag :type="record.action === 'APPROVE' ? 'success' : record.action === 'REJECT' ? 'danger' : 'info'" size="small">
                {{ getActionLabel(record.action) }}
              </el-tag>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Approval Dialog -->
    <el-dialog
      :title="approveAction === 'APPROVE' ? t('quotation.approve') : t('quotation.reject')"
      v-model="approveDialogVisible"
      width="480px"
      class="slds-dialog"
    >
      <div class="dialog-body">
        <el-form :model="approveForm" label-width="80px">
          <el-form-item :label="t('quotation.comment')">
            <el-input
              v-model="approveForm.comment"
              type="textarea"
              :rows="3"
              :placeholder="t('common.pleaseInput')"
            />
          </el-form-item>
          <el-form-item :label="t('quotation.totalAmount')" v-if="approveAction === 'APPROVE' && isReviewStep">
            <div class="info-value amount">{{ formatCurrency(detail?.totalAmount || 0) }}</div>
          </el-form-item>
        </el-form>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="approveDialogVisible = false">{{ t('common.cancel') }}</el-button>
          <el-button :type="approveAction === 'APPROVE' ? 'success' : 'danger'" :loading="approveLoading" @click="handleApprove">
            {{ t('common.confirm') }}
          </el-button>
        </div>
      </template>
    </el-dialog>

    <!-- Forecast Dialog -->
    <el-dialog
      :title="t('quotation.forecast') || '审批预测'"
      v-model="forecastDialogVisible"
      width="520px"
      class="slds-dialog"
    >
      <div class="dialog-body" v-loading="forecastLoading">
        <div v-if="forecastSteps.length > 0" class="forecast-list">
          <div
            v-for="(step, index) in forecastSteps"
            :key="step.stepId"
            class="forecast-step"
          >
            <div class="forecast-step-header">
              <span class="forecast-step-number">{{ index + 1 }}</span>
              <span class="forecast-step-name">{{ step.stepName }}</span>
              <el-tag size="small" :type="step.stepType === 'REVIEW' ? 'warning' : 'primary'">
                {{ getStepTypeLabel(step.stepType) }}
              </el-tag>
            </div>
            <div class="forecast-step-approvers">
              <div
                v-for="approver in step.approvers"
                :key="approver.userId || approver.roleCode"
                class="forecast-approver"
              >
                <el-icon v-if="approver.type === 'USER'" size="14"><User /></el-icon>
                <el-icon v-else size="14"><UserFilled /></el-icon>
                <span class="forecast-approver-name">{{ approver.name }}</span>
                <el-tag v-if="approver.type === 'ROLE'" size="small" type="info">{{ approver.roleCode }}</el-tag>
              </div>
            </div>
          </div>
        </div>
        <el-empty v-else :description="t('common.noData')" />
      </div>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { quotationApi } from '@/api/quotation'
import { useI18n } from '@/composables/useI18n'
import { useFieldControl } from '@/composables/useFieldControl'
import { useGeneralizedCode, type GcOption } from '@/composables/useGeneralizedCode'
import { groupQuotationItems, type ProductGroup } from '@/composables/useQuotationGroups'
import {
  Document,
  Tickets,
  Timer,
  CircleCheck,
  CircleClose,
  Promotion,
  View,
  User,
  UserFilled,
  Box,
  Paperclip,
  List
} from '@element-plus/icons-vue'

const route = useRoute()
const { t } = useI18n()
const { isVisible, load: loadFieldConfig } = useFieldControl('Quotation', 'QuotationDetail')
const { getOptions } = useGeneralizedCode()
const loading = ref(false)
const detail = ref<any>(null)
const approvalSteps = ref<any[]>([])
const approvalRecords = ref<any[]>([])
const canApprove = ref(false)
const canReject = ref(false)
const isReviewStep = ref(false)
const statusOptions = ref<GcOption[]>([])

const statusFallback = [
  { code: '0', label: t('quotation.draft'), tagType: 'info' },
  { code: '1', label: t('quotation.pendingReview'), tagType: 'warning' },
  { code: '2', label: t('quotation.pendingApproval'), tagType: 'warning' },
  { code: '3', label: t('quotation.issued'), tagType: 'success' },
  { code: '9', label: t('quotation.completed'), tagType: 'success' }
]

/** 产品分组数据 */
const productGroups = computed<ProductGroup[]>(() => {
  if (!detail.value?.items) return []
  return groupQuotationItems(detail.value.items)
})

/** 总工艺数 */
const totalProcessCount = computed(() =>
  productGroups.value.reduce((sum, g) => sum + g.processes.length, 0)
)

/** 总包装费 */
const totalPackagingCost = computed(() =>
  productGroups.value.reduce((sum, g) => sum + g.packagingCost, 0)
)

/** 总运输费 */
const totalTransportCost = computed(() =>
  productGroups.value.reduce((sum, g) => sum + g.transportCost, 0)
)

/** 货币格式化 */
const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('zh-CN', { style: 'currency', currency: 'CNY' }).format(value)
}

const forecastDialogVisible = ref(false)
const forecastLoading = ref(false)
const forecastSteps = ref<any[]>([])

const approveDialogVisible = ref(false)
const approveAction = ref('APPROVE')
const approveLoading = ref(false)
const approveForm = ref({ comment: '', reviewCost: undefined as number | undefined })

const getStatusOption = (status?: number) => statusOptions.value.find(o => o.value === String(status))

function getStatusType(status?: number) {
  return getStatusOption(status)?.tagType || 'info'
}

function getStatusLabel(status?: number) {
  return getStatusOption(status)?.label || t('common.noData')
}

const getStepTypeLabel = (type: string) => {
  const map: Record<string, string> = {
    REVIEW: t('approval.stepTypeReview'),
    APPROVAL: t('approval.stepTypeApproval'),
    NOTIFY: t('approval.stepTypeNotify')
  }
  return map[type] || type
}

const getActionLabel = (action: string) => {
  const map: Record<string, string> = {
    APPROVE: t('quotation.approve'),
    REJECT: t('quotation.reject'),
    TRANSFER: t('approval.canTransfer')
  }
  return map[action] || action
}

const formatDate = (date: string) => {
  return new Date(date).toLocaleDateString()
}

const formatDateTime = (date: string) => {
  return new Date(date).toLocaleString()
}

const loadDetail = async () => {
  loading.value = true
  try {
    await loadFieldConfig()
    const id = Number(route.params.id)
    const res = await quotationApi.getById(id)
    detail.value = res.data

    const stepsRes = await quotationApi.getApprovalSteps(id)
    approvalSteps.value = stepsRes.data

    const recordsRes = await quotationApi.getApprovalRecords(id)
    approvalRecords.value = recordsRes.data

    const currentStep = approvalSteps.value.find((s: any) => s.status === 0)
    if (currentStep && (detail.value?.status === 1 || detail.value?.status === 2)) {
      try {
        const permRes = await quotationApi.canApprove(id)
        canApprove.value = permRes.data === true
      } catch {
        canApprove.value = false
      }
      canReject.value = canApprove.value && currentStep.canReject
      isReviewStep.value = currentStep.stepType === 'REVIEW'
    } else {
      canApprove.value = false
      canReject.value = false
      isReviewStep.value = false
    }
  } finally {
    loading.value = false
  }
}

const handleSubmit = async () => {
  await ElMessageBox.confirm(t('message.deleteConfirm'), t('quotation.submitApproval'), { type: 'warning' })
  await quotationApi.submitForApproval(Number(route.params.id))
  ElMessage.success(t('common.success'))
  loadDetail()
}

const showApproveDialog = (action: string) => {
  approveAction.value = action
  approveForm.value = { comment: '', reviewCost: undefined }
  approveDialogVisible.value = true
}

const handleApprove = async () => {
  approveLoading.value = true
  try {
    await quotationApi.processApproval(Number(route.params.id), {
      action: approveAction.value,
      comment: approveForm.value.comment
    })
    ElMessage.success(t('common.success'))
    approveDialogVisible.value = false
    loadDetail()
  } finally {
    approveLoading.value = false
  }
}

const showForecast = async () => {
  forecastDialogVisible.value = true
  forecastLoading.value = true
  try {
    const res = await quotationApi.getApprovalForecast(Number(route.params.id))
    forecastSteps.value = res.data || []
  } catch (err: any) {
    ElMessage.error(err?.response?.data?.message || '预测失败')
  } finally {
    forecastLoading.value = false
  }
}

onMounted(async () => {
  statusOptions.value = await getOptions('QUO_STATUS', statusFallback)
  loadDetail()
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

.detail-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--slds-spacing-lg);
}

.content-card {
  background: var(--slds-bg-card);
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
  box-shadow: var(--slds-shadow-card);
  overflow: hidden;
  margin-bottom: var(--slds-spacing-md);
}

.card-header {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
  padding: var(--slds-spacing-md) var(--slds-spacing-lg);
  border-bottom: 1px solid var(--slds-border-color-light);
  font-size: 16px;
  font-weight: 600;
  color: var(--slds-text-primary);
}

.items-count {
  margin-left: auto;
  font-size: 12px;
  color: var(--slds-text-secondary);
  font-weight: 400;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 20px;
  padding: var(--slds-spacing-lg);
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.info-label {
  font-size: 12px;
  color: var(--slds-text-secondary);
  font-weight: 500;
  letter-spacing: 0.3px;
}

.info-value {
  font-size: 14px;
  color: var(--slds-text-primary);
  font-weight: 500;
}

.info-value.code {
  font-family: 'SF Mono', Monaco, monospace;
  color: var(--slds-brand-primary);
  font-weight: 600;
}

.info-value.amount {
  font-size: 18px;
  font-weight: 700;
  color: var(--slds-brand-primary);
}

/* Product Group Styles */
.product-groups {
  padding: var(--slds-spacing-md) var(--slds-spacing-lg);
  display: flex;
  flex-direction: column;
  gap: var(--slds-spacing-lg);
}

.product-group {
  border: 1px solid var(--slds-border-color-light);
  border-radius: var(--slds-border-radius);
  overflow: hidden;
}

.group-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--slds-spacing-md) var(--slds-spacing-lg);
  background: #FAFBFC;
  border-bottom: 1px solid var(--slds-border-color-light);
}

.group-product {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
  font-size: 14px;
  font-weight: 600;
  color: var(--slds-text-primary);
}

.group-product-name {
  color: var(--slds-brand-primary);
}

.group-summary {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-md);
  font-size: 13px;
}

.group-cost {
  color: var(--slds-text-secondary);
}

.group-amount {
  font-size: 16px;
  font-weight: 700;
  color: var(--slds-brand-primary);
}

/* Group Table */
.group-table {
  width: 100%;
}

.group-table-header {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr 80px 80px 90px 90px;
  gap: 8px;
  padding: 8px var(--slds-spacing-lg);
  background: #F5F7FA;
  font-size: 12px;
  font-weight: 600;
  color: var(--slds-text-secondary);
  border-bottom: 1px solid var(--slds-border-color-light);
}

.group-table-header .th {
  white-space: nowrap;
}

.group-table-header .th[align="right"] {
  text-align: right;
}

.group-table-row {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr 80px 80px 90px 90px;
  gap: 8px;
  padding: 10px var(--slds-spacing-lg);
  font-size: 13px;
  color: var(--slds-text-primary);
  border-bottom: 1px solid var(--slds-border-color-light);
  transition: background 0.15s ease;
}

.group-table-row:last-child {
  border-bottom: none;
}

.group-table-row:hover {
  background: #FAFBFC;
}

.group-table-row.is-master {
  background: #FFF8F0;
}

.group-table-row .td {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.group-table-row .td[align="right"] {
  text-align: right;
}

.group-table-row .td.cost {
  color: var(--slds-brand-primary);
  font-weight: 600;
}

.group-table-row .td.amount {
  font-weight: 700;
  color: var(--slds-text-primary);
}

/* Items Total */
.items-total {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--slds-spacing-md) var(--slds-spacing-lg);
  border-top: 1px solid var(--slds-border-color-light);
  font-size: 14px;
  color: var(--slds-text-secondary);
}

.total-breakdown {
  display: flex;
  gap: var(--slds-spacing-lg);
}

.extra-cost-label {
  font-size: 13px;
  color: var(--slds-text-secondary);
}

.total-final {
  font-size: 14px;
}

.total-amount {
  font-size: 20px;
  font-weight: 700;
  color: var(--slds-brand-primary);
  margin-left: 8px;
}

/* Attachments */
.attachments-list {
  padding: var(--slds-spacing-md) var(--slds-spacing-lg);
}

.attachment-item {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
  padding: var(--slds-spacing-sm) 0;
  font-size: 13px;
  color: var(--slds-text-secondary);
}

.attachment-name {
  color: var(--slds-text-primary);
}

/* Approval */
.approval-card {
  padding-bottom: var(--slds-spacing-lg);
}

.approval-steps {
  padding: var(--slds-spacing-lg);
}

.approval-step {
  display: flex;
  align-items: flex-start;
  gap: var(--slds-spacing-md);
  padding: var(--slds-spacing-md) 0;
  border-bottom: 1px solid var(--slds-border-color-light);
}

.approval-step:last-child {
  border-bottom: none;
}

.step-icon {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: var(--slds-bg-hover);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.step-number {
  font-size: 13px;
  font-weight: 600;
  color: var(--slds-text-secondary);
}

.approval-step.is-current .step-icon {
  background: #E8F4FD;
}

.approval-step.is-completed .step-icon {
  background: #E6F3E6;
}

.step-content {
  flex: 1;
}

.step-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--slds-text-primary);
  margin-bottom: 4px;
}

.step-meta {
  display: flex;
  gap: var(--slds-spacing-sm);
  font-size: 12px;
}

.step-type {
  color: var(--slds-brand-primary);
  background: #F0F8FF;
  padding: 2px 8px;
  border-radius: 10px;
}

.step-approver,
.step-role {
  color: var(--slds-text-secondary);
}

.approval-records {
  padding: 0 var(--slds-spacing-lg);
}

.records-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--slds-text-primary);
  padding: var(--slds-spacing-md) var(--slds-spacing-lg);
}

.record-list {
  display: flex;
  flex-direction: column;
  gap: var(--slds-spacing-md);
  padding: 0 var(--slds-spacing-lg) var(--slds-spacing-md);
}

.record-item {
  display: flex;
  align-items: center;
  gap: 60px;
  background: #FAFBFC;
  border-radius: var(--slds-border-radius);
  padding: var(--slds-spacing-md) var(--slds-spacing-lg);
  border-left: 3px solid var(--slds-border-color);
}

.record-item:has(.el-tag--success) {
  border-left-color: var(--slds-success);
}

.record-item:has(.el-tag--danger) {
  border-left-color: var(--slds-error);
}

.record-step {
  font-size: 13px;
  font-weight: 600;
  color: var(--slds-text-primary);
  min-width: 80px;
}

.record-approver {
  font-size: 13px;
  font-weight: 600;
  color: var(--slds-text-primary);
}

.record-comment {
  font-size: 13px;
  color: var(--slds-text-secondary);
  flex: 1;
}

.record-time {
  font-size: 11px;
  color: var(--slds-text-secondary);
  opacity: 0.7;
  white-space: nowrap;
}

.dialog-body {
  padding: 24px 8px;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.forecast-list {
  display: flex;
  flex-direction: column;
  gap: var(--slds-spacing-md);
}

.forecast-step {
  background: #FAFBFC;
  border-radius: var(--slds-border-radius);
  padding: var(--slds-spacing-md);
  border-left: 3px solid var(--slds-brand-primary);
}

.forecast-step-header {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
  margin-bottom: var(--slds-spacing-sm);
}

.forecast-step-number {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: var(--slds-brand-primary);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
}

.forecast-step-name {
  font-size: 14px;
  font-weight: 600;
  color: var(--slds-text-primary);
  flex: 1;
}

.forecast-step-approvers {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding-left: 32px;
}

.forecast-approver {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: var(--slds-text-secondary);
}

.forecast-approver-name {
  color: var(--slds-text-primary);
  font-weight: 500;
}

@media (max-width: 1024px) {
  .detail-grid {
    grid-template-columns: 1fr;
  }

  .group-table-header,
  .group-table-row {
    grid-template-columns: 1fr 1fr 1fr 70px 70px 80px 80px;
    font-size: 12px;
  }
}

@media (max-width: 768px) {
  .group-table-header,
  .group-table-row {
    grid-template-columns: 1fr 1fr 80px 80px;
  }

  .group-table-header .th:nth-child(3),
  .group-table-header .th:nth-child(4),
  .group-table-row .td:nth-child(3),
  .group-table-row .td:nth-child(4) {
    display: none;
  }
}
</style>
