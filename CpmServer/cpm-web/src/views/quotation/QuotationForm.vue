<template>
  <el-dialog
    :title="isEdit ? t('common.edit') : t('common.create')"
    v-model="visible"
    :width="isMaximized ? '95vw' : '1080px'"
    :close-on-click-modal="false"
    :class="['slds-dialog', isMaximized ? 'maximized-dialog' : '']"
    @closed="handleClose"
  >
    <template #header="{ close }"
    >
      <div class="dialog-header">
        <span class="dialog-title">{{ isEdit ? t('common.edit') : t('common.create') }}</span>
        <div class="dialog-actions">
          <el-button
            link
            :icon="isMaximized ? CopyDocument : FullScreen"
            @click="isMaximized = !isMaximized"
          >
            {{ isMaximized ? t('quotation.restore') : t('quotation.maximize') }}
          </el-button>
          <el-button link :icon="Close" @click="close" />
        </div>
      </div>
    </template>
    <div class="dialog-body">
      <el-form
        :model="form"
        label-width="100px"
        :rules="rules"
        ref="formRef"
        class="slds-form"
      >
        <el-form-item :label="t('opportunity.title')" prop="opportunityId">
          <el-select
            v-model="form.opportunityId"
            :placeholder="t('common.pleaseSelect')"
            filterable
            style="width: 100%"
            :disabled="!!props.opportunity"
          >
            <el-option
              v-for="o in opportunityList"
              :key="o.id"
              :label="`${o.opportunityNo} - ${o.title}`"
              :value="o.id"
            />
          </el-select>
        </el-form-item>

        <el-form-item :label="t('quotation.customer')" prop="customerId">
          <el-select
            v-model="form.customerId"
            :placeholder="t('common.pleaseSelect')"
            filterable
            style="width: 100%"
          >
            <el-option
              v-for="c in customerList"
              :key="c.id"
              :label="c.customerName"
              :value="c.id"
            />
          </el-select>
        </el-form-item>

        <!-- Quotation Items -->
        <div class="items-section">
          <div class="items-header">
            <span class="items-title">{{ t('quotation.items') }}</span>
            <el-button type="primary" size="small" :icon="Plus" @click="addProduct">{{ t('quotation.addProduct') }}</el-button>
          </div>

          <el-table :data="form.items" size="small" style="width: 100%">
            <el-table-column :label="t('quotation.productName')" min-width="160">
              <template #default="{ row, $index }">
                <el-select
                  v-if="!row.isProcessRow"
                  v-model="row.productId"
                  :placeholder="t('common.pleaseSelect')"
                  filterable
                  size="small"
                  @change="(val: number) => onProductChange(val, $index)"
                >
                  <el-option
                    v-for="p in productList"
                    :key="p.id"
                    :label="p.productName"
                    :value="p.id"
                  />
                </el-select>
                <span v-else></span>
              </template>
            </el-table-column>
            <el-table-column :label="t('quotation.qty')" min-width="80">
              <template #default="{ row, $index }">
                <el-input
                  v-if="!row.isProcessRow"
                  v-model="row.qty"
                  size="small"
                  style="width: 100%"
                  @blur="() => onQtyChange($index)"
                />
                <span v-else></span>
              </template>
            </el-table-column>
            <el-table-column :label="t('quotation.processType')" width="180">
              <template #default="{ row, $index }">
                <el-select
                  v-model="row.processId"
                  size="small"
                  filterable
                  :placeholder="t('common.pleaseSelect')"
                  style="width: 100%"
                  @change="(val: number) => onProcessSelect(val, $index)"
                >
                  <el-option
                    v-for="p in processOptions"
                    :key="p.id"
                    :label="p.label"
                    :value="p.id"
                  />
                </el-select>
              </template>
            </el-table-column>
            <el-table-column :label="t('quotation.equipmentType')" width="180">
              <template #default="{ row, $index }">
                <el-select
                  v-model="row.subCategoryId"
                  size="small"
                  filterable
                  :placeholder="t('common.pleaseSelect')"
                  style="width: 100%"
                  :disabled="!row.processId"
                  @change="(val: number) => onSubCategorySelect(val, $index)"
                >
                  <el-option
                    v-for="s in row.subCategoryOptions || []"
                    :key="s.id"
                    :label="s.label"
                    :value="s.id"
                  />
                </el-select>
              </template>
            </el-table-column>
            <el-table-column :label="t('quotation.equipment')" width="195">
              <template #default="{ row, $index }">
                <el-select
                  v-model="row.equipmentId"
                  size="small"
                  filterable
                  :placeholder="t('common.pleaseSelect')"
                  style="width: 100%"
                  :disabled="!row.subCategoryId"
                  @change="(val: number) => onEquipmentSelect(val, $index)"
                >
                  <el-option
                    v-for="e in row.equipmentOptions || []"
                    :key="e.id"
                    :label="e.label"
                    :value="e.id"
                  />
                </el-select>
              </template>
            </el-table-column>
            <el-table-column :label="t('quotation.cycleTime')" min-width="80">
              <template #default="{ row, $index }">
                <el-input-number
                  v-model="row.cycleTime"
                  :min="0"
                  :precision="2"
                  size="small"
                  style="width: 100%"
                  controls-position="right"
                  @change="() => calcCost($index)"
                />
              </template>
            </el-table-column>
            <el-table-column :label="t('quotation.hourlyRate')" min-width="90">
              <template #default="{ row, $index }">
                <el-input-number
                  v-model="row.hourlyRate"
                  :min="0"
                  :precision="2"
                  size="small"
                  style="width: 100%"
                  controls-position="right"
                  @change="() => calcCost($index)"
                />
              </template>
            </el-table-column>
            <el-table-column :label="t('quotation.processingFee')" min-width="90">
              <template #default="{ row }">
                <span class="line-cost">{{ formatCurrency(row.cost || 0) }}</span>
              </template>
            </el-table-column>
            <el-table-column :label="t('quotation.lineAmount')" min-width="90">
              <template #default="{ row }">
                <span class="line-amount">{{ formatCurrency(Number(row.lineAmount) || 0) }}</span>
              </template>
            </el-table-column>
            <el-table-column :label="t('common.action')" min-width="80" align="center">
              <template #default="{ row, $index }">
                <el-button
                  v-if="!row.isProcessRow && row.productId"
                  link
                  type="primary"
                  size="small"
                  :icon="Plus"
                  :title="t('quotation.addProcess')"
                  @click="addProcessRow($index)"
                />
                <el-button link type="danger" size="small" :icon="Delete" @click="removeItem($index)" />
              </template>
            </el-table-column>
          </el-table>

          <div class="extra-costs-row">
            <div class="extra-cost-item">
              <span class="extra-cost-label">{{ t('quotation.packaging') }}:</span>
              <el-input
                v-model="form.packagingCost"
                size="small"
                style="width: 100px"
                @blur="() => { form.packagingCost = Number(form.packagingCost) || 0 }"
              />
            </div>
            <div class="extra-cost-item">
              <span class="extra-cost-label">{{ t('quotation.transport') }}:</span>
              <el-input
                v-model="form.transportCost"
                size="small"
                style="width: 100px"
                @blur="() => { form.transportCost = Number(form.transportCost) || 0 }"
              />
            </div>
          </div>
          <div class="total-row">
            <span>{{ t('quotation.totalAmount') }}:</span>
            <span class="total-amount">{{ formatCurrency(totalAmount) }}</span>
          </div>
        </div>

        <!-- Attachments -->
        <div class="attachments-section">
          <div class="attachments-label">
            <el-icon size="16"><Paperclip /></el-icon>
            <span>{{ t('quotation.attachments') }}</span>
          </div>
          <el-upload
            ref="uploadRef"
            :http-request="customUpload"
            :on-remove="handleUploadRemove"
            :file-list="fileList"
            multiple
            :limit="10"
            :auto-upload="true"
          >
            <el-button type="primary" size="small" :icon="Paperclip">{{ t('quotation.uploadAttachment') }}</el-button>
            <template #tip>
              <div class="upload-tip">{{ t('quotation.uploadTip') }}</div>
            </template>
          </el-upload>
        </div>
      </el-form>
    </div>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="visible = false">{{ t('common.cancel') }}</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmit">
          {{ isEdit ? t('common.save') : t('common.create') }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, watch, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { quotationApi, opportunityApi, fileApi } from '@/api/quotation'
import { customerApi } from '@/api/customer'
import { productApi } from '@/api/product'
import { mfgProcessApi } from '@/api/mfgProcess'
import { useI18n } from '@/composables/useI18n'
import { Plus, Delete, Close, FullScreen, CopyDocument, Paperclip } from '@element-plus/icons-vue'
import request from '@/api/request'

interface MfgOption { id: number; label: string }
interface EquipmentOption extends MfgOption { costRate?: number }

const props = defineProps<{ visible: boolean; data?: any; opportunity?: any }>()
const emit = defineEmits(['update:visible', 'success'])
const { t } = useI18n()

const visible = computed({
  get: () => props.visible,
  set: (val) => emit('update:visible', val)
})

const isEdit = computed(() => !!props.data?.id)
const isMaximized = ref(false)
const formRef = ref()
const submitting = ref(false)
const opportunityList = ref<any[]>([])
const customerList = ref<any[]>([])
const productList = ref<any[]>([])
const processOptions = ref<MfgOption[]>([])
const uploadRef = ref()
const fileList = ref<any[]>([])

const formatCurrency = (value: number) => {
  return new Intl.NumberFormat('zh-CN', { style: 'currency', currency: 'CNY' }).format(value)
}

const customUpload = async (options: any) => {
  const { file, onProgress, onSuccess, onError } = options
  const formData = new FormData()
  formData.append('file', file)
  formData.append('moduleType', 'quotation')
  formData.append('businessId', String(props.data?.id || 0))

  try {
    const res: any = await request.post('/fileupload/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (progressEvent: any) => {
        const percent = Math.round((progressEvent.loaded * 100) / (progressEvent.total || 1))
        onProgress({ percent })
      }
    })
    if (res.code === 200 && res.data) {
      const uploaded = {
        name: res.data.originalName || file.name,
        fileId: res.data.id,
        url: res.data.filePath,
        status: 'success',
        uid: Date.now() + Math.random()
      }
      fileList.value.push(uploaded)
      onSuccess(res)
      ElMessage.success(t('quotation.uploadSuccess'))
    } else {
      onError(new Error(res.message || 'Upload failed'))
      ElMessage.error(res.message || t('quotation.uploadFailed'))
    }
  } catch (err: any) {
    onError(err)
    ElMessage.error(err?.message || t('quotation.uploadFailed'))
  }
}

