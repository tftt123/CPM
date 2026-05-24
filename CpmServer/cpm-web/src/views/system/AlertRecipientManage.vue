<template>
  <div>
    <div class="section-header">
      <el-button type="primary" size="small" :icon="Plus" @click="handleAdd">
        {{ t('common.create') }}
      </el-button>
    </div>
    <el-table :data="tableData" v-loading="loading" stripe border size="small">
      <el-table-column type="index" label="#" width="50" align="center" />
      <el-table-column prop="alertType" :label="t('alertRecipient.alertType')" min-width="160" />
      <el-table-column prop="recipientType" :label="t('alertRecipient.recipientType')" width="100" align="center">
        <template #default="{ row }">
          <el-tag :type="row.recipientType === 'EMAIL' ? 'primary' : 'warning'" size="small">
            {{ row.recipientType === 'EMAIL' ? t('alertRecipient.typeEmail') : t('alertRecipient.typeRole') }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="recipientValue" :label="t('alertRecipient.recipientValue')" min-width="180" />
      <el-table-column prop="isActive" :label="t('common.status')" width="80" align="center">
        <template #default="{ row }">
          <el-tag :type="row.isActive ? 'success' : 'info'" size="small">
            {{ row.isActive ? t('common.active') : t('common.inactive') }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column :label="t('common.action')" width="120" align="center" fixed="right">
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

    <el-dialog
      :title="isEdit ? t('common.edit') : t('common.create')"
      v-model="dialogVisible"
      width="500px"
      :close-on-click-modal="false"
    >
      <el-form :model="form" label-width="100px" ref="formRef">
        <el-form-item :label="t('alertRecipient.alertType')">
          <el-select v-model="form.alertType" style="width: 100%">
            <el-option :label="t('alertRecipient.cycleTimeExceeded')" value="CYCLE_TIME_EXCEEDED" />
          </el-select>
        </el-form-item>
        <el-form-item :label="t('alertRecipient.recipientType')">
          <el-select v-model="form.recipientType" style="width: 100%">
            <el-option :label="t('alertRecipient.typeEmail')" value="EMAIL" />
            <el-option :label="t('alertRecipient.typeRole')" value="ROLE" />
          </el-select>
        </el-form-item>
        <el-form-item :label="t('alertRecipient.recipientValue')">
          <el-input v-model="form.recipientValue" :placeholder="form.recipientType === 'EMAIL' ? 'email@example.com' : 'ROLE_NAME'" />
        </el-form-item>
        <el-form-item :label="t('common.status')">
          <el-switch v-model="form.isActive" />
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
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useI18n } from '@/composables/useI18n'
import { alertRecipientApi, type AlertRecipient } from '@/api/alertRecipient'
import { Plus, Edit, Delete } from '@element-plus/icons-vue'

const { t } = useI18n()
const loading = ref(false)
const tableData = ref<AlertRecipient[]>([])
const dialogVisible = ref(false)
const submitting = ref(false)
const editId = ref<number | null>(null)

const isEdit = computed(() => !!editId.value)

const form = ref({
  alertType: 'CYCLE_TIME_EXCEEDED',
  recipientType: 'EMAIL',
  recipientValue: '',
  isActive: true
})

const loadData = async () => {
  loading.value = true
  try {
    const res = await alertRecipientApi.getList()
    tableData.value = res.data
  } catch (e) {
    console.error(e)
    ElMessage.error(t('common.failed'))
  } finally {
    loading.value = false
  }
}

const handleAdd = () => {
  editId.value = null
  form.value = {
    alertType: 'CYCLE_TIME_EXCEEDED',
    recipientType: 'EMAIL',
    recipientValue: '',
    isActive: true
  }
  dialogVisible.value = true
}

const handleEdit = (row: AlertRecipient) => {
  editId.value = row.id
  form.value = {
    alertType: row.alertType,
    recipientType: row.recipientType,
    recipientValue: row.recipientValue,
    isActive: row.isActive
  }
  dialogVisible.value = true
}

const handleDelete = async (row: AlertRecipient) => {
  try {
    await ElMessageBox.confirm(t('message.deleteConfirm'), t('common.tip'), { type: 'warning' })
    await alertRecipientApi.delete(row.id)
    ElMessage.success(t('message.deleteSuccess'))
    loadData()
  } catch (e: any) {
    if (e !== 'cancel') {
      console.error(e)
      ElMessage.error(t('common.failed'))
    }
  }
}

const handleSubmit = async () => {
  if (!form.value.recipientValue.trim()) {
    ElMessage.warning(t('validation.required', { field: t('alertRecipient.recipientValue') }))
    return
  }

  submitting.value = true
  try {
    if (isEdit.value && editId.value) {
      await alertRecipientApi.update(editId.value, form.value)
      ElMessage.success(t('message.updateSuccess'))
    } else {
      await alertRecipientApi.create(form.value)
      ElMessage.success(t('message.createSuccess'))
    }
    dialogVisible.value = false
    loadData()
  } catch (e) {
    console.error(e)
    ElMessage.error(t('common.failed'))
  } finally {
    submitting.value = false
  }
}

onMounted(loadData)
</script>

<style scoped>
.section-header {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 12px;
}
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>
