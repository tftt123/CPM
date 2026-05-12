<template>
  <div class="page-container">
    <div class="page-header">
      <el-button @click="router.back()">
        <el-icon><ArrowLeft /></el-icon>
        {{ t('common.back') }}
      </el-button>
      <h2>{{ isEdit ? t('pmTrace.editTitle') : t('pmTrace.createTitle') }}</h2>
      <el-button type="primary" @click="handleSave">
        <el-icon><Check /></el-icon>
        {{ t('common.save') }}
      </el-button>
    </div>

    <!-- 顶部信息行 -->
    <div class="info-bar">
      <div class="info-cell">
        <label>{{ t('pmTrace.customer') }}</label>
        <span>{{ form.customerName || '-' }}</span>
      </div>
      <div class="info-cell">
        <label>{{ t('pmTrace.partNo') }}</label>
        <span>{{ form.productCode || '-' }}</span>
      </div>
      <div class="info-cell">
        <label>{{ t('pmTrace.plannedQty') }}</label>
        <el-input-number v-model="form.plannedQty" :min="1" controls-position="right" style="width: 100px" size="small" />
      </div>
      <div class="info-cell">
        <label>{{ t('pmTrace.startDate') }}</label>
        <el-date-picker v-model="form.projectStartDate" type="date" value-format="YYYY-MM-DD" size="small" style="width: 140px" />
      </div>
      <div class="info-cell">
        <label>{{ t('pmTrace.displayWeeks') }}</label>
        <el-input-number v-model="form.displayWeeks" :min="1" :max="52" controls-position="right" style="width: 80px" size="small" />
      </div>
      <div class="info-cell">
        <label>{{ t('common.status') }}</label>
        <el-select v-model="form.status" size="small" style="width: 100px">
          <el-option :label="t('pmTrace.statusDraft')" :value="0" />
          <el-option :label="t('pmTrace.statusRunning')" :value="1" />
          <el-option :label="t('pmTrace.statusCompleted')" :value="2" />
        </el-select>
      </div>
    </div>

    <!-- 计划信息 + 计划甘特图 -->
    <el-card class="section-card">
      <template #header>
        <div class="card-header">
          <span>{{ t('pmTrace.planInfo') }}</span>
          <el-button type="primary" size="small" @click="addStep">
            <el-icon><Plus /></el-icon>
            {{ t('pmTrace.addStep') }}
          </el-button>
        </div>
      </template>

      <div class="gantt-section">
        <!-- 左侧表格 -->
        <div class="gantt-left">
          <div class="gantt-table-header">
            <div class="th" style="width:50px">#</div>
            <div class="th" style="width:90px">{{ t('pmTrace.process') }}</div>
            <div class="th" style="width:70px">{{ t('pmTrace.person') }}</div>
            <div class="th" style="width:80px">{{ t('pmTrace.planDays') }}</div>
            <div class="th" style="width:110px">{{ t('pmTrace.planStart') }}</div>
            <div class="th" style="width:110px">{{ t('pmTrace.planEnd') }}</div>
            <div class="th" style="width:50px"></div>
          </div>
          <div class="gantt-table-body">
            <div v-for="(step, i) in form.steps" :key="i" class="gantt-table-row">
              <div class="td" style="width:50px;text-align:center">{{ i + 1 }}</div>
              <div class="td" style="width:90px">
                <el-select v-model="step.processName" size="small" style="width:80px" filterable
                  @change="(val: string) => onProcessChange(step, val)">
                  <el-option v-for="opt in processOptions" :key="opt.id" :label="opt.label" :value="opt.label" />
                </el-select>
              </div>
              <div class="td" style="width:70px">
                <el-input v-model="step.personInCharge" size="small" disabled style="width:60px" />
              </div>
              <div class="td" style="width:80px;text-align:center">
                <el-input-number v-model="step.planDurationDays" :min="1" size="small" controls-position="right" style="width:65px" />
              </div>
              <div class="td" style="width:110px">
                <el-date-picker v-model="step.planStartDate" type="date" value-format="YYYY-MM-DD" size="small" style="width:105px" />
              </div>
              <div class="td" style="width:110px">{{ calcPlanEnd(step) || '-' }}</div>
              <div class="td" style="width:50px;text-align:center">
                <el-button link type="danger" size="small" @click="removeStep(i)">
                  <el-icon><Delete /></el-icon>
                </el-button>
              </div>
            </div>
          </div>
        </div>

        <!-- 右侧甘特图 -->
        <div class="gantt-right" v-if="ganttRange">
          <div class="gantt-scale-header">
            <div v-for="(label, idx) in weekLabels" :key="idx" class="gantt-scale-cell">
              {{ label }}
            </div>
          </div>
          <div class="gantt-scale-body">
            <div v-for="(step, i) in form.steps" :key="i" class="gantt-scale-row">
              <div v-if="step.planStartDate && step.planDurationDays"
                class="gantt-bar plan"
                :style="calcGanttStyle(step.planStartDate, step.planDurationDays)"
              />
            </div>
          </div>
        </div>
      </div>
    </el-card>

    <!-- 实际信息 + 实际甘特图 -->
    <el-card class="section-card">
      <template #header>
        <span>{{ t('pmTrace.actualInfo') }}</span>
      </template>

      <div class="gantt-section">
        <!-- 左侧表格 -->
        <div class="gantt-left">
          <div class="gantt-table-header">
            <div class="th" style="width:50px">#</div>
            <div class="th" style="width:90px">{{ t('pmTrace.process') }}</div>
            <div class="th" style="width:70px">{{ t('pmTrace.person') }}</div>
            <div class="th" style="width:110px">{{ t('pmTrace.actualStart') }}</div>
            <div class="th" style="width:80px">{{ t('pmTrace.actualDays') }}</div>
            <div class="th" style="width:110px">{{ t('pmTrace.actualEnd') }}</div>
          </div>
          <div class="gantt-table-body">
            <div v-for="(step, i) in form.steps" :key="i" class="gantt-table-row">
              <div class="td" style="width:50px;text-align:center">{{ i + 1 }}</div>
              <div class="td" style="width:90px">{{ step.processName }}</div>
              <div class="td" style="width:70px">
                <el-input v-model="step.personInCharge" size="small" disabled style="width:60px" />
              </div>
              <div class="td" style="width:110px">
                <el-date-picker v-model="step.actualStartDate" type="date" value-format="YYYY-MM-DD" size="small" style="width:105px" />
              </div>
              <div class="td" style="width:80px;text-align:center">
                <el-input-number v-model="step.actualDurationDays" :min="1" size="small" controls-position="right" style="width:65px" />
              </div>
              <div class="td" style="width:110px">{{ calcActualEnd(step) || '-' }}</div>
            </div>
          </div>
        </div>

        <!-- 右侧甘特图 -->
        <div class="gantt-right" v-if="ganttRange">
          <div class="gantt-scale-header">
            <div v-for="(label, idx) in weekLabels" :key="idx" class="gantt-scale-cell">
              {{ label }}
            </div>
          </div>
          <div class="gantt-scale-body">
            <div v-for="(step, i) in form.steps" :key="i" class="gantt-scale-row">
              <div v-if="step.actualStartDate && step.actualDurationDays"
                class="gantt-bar actual"
                :style="calcGanttStyle(step.actualStartDate, step.actualDurationDays)"
              />
            </div>
          </div>
        </div>
      </div>
    </el-card>

    <!-- 工艺路线汇总 -->
    <el-card class="section-card">
      <template #header>
        <span>{{ t('pmTrace.routeSummary') }}</span>
      </template>
      <el-table :data="form.steps" border stripe size="small">
        <el-table-column type="index" width="50" align="center" />
        <el-table-column :label="t('pmTrace.process')" width="120">
          <template #default="{ row }">{{ row.processName }}</template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.person')" width="100">
          <template #default="{ row }">
            <el-input v-model="row.personInCharge" size="small" disabled />
          </template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.cycleTime')" width="100" align="center">
          <template #default="{ row }">
            <el-input-number v-model="row.cycleTime" :min="0" size="small" controls-position="right" style="width: 80px" />
          </template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.actualCycleTime')" width="130" align="center">
          <template #default="{ row }">
            <span :class="{ 'text-warning': getLatestActualCycleTimeValue(row) != null && row.cycleTime != null && getLatestActualCycleTimeValue(row)! > row.cycleTime }">
              {{ getLatestActualCycleTime(row) }}
            </span>
          </template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.settingDays')" width="100" align="center">
          <template #default="{ row }">
            <el-input-number v-model="row.settingDays" :min="0" size="small" controls-position="right" style="width: 80px" />
          </template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.estimatedHours')" width="110" align="center">
          <template #default="{ row }">
            <el-input-number v-model="row.estimatedHours" :min="0" size="small" controls-position="right" style="width: 80px" />
          </template>
        </el-table-column>
      <el-table-column :label="t('pmTrace.remarks')">
          <template #default="{ row }">
            <el-input v-model="row.remarks" size="small" />
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>