const handleUploadRemove = async (uploadFile: any) => {
  const fileId = uploadFile.fileId
  if (fileId) {
    try {
      await fileApi.deleteFile(fileId)
    } catch (e) {
      // ignore delete errors
    }
  }
  const idx = fileList.value.findIndex((f: any) => f.fileId === fileId || f.uid === uploadFile.uid)
  if (idx > -1) fileList.value.splice(idx, 1)
}

const loadProcessOptions = async () => {
  const res = await mfgProcessApi.getProcessOptions()
  processOptions.value = res.data || []
}

const onProcessSelect = async (processId: number, index: number) => {
  const item = form.value.items[index]
  const selected = processOptions.value.find(p => p.id === processId)
  item.processType = selected?.label || ''
  item.subCategoryId = null
  item.equipmentId = null
  item.equipmentType = ''
  item.equipment = ''
  item.hourlyRate = 0
  item.subCategoryOptions = []
  item.equipmentOptions = []
  if (!processId) return
  const res = await mfgProcessApi.getSubCategoryOptions(processId)
  item.subCategoryOptions = res.data || []
}

const onSubCategorySelect = async (subCategoryId: number, index: number) => {
  const item = form.value.items[index]
  const selected = item.subCategoryOptions?.find((s: MfgOption) => s.id === subCategoryId)
  item.equipmentType = selected?.label || ''
  item.equipmentId = null
  item.equipment = ''
  item.hourlyRate = 0
  item.equipmentOptions = []
  if (!subCategoryId) return
  const res = await mfgProcessApi.getEquipments(subCategoryId)
  item.equipmentOptions = (res.data || []).map((e: any) => ({
    id: e.id,
    label: e.equipmentName,
    costRate: e.costRate
  }))
}

