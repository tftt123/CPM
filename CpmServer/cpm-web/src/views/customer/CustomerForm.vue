<template>
  <el-dialog
    :title="isEdit ? t('common.edit') : t('common.create')"
    v-model="visible"
    width="520px"
    :close-on-click-modal="false"
    class="slds-dialog"
    @closed="handleClose"
  >
    <div class="dialog-body">
      <el-form
        :model="form"
        label-width="100px"
        :rules="rules"
        ref="formRef"
        class="slds-form"
      >
        <el-form-item :label="t('customer.customerCode')" prop="customerCode" v-if="!isEdit && isVisible('customerCode')">
          <el-input
            v-model="form.customerCode"
            :placeholder="t('common.pleaseInput')"
            :prefix-icon="Document"
          />
        </el-form-item>

        <el-form-item :label="t('customer.customerName')" prop="customerName" v-if="isVisible('customerName')">
          <el-input
            v-model="form.customerName"
            :placeholder="t('common.pleaseInput')"
            :prefix-icon="OfficeBuilding"
          />
        </el-form-item>

        <el-form-item :label="t('customer.industry')" v-if="isVisible('industry')">
          <GcSelect
            v-model="form.industry"
            domain="INDUSTRY"
            :placeholder="t('common.pleaseSelect')"
            clearable
            filterable
            style="width: 100%"
            :fallback="[
              { code: 'AUTOMOTIVE', label: '汽车', tagType: 'info' },
              { code: 'Data Storage', label: '数据存储', tagType: 'info' },
              { code: 'Domestic Appliances', label: '家用电器', tagType: 'info' },
              { code: 'HealthCare', label: '医疗保健', tagType: 'info' },
              { code: 'Imaging and Printing', label: '影像与印刷', tagType: 'info' },
              { code: 'Leisure', label: '休闲', tagType: 'info' },
              { code: 'Machinery', label: '机械', tagType: 'info' },
              { code: 'Others', label: '其他', tagType: 'info' }
            ]"
          />
        </el-form-item>

        <el-form-item :label="t('customer.currency')" v-if="isVisible('currency')">
          <el-input
            v-model="form.currency"
            :placeholder="t('common.pleaseInput')"
          />
        </el-form-item>

        <el-form-item :label="t('customer.contactName')" v-if="isVisible('contactName')">
          <el-input
            v-model="form.contactName"
            :placeholder="t('common.pleaseInput')"
            :prefix-icon="User"
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
import { ref, watch, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { customerApi } from '@/api/customer'
import { useI18n } from '@/composables/useI18n'
import { useFieldControl } from '@/composables/useFieldControl'
import GcSelect from '@/components/GcSelect.vue'
import { Document, OfficeBuilding, User } from '@element-plus/icons-vue'

const { isVisible } = useFieldControl('Customer', 'CustomerForm')

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
const form = ref({ customerCode: '', customerName: '', industry: '', currency: '', contactName: '' })

const rules = {
  customerCode: [{ required: true, message: t('validation.required', { field: t('customer.customerCode') }), trigger: 'blur' }],
  customerName: [{ required: true, message: t('validation.required', { field: t('customer.customerName') }), trigger: 'blur' }]
}

watch(() => props.data, (val) => {
  if (val) {
    form.value = { ...val }
  } else {
    form.value = { customerCode: '', customerName: '', industry: '', currency: '', contactName: '' }
  }
}, { immediate: true })

const handleSubmit = async () => {
  await formRef.value.validate()
  submitting.value = true
  try {
    if (isEdit.value) {
      await customerApi.update(props.data.id, form.value)
      ElMessage.success(t('message.updateSuccess'))
    } else {
      await customerApi.add(form.value)
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

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>
