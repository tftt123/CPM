<template>
  <el-dialog
    :title="isEdit ? t('common.edit') : t('common.create')"
    v-model="visible"
    width="560px"
    :close-on-click-modal="false"
    class="slds-dialog"
    @closed="handleClose"
  >
    <div class="dialog-body">
      <el-form
        :model="form"
        label-width="110px"
        :rules="rules"
        ref="formRef"
        class="slds-form"
      >
        <el-form-item :label="t('opportunity.title')" prop="title" v-if="isVisible('title')">
          <el-input
            v-model="form.title"
            :placeholder="t('common.pleaseInput')"
            :prefix-icon="Document"
          />
        </el-form-item>

        <el-form-item :label="t('quotation.customer')" prop="customerId" v-if="isVisible('customerId')">
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

        <el-form-item :label="t('opportunity.expectedAmount')" v-if="isVisible('expectedAmount')">
          <el-input-number
            v-model="form.expectedAmount"
            :min="0"
            :precision="2"
            controls-position="right"
            style="width: 100%"
            :placeholder="t('common.pleaseInput')"
          />
        </el-form-item>

        <el-form-item :label="t('opportunity.quoteDeadline')" v-if="isVisible('quoteDeadline')">
          <el-date-picker
            v-model="form.quoteDeadline"
            type="date"
            :placeholder="t('common.pleaseSelect')"
            style="width: 100%"
            value-format="YYYY-MM-DD"
          />
        </el-form-item>
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
import { opportunityApi } from '@/api/quotation'
import { customerApi } from '@/api/customer'
import { useI18n } from '@/composables/useI18n'
import { useFieldControl } from '@/composables/useFieldControl'
import { Document } from '@element-plus/icons-vue'

const { isVisible } = useFieldControl('Opportunity', 'OpportunityForm')

const props = defineProps<{ visible: boolean; data: any }>()
const emit = defineEmits(['update:visible', 'success'])
const { t } = useI18n()

const visible = computed({
  get: () => props.visible,
  set: (val) => emit('update:visible', val)
})

const isEdit = computed(() => !!props.data?.id)
const formRef = ref()
const submitting = ref(false)
const customerList = ref<any[]>([])
const form = ref({
  title: '',
  customerId: null as number | null,
  expectedAmount: undefined as number | undefined,
  quoteDeadline: ''
})

const rules = {
  title: [{ required: true, message: t('validation.required', { field: t('opportunity.title') }), trigger: 'blur' }],
  customerId: [{ required: true, message: t('validation.selectRequired', { field: t('quotation.customer') }), trigger: 'change' }]
}

watch(() => props.data, (val) => {
  if (val) {
    form.value = {
      title: val.title || '',
      customerId: val.customerId || null,
      expectedAmount: val.expectedAmount,
      quoteDeadline: val.quoteDeadline || ''
    }
  } else {
    form.value = { title: '', customerId: null, expectedAmount: undefined, quoteDeadline: '' }
  }
}, { immediate: true })

const loadCustomers = async () => {
  const res = await customerApi.list({ pageNum: 1, pageSize: 999 })
  customerList.value = res.data.list
}

const handleSubmit = async () => {
  await formRef.value.validate()
  submitting.value = true
  try {
    if (isEdit.value) {
      await opportunityApi.update(props.data.id, form.value)
      ElMessage.success(t('message.updateSuccess'))
    } else {
      await opportunityApi.create(form.value)
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
  submitting.value = false
}

onMounted(loadCustomers)
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
  height: 32px !important;
  min-height: 32px !important;
  padding: 1px 11px !important;
}

.slds-form :deep(.el-input__inner) {
  height: 30px !important;
  line-height: 30px !important;
}

.slds-form :deep(.el-select__wrapper) {
  height: 32px !important;
  min-height: 32px !important;
  padding: 1px 11px !important;
}

.slds-form :deep(.el-select__placeholder) {
  line-height: 30px !important;
}

.slds-form :deep(.el-input-number .el-input__wrapper) {
  height: 32px !important;
  min-height: 32px !important;
  padding: 1px 11px !important;
}

.slds-form :deep(.el-input-number .el-input__inner) {
  height: 30px !important;
  line-height: 30px !important;
}

.slds-form :deep(.el-date-editor.el-input__wrapper) {
  height: 32px !important;
  min-height: 32px !important;
  padding: 1px 11px !important;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>