const onEquipmentSelect = (equipmentId: number, index: number) => {
  const item = form.value.items[index]
  const selected = item.equipmentOptions?.find((e: EquipmentOption) => e.id === equipmentId)
  item.equipment = selected?.label || ''
  if (selected?.costRate != null) {
    item.hourlyRate = selected.costRate
  }
  calcCost(index)
}

const form = ref({
  opportunityId: null as number | null,
  customerId: null as number | null,
  packagingCost: 0 as number,
  transportCost: 0 as number,
  items: [] as any[]
})

const rules = {
  opportunityId: [{ required: true, message: t('validation.selectRequired', { field: t('opportunity.title') }), trigger: 'change' }],
  customerId: [{ required: true, message: t('validation.selectRequired', { field: t('quotation.customer') }), trigger: 'change' }]
}

const totalAmount = computed(() => {
  const lineSum = form.value.items.reduce((sum, item) => sum + (item.lineAmount || 0), 0)
  return lineSum + (form.value.packagingCost || 0) + (form.value.transportCost || 0)
})

watch(() => props.data, async (val) => {
  if (val) {
    await loadProcessOptions()
    const items = []
    const seenProducts = new Set<number>()
    for (const i of val.items || []) {
      const isProcess = i.productId != null && seenProducts.has(i.productId)
      if (i.productId != null && !isProcess) {
        seenProducts.add(i.productId)
      }
      const item: any = {
        isProcessRow: isProcess,
        productId: i.productId,
        productName: i.productName,
        qty: i.qty,
        lineAmount: i.lineAmount,
        processType: i.processType,
        equipmentType: i.equipmentType,
        equipment: i.equipment,
        cycleTime: i.cycleTime,
        hourlyRate: i.hourlyRate,
        cost: i.cost,
        subCategoryOptions: [],
        equipmentOptions: []
      }
      const proc = processOptions.value.find(p => p.label === i.processType)
      if (proc) {
        item.processId = proc.id
        const sRes = await mfgProcessApi.getSubCategoryOptions(proc.id)
        item.subCategoryOptions = sRes.data || []
        const sub = item.subCategoryOptions.find((s: MfgOption) => s.label === i.equipmentType)
        if (sub) {
          item.subCategoryId = sub.id
          const eRes = await mfgProcessApi.getEquipments(sub.id)
          item.equipmentOptions = (eRes.data || []).map((e: any) => ({ id: e.id, label: e.equipmentName, costRate: e.costRate }))
          const eq = item.equipmentOptions.find((e: EquipmentOption) => e.label === i.equipment)
          if (eq) item.equipmentId = eq.id
        }
      }
      items.push(item)
    }
    form.value = {
      opportunityId: val.opportunityId || null,
      customerId: val.customerId || null,
      packagingCost: val.packagingCost ?? 0,
      transportCost: val.transportCost ?? 0,
      items
    }
    // Load existing attachments
    if (val.id) {
      fileApi.getFiles('quotation', val.id).then((res: any) => {
        if (res.code === 200) {
          fileList.value = (res.data || []).map((f: any, idx: number) => ({
            name: f.originalName,
            fileId: f.id,
            url: f.filePath,
            status: 'success',
            uid: Date.now() + idx
          }))
        }
      }).catch(() => {
        fileList.value = []
      })
    }
  } else {
    form.value = {
      opportunityId: props.opportunity?.id || null,
      customerId: props.opportunity?.customerId || null,
      packagingCost: 0,
      transportCost: 0,
      items: []
    }
    fileList.value = []
  }
}, { immediate: true })