</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { ArrowLeft, Check, Plus, Delete } from '@element-plus/icons-vue'
import { useI18n } from '@/composables/useI18n'
import { getTraceDetail, createTrace, updateTrace } from '@/api/pmProjectTrace'
import type { PmProjectTrace, PmProjectTraceStep, PmProjectTraceStepActualCycleTime } from '@/api/pmProjectTrace'
import { mfgProcessApi, type MfgCascadeOption } from '@/api/mfgProcess'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()

const isEdit = computed(() => !!route.params.id)
const traceId = computed(() => Number(route.params.id))
const processOptions = ref<MfgCascadeOption[]>([])

const form = ref<PmProjectTrace>({
  customerId: 0,
  customerName: '',
  productCode: '',
  status: 0,
  steps: [],
})

function getLatestActualCycleTimeValue(step: PmProjectTraceStep): number | null {
  if (!step.actualCycleTimes || step.actualCycleTimes.length === 0) return null
  const latest = step.actualCycleTimes[step.actualCycleTimes.length - 1]
  return latest?.actualCycleTime ?? null
}

function getLatestActualCycleTime(step: PmProjectTraceStep): string {
  if (!step.actualCycleTimes || step.actualCycleTimes.length === 0) return '-'
  const latest = step.actualCycleTimes[step.actualCycleTimes.length - 1]
  if (!latest) return '-'
  return latest.actualCycleTime != null ? latest.actualCycleTime.toFixed(4) : '-'
}

