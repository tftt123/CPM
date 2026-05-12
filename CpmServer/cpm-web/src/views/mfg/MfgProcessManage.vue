<template>
  <div class="page-container">
    <div class="page-header-section">
      <el-page-header @back="$router.push('/home')">
        <template #content>
          <div class="page-header-content">
            <h1 class="page-title">{{ t('mfg.title') }}</h1>
            <span class="page-subtitle">{{ t('mfg.subtitle') }}</span>
          </div>
        </template>
      </el-page-header>
    </div>

    <div class="content-card" style="padding: 0;">
      <!-- Toolbar -->
      <div class="table-toolbar">
        <div class="toolbar-left">
          <el-input
            v-model="keyword"
            :placeholder="t('common.search')"
            style="width: 260px;"
            clearable
            @keyup.enter="loadData"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
          <el-button style="margin-left: 12px;" @click="loadData">{{ t('common.search') }}</el-button>
        </div>
        <el-button type="primary" :icon="Plus" @click="handleAdd">
          {{ t('common.add') }}
        </el-button>
      </div>

      <!-- Flat Table -->
      <el-table :data="records" v-loading="loading" stripe>
        <el-table-column type="index" label="#" width="50" align="center" />
        <el-table-column prop="processName" :label="t('mfg.process')" width="160" />
        <el-table-column prop="subCategoryName" :label="t('mfg.subCategory')" width="160" />
        <el-table-column prop="equipmentName" :label="t('mfg.equipment')" min-width="160" />
        <el-table-column prop="owner" :label="t('mfg.owner')" width="120" />
        <el-table-column :label="t('mfg.hourlyRate')" width="120" align="right">
          <template #default="{ row }">
            <span v-if="row.costRate">{{ row.costRate }}</span>
            <span v-else class="text-muted">—</span>
          </template>
        </el-table-column>
        <el-table-column :label="t('common.status')" width="90" align="center">
          <template #default="{ row }">
            <el-tag size="small" :type="row.isActive ? 'success' : 'info'">
              {{ row.isActive ? t('common.active') : t('common.inactive') }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column :label="t('common.action')" width="140" align="center" fixed="right">
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

    <!-- Form Dialog -->
    <el-dialog :title="dialogTitle" v-model="dialogVisible" width="600px" :close-on-click-modal="false">
      <div class="dialog-body">
        <el-form :model="form" label-width="120px" :rules="rules" ref="formRef" class="slds-form">
          <!-- Cascade: Process -->
          <el-form-item :label="t('mfg.process')" prop="processIdOrName">
            <el-select
              ref="processSelectRef"
              v-model="form.processIdOrName"
              :placeholder="t('common.pleaseSelect')"
              filterable
              allow-create
              style="width: 100%"
              @change="onProcessChange"
              @blur="onProcessBlur"
            >
              <el-option
                v-for="p in processOptions"
                :key="p.id"
                :label="p.label"
                :value="p.id"
              />
            </el-select>
          </el-form-item>

          <!-- Cascade: SubCategory -->
          <el-form-item :label="t('mfg.subCategory')" prop="subCategoryIdOrName">
            <el-select
              ref="subCategorySelectRef"
              v-model="form.subCategoryIdOrName"
              :placeholder="t('common.pleaseSelect')"
              filterable
              allow-create
              style="width: 100%"
              @blur="onSubCategoryBlur"
            >
              <el-option
                v-for="s in subCategoryOptions"
                :key="s.id"
                :label="s.label"
                :value="s.id"
              />
            </el-select>
          </el-form-item>

          <!-- Equipment Name -->
          <el-form-item :label="t('mfg.equipment')" prop="equipmentName">
            <el-input v-model="form.equipmentName" :placeholder="t('common.pleaseInput')" />
          </el-form-item>

          <el-divider />

          <el-form-item :label="t('mfg.hourlyRate')">
            <el-input-number
              v-model="form.costRate"
              :min="0"
              :precision="2"
              :controls="true"
              style="width: 100%"
              :placeholder="t('common.pleaseInput')"
            />
          </el-form-item>

          <el-form-item :label="t('mfg.owner')">
            <el-input v-model="form.owner" :placeholder="t('common.pleaseInput')" />
          </el-form-item>

          <el-form-item :label="t('mfg.description')">
            <el-input v-model="form.description" type="textarea" :rows="2" :placeholder="t('common.pleaseInput')" />
          </el-form-item>

          <el-form-item :label="t('common.status')">
            <el-switch v-model="form.isActive" :active-text="t('common.active')" :inactive-text="t('common.inactive')" />
          </el-form-item>
        </el-form>
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogVisible = false">{{ t('common.cancel') }}</el-button>
          <el-button type="primary" :loading="submitting" @click="handleSubmit">{{ t('common.save') }}</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, nextTick, reactive } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { mfgProcessApi, type MfgProcessRecord, type MfgCascadeOption } from '@/api/mfgProcess'
