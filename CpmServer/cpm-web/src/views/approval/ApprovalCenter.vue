<template>
  <div class="page-container">
    <div class="page-header">
      <h2>{{ t('nav.approvalCenter') }}</h2>
    </div>

    <el-table :data="pagedTaskList" v-loading="loading" stripe>
      <el-table-column type="index" :index="(idx: number) => idx + 1 + (currentPage - 1) * pageSize" label="#" width="50" align="center" />
      <el-table-column :label="t('approval.businessType')" min-width="120">
        <template #default="{ row }">
          <el-tag size="small">{{ row.businessType }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column :label="t('approval.templateDesc')" min-width="160">
        <template #default="{ row }">
          {{ row.templateName || '-' }}
        </template>
      </el-table-column>
      <el-table-column :label="t('approval.stepName')" min-width="120">
        <template #default="{ row }">
          {{ row.stepName || '-' }}
        </template>
      </el-table-column>
      <el-table-column :label="t('approval.approverRole')" min-width="120">
        <template #default="{ row }">
          {{ row.assigneeName || row.assigneeRole || '-' }}
        </template>
      </el-table-column>
      <el-table-column :label="t('common.createTime')" width="160">
        <template #default="{ row }">
          {{ formatDate(row.createdAt) }}
        </template>
      </el-table-column>
      <el-table-column :label="t('common.action')" min-width="180" align="center" fixed="right">
        <template #default="{ row }">
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

    <!-- 瀹℃壒寮圭獥 -->
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
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { useI18n } from '@/composables/useI18n'
import { getMyPendingTasks, approveInstance, rejectInstance } from '@/api/approval'
import type { ApprovalTask } from '@/api/approval'
import { Check, Close } from '@element-plus/icons-vue'

const { t } = useI18n()

const loading = ref(false)
const taskList = ref<ApprovalTask[]>([])
const currentPage = ref(1)
const pageSize = ref(10)
const dialogVisible = ref(false)
const submitting = ref(false)
const action = ref('APPROVE')
const currentTask = ref<ApprovalTask | null>(null)
const form = ref({ comment: '' })

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
</style>
