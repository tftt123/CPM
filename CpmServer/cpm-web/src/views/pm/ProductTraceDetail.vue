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

    <!-- 顶部信息卡 -->
    <el-card class="info-card">
      <el-row :gutter="24">
        <el-col :span="6">
          <div class="info-item">
            <label>{{ t('pmTrace.customer') }}</label>
            <span>{{ form.customerName || '-' }}</span>
          </div>
        </el-col>
        <el-col :span="6">
          <div class="info-item">
            <label>{{ t('pmTrace.partNo') }}</label>
            <span>{{ form.productCode || '-' }}</span>
          </div>
        </el-col>
        <el-col :span="6">
          <div class="info-item">
            <label>{{ t('pmTrace.plannedQty') }}</label>
            <el-input-number v-model="form.plannedQty" :min="1" controls-position="right" style="width: 120px" />
          </div>
        </el-col>
        <el-col :span="6">
          <div class="info-item">
            <label>{{ t('pmTrace.startDate') }}</label>
            <el-date-picker v-model="form.projectStartDate" type="date" value-format="YYYY-MM-DD" style="width: 140px" />
          </div>
        </el-col>
      </el-row>
      <el-row :gutter="24" style="margin-top: 12px">
        <el-col :span="6">
          <div class="info-item">
            <label>{{ t('pmTrace.displayWeeks') }}</label>
            <el-input-number v-model="form.displayWeeks" :min="1" :max="52" controls-position="right" style="width: 100px" />
          </div>
        </el-col>
        <el-col :span="6">
          <div class="info-item">
            <label>{{ t('common.status') }}</label>
            <el-select v-model="form.status" style="width: 120px">
              <el-option :label="t('pmTrace.statusDraft')" :value="0" />
              <el-option :label="t('pmTrace.statusRunning')" :value="1" />
              <el-option :label="t('pmTrace.statusCompleted')" :value="2" />
            </el-select>
          </div>
        </el-col>
      </el-row>
    </el-card>

    <!-- 计划与实际表格 -->
    <el-card class="table-card">
      <template #header>
        <div class="card-header">
          <span>{{ t('pmTrace.planActual') }}</span>
          <el-button type="primary" size="small" @click="addStep">
            <el-icon><Plus /></el-icon>
            {{ t('pmTrace.addStep') }}
          </el-button>
        </div>
      </template>

      <el-table :data="form.steps" border stripe>
        <el-table-column type="index" width="50" align="center" />
        <el-table-column :label="t('pmTrace.process')" width="120">
          <template #default="{ row, $index }">
            <el-input v-model="row.processName" size="small" />
          </template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.person')" width="100">
          <template #default="{ row }">
            <el-input v-model="row.personInCharge" size="small" />
          </template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.planDays')" width="90" align="center">
          <template #default="{ row }">
            <el-input-number v-model="row.planDurationDays" :min="1" size="small" controls-position="right" style="width: 70px" />
          </template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.planStart')" width="140">
          <template #default="{ row }">
            <el-date-picker v-model="row.planStartDate" type="date" value-format="YYYY-MM-DD" size="small" style="width: 130px" />
          </template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.planEnd')" width="140">
          <template #default="{ row }">
            <span>{{ calcPlanEnd(row) || '-' }}</span>
          </template>
        </el-table-column>
        <!-- 实际 -->
        <el-table-column :label="t('pmTrace.actualStart')" width="140">
          <template #default="{ row }">
            <el-date-picker v-model="row.actualStartDate" type="date" value-format="YYYY-MM-DD" size="small" style="width: 130px" />
          </template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.actualDays')" width="90" align="center">
          <template #default="{ row }">
            <el-input-number v-model="row.actualDurationDays" :min="1" size="small" controls-position="right" style="width: 70px" />
          </template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.actualEnd')" width="140">
          <template #default="{ row }">
            <el-date-picker v-model="row.actualEndDate" type="date" value-format="YYYY-MM-DD" size="small" style="width: 130px" />
          </template>
        </el-table-column>
        <el-table-column width="60" align="center">
          <template #default="{ $index }">
            <el-button link type="danger" @click="removeStep($index)">
              <el-icon><Delete /></el-icon>
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 甘特图 -->
    <el-card class="gantt-card" v-if="form.steps.length > 0 && ganttRange">
      <template #header>
        <span>{{ t('pmTrace.ganttChart') }}</span>
      </template>
      <div class="gantt-wrapper">
        <div class="gantt-header">
          <div class="gantt-process-col">{{ t('pmTrace.process') }}</div>
          <div class="gantt-timeline">
            <div class="gantt-weeks">
              <div v-for="w in ganttRange.weeks" :key="w" class="gantt-week" :style="{ width: ganttRange.dayWidth * 7 + 'px' }">
                W{{ w }}
              </div>
            </div>
            <div class="gantt-days">
              <div v-for="d in ganttRange.totalDays" :key="d" class="gantt-day" :style="{ width: ganttRange.dayWidth + 'px' }">
                {{ d % 7 === 1 ? ((d-1)/7+1) : '' }}
              </div>
            </div>
          </div>
        </div>
        <div class="gantt-body">
          <div v-for="(step, i) in form.steps" :key="i" class="gantt-row">
            <div class="gantt-process-col">{{ step.processName }}</div>
            <div class="gantt-timeline">
              <div class="gantt-bars">
                <!-- 计划条 -->
                <div v-if="step.planStartDate && step.planDurationDays"
                  class="gantt-bar plan"
                  :style="calcGanttStyle(step.planStartDate, step.planDurationDays)"
                />
                <!-- 实际条 -->
                <div v-if="step.actualStartDate && step.actualDurationDays"
                  class="gantt-bar actual"
                  :style="calcGanttStyle(step.actualStartDate, step.actualDurationDays)"
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </el-card>

    <!-- 工艺路线汇总 -->
    <el-card class="table-card">
      <template #header>
        <span>{{ t('pmTrace.routeSummary') }}</span>
      </template>
      <el-table :data="form.steps" border stripe>
        <el-table-column type="index" width="50" align="center" />
        <el-table-column :label="t('pmTrace.process')" width="120">
          <template #default="{ row }">{{ row.processName }}</template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.person')" width="100">
          <template #default="{ row }">
            <el-input v-model="row.personInCharge" size="small" />
          </template>
        </el-table-column>
        <el-table-column :label="t('pmTrace.cycleTime')" width="100" align="center">
          <template #default="{ row }">
            <el-input-number v-model="row.cycleTime" :min="0" size="small" controls-position="right" style="width: 80px" />
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
        <el-table-column :label="t('common.remark')">
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
import type { PmProjectTrace, PmProjectTraceStep } from '@/api/pmProjectTrace'