import { useI18n } from '@/composables/useI18n'
import { Plus, Edit, Delete, Search } from '@element-plus/icons-vue'

const { t } = useI18n()
const loading = ref(false)
const dialogVisible = ref(false)
const submitting = ref(false)
const records = ref<MfgProcessRecord[]>([])
const processOptions = ref<MfgCascadeOption[]>([])
const subCategoryOptions = ref<MfgCascadeOption[]>([])
const formRef = ref()
const editId = ref<number | null>(null)
const keyword = ref('')
const processSelectRef = ref<any>(null)
const subCategorySelectRef = ref<any>(null)

const selectInputCache = reactive({
  process: '',
  subCategory: ''
})

const dialogTitle = computed(() => editId.value ? t('common.edit') : t('common.create'))

const defaultForm = () => ({
  processIdOrName: undefined as number | string | undefined,
  subCategoryIdOrName: undefined as number | string | undefined,
  equipmentName: '' as string,
  owner: '' as string,
  description: '' as string,
  costRate: undefined as number | undefined,
  isActive: true as boolean
})

const form = ref(defaultForm())

const rules = {
  processIdOrName: [{ required: true, message: t('validation.selectRequired', { field: t('mfg.process') }), trigger: 'change' }],
  subCategoryIdOrName: [{ required: true, message: t('validation.selectRequired', { field: t('mfg.subCategory') }), trigger: 'change' }],
  equipmentName: [{ required: true, message: t('validation.required', { field: t('mfg.equipment') }), trigger: 'blur' }]
}

const loadData = async () => {
  loading.value = true
  try {
    const res = await mfgProcessApi.getFlatRecords({
      keyword: keyword.value || undefined
    })
    records.value = res.data || []
  } finally {
    loading.value = false
  }
}

const onProcessChange = async (val: number | string) => {
  selectInputCache.process = ''
  form.value.subCategoryIdOrName = undefined
  subCategoryOptions.value = []
  if (!val) return

  // If existing process selected (number), load subcategories
  if (typeof val === 'number') {
    const res = await mfgProcessApi.getSubCategoryOptions(val)
    subCategoryOptions.value = res.data || []
  }
}

const bindSelectInputListeners = () => {
  nextTick(() => {
    const pInput = processSelectRef.value?.$el?.querySelector('input')
    if (pInput && !(pInput as any)._bound) {
      pInput.addEventListener('input', (e: Event) => {
        selectInputCache.process = (e.target as HTMLInputElement).value
      })
      ;(pInput as any)._bound = true
    }
    const sInput = subCategorySelectRef.value?.$el?.querySelector('input')
    if (sInput && !(sInput as any)._bound) {
      sInput.addEventListener('input', (e: Event) => {
        selectInputCache.subCategory = (e.target as HTMLInputElement).value
      })
      ;(sInput as any)._bound = true
    }
  })
}

const onProcessBlur = () => {
  const val = selectInputCache.process.trim()
  selectInputCache.process = ''
  if (!val) return
  setTimeout(() => {
    const matched = processOptions.value.find(p => p.label === val)
    if (matched) {
      form.value.processIdOrName = matched.id
      onProcessChange(matched.id)
    } else {
      form.value.processIdOrName = val
      subCategoryOptions.value = []
    }
  }, 100)
}

