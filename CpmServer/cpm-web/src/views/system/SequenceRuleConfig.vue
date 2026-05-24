<template>
  <div class="sequence-rule-config">
    <div class="toolbar">
      <el-button type="primary" :icon="Plus" @click="openDialog()">
        {{ t('sequenceRule.add') }}
      </el-button>
    </div>

    <el-table border :data="ruleList" v-loading="loading" stripe>
      <el-table-column type="index" label="#" width="50" align="center" />
      <el-table-column :label="t('sequenceRule.moduleType')" min-width="120">
        <template #default="{ row }">
          <el-tag size="small">{{ row.moduleType }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column :label="t('sequenceRule.moduleName')" min-width="140">
        <template #default="{ row }">
          {{ row.moduleName || '-' }}
        </template>
      </el-table-column>
      <el-table-column :label="t('sequenceRule.prefix')" width="100" align="center">
        <template #default="{ row }">
          <el-tag type="warning" size="small">{{ row.prefix }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column :label="t('sequenceRule.sequenceLength')" width="100" align="center">
        <template #default="{ row }">
          {{ row.sequenceLength }}
        </template>
      </el-table-column>
      <el-table-column :label="t('sequenceRule.currentSequence')" width="120" align="center">
        <template #default="{ row }">
          {{ row.currentSequence }}
        </template>
      </el-table-column>
      <el-table-column :label="t('sequenceRule.resetRule')" width="100" align="center">
        <template #default="{ row }">
          {{ getResetRuleLabel(row.resetRule) }}
        </template>
      </el-table-column>
      <el-table-column :label="t('sequenceRule.lastGeneratedNo')" min-width="160">
        <template #default="{ row }">
          <span v-if="row.lastGeneratedNo" class="last-no">{{ row.lastGeneratedNo }}</span>
          <span v-else class="no-data">-</span>
        </template>
      </el-table-column>
      <el-table-column :label="t('common.action')" width="200" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" :icon="Edit" @click="openDialog(row)">
            {{ t('common.edit') }}
          </el-button>
          <el-button link type="success" size="small" :icon="Document" @click="previewGenerate(row)">
            {{ t('sequenceRule.preview') }}
          </el-button>
          <el-button link type="danger" size="small" :icon="Delete" @click="handleDelete(row)">
            {{ t('common.delete') }}
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 新增/编辑弹窗 -->
    <el-dialog
      :title="isEdit ? t('common.edit') : t('sequenceRule.add')"
      v-model="dialogVisible"
      width="500px"
      :close-on-click-modal="false"
    >
      <el-form :model="form" label-width="120px" :rules="rules" ref="formRef">
        <el-form-item :label="t('sequenceRule.moduleType')" prop="moduleType">
          <el-select
            v-model="form.moduleType"
            :placeholder="t('sequenceRule.moduleTypePlaceholder')"
            :disabled="isEdit"
            style="width: 100%"
          >
            <el-option
              v-for="item in moduleTypeList"
              :key="item.moduleType"
              :label="item.moduleName || item.moduleType"
              :value="item.moduleType"
            />
          </el-select>
        </el-form-item>
        <el-form-item :label="t('sequenceRule.year')">
          <el-input :value="currentYear" disabled />
        </el-form-item>
        <el-form-item :label="t('sequenceRule.moduleName')">
          <el-input v-model="form.moduleName" :placeholder="t('sequenceRule.moduleNamePlaceholder')" />
        </el-form-item>
        <el-form-item :label="t('sequenceRule.prefix')" prop="prefix">
          <el-input v-model="form.prefix" :placeholder="t('sequenceRule.prefixPlaceholder')" maxlength="20" />
        </el-form-item>
        <el-form-item :label="t('sequenceRule.sequenceLength')">
          <el-input-number v-model="form.sequenceLength" :min="1" :max="10" />
        </el-form-item>
        <el-form-item :label="t('sequenceRule.resetRule')">
          <GcSelect
            v-model="form.resetRule"
            domain="SEQUENCE_RESET_RULE"
            style="width: 100%"
            :fallback="resetRuleFallback"
          />
        </el-form-item>
        <el-form-item>
          <div class="preview-box">
            <span class="preview-label">{{ t('sequenceRule.formatPreview') }}:</span>
            <span class="preview-value">{{ formatPreview }}</span>
          </div>
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogVisible = false">{{ t('common.cancel') }}</el-button>
          <el-button type="primary" :loading="submitting" @click="handleSubmit">
            {{ isEdit ? t('common.save') : t('common.create') }}
          </el-button>
        </div>
      </template>
    </el-dialog>

    <!-- 生成预览弹窗 -->
    <el-dialog
      :title="t('sequenceRule.previewTitle')"
      v-model="previewVisible"
      width="400px"
    >
      <div class="preview-result">
        <div class="preview-item">
          <span class="preview-label">{{ t('sequenceRule.moduleType') }}:</span>
          <span>{{ previewData.moduleType }}</span>
        </div>
        <div class="preview-item">
          <span class="preview-label">{{ t('sequenceRule.generatedNo') }}:</span>
          <span class="preview-no">{{ previewData.generatedNo }}</span>
        </div>
      </div>
      <template #footer>
        <el-button @click="previewVisible = false">{{ t('common.close') }}</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useI18n } from '@/composables/useI18n'
import { useGeneralizedCode, type GcOption } from '@/composables/useGeneralizedCode'
import GcSelect from '@/components/GcSelect.vue'
import {
  getSequenceRules,
  createSequenceRule,
  updateSequenceRule,
  deleteSequenceRule,
  generateSequence,
} from '@/api/sequenceRule'
import type { SequenceRule, SequenceRuleCreateDto } from '@/api/sequenceRule'
import { Plus, Edit, Delete, Document } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/user'
import { getSiteSettings } from '@/api/siteSettings'
import { getModuleTypeList } from '@/api/moduleTypeConfig'

const { t } = useI18n()
const userStore = useUserStore()
const { getOptions } = useGeneralizedCode()

const loading = ref(false)
const ruleList = ref<SequenceRule[]>([])
const dialogVisible = ref(false)
const submitting = ref(false)
const isEdit = ref(false)
const currentId = ref<number | null>(null)
const formRef = ref()
const siteCode = ref('')
const moduleTypeList = ref<{ moduleType: string; moduleName?: string }[]>([])
const resetRuleOptions = ref<GcOption[]>([])

const resetRuleFallback = [
  { code: '0', label: t('sequenceRule.resetNever'), tagType: 'info' },
  { code: '1', label: t('sequenceRule.resetDaily'), tagType: 'info' },
  { code: '2', label: t('sequenceRule.resetMonthly'), tagType: 'info' },
  { code: '3', label: t('sequenceRule.resetYearly'), tagType: 'info' }
]

const form = ref<SequenceRuleCreateDto>({
  moduleType: '',
  moduleName: '',
  prefix: '',
  sequenceLength: 5,
  resetRule: 0,
})

const previewVisible = ref(false)
const previewData = ref({ moduleType: '', generatedNo: '' })

const currentYear = computed(() => {
  return (new Date().getFullYear() % 100).toString().padStart(2, '0')
})

const formatPreview = computed(() => {
  const sc = siteCode.value || userStore.currentSite || 'SITE'
  const prefix = form.value.prefix || 'PREFIX'
  const year = currentYear.value
  const seq = '1'.padStart(form.value.sequenceLength || 5, '0')
  return `${sc}${prefix}${year}${seq}`
})

const rules = {
  moduleType: [{ required: true, message: t('validation.required'), trigger: 'blur' }],
  prefix: [{ required: true, message: t('validation.required'), trigger: 'blur' }],
}

const getResetRuleOption = (val: number) => resetRuleOptions.value.find(o => o.value === String(val))

function getResetRuleLabel(val: number) {
  return getResetRuleOption(val)?.label || '-'
}

async function loadData() {
  loading.value = true
  try {
    const res = await getSequenceRules()
    ruleList.value = res.data || []
  } catch (e) {
    console.error(e)
    ElMessage.error(t('common.failed'))
  } finally {
    loading.value = false
  }
}

function openDialog(row?: SequenceRule) {
  if (row) {
    isEdit.value = true
    currentId.value = row.id || null
    form.value = {
      moduleType: row.moduleType,
      moduleName: row.moduleName || '',
      prefix: row.prefix,
      sequenceLength: row.sequenceLength,
      resetRule: row.resetRule,
    }
  } else {
    isEdit.value = false
    currentId.value = null
    form.value = {
      moduleType: '',
      moduleName: '',
      prefix: '',
      sequenceLength: 5,
      resetRule: 0,
    }
  }
  dialogVisible.value = true
}

async function handleSubmit() {
  await formRef.value.validate()
  submitting.value = true
  try {
    if (isEdit.value && currentId.value) {
      await updateSequenceRule(currentId.value, form.value)
      ElMessage.success(t('message.updateSuccess'))
    } else {
      await createSequenceRule(form.value)
      ElMessage.success(t('message.createSuccess'))
    }
    dialogVisible.value = false
    loadData()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || t('common.failed'))
  } finally {
    submitting.value = false
  }
}

