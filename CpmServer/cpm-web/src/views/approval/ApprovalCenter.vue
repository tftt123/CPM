<template>
  <div class="page-container">
    <div class="page-header">
      <h2>{{ t('nav.approvalCenter') }}</h2>
    </div>

    <el-table :data="pagedTaskList" v-loading="loading" stripe>
      <el-table-column type="index" :index="(idx: number) => idx + 1 + (currentPage - 1) * pageSize" label="#" width="50" align="center" />
      <el-table-column :label="t('quotation.rfqNo')" min-width="140">
        <template #default="{ row }">
          {{ row.rfqNo || '-' }}
        </template>
      </el-table-column>
      <el-table-column :label="t('quotation.customer')" min-width="140">
        <template #default="{ row }">
          {{ row.customerName || '-' }}
        </template>
      </el-table-column>
      <el-table-column :label="t('opportunity.title')" min-width="180">
        <template #default="{ row }">
          {{ row.title || '-' }}
        </template>
      </el-table-column>
      <el-table-column :label="t('approval.stepName')" min-width="120">
        <template #default="{ row }">
          {{ row.stepName || '-' }}
        </template>
      </el-table-column>
      <el-table-column :label="t('common.createTime')" width="160">
        <template #default="{ row }">
          {{ formatDate(row.createdAt) }}
        </template>
      </el-table-column>
      <el-table-column :label="t('common.action')" min-width="240" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" :icon="View" @click="handleView(row)">
            {{ t('common.detail') }}
          </el-button>
          <el-button link type="success" size="small" :icon="Check" @click="openApproveDialog(row, 'APPROVE')">
            {{ t('common.approve') }}
          </el-button>
          <el-button link type="danger" size="small" :icon="Close" @click="openApproveDialog(row, 'REJECT')">
            {{ t('common.reject') }}
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-pagination
      v-if="taskList.length > pageSize"
      v-model:current-page="currentPage"
      v-model:page-size="pageSize"
      :total="taskList.length"
      layout="prev, pager, next, jumper, total"
      class="pagination"
    />

    <!-- 审批弹窗 -->
    <el-dialog
      :title="dialogTitle"
      v-model="dialogVisible"
      width="400px"
      :close-on-click-modal="false"
    >
      <el-form :model="form" label-width="80px">
        <el-form-item :label="t('common.remark')">
          <el-input v-model="form.comment" type="textarea" :rows="3" :placeholder="t('common.pleaseInput')" />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogVisible = false">{{ t('common.cancel') }}</el-button>
          <el-button :type="action === 'APPROVE' ? 'success' : 'danger'" :loading="submitting" @click="handleSubmit">
            {{ dialogTitle }}
          </el-button>
        </div>
      </template>
    </el-dialog>

    <!-- PmStepCycleTime 变更详情弹窗 -->
    <el-dialog
      :title="cycleTimeDialogTitle"
      v-model="cycleTimeDialogVisible"
      width="650px"
      :close-on-click-modal="false"
    >
      <div v-loading="cycleTimeLoading">
        <div class="detail-info" v-if="cycleTimeDetail">
          <div class="info-row">
            <span class="info-label">{{ t('pmTrace.customer') }}</span>
            <span class="info-value">{{ cycleTimeDetail.customerName || '-' }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">{{ t('pmTrace.productName') }}</span>
            <span class="info-value">{{ cycleTimeDetail.productName || '-' }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">{{ t('pmTrace.partNo') }}</span>
            <span class="info-value">{{ cycleTimeDetail.productCode || '-' }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">{{ t('pmTrace.process') }}</span>
            <span class="info-value">{{ cycleTimeDetail.processName || '-' }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">{{ t('quotation.requestor') }}</span>
            <span class="info-value">{{ cycleTimeDetail.submitterName || '-' }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">{{ t('pmTrace.cycleTime') }}</span>
            <span class="info-value">{{ cycleTimeDetail.cycleTime != null ? Math.round(cycleTimeDetail.cycleTime) : '-' }}</span>
          </div>
        </div>
        <el-table :data="cycleTimeDetail?.details || []" border stripe size="small" class="detail-table">
          <el-table-column :label="t('pmTrace.changeType')" width="100" align="center">
            <template #default="{ row }">
              <el-tag :type="getChangeTypeTag(row.changeType)" size="small">
                {{ getChangeTypeLabel(row.changeType) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column :label="t('pmTrace.recordDate')" width="120" align="center">
            <template #default="{ row }">
              {{ row.recordDate }}
            </template>
          </el-table-column>
          <el-table-column :label="t('pmTrace.actualCycleTime')" width="130" align="right">
            <template #default="{ row }">
              {{ row.actualCycleTime != null ? Math.round(row.actualCycleTime) : '-' }}
            </template>
          </el-table-column>
          <el-table-column :label="t('pmTrace.remarks')" min-width="120">
            <template #default="{ row }">
              {{ row.remarks || '-' }}
            </template>
          </el-table-column>
        </el-table>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="cycleTimeDialogVisible = false">{{ t('common.close') }}</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useI18n } from '@/composables/useI18n'
import { useGeneralizedCode, type GcOption } from '@/composables/useGeneralizedCode'
import { getMyPendingTasks, approveInstance, rejectInstance } from '@/api/approval'
import type { ApprovalTask } from '@/api/approval'
import { getCycleTimeChangeRequest } from '@/api/pmProjectTrace'
import type { PmStepCycleTimeChangeRequestDetail } from '@/api/pmProjectTrace'
import { Check, Close, View } from '@element-plus/icons-vue'

const { t } = useI18n()
const router = useRouter()
const { getOptions } = useGeneralizedCode()

const loading = ref(false)
const changeTypeOptions = ref<GcOption[]>([])

const changeTypeFallback = [
  { code: '0', label: t('pmTrace.changeTypeAdd') || '新增', tagType: 'success' },
  { code: '1', label: t('pmTrace.changeTypeEdit') || '修改', tagType: 'warning' },
  { code: '2', label: t('pmTrace.changeTypeDelete') || '删除', tagType: 'danger' }
]
const taskList = ref<ApprovalTask[]>([])
const currentPage = ref(1)
const pageSize = ref(10)
const dialogVisible = ref(false)
const submitting = ref(false)
const action = ref('APPROVE')
const currentTask = ref<ApprovalTask | null>(null)
const form = ref({ comment: '' })

// PmStepCycleTime 详情弹窗
const cycleTimeDialogVisible = ref(false)
const cycleTimeLoading = ref(false)
const cycleTimeDetail = ref<PmStepCycleTimeChangeRequestDetail | null>(null)
const cycleTimeDialogTitle = computed(() => {
  return cycleTimeDetail.value?.processName
    ? `${t('pmTrace.actualCycleTime')} - ${cycleTimeDetail.value.processName}`
    : t('pmTrace.actualCycleTime')
})

const pagedTaskList = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return taskList.value.slice(start, start + pageSize.value)
})

const dialogTitle = computed(() => {
  return action.value === 'APPROVE' ? t('common.approve') : t('common.reject')
})

function formatDate(date?: string) {
  if (!date) return '-'
  return new Date(date).toLocaleString()
}

const getChangeTypeOption = (type: number) => changeTypeOptions.value.find(o => o.value === String(type))

function getChangeTypeLabel(type: number): string {
  return getChangeTypeOption(type)?.label || '-'
}

function getChangeTypeTag(type: number): string {
  return getChangeTypeOption(type)?.tagType || ''
}

async function loadData() {
  loading.value = true
  try {
    const res = await getMyPendingTasks()
    taskList.value = res.data
    currentPage.value = 1
  } catch (e) {
    console.error(e)
    ElMessage.error(t('common.failed'))
  } finally {
    loading.value = false
  }
}

async function handleView(row: ApprovalTask) {
  if (row.businessType === 'Quotation' && row.businessId) {
    router.push(`/quotation/detail/${row.businessId}`)
  } else if (row.businessType === 'PmStepCycleTime' && row.businessId) {
    cycleTimeDialogVisible.value = true
    cycleTimeLoading.value = true
    try {
      const res = await getCycleTimeChangeRequest(row.businessId)
      cycleTimeDetail.value = res.data
    } catch (e) {
      console.error(e)
      ElMessage.error(t('common.failed'))
    } finally {
      cycleTimeLoading.value = false
    }
  }
}

function openApproveDialog(row: ApprovalTask, act: string) {
  currentTask.value = row
  action.value = act
  form.value.comment = ''
  dialogVisible.value = true
}

async function handleSubmit() {
  if (!currentTask.value) return
  submitting.value = true
  try {
    if (action.value === 'APPROVE') {
      await approveInstance(currentTask.value.instanceId, { comment: form.value.comment })
      ElMessage.success(t('message.approveSuccess'))
    } else {
      await rejectInstance(currentTask.value.instanceId, { comment: form.value.comment })
      ElMessage.success(t('message.rejectSuccess'))
    }
    dialogVisible.value = false
    loadData()
  } catch (e) {
    console.error(e)
    ElMessage.error(t('common.failed'))
  } finally {
    submitting.value = false
  }
}

getOptions('CYCLE_TIME_CHANGE_TYPE', changeTypeFallback).then(opts => {
  changeTypeOptions.value = opts
})
loadData()
</script>

<style scoped>
.page-container {
  padding: var(--cpm-space-6);
}
.page-header {
  margin-bottom: var(--cpm-space-4);
}
.page-header h2 {
  margin: 0;
  font-size: var(--cpm-text-display);
  font-weight: 700;
  color: var(--cpm-text-primary);
  letter-spacing: -0.5px;
  line-height: 1.2;
}
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--cpm-space-3);
}
.pagination {
  margin-top: var(--cpm-space-4);
  justify-content: flex-end;
}
.detail-info {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px 24px;
  margin-bottom: 16px;
  padding: 12px 16px;
  background: #f5f7fa;
  border-radius: 4px;
}
.info-row {
  display: flex;
  gap: 8px;
}
.info-label {
  font-size: 13px;
  color: var(--el-text-color-secondary);
  min-width: 60px;
}
.info-value {
  font-size: 13px;
  font-weight: 500;
  color: var(--el-text-color-primary);
}
.detail-table {
  margin-top: 8px;
}
</style>
