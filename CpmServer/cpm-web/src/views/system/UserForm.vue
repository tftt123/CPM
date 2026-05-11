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
        label-width="100px"
        :rules="rules"
        ref="formRef"
        class="slds-form"
      >
        <el-form-item :label="t('common.user')" prop="username">
          <el-input
            v-model="form.username"
            :placeholder="t('common.pleaseInput')"
            :disabled="isEdit"
            :prefix-icon="User"
          />
        </el-form-item>

        <el-form-item :label="t('common.password')" prop="password" v-if="!isEdit">
          <el-input
            v-model="form.password"
            type="password"
            show-password
            :placeholder="t('common.pleaseInput')"
            :prefix-icon="Lock"
          />
        </el-form-item>

        <el-form-item :label="t('common.name')" prop="realName">
          <el-input
            v-model="form.realName"
            :placeholder="t('common.pleaseInput')"
            :prefix-icon="UserFilled"
          />
        </el-form-item>

        <el-form-item :label="t('common.email')">
          <el-input
            v-model="form.email"
            :placeholder="t('common.pleaseInput')"
            :prefix-icon="Message"
          />
        </el-form-item>

        <el-form-item :label="t('common.phone')">
          <el-input
            v-model="form.phone"
            :placeholder="t('common.pleaseInput')"
            :prefix-icon="Phone"
          />
        </el-form-item>

        <el-form-item :label="t('common.site')">
          <el-input
            v-model="form.site"
            :placeholder="t('common.pleaseInput')"
            :prefix-icon="OfficeBuilding"
          />
        </el-form-item>

        <el-form-item :label="t('common.role')">
          <el-select
            v-model="form.roleIds"
            :placeholder="t('common.pleaseSelect')"
            multiple
            style="width: 100%"
          >
            <el-option
              v-for="role in roleList"
              :key="role.id"
              :label="role.roleName"
              :value="role.id"
            />
          </el-select>
        </el-form-item>

        <el-form-item :label="t('common.status')">
          <el-switch
            v-model="form.isActive"
            :active-text="t('common.active')"
            :inactive-text="t('common.inactive')"
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
import { userApi } from '@/api/user'
import { roleApi } from '@/api/role'
import { useI18n } from '@/composables/useI18n'
import { User, Lock, UserFilled, Message, Phone, OfficeBuilding } from '@element-plus/icons-vue'

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
const roleList = ref<any[]>([])

const form = ref({
  username: '',
  password: '',
  realName: '',
  email: '',
  phone: '',
  site: '',
  roleIds: [] as number[],
  isActive: true
})

const rules = {
  username: [{ required: true, message: t('validation.required', { field: t('common.user') }), trigger: 'blur' }],
  password: [
    { required: true, message: t('validation.required', { field: t('common.password') }), trigger: 'blur' },
    { min: 6, message: t('validation.minLength', { field: t('common.password'), min: 6 }), trigger: 'blur' }
  ],
  realName: [{ required: true, message: t('validation.required', { field: t('common.name') }), trigger: 'blur' }]
}

watch(() => props.data, (val) => {
  if (val) {
    form.value = {
      username: val.username || '',
      password: '',
      realName: val.realName || '',
      email: val.email || '',
      phone: val.phone || '',
      site: val.site || '',
      roleIds: val.roleIds || [],
      isActive: val.isActive !== false
    }
  } else {
    form.value = { username: '', password: '', realName: '', email: '', phone: '', site: '', roleIds: [], isActive: true }
  }
}, { immediate: true })

const loadRoles = async () => {
  try {
    const res = await roleApi.list()
    roleList.value = res.data || []
  } catch {
    roleList.value = []
  }
}

const handleSubmit = async () => {
  await formRef.value.validate()
  submitting.value = true
  try {
    if (isEdit.value) {
      const updateData = {
        realName: form.value.realName,
        email: form.value.email,
        phone: form.value.phone,
        site: form.value.site,
        isActive: form.value.isActive,
        roleIds: form.value.roleIds
      }
      await userApi.update(props.data.id, updateData)
      ElMessage.success(t('message.updateSuccess'))
    } else {
      const createData = {
        username: form.value.username,
        password: form.value.password,
        realName: form.value.realName,
        email: form.value.email,
        phone: form.value.phone,
        site: form.value.site,
        isActive: form.value.isActive,
        roleIds: form.value.roleIds
      }
      await userApi.create(createData)
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

onMounted(loadRoles)
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

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>
