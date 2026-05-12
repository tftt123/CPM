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
        </template>
      </el-page-header>
    </div>

    <div class="detail-grid" v-loading="loading">
      <!-- Left: Quotation Info -->
      <div class="detail-left">
        <div class="content-card info-card">
          <div class="card-header">
            <el-icon size="18"><Document /></el-icon>
            <span>{{ t('common.detail') }}</span>
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
              <div class="info-label">{{ t('opportunity.title') }}</div>
              <div class="info-value">{{ detail?.opportunityTitle }}</div>
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
              <div class="info-label">{{ t('quotation.totalAmount') }}</div>
              <div class="info-value amount">¥{{ detail?.totalAmount?.toLocaleString() || '0.00' }}</div>
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

        <div class="content-card items-card">
          <div class="card-header">
            <el-icon size="18"><Tickets /></el-icon>
            <span>{{ t('quotation.items') }}</span>
          </div>
          <el-table :data="detail?.items" size="small">
            <el-table-column type="index" label="#" width="50" align="center" />
            <el-table-column prop="productName" :label="t('quotation.productName')" min-width="140" />
            <el-table-column prop="qty" :label="t('quotation.qty')" width="60" align="center" />
            <el-table-column prop="unitPrice" :label="t('quotation.unitPrice')" width="100" align="right">
              <template #default="{ row }">
                ¥{{ row.unitPrice?.toFixed(2) }}
              </template>
            </el-table-column>
            <el-table-column prop="lineAmount" :label="t('quotation.lineAmount')" width="100" align="right">
              <template #default="{ row }">
                <span class="amount">¥{{ row.lineAmount?.toFixed(2) }}</span>
              </template>
            </el-table-column>
            <el-table-column prop="processType" label="加工类型" width="90" />
            <el-table-column prop="equipmentType" label="设备类型" width="90" />
            <el-table-column prop="equipment" label="设备" width="100" />
            <el-table-column prop="cycleTime" :label="t('quotation.cycleTime')" width="80" align="right">
              <template #default="{ row }">
                {{ row.cycleTime?.toFixed(4) || '-' }}
              </template>
            </el-table-column>
            <el-table-column prop="hourlyRate" label="小时费率" width="90" align="right">
              <template #default="{ row }">
                {{ row.hourlyRate?.toFixed(4) || '-' }}
              </template>
            </el-table-column>
            <el-table-column prop="cost" label="成本" width="100" align="right">
              <template #default="{ row }">
                <span class="cost">¥{{ row.cost?.toFixed(4) || '0.0000' }}</span>
              </template>
            </el-table-column>
          </el-table>
          <div class="items-total">
            <span>{{ t('common.total') }}：</span>
            <span class="total-amount">¥{{ detail?.totalAmount?.toFixed(2) || '0.00' }}</span>
            <span style="margin-left: 24px;">成本合计：</span>
            <span class="total-cost">¥{{ totalItemCost.toFixed(4) }}</span>
          </div>
        </div>
      </div>

      <!-- Right: Approval Flow -->
      <div class="detail-right">
        <div class="content-card approval-card">
          <div class="card-header" style="justify-content: space-between;">
            <div style="display: flex; align-items: center; gap: var(--slds-spacing-sm);">
              <el-icon size="18"><Timer /></el-icon>
              <span>{{ t('quotation.approvalFlow') }}</span>
            </div>
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
          </div>

          <!-- Approval Steps -->
          <div class="approval-steps" v-if="approvalSteps.length > 0">
            <div
              v-for="(step, index) in approvalSteps"
              :key="step.id"
              class="approval-step"
              :class="{ 'is-current': step.isCurrent, 'is-completed': step.isCompleted }"
            >
              <div class="step-icon">
                <el-icon v-if="step.isCompleted" size="20" color="#2E844A"><CircleCheck /></el-icon>
                <el-icon v-else-if="step.isCurrent" size="20" color="#0176D3"><Timer /></el-icon>
                <span v-else class="step-number">{{ index + 1 }}</span>
              </div>
              <div class="step-content">
                <div class="step-name">{{ step.stepName }}</div>
                <div class="step-meta">
                  <span class="step-type">{{ getStepTypeLabel(step.stepType) }}</span>
                  <span class="step-approver" v-if="step.approverUserName">
                    {{ step.approverUserName }}
                  </span>
                  <span class="step-role" v-else-if="step.approverRole">
                    {{ step.approverRole }}
                  </span>
                </div>
              </div>
            </div>
          </div>
          <el-empty v-else :description="t('common.noData')" />

          <!-- Approval Records -->
          <div class="approval-records" v-if="approvalRecords.length > 0">
            <div class="records-title">{{ t('quotation.approvalRecords') }}</div>
            <div class="record-list">
              <div v-for="record in approvalRecords" :key="record.id" class="record-item">
                <div class="record-header">
                  <span class="record-step">{{ record.stepName }}</span>
                  <el-tag :type="record.action === 'APPROVE' ? 'success' : record.action === 'REJECT' ? 'danger' : 'info'" size="small">
                    {{ getActionLabel(record.action) }}
                  </el-tag>
                </div>
                <div class="record-body">
                  <span class="record-approver">{{ record.approverName }}</span>
                  <span class="record-comment" v-if="record.comment">：{{ record.comment }}</span>
                </div>
                <div class="record-time">{{ formatDateTime(record.createdAt) }}</div>
              </div>
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
            <div class="info-value amount">¥{{ detail?.totalAmount?.toLocaleString() || '0.00' }}</div>
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
import {
  Document,
  Tickets,
  Timer,
  CircleCheck,
  CircleClose,
  Promotion,
  View,
  User,
  UserFilled
} from '@element-plus/icons-vue'

