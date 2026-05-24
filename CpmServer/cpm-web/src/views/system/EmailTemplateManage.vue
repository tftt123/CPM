<template>
  <div>
    <div class="section-header">
      <el-button type="primary" size="small" :icon="Refresh" @click="loadData">
        {{ t('common.refresh') }}
      </el-button>
    </div>
    <el-table :data="tableData" v-loading="loading" stripe border size="small">
      <el-table-column type="index" label="#" width="50" align="center" />
      <el-table-column prop="templateCode" :label="t('emailTemplate.code')" min-width="160" />
      <el-table-column prop="templateName" :label="t('emailTemplate.name')" min-width="160" />
      <el-table-column prop="subject" :label="t('emailTemplate.subject')" min-width="200" show-overflow-tooltip />
      <el-table-column prop="isActive" :label="t('common.status')" width="80" align="center">
        <template #default="{ row }">
          <el-tag :type="row.isActive ? 'success' : 'info'" size="small">
            {{ row.isActive ? t('common.active') : t('common.inactive') }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column :label="t('common.action')" width="100" align="center" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" size="small" :icon="Edit" @click="handleEdit(row)">
            {{ t('common.edit') }}
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-dialog :title="t('common.edit')" v-model="dialogVisible" width="600px" :close-on-click-modal="false">
      <el-form :model="form" label-width="80px" ref="formRef">
        <el-form-item :label="t('emailTemplate.code')">
          <el-input v-model="form.templateCode" disabled />
        </el-form-item>
        <el-form-item :label="t('emailTemplate.name')">
          <el-input v-model="form.templateName" />
        </el-form-item>
        <el-form-item :label="t('emailTemplate.subject')">
          <el-input v-model="form.subject" />
        </el-form-item>
        <el-form-item :label="t('emailTemplate.body')">
          <el-input v-model="form.body" type="textarea" :rows="8" />
        </el-form-item>
        <el-form-item :label="t('common.status')">
          <el-switch v-model="form.isActive" />
        </el-form-item>
      </el-form>
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
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useI18n } from '@/composables/useI18n'
import { emailApi, type EmailTemplate } from '@/api/email'
import { Edit, Refresh } from '@element-plus/icons-vue'

const { t } = useI18n()
const loading = ref(false)
const tableData = ref<EmailTemplate[]>([])
const dialogVisible = ref(false)
const submitting = ref(false)
const formRef = ref()

const form = ref<Partial<EmailTemplate>>({
  id: 0,
  templateCode: '',
  templateName: '',
  subject: '',
  body: '',
  isActive: true
})

const loadData = async () => {
  loading.value = true
  try {
    const res = await emailApi.getTemplates()
    tableData.value = res.data
  } catch (e) {
    console.error(e)
    ElMessage.error(t('common.failed'))
  } finally {
    loading.value = false
  }
}

const handleEdit = (row: EmailTemplate) => {
  form.value = { ...row }
  dialogVisible.value = true
}

const handleSubmit = async () => {
  if (!form.value.id) return
  submitting.value = true
  try {
    await emailApi.updateTemplate(form.value.id, {
      templateName: form.value.templateName,
      subject: form.value.subject,
      body: form.value.body,
      isActive: form.value.isActive
    })
    ElMessage.success(t('message.updateSuccess'))
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