const loadData = async () => {
  const [oRes, cRes, pRes] = await Promise.all([
    opportunityApi.list({ pageNum: 1, pageSize: 999 }),
    customerApi.list({ pageNum: 1, pageSize: 999 }),
    productApi.list({ pageNum: 1, pageSize: 999 })
  ])
  opportunityList.value = oRes.data.list
  customerList.value = cRes.data.list
  productList.value = pRes.data.list
  await loadProcessOptions()
}

const addProduct = () => {
  form.value.items.push({
    isProcessRow: false,
    productId: null, productName: '', qty: 1, lineAmount: 0,
    processId: null, processType: '', subCategoryId: null, equipmentType: '',
    equipmentId: null, equipment: '', cycleTime: 0, hourlyRate: 0, cost: 0,
    subCategoryOptions: [], equipmentOptions: []
  })
}

const addProcessRow = (index: number) => {
  let productIndex = index
  while (productIndex >= 0 && form.value.items[productIndex].isProcessRow) {
    productIndex--
  }
  if (productIndex < 0 || form.value.items[productIndex].productId == null) {
    ElMessage.warning(t('quotation.productRequired'))
    return
  }
  const productRow = form.value.items[productIndex]

  let insertIndex = productIndex + 1
  while (insertIndex < form.value.items.length && form.value.items[insertIndex].isProcessRow) {
    insertIndex++
  }

  form.value.items.splice(insertIndex, 0, {
    isProcessRow: true,
    productId: productRow.productId,
    productName: productRow.productName || '',
    qty: productRow.qty || 1,
    lineAmount: 0,
    processId: null, processType: '', subCategoryId: null, equipmentType: '',
    equipmentId: null, equipment: '', cycleTime: 0, hourlyRate: 0, cost: 0,
    subCategoryOptions: [], equipmentOptions: []
  })
}