function formatDate(d: Date): string {
  const year = d.getFullYear()
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function calcPlanEnd(step: PmProjectTraceStep) {
  if (step.planStartDate && step.planDurationDays) {
    const d = new Date(step.planStartDate)
    d.setDate(d.getDate() + step.planDurationDays)
    return formatDate(d)
  }
  return null
}

function calcActualEnd(step: PmProjectTraceStep) {
  if (step.actualStartDate && step.actualDurationDays) {
    const d = new Date(step.actualStartDate)
    d.setDate(d.getDate() + step.actualDurationDays)
    return formatDate(d)
  }
  return null
}

// 甘特图计算
const ganttRange = computed(() => {
  const steps = form.value.steps
  if (!steps.length) return null

  // 以项目开始日期或最早的计划/实际开始日期为起点
  let minDate: Date
  if (form.value.projectStartDate) {
    minDate = new Date(form.value.projectStartDate)
  } else {
    const dates = steps
      .flatMap(s => [s.planStartDate, s.actualStartDate])
      .filter(Boolean)
      .map(d => new Date(d!))
    if (!dates.length) return null
    minDate = new Date(Math.min(...dates.map(d => d.getTime())))
  }

  // 对齐到周一
  const dayOfWeek = minDate.getDay()
  const offsetToMonday = dayOfWeek === 0 ? 6 : dayOfWeek - 1
  minDate.setDate(minDate.getDate() - offsetToMonday)

  const displayWeeks = form.value.displayWeeks || 8
  const weekWidth = 120
  const dayWidth = Math.floor(weekWidth / 7)

  return { minDate, displayWeeks, weekWidth, dayWidth }
})

// 生成周标签（如 2025/12/29）
const weekLabels = computed(() => {
  const range = ganttRange.value
  if (!range) return []
  const labels: string[] = []
  for (let w = 0; w < range.displayWeeks; w++) {
    const d = new Date(range.minDate)
    d.setDate(d.getDate() + w * 7)
    labels.push(`${d.getFullYear()}/${d.getMonth() + 1}/${d.getDate()}`)
  }
  return labels
})

function calcGanttStyle(startDateStr: string, durationDays: number) {
  const range = ganttRange.value
  if (!range) return {}

  const start = new Date(startDateStr)
  const offsetDays = Math.floor((start.getTime() - range.minDate.getTime()) / 86400000)
  const left = offsetDays * range.dayWidth
  const width = durationDays * range.dayWidth

  return {
    left: left + 'px',
    width: Math.max(width, 4) + 'px',
  }
}

async function loadProcessOptions() {
  const res = await mfgProcessApi.getProcessOptions()
  processOptions.value = res.data || []
}

function onProcessChange(row: PmProjectTraceStep, val: string) {
  const matched = processOptions.value.find(p => p.label === val)
  row.personInCharge = matched?.owner || ''
}

function addStep() {
  form.value.steps.push({
    stepOrder: form.value.steps.length + 1,
    processName: '',
    actualCycleTimes: [],
  })
}

function removeStep(index: number) {
  form.value.steps.splice(index, 1)
  form.value.steps.forEach((s, i) => { s.stepOrder = i + 1 })
}

async function handleSave() {
  try {
    if (isEdit.value) {
      await updateTrace(traceId.value, form.value)
      ElMessage.success(t('common.saveSuccess'))
    } else {
      await createTrace(form.value)
      ElMessage.success(t('common.createSuccess'))
      router.push('/pm/trace')
    }
  } catch (e) {
    console.error(e)
    ElMessage.error(t('common.saveFailed'))
  }
}

async function loadDetail() {
  if (!isEdit.value) return
  try {
    const res = await getTraceDetail(traceId.value)
    form.value = res.data
    form.value.steps.forEach((s) => {
      if (!s.personInCharge && s.processName) {
        const matched = processOptions.value.find((p) => p.label === s.processName)
        s.personInCharge = matched?.owner || ''
      }
    })
  } catch (e) {
    console.error(e)
  }
}

onMounted(async () => {
  await loadProcessOptions()
  await loadDetail()
})
</script>

<style scoped>
.page-container {
  padding: 16px;
}
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}
.page-header h2 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
}