const onSubCategoryBlur = () => {
  const val = selectInputCache.subCategory.trim()
  selectInputCache.subCategory = ''
  if (!val) return
  setTimeout(() => {
    const matched = subCategoryOptions.value.find(s => s.label === val)
    form.value.subCategoryIdOrName = matched ? matched.id : val
  }, 100)
}

const handleAdd = () => {
  editId.value = null
  form.value = defaultForm()
  processOptions.value = []
  subCategoryOptions.value = []
  loadAllProcesses()
  dialogVisible.value = true
  bindSelectInputListeners()
}

const loadAllProcesses = async () => {
  const res = await mfgProcessApi.getProcessOptions()
  processOptions.value = res.data || []
}

const handleEdit = (row: MfgProcessRecord) => {
  editId.value = row.equipmentId
  form.value = {
    processIdOrName: row.processId,
    subCategoryIdOrName: row.subCategoryId,
    equipmentName: row.equipmentName,
    owner: row.owner || '',
    description: row.description || '',
    costRate: row.costRate,
    isActive: row.isActive
  }
  // Load cascade options
  Promise.all([
    mfgProcessApi.getProcessOptions(),
    mfgProcessApi.getSubCategoryOptions(row.processId)
  ]).then(([pRes, sRes]) => {
    processOptions.value = pRes.data || []
    subCategoryOptions.value = sRes.data || []
  })
  dialogVisible.value = true
  bindSelectInputListeners()
}

const handleDelete = async (row: MfgProcessRecord) => {
  try {
    await ElMessageBox.confirm(t('message.deleteConfirm'), t('common.tip'), { type: 'warning' })
    await mfgProcessApi.deleteEquipment(row.equipmentId)
    ElMessage.success(t('message.deleteSuccess'))
    loadData()
  } catch {
    // user cancelled
  }
}

const handleSubmit = async () => {
  await formRef.value.validate()
  submitting.value = true
  try {
    // Resolve process: create if new
    let finalProcessId: number
    if (typeof form.value.processIdOrName === 'string') {
      const res = await mfgProcessApi.createProcess({
        category: '通用',
        processName: form.value.processIdOrName.trim(),
        sortOrder: 0
      })
      finalProcessId = res.data
    } else {
      finalProcessId = form.value.processIdOrName!
    }

    // Resolve subcategory: create if new
    let finalSubCategoryId: number
    if (typeof form.value.subCategoryIdOrName === 'string') {
      const res = await mfgProcessApi.createSubCategory({
        processId: finalProcessId,
        subCategoryName: form.value.subCategoryIdOrName.trim(),
        sortOrder: 0,
        equipments: []
      })
      finalSubCategoryId = res.data
    } else {
      finalSubCategoryId = form.value.subCategoryIdOrName!
    }

    const data = {
      subCategoryId: finalSubCategoryId,
      equipmentCode: undefined as string | undefined,
      equipmentName: form.value.equipmentName.trim(),
      owner: form.value.owner || undefined,
      description: form.value.description || undefined,
      costRate: form.value.costRate
    }
    if (editId.value) {
      await mfgProcessApi.updateEquipment(editId.value, data)
    } else {
      await mfgProcessApi.createEquipment(data)
    }
    dialogVisible.value = false
    ElMessage.success(editId.value ? t('message.updateSuccess') : t('message.createSuccess'))
    loadData()
  } finally {
    submitting.value = false
  }
}

onMounted(loadData)
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
.table-toolbar {
  padding: 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.toolbar-left {
  display: flex;
  align-items: center;
}
.text-muted {
  color: #ccc;
}
.dialog-body {
  padding: 24px 8px;
}
.slds-form :deep(.el-form-item__label) {
  font-size: 13px;
  font-weight: 600;
  color: var(--slds-text-secondary);
}
.slds-form :deep(.el-input__wrapper),
.slds-form :deep(.el-select .el-input__wrapper),
.slds-form :deep(.el-input-number .el-input__wrapper) {
  min-height: 40px !important;
  height: 40px !important;
}
.slds-form :deep(.el-input__inner) {
  height: 38px !important;
  line-height: 38px !important;
}
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>