const removeItem = (index: number) => {
  const item = form.value.items[index]
  if (!item.isProcessRow) {
    let endIndex = index + 1
    while (endIndex < form.value.items.length && form.value.items[endIndex].isProcessRow) {
      endIndex++
    }
    form.value.items.splice(index, endIndex - index)
  } else {
    form.value.items.splice(index, 1)
  }
}

const onProductChange = (productId: number, index: number) => {
  const product = productList.value.find(p => p.id === productId)
  const row = form.value.items[index]
  if (product) {
    row.productName = product.productName
  }
  // Sync product info to all following process rows
  let i = index + 1
  while (i < form.value.items.length && form.value.items[i].isProcessRow) {
    form.value.items[i].productId = productId
    form.value.items[i].productName = row.productName
    i++
  }
}

const onQtyChange = (index: number) => {
  const row = form.value.items[index]
  row.qty = Number(row.qty) || 0
  calcAmount(index)
  // Sync qty to all following process rows
  let i = index + 1
  while (i < form.value.items.length && form.value.items[i].isProcessRow) {
    form.value.items[i].qty = row.qty
    calcAmount(i)
    i++
  }
}

const calcCost = (index: number) => {
  const item = form.value.items[index]
  item.cost = (item.cycleTime || 0) / 3600 * (item.hourlyRate || 0)
  calcAmount(index)
}

const calcAmount = (index: number) => {
  const item = form.value.items[index]
  item.lineAmount = (item.cost || 0) * (item.qty || 1)
}

const handleSubmit = async () => {
  await formRef.value.validate()
  if (form.value.items.length === 0) {
    ElMessage.warning(t('common.noData'))
    return
  }
  // Resolve inherited productIds for process rows and validate
  const resolvedItems: any[] = []
  for (let idx = 0; idx < form.value.items.length; idx++) {
    const i = form.value.items[idx]
    const rowNum = idx + 1
    if (!i.isProcessRow) {
      if (!i.productId) {
        ElMessage.warning(`${t('quotation.no')} ${rowNum} ${t('quotation.row')}: ${t('quotation.productRequired')}`)
        return
      }
    } else {
      let parentFound = false
      for (let j = idx - 1; j >= 0; j--) {
        if (!form.value.items[j].isProcessRow && form.value.items[j].productId) {
          i.productId = form.value.items[j].productId
          i.qty = form.value.items[j].qty || 1
          parentFound = true
          break
        }
      }
      if (!parentFound) {
        ElMessage.warning(`${t('quotation.no')} ${rowNum} ${t('quotation.row')}: ${t('quotation.productRequired')}`)
        return
      }
    }
    if (!i.qty || i.qty < 1) {
      ElMessage.warning(`${t('quotation.no')} ${rowNum} ${t('quotation.row')}: ${t('quotation.qtyRequired')}`)
      return
    }
    if (!i.processId) {
      ElMessage.warning(`${t('quotation.no')} ${rowNum} ${t('quotation.row')}: ${t('quotation.processTypeRequired')}`)
      return
    }
    if (!i.subCategoryId) {
      ElMessage.warning(`${t('quotation.no')} ${rowNum} ${t('quotation.row')}: ${t('quotation.equipmentTypeRequired')}`)
      return
    }
    if (!i.equipmentId) {
      ElMessage.warning(`${t('quotation.no')} ${rowNum} ${t('quotation.row')}: ${t('quotation.equipmentRequired')}`)
      return
    }
    if (!i.cycleTime || i.cycleTime <= 0) {
      ElMessage.warning(`${t('quotation.no')} ${rowNum} ${t('quotation.row')}: ${t('quotation.cycleTimeRequired')}`)
      return
    }
    if (!i.hourlyRate || i.hourlyRate <= 0) {
      ElMessage.warning(`${t('quotation.no')} ${rowNum} ${t('quotation.row')}: ${t('quotation.hourlyRateRequired')}`)
      return
    }
    resolvedItems.push(i)
  }
  submitting.value = true
  try {
    const fileIds = fileList.value
      .map((f: any) => f.fileId || f.response?.data?.id)
      .filter((id: any) => id != null)
    const data = {
      opportunityId: form.value.opportunityId,
      customerId: form.value.customerId,
      packagingCost: form.value.packagingCost,
      transportCost: form.value.transportCost,
      fileIds,
      items: resolvedItems.map((i: any) => ({
        productId: i.productId,
        qty: i.qty || 1,
        lineAmount: i.lineAmount,
        processType: i.processType,
        equipmentType: i.equipmentType,
        equipment: i.equipment,
        cycleTime: i.cycleTime,
        hourlyRate: i.hourlyRate,
        cost: i.cost,
        isProcessRow: i.isProcessRow
      }))
    }
    if (isEdit.value) {
      await quotationApi.update(props.data.id, data)
      ElMessage.success(t('message.updateSuccess'))
    } else {
      await quotationApi.create(data)
      ElMessage.success(t('message.createSuccess'))
    }
    visible.value = false
    emit('success')
  } finally {
    submitting.value = false
  }
}

