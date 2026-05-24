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
        <el-form-item :label="t('product.productCode')" prop="productCode" v-if="!isEdit && isVisible('productCode')">
          <el-input
            v-model="form.productCode"
            :placeholder="t('common.pleaseInput')"
            :prefix-icon="Document"
          />
        </el-form-item>

        <el-form-item :label="t('product.productName')" prop="productName" v-if="isVisible('productName')">
          <el-input
            v-model="form.productName"
            :placeholder="t('common.pleaseInput')"
            :prefix-icon="Box"
          />
        </el-form-item>

        <el-form-item :label="t('product.material')" v-if="isVisible('material')">
          <el-input
            v-model="form.material"
            placeholder="SUS304 / AL6061"
            :prefix-icon="Collection"
          />
        </el-form-item>

        <el-form-item :label="t('product.surfaceTreatment')" v-if="isVisible('surfaceTreatment')">
          <el-input
            v-model="form.surfaceTreatment"
            placeholder="Polishing / Anodizing"
            :prefix-icon="Brush"
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
import { productApi } from '@/api/product'
import { useI18n } from '@/composables/useI18n'
import { useFieldControl } from '@/composables/useFieldControl'
import { Document, Box, Collection, Brush } from '@element-plus/icons-vue'

const { isVisible } = useFieldControl('Product', 'ProductForm')

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
const form = ref({ productCode: '', productName: '', material: '', surfaceTreatment: '' })

const rules = {
  productCode: [{ required: true, message: t('validation.required', { field: t('product.productCode') }), trigger: 'blur' }],
  productName: [{ required: true, message: t('validation.required', { field: t('product.productName') }), trigger: 'blur' }]
}

watch(() => props.data, (val) => {
  if (val) {
    form.value = { ...val }
  } else {
    form.value = { productCode: '', productName: '', material: '', surfaceTreatment: '' }
  }
}, { immediate: true })

const handleSubmit = async () => {
  await formRef.value.validate()
  submitting.value = true
  try {
    if (isEdit.value) {
      await productApi.update(props.data.id, form.value)
      ElMessage.success(t('message.updateSuccess'))
    } else {
      await productApi.add(form.value)
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