/* 顶部信息行 */
.info-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 16px 24px;
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-light);
  border-radius: 4px;
  padding: 12px 16px;
  margin-bottom: 12px;
}
.info-cell {
  display: flex;
  flex-direction: column;
  gap: 2px;
}
.info-cell label {
  font-size: 11px;
  color: var(--el-text-color-secondary);
}
.info-cell span {
  font-size: 13px;
  font-weight: 500;
}

/* 卡片 */
.section-card {
  margin-bottom: 12px;
}
.section-card :deep(.el-card__header) {
  padding: 10px 16px;
  font-size: 14px;
  font-weight: 600;
}
.section-card :deep(.el-card__body) {
  padding: 12px;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

/* 甘特图区域：左侧表格 + 右侧甘特图 */
.gantt-section {
  display: flex;
  gap: 0;
  overflow-x: auto;
}

/* 左侧表格 */
.gantt-left {
  flex-shrink: 0;
  border: 1px solid #e4e7ed;
  border-right: none;
}
.gantt-table-header,
.gantt-table-row {
  display: flex;
  height: 40px;
  align-items: center;
  border-bottom: 1px solid #e4e7ed;
}
.gantt-table-header {
  background: #f5f7fa;
  font-size: 12px;
  font-weight: 600;
  color: #606266;
}
.gantt-table-row:last-child {
  border-bottom: none;
}
.gantt-table-header .th,
.gantt-table-row .td {
  padding: 0 4px;
  font-size: 12px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  box-sizing: border-box;
  height: 40px;
  display: flex;
  align-items: center;
}
.gantt-table-header .th {
  justify-content: center;
  border-right: 1px solid #e4e7ed;
}
.gantt-table-row .td {
  border-right: 1px solid #e4e7ed;
}

/* 右侧甘特图 */
.gantt-right {
  flex: 1;
  min-width: 600px;
  border: 1px solid #e4e7ed;
  position: relative;
}
.gantt-scale-header {
  display: flex;
  height: 40px;
  background: #f5f7fa;
  border-bottom: 1px solid #e4e7ed;
  font-size: 11px;
  color: #606266;
}
.gantt-scale-cell {
  flex: 1;
  min-width: 120px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-right: 1px solid #e4e7ed;
  white-space: nowrap;
}
.gantt-scale-body {
  position: relative;
}
.gantt-scale-row {
  height: 40px;
  border-bottom: 1px solid #ebeef5;
  position: relative;
}
.gantt-scale-row:last-child {
  border-bottom: none;
}

/* 甘特条 */
.gantt-bar {
  position: absolute;
  top: 10px;
  height: 20px;
  border-radius: 3px;
  opacity: 0.9;
  min-width: 4px;
}
.gantt-bar.plan {
  background: #409eff;
}
.gantt-bar.actual {
  background: #67c23a;
}

/* 工艺路线汇总表格 */
:deep(.el-table--small) {
  font-size: 12px;
}

/* 统一所有控件高度（与报价单管理一致） */
.page-container :deep(.el-input__wrapper),
.page-container :deep(.el-select .el-input__wrapper),
.page-container :deep(.el-date-editor.el-input__wrapper),
.page-container :deep(.el-input-number .el-input__wrapper) {
  min-height: 24px !important;
  height: 24px !important;
  padding: 0 8px !important;
}

.page-container :deep(.el-input__inner),
.page-container :deep(.el-select .el-input__inner),
.page-container :deep(.el-date-editor .el-input__inner),
.page-container :deep(.el-input-number .el-input__inner) {
  height: 22px !important;
  line-height: 22px !important;
}

.page-container :deep(.el-input-number .el-input-number__decrease),
.page-container :deep(.el-input-number .el-input-number__increase) {
  height: 22px !important;
  line-height: 22px !important;
  top: 1px;
}
</style>