const handleClose = () => {
  formRef.value?.resetFields()
  form.value.items = []
  fileList.value = []
  submitting.value = false
}

onMounted(loadData)
</script>

<style scoped>
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

.dialog-body {
  padding: 24px 8px;
}

.slds-form :deep(.el-form-item__label) {
  font-size: 13px;
  font-weight: 600;
  color: var(--slds-text-secondary);
}

.slds-form :deep(.el-input__wrapper) {
  min-height: 40px;
}

/* Table input & select height unify to size=small standard height */
.items-section :deep(.el-input__wrapper) {
  min-height: 24px !important;
  height: 24px !important;
  padding: 0 8px !important;
}

.items-section :deep(.el-input__inner) {
  height: 22px !important;
  line-height: 22px !important;
}

.items-section :deep(.el-select .el-input__wrapper) {
  min-height: 24px !important;
  height: 24px !important;
  padding: 0 8px !important;
}

.items-section :deep(.el-select .el-input__inner) {
  height: 22px !important;
  line-height: 22px !important;
}

.items-section {
  margin-top: 16px;
  padding: 16px;
  background: #FAFBFC;
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
}

.items-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.items-title {
  font-size: 14px;
  font-weight: 600;
  color: var(--slds-text-primary);
}

.line-amount {
  font-weight: 600;
  color: var(--slds-text-primary);
}

.line-cost {
  font-weight: 600;
  color: var(--slds-brand-primary);
}

.hourly-rate {
  font-weight: 600;
  color: var(--slds-text-secondary);
  font-size: 13px;
}

.extra-costs-row {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 24px;
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid var(--slds-border-color-light);
}

.extra-cost-item {
  display: flex;
  align-items: center;
  gap: 8px;
}

.extra-cost-label {
  font-size: 14px;
  color: var(--slds-text-secondary);
}

.total-row {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  margin-top: 8px;
  font-size: 14px;
  color: var(--slds-text-secondary);
}

.total-amount {
  font-size: 20px;
  font-weight: 700;
  color: var(--slds-brand-primary);
  margin-left: 8px;
}

.cost-row {
  margin-top: 4px;
  padding-top: 4px;
  border-top: none;
}

.total-cost {
  font-size: 18px;
  font-weight: 700;
  color: var(--slds-error);
  margin-left: 8px;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.dialog-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.dialog-title {
  font-size: 18px;
  font-weight: 700;
  color: var(--slds-text-primary);
}

.dialog-actions {
  display: flex;
  align-items: center;
  gap: 4px;
}

:deep(.maximized-dialog) {
  margin-top: 2vh !important;
  height: 96vh;
}

:deep(.maximized-dialog .el-dialog__body) {
  height: calc(96vh - 120px);
  overflow-y: auto;
}

.attachments-section {
  margin-top: 16px;
  padding: 16px;
  background: #FAFBFC;
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
}

.attachments-label {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  font-weight: 600;
  color: var(--slds-text-primary);
  margin-bottom: 8px;
}

.upload-tip {
  font-size: 12px;
  color: var(--slds-text-secondary);
  margin-top: 4px;
}

.inherited-product {
  font-size: 13px;
  color: var(--slds-text-secondary);
  padding-left: 8px;
}
</style>
