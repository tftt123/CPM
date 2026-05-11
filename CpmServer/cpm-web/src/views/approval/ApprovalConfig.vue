<template>
  <div class="page-container">
    <div class="page-header-section" v-if="!embedded">
      <el-page-header @back="$router.push('/home')">
        <template #content>
          <div class="page-header-content">
            <h1 class="page-title">{{ t('system.approvalConfigTitle') }}</h1>
            <span class="page-subtitle">{{ t('system.approvalConfigSubtitle') }}</span>
          </div>
        </template>
        <template #extra>
          <el-button type="primary" :icon="Plus" @click="handleAdd">
            {{ t('common.create') }}
          </el-button>
        </template>
      </el-page-header>
    </div>
    <div v-else class="embedded-header">
      <el-button type="primary" :icon="Plus" @click="handleAdd">
        {{ t('common.create') }}
      </el-button>
    </div>

    <div class="content-card" style="padding: 0;">
      <el-table :data="tableData" v-loading="loading" stripe style="width: 100%">
        <el-table-column type="index" label="#" width="50" align="center" />
        <el-table-column prop="templateCode" :label="t('approval.templateCode')" width="140">
          <template #default="{ row }">
            <span class="code-link">{{ row.templateCode }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="templateName" :label="t('approval.templateName')" min-width="160">
          <template #default="{ row }">
            <span class="template-name">{{ row.templateName }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="moduleType" :label="t('approval.moduleType')" width="110">
          <template #default="{ row }">
            <el-tag size="small" effect="light">{{ row.moduleType }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column :label="t('approval.steps')" min-width="200">
          <template #default="{ row }">
            <div class="steps-preview">
              <el-tag
                v-for="(step, idx) in row.steps"
                :key="idx"
                size="small"
                :type="step.stepType === 'REVIEW' ? 'primary' : step.stepType === 'APPROVAL' ? 'warning' : 'info'"
                class="step-tag"
              >
                {{ step.stepName }}
              </el-tag>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="isDefault" :label="t('approval.isDefault')" width="80" align="center">
          <template #default="{ row }">
            <el-tag v-if="row.isDefault" type="success" size="small">{{ t('common.yes') }}</el-tag>
            <span v-else class="text-secondary">-</span>
          </template>
        </el-table-column>
        <el-table-column prop="isActive" :label="t('common.status')" width="80" align="center">
          <template #default="{ row }">
            <el-switch v-model="row.isActive" disabled size="small" />
          </template>
        </el-table-column>
        <el-table-column :label="t('common.action')" width="150" align="right" fixed="right">
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

    <!-- Template Form Dialog -->
    <el-dialog
      :title="isEdit ? t('common.edit') : t('common.create')"
      v-model="dialogVisible"
      width="720px"
      :close-on-click-modal="false"
      class="slds-dialog"
      @closed="handleClose"
    >
      <div class="dialog-body">
        <el-form :model="form" label-width="100px" :rules="rules" ref="formRef" class="slds-form">
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item :label="t('approval.templateCode')" prop="templateCode">
                <el-input v-model="form.templateCode" :placeholder="t('common.pleaseInput')" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item :label="t('approval.templateName')" prop="templateName">
                <el-input v-model="form.templateName" :placeholder="t('common.pleaseInput')" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item :label="t('approval.moduleType')" prop="moduleType">
                <el-select v-model="form.moduleType" :placeholder="t('common.pleaseSelect')" style="width: 100%">
                  <el-option :label="t('nav.quotation')" value="Quotation" />
                  <el-option label="Sample" value="Sample" />
                  <el-option label="Procurement" value="Procurement" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item :label="t('approval.isDefault')">
                <el-switch v-model="form.isDefault" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-form-item :label="t('common.detail')">
            <el-input v-model="form.description" type="textarea" :rows="2" :placeholder="t('common.pleaseInput')" />
          </el-form-item>

          <!-- Approval Steps Config -->
          <div class="steps-section">
            <div class="steps-header">
              <span class="steps-title">{{ t('approval.steps') }}</span>
              <el-button type="primary" size="small" :icon="Plus" @click="addStep">{{ t('common.add') }}</el-button>
            </div>

            <div v-for="(step, index) in form.steps" :key="index" class="step-config">
              <div class="step-config-header">
                <span class="step-config-title">{{ t('approval.stepCardTitle', { index: index + 1 }) }}</span>
                <el-button link type="danger" size="small" :icon="Delete" @click="removeStep(index)">
                  {{ t('common.delete') }}
                </el-button>
              </div>
              <div class="step-grid approval-config-step-grid">
                <div class="step-field">
                  <div class="step-label">{{ t('approval.stepName') }} *</div>
                  <el-input v-model="step.stepName" :placeholder="t('common.pleaseInput')" />
                </div>
                <div class="step-field">
                  <div class="step-label">{{ t('approval.stepType') }} *</div>
                  <el-select v-model="step.stepType" :placeholder="t('common.pleaseSelect')" style="width: 100%">
                    <el-option :label="t('approval.stepTypeReview')" value="REVIEW" />
                    <el-option :label="t('approval.stepTypeApproval')" value="APPROVAL" />
                    <el-option :label="t('approval.stepTypeNotify')" value="NOTIFY" />
                  </el-select>
                </div>
                <div class="step-field">
                  <div class="step-label">{{ t('approval.stepMode') }}</div>
                  <el-select v-model="step.stepMode" :placeholder="t('common.pleaseSelect')" style="width: 100%">
                    <el-option :label="t('approval.stepModeSequential')" value="SEQUENTIAL" />
                    <el-option :label="t('approval.stepModeParallel')" value="PARALLEL" />
                    <el-option :label="t('approval.stepModeParallelAny')" value="PARALLEL_ANY" />
                    <el-option :label="t('approval.stepModeConditional')" value="CONDITIONAL" />
                    <el-option :label="t('approval.stepModeCC')" value="CC" />

                  </el-select>
                </div>
                <div class="step-field">
                  <div class="step-label">{{ t('approval.approverRole') }}</div>
                  <el-select v-model="step.approverRole" :placeholder="t('common.pleaseSelect')" style="width: 100%" clearable>
                    <el-option v-for="role in roleList" :key="role.roleCode" :label="role.roleName" :value="role.roleCode" />
                  </el-select>
                </div>
                <div class="step-field">
                  <div class="step-label">{{ t('approval.designatedApprover') }}</div>
                  <el-select v-model="step.approverUserId" :placeholder="t('common.pleaseSelect')" style="width: 100%" clearable>
                    <el-option v-for="u in userList" :key="u.id" :label="u.realName || u.username" :value="u.id" />
                  </el-select>
                </div>
                <div class="step-field">
                  <div class="step-label">{{ t('approval.emailTemplate') }}</div>
                  <el-input v-model="step.notifyEmailTemplate" :placeholder="t('approval.emailTemplate')" />
                </div>
                <div class="step-field">
                  <div class="step-label">{{ t('approval.rejectBehavior') }}</div>
                  <el-select v-model="step.rejectBehavior" :placeholder="t('common.pleaseSelect')" style="width: 100%">
                    <el-option :label="t('approval.rejectBehaviorClose')" value="REJECT_AND_CLOSE" />
                    <el-option :label="t('approval.rejectBehaviorPrev')" value="REJECT_TO_PREV" />
                    <el-option :label="t('approval.rejectBehaviorStep')" value="REJECT_TO_STEP" />
                    <el-option :label="t('approval.rejectBehaviorStart')" value="REJECT_TO_START" />
                    <el-option :label="t('approval.rejectBehaviorRequestor')" value="REJECT_TO_REQUESTOR" />
                  </el-select>
                </div>
                <div class="step-field">
                  <div class="step-label">{{ t('approval.rejectTargetStep') }}</div>
                  <el-select
                    v-model="step.rejectTargetStepId"
                    :placeholder="t('common.pleaseSelect')"
                    style="width: 100%"
                    clearable
                    filterable
                    allow-create
                    default-first-option
                    :disabled="step.rejectBehavior !== 'REJECT_TO_STEP'"
                  >
                    <el-option
                      v-for="opt in stepRejectTargetOptions(index)"
                      :key="opt.value"
                      :label="opt.label"
                      :value="opt.value"
                    />
                  </el-select>
                </div>
                <div class="step-field">
                  <div class="step-label">{{ t('approval.timeoutHours') }}</div>
                  <el-input-number v-model="step.timeoutHours" :min="0" :controls="false" style="width: 100%" :placeholder="t('approval.timeoutHours')" />
                </div>
                <div class="step-field">
                  <div class="step-label">{{ t('common.action') }}</div>
                  <div class="step-checkboxes">
                    <el-checkbox v-model="step.canReject">{{ t('approval.canReject') }}</el-checkbox>
                    <el-checkbox v-model="step.canTransfer">{{ t('approval.canTransfer') }}</el-checkbox>
                  </div>
                </div>
              </div>

              <!-- 条件分支 & 审批规则 -->
              <el-collapse v-model="step._collapse" class="step-collapse">
                <el-collapse-item :title="t('approval.conditions')" name="conditions">
                  <div v-for="(cond, cIdx) in step.conditions" :key="cIdx" class="sub-item-row">
                    <el-input v-model="cond.conditionName" :placeholder="t('approval.conditionName')" style="width: 120px" />
                    <el-input v-model="cond.expression" :placeholder="t('approval.expression')" style="flex: 1" />
                    <el-select
                      v-model="cond.targetStepId"
                      :placeholder="t('approval.targetStep')"
                      style="width: 140px"
                      clearable
                      filterable
                      allow-create
                      default-first-option
                    >
                      <el-option
                        v-for="opt in stepTargetOptions(index)"
                        :key="opt.value"
                        :label="opt.label"
                        :value="opt.value"
                      />
                    </el-select>
                    <el-input-number v-model="cond.priority" :min="0" :max="99" :controls="false" style="width: 70px" />
                    <el-button link type="danger" :icon="Delete" @click="removeCondition(index, cIdx)">{{ t('common.delete') }}</el-button>
                  </div>
                  <el-button type="primary" size="small" :icon="Plus" @click="addCondition(index)">{{ t('approval.addCondition') }}</el-button>
                </el-collapse-item>
                <el-collapse-item :title="t('approval.rules')" name="rules">
                  <div v-for="(rule, rIdx) in step.rules" :key="rIdx" class="sub-item-row">
                    <el-select v-model="rule.ruleType" :placeholder="t('approval.ruleType')" style="width: 120px">
                      <el-option :label="t('approval.ruleTypeFixedRole')" value="FIXED_ROLE" />
                      <el-option :label="t('approval.ruleTypeFixedUser')" value="FIXED_USER" />
                      <el-option :label="t('approval.ruleTypeOrgTree')" value="ORG_TREE" />
                    </el-select>
                    <el-select
                      v-model="rule.ruleValue"
                      :placeholder="t('approval.ruleValue')"
                      style="flex: 1"
                      filterable
                      allow-create
                      default-first-option
                    >
                      <el-option
                        v-for="opt in ruleValueOptions(rule.ruleType)"
                        :key="opt.value"
                        :label="opt.label"
                        :value="opt.value"
                      />
                    </el-select>
                    <el-select
                      v-model="rule.fallback"
                      :placeholder="t('approval.fallback')"
                      style="width: 120px"
                      filterable
                      allow-create
                      default-first-option
                    >
                      <el-option
                        v-for="opt in fallbackOptions(rule.ruleType)"
                        :key="opt.value"
                        :label="opt.label"
                        :value="opt.value"
                      />
                    </el-select>
                    <el-input-number v-model="rule.priority" :min="0" :max="99" :controls="false" style="width: 70px" />
                    <el-button link type="danger" :icon="Delete" @click="removeRule(index, rIdx)">{{ t('common.delete') }}</el-button>
                  </div>
                  <el-button type="primary" size="small" :icon="Plus" @click="addRule(index)">{{ t('approval.addRule') }}</el-button>
                </el-collapse-item>
              </el-collapse>
            </div>
          </div>
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
import { approvalApi } from '@/api/quotation'
import { userApi } from '@/api/user'
import { roleApi } from '@/api/role'
import { useI18n } from '@/composables/useI18n'
import { Plus, Edit, Delete } from '@element-plus/icons-vue'

const props = defineProps<{ embedded?: boolean }>()
const { t } = useI18n()

const loading = ref(false)
const dialogVisible = ref(false)
const submitting = ref(false)
const tableData = ref<any[]>([])
const userList = ref<any[]>([])
const roleList = ref<any[]>([])
const editId = ref<number | null>(null)
const formRef = ref()

const isEdit = computed(() => !!editId.value)

const form = ref({
  templateCode: '',
  templateName: '',
  moduleType: 'Quotation',
  description: '',
  isDefault: false,
  isActive: true,
  steps: [] as any[]
})

const rules = {
  templateCode: [{ required: true, message: t('validation.required', { field: t('approval.templateCode') }), trigger: 'blur' }],
  templateName: [{ required: true, message: t('validation.required', { field: t('approval.templateName') }), trigger: 'blur' }],
  moduleType: [{ required: true, message: t('validation.selectRequired', { field: t('approval.moduleType') }), trigger: 'change' }]
}

const loadData = async () => {
  loading.value = true
  try {
    const res = await approvalApi.getTemplates()
    tableData.value = res.data
  } finally {
    loading.value = false
  }
}

const loadUsers = async () => {
  try {
    const res = await userApi.list({ pageNum: 1, pageSize: 999 })
    userList.value = res.data.list || []
  } catch {
    userList.value = []
  }
}

const loadRoles = async () => {
  try {
    const res = await roleApi.list()
    roleList.value = res.data || []
  } catch {
    roleList.value = []
  }
}

const handleAdd = () => {
  editId.value = null
  form.value = {
    templateCode: '',
    templateName: '',
    moduleType: 'Quotation',
    description: '',
    isDefault: false,
    isActive: true,
    steps: []
  }
  dialogVisible.value = true
}

const handleEdit = (row: any) => {
  editId.value = row.id
  form.value = {
    templateCode: row.templateCode,
    templateName: row.templateName,
    moduleType: row.moduleType,
    description: row.description || '',
    isDefault: row.isDefault,
    isActive: row.isActive,
    steps: (row.steps || []).map((s: any) => ({
      ...s,
      stepMode: s.stepMode || 'SEQUENTIAL',
      rejectBehavior: s.rejectBehavior || 'REJECT_AND_CLOSE',
      rejectTargetStepId: s.rejectTargetStepId ?? null,
      timeoutHours: s.timeoutHours ?? null,
      rules: (s.rules || []).map((r: any) => ({ ...r })),
      conditions: (s.conditions || []).map((c: any) => ({ ...c })),
      _collapse: []
    }))
  }
  dialogVisible.value = true
}

const handleDelete = async (row: any) => {
  await ElMessageBox.confirm(t('message.deleteConfirm'), t('common.tip'), { type: 'warning' })
  await approvalApi.deleteTemplate(row.id)
  ElMessage.success(t('message.deleteSuccess'))
  loadData()
}

const addStep = () => {
  form.value.steps.push({
    stepName: '',
    stepOrder: form.value.steps.length + 1,
    stepType: 'REVIEW',
    stepMode: 'SEQUENTIAL',
    approverRole: '',
    approverUserId: null,
    canReject: true,
    canTransfer: false,
    notifyEmailTemplate: '',
    rejectBehavior: 'REJECT_AND_CLOSE',
    rejectTargetStepId: null,
    timeoutHours: null,
    rules: [],
    conditions: [],
    _collapse: []
  })
}

const addCondition = (stepIndex: number) => {
  form.value.steps[stepIndex].conditions.push({
    conditionName: '',
    expression: '',
    targetStepId: null,
    priority: 0,
    isActive: true
  })
}

const removeCondition = (stepIndex: number, condIndex: number | string) => {
  form.value.steps[stepIndex].conditions.splice(Number(condIndex), 1)
}

const addRule = (stepIndex: number) => {
  form.value.steps[stepIndex].rules.push({
    ruleType: 'FIXED_ROLE',
    ruleValue: '',
    fallback: '',
    priority: 0,
    isActive: true
  })
}

const removeRule = (stepIndex: number, ruleIndex: number | string) => {
  form.value.steps[stepIndex].rules.splice(Number(ruleIndex), 1)
}

// 步骤目标选项（用于驳回目标、条件分支目标）
const stepRejectTargetOptions = (currentStepIndex: number) => {
  return form.value.steps
    .filter((_, i) => i !== currentStepIndex && (_.stepName || _.id))
    .map(s => ({ label: s.stepName || `Step ${s.stepOrder}`, value: s.id }))
}

const stepTargetOptions = (currentStepIndex: number) => {
  return form.value.steps
    .filter((_, i) => i !== currentStepIndex && (_.stepName || _.id))
    .map(s => ({ label: s.stepName || `Step ${s.stepOrder}`, value: s.id }))
}

// 规则值选项（根据规则类型提供建议）
const ruleValueOptions = (ruleType: string) => {
  if (ruleType === 'FIXED_ROLE') {
    return roleList.value.map(r => ({ label: r.roleName, value: r.roleCode }))
  }
  if (ruleType === 'FIXED_USER') {
    return userList.value.map(u => ({ label: u.realName || u.username, value: String(u.id) }))
  }
  return []
}

const fallbackOptions = (ruleType: string) => {
  if (ruleType === 'FIXED_ROLE') {
    return roleList.value.map(r => ({ label: r.roleName, value: r.roleCode }))
  }
  if (ruleType === 'FIXED_USER') {
    return userList.value.map(u => ({ label: u.realName || u.username, value: String(u.id) }))
  }
  return []
}

const removeStep = (index: number) => {
  form.value.steps.splice(index, 1)
  form.value.steps.forEach((s, i) => { s.stepOrder = i + 1 })
}

const handleSubmit = async () => {
  await formRef.value.validate()
  if (form.value.steps.length === 0) {
    ElMessage.warning(t('common.noData'))
    return
  }
  for (const step of form.value.steps) {
    if (!step.stepName) {
      ElMessage.warning(t('validation.required', { field: t('approval.stepName') }))
      return
    }
  }

  submitting.value = true
  try {
    const payload = {
      ...form.value,
      steps: form.value.steps.map((s: any) => {
        const { _collapse, ...rest } = s
        return rest
      })
    }
    if (isEdit.value && editId.value) {
      await approvalApi.updateTemplate(editId.value, payload)
      ElMessage.success(t('message.updateSuccess'))
    } else {
      await approvalApi.createTemplate(payload)
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

onMounted(() => {
  loadData()
  loadUsers()
  loadRoles()
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

.template-name {
  font-weight: 600;
  color: var(--slds-text-primary);
}

.steps-preview {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.step-tag {
  margin: 0;
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

.steps-section {
  margin-top: 16px;
  padding: 16px;
  background: #FAFBFC;
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
}

.steps-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.steps-title {
  font-size: 14px;
  font-weight: 600;
  color: var(--slds-text-primary);
}

.step-config {
  background: var(--slds-bg-card);
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
  padding: 12px;
  margin-bottom: 12px;
}

.step-config-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.step-config-title {
  font-size: 13px;
  font-weight: 600;
  color: var(--slds-text-primary);
}

.step-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px 16px;
}

.step-field {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.step-label {
  font-size: 12px;
  font-weight: 600;
  color: var(--slds-text-secondary);
  line-height: 1.4;
}


.step-checkboxes {
  display: flex;
  align-items: center;
  gap: 16px;
}

.step-checkboxes :deep(.el-checkbox) {
  margin: 0;
}

/* 统一审批步骤表单控件高度为 32px */
.step-grid :deep(.el-input),
.step-grid :deep(.el-select) {
  width: 100%;
}

/* 强制覆盖 Element Plus 默认样式，统一 wrapper 高度和 padding */
.slds-form :deep(.el-input__wrapper),
.step-grid :deep(.el-input__wrapper) {
  height: 32px !important;
  min-height: 32px !important;
  padding: 1px 11px !important;
}

.slds-form :deep(.el-select__wrapper),
.step-grid :deep(.el-select__wrapper) {
  height: 32px !important;
  min-height: 32px !important;
  padding: 1px 11px !important;
}

/* 统一内部输入框高度 */
.slds-form :deep(.el-input__inner),
.step-grid :deep(.el-input__inner) {
  height: 30px !important;
  line-height: 30px !important;
}

/* el-select 内部 input（filterable 模式时） */
.slds-form :deep(.el-select .el-input__inner),
.step-grid :deep(.el-select .el-input__inner) {
  height: 30px !important;
  line-height: 30px !important;
}

/* 统一 placeholder 行高 */
.slds-form :deep(.el-select__placeholder),
.step-grid :deep(.el-select__placeholder) {
  line-height: 30px !important;
}

@media (max-width: 680px) {
  .step-grid {
    grid-template-columns: repeat(2, 1fr);
  }
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

.step-collapse {
  margin-top: 12px;
}

.step-collapse :deep(.el-collapse-item__header) {
  font-size: 13px;
  font-weight: 600;
  color: var(--slds-text-primary);
  background: #F5F7FA;
  padding: 0 12px;
  border-radius: 4px;
}

.step-collapse :deep(.el-collapse-item__wrap) {
  background: transparent;
}

.step-collapse :deep(.el-collapse-item__content) {
  padding: 12px 0 0 0;
}

.sub-item-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}

.sub-item-row :deep(.el-input__wrapper),
.sub-item-row :deep(.el-select__wrapper) {
  height: 30px !important;
  min-height: 30px !important;
}
</style>