const route = useRoute()
const router = useRouter()
const { t } = useI18n()

const isEdit = computed(() => !!route.params.id)
const traceId = computed(() => Number(route.params.id))

const form = ref<PmProjectTrace>({
  customerId: 0,
  customerName: '',
  productCode: '',
  status: 0,
  steps: [],
})

function calcPlanEnd(step: PmProjectTraceStep) {
  if (step.planStartDate && step.planDurationDays) {
    const d = new Date(step.planStartDate)
    d.setDate(d.getDate() + step.planDurationDays)
    return d.toLocaleDateString()
  }
  return null
}

// 甘特图计算
const ganttRange = computed(() => {
  const steps = form.value.steps
  if (!steps.length) return null

  const dates = steps
    .flatMap(s => [s.planStartDate, s.actualStartDate])
    .filter(Boolean)
    .map(d => new Date(d!))

  if (!dates.length) return null

  const minDate = new Date(Math.min(...dates.map(d => d.getTime())))
  const maxDate = new Date(Math.max(...dates.map(d => d.getTime())))
  // 给最大日期加一些余量
  maxDate.setDate(maxDate.getDate() + 14)

  const dayMs = 86400000
  const totalDays = Math.ceil((maxDate.getTime() - minDate.getTime()) / dayMs) + 1
  const weeks = Math.ceil(totalDays / 7)
  const containerWidth = 800
  const dayWidth = Math.max(20, Math.floor(containerWidth / totalDays))

  return { minDate, maxDate, totalDays, weeks, dayWidth }
})

function calcGanttStyle(startDateStr: string, durationDays: number) {
  const range = ganttRange.value
  if (!range) return {}

  const start = new Date(startDateStr)
  const offsetDays = Math.floor((start.getTime() - range.minDate.getTime()) / 86400000)
  const widthDays = durationDays

  return {
    left: offsetDays * range.dayWidth + 'px',
    width: widthDays * range.dayWidth + 'px',
  }
}

function addStep() {
  form.value.steps.push({
    stepOrder: form.value.steps.length + 1,
    processName: '',
  })
}

function removeStep(index: number) {
  form.value.steps.splice(index, 1)
  // 重新排序
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
  } catch (e) {
    console.error(e)
  }
}

onMounted(loadDetail)
</script>

<style scoped>
.page-container {
  padding: 20px;
}
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}
.page-header h2 {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
}
.info-card {
  margin-bottom: 16px;
}
.info-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.info-item label {
  font-size: 12px;
  color: var(--el-text-color-secondary);
}
.info-item span {
  font-size: 14px;
  font-weight: 500;
}
.table-card {
  margin-bottom: 16px;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

/* 甘特图 */
.gantt-card {
  margin-bottom: 16px;
  overflow-x: auto;
}
.gantt-wrapper {
  min-width: 900px;
}
.gantt-header, .gantt-row {
  display: flex;
  border-bottom: 1px solid #e4e7ed;
}
.gantt-process-col {
  width: 120px;
  padding: 8px 12px;
  font-size: 13px;
  font-weight: 500;
  border-right: 1px solid #e4e7ed;
  flex-shrink: 0;
}
.gantt-timeline {
  flex: 1;
  position: relative;
}
.gantt-weeks {
  display: flex;
  border-bottom: 1px solid #e4e7ed;
}
.gantt-week {
  text-align: center;
  font-size: 11px;
  color: #909399;
  padding: 4px 0;
  border-right: 1px solid #e4e7ed;
}
.gantt-days {
  display: flex;
}
.gantt-day {
  text-align: center;
  font-size: 10px;
  color: #c0c4cc;
  padding: 2px 0;
  border-right: 1px solid #f0f0f0;
}
.gantt-body .gantt-row {
  height: 32px;
  align-items: center;
}
.gantt-bars {
  position: relative;
  height: 100%;
}
.gantt-bar {
  position: absolute;
  top: 6px;
  height: 18px;
  border-radius: 3px;
  opacity: 0.85;
}
.gantt-bar.plan {
  background: #409eff;
}
.gantt-bar.actual {
  background: #67c23a;
  top: 10px;
}
</style>