async function handleDelete(row: SequenceRule) {
  try {
    await ElMessageBox.confirm(
      t('common.deleteConfirm'),
      t('common.confirm'),
      { type: 'warning' }
    )
    await deleteSequenceRule(row.id!)
    ElMessage.success(t('message.deleteSuccess'))
    loadData()
  } catch {
    // cancelled
  }
}

async function previewGenerate(row: SequenceRule) {
  try {
    const res = await generateSequence(row.moduleType)
    previewData.value = {
      moduleType: row.moduleType,
      generatedNo: res.data,
    }
    previewVisible.value = true
    loadData()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || t('common.failed'))
  }
}

async function loadSiteCode() {
  try {
    const res = await getSiteSettings()
    if (res.data?.siteCode) {
      siteCode.value = res.data.siteCode
    }
  } catch {
    // ignore
  }
}

async function loadModuleTypes() {
  try {
    const res = await getModuleTypeList()
    moduleTypeList.value = res.data || []
  } catch {
    moduleTypeList.value = []
  }
}

onMounted(async () => {
  resetRuleOptions.value = await getOptions('SEQUENCE_RESET_RULE', resetRuleFallback)
  loadData()
  loadSiteCode()
  loadModuleTypes()
})
</script>

<style scoped>
.sequence-rule-config {
  padding: var(--cpm-space-4);
}

.toolbar {
  margin-bottom: var(--cpm-space-4);
}

.last-no {
  font-family: monospace;
  font-weight: 600;
  color: var(--cpm-brand-primary);
}

.no-data {
  color: var(--cpm-text-secondary);
}

.preview-box {
  padding: 12px 16px;
  background: #f5f7fa;
  border-radius: 4px;
  font-size: 13px;
}

.preview-label {
  color: var(--cpm-text-secondary);
  margin-right: 8px;
}

.preview-value {
  font-family: monospace;
  font-weight: 600;
  color: var(--cpm-brand-primary);
}

.preview-result {
  padding: 16px;
}

.preview-item {
  display: flex;
  align-items: center;
  margin-bottom: 12px;
}

.preview-item .preview-label {
  width: 100px;
  flex-shrink: 0;
}

.preview-no {
  font-family: monospace;
  font-size: 18px;
  font-weight: 700;
  color: var(--cpm-brand-primary);
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--cpm-space-3);
}
</style>