const route = useRoute()
const { t } = useI18n()
const loading = ref(false)
const detail = ref<any>(null)
const approvalSteps = ref<any[]>([])
const approvalRecords = ref<any[]>([])
const canApprove = ref(false)
const canReject = ref(false)
const isReviewStep = ref(false)

const totalItemCost = computed(() => {
  return (detail.value?.items || []).reduce((sum: number, item: any) => sum + (item.cost || 0), 0)
})

const forecastDialogVisible = ref(false)
const forecastLoading = ref(false)
const forecastSteps = ref<any[]>([])

const approveDialogVisible = ref(false)
const approveAction = ref('APPROVE')
const approveLoading = ref(false)
const approveForm = ref({ comment: '', reviewCost: undefined as number | undefined })

const getStatusType = (status?: number) => {
  const map: Record<number, any> = { 0: 'info', 1: 'warning', 2: 'warning', 3: 'success', 9: 'success' }
  return map[status ?? -1] || 'info'
}

const getStatusLabel = (status?: number) => {
  const map: Record<number, string> = {
    0: t('quotation.draft'),
    1: t('quotation.pendingReview'),
    2: t('quotation.pendingApproval'),
    3: t('quotation.issued'),
    9: t('quotation.completed')
  }
  return map[status ?? -1] || t('common.noData')
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

onMounted(loadDetail)
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
  grid-template-columns: 1.2fr 0.8fr;
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

.info-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--slds-spacing-md);
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
  text-transform: uppercase;
  letter-spacing: 0.5px;
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

.items-card {
  padding-bottom: var(--slds-spacing-md);
}

.items-total {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  padding: var(--slds-spacing-md) var(--slds-spacing-lg);
  border-top: 1px solid var(--slds-border-color-light);
  font-size: 14px;
  color: var(--slds-text-secondary);
}

.total-amount {
  font-size: 20px;
  font-weight: 700;
  color: var(--slds-brand-primary);
  margin-left: 8px;
}

.amount {
  font-weight: 600;
  color: var(--slds-text-primary);
}

.cost {
  font-weight: 600;
  color: var(--slds-error);
}

.total-cost {
  font-size: 18px;
  font-weight: 700;
  color: var(--slds-error);
}

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
  font-size: 14px;
  font-weight: 600;
  color: var(--slds-text-primary);
  margin-bottom: var(--slds-spacing-md);
  padding-top: var(--slds-spacing-md);
  border-top: 1px solid var(--slds-border-color-light);
}

.record-list {
  display: flex;
  flex-direction: column;
  gap: var(--slds-spacing-md);
}

.record-item {
  background: #FAFBFC;
  border-radius: var(--slds-border-radius);
  padding: var(--slds-spacing-md);
  border-left: 3px solid var(--slds-border-color);
}

.record-item:has(.el-tag--success) {
  border-left-color: var(--slds-success);
}

.record-item:has(.el-tag--danger) {
  border-left-color: var(--slds-error);
}

.record-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.record-step {
  font-size: 13px;
  font-weight: 600;
  color: var(--slds-text-primary);
}

.record-body {
  font-size: 13px;
  color: var(--slds-text-secondary);
  margin-bottom: 4px;
}

.record-approver {
  font-weight: 600;
  color: var(--slds-text-primary);
}

.record-comment {
  color: var(--slds-text-secondary);
}

.record-time {
  font-size: 11px;
  color: var(--slds-text-secondary);
  opacity: 0.7;
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
}
</style>
