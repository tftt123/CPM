<template>
  <div class="page-container">
    <div class="page-header">
      <h2>{{ t('pmTrace.pageTitle') }}</h2>
      <el-button v-if="userStore.hasPermission('pm.manage')" type="primary" @click="handleCreate">
        <el-icon><Plus /></el-icon>
        {{ t('common.new') }}
      </el-button>
    </div>

    <el-card class="search-card">
      <el-form :inline="true" :model="searchForm">
        <el-form-item>
          <el-input v-model="searchForm.keyword" :placeholder="t('pmTrace.searchPlaceholder')" clearable />
        </el-form-item>
        <el-form-item :label="t('common.status')">
          <GcSelect
            v-model="searchForm.status"
            domain="PM_TRACE_STATUS"
            clearable
            style="width: 120px"
            :fallback="statusFallback"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadData">
            <el-icon><Search /></el-icon>
            {{ t('common.search') }}
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-table border :data="tableData" v-loading="loading" stripe>
      <el-table-column v-if="isVisible('customerName')" prop="customerName" :label="t('pmTrace.customer')" min-width="140" />
      <el-table-column v-if="isVisible('productCode')" prop="productCode" :label="t('pmTrace.partNo')" min-width="140" />
      <el-table-column v-if="isVisible('productName')" prop="productName" :label="t('pmTrace.productName')" min-width="160" />
      <el-table-column v-if="isVisible('plannedQty')" prop="plannedQty" :label="t('pmTrace.plannedQty')" min-width="100" align="center" />
      <el-table-column v-if="isVisible('projectStartDate')" prop="projectStartDate" :label="t('pmTrace.startDate')" min-width="120">
        <template #default="{ row }">
          {{ formatDate(row.projectStartDate) }}
        </template>
      </el-table-column>
      <el-table-column v-if="isVisible('status')" prop="status" :label="t('common.status')" min-width="100">
        <template #default="{ row }">
          <el-tag :type="statusType(row.status)">{{ statusLabel(row.status) }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column :label="t('common.action')" min-width="180" fixed="right">
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

    <el-pagination
      v-model:current-page="pagination.page"
      v-model:page-size="pagination.pageSize"
      :total="pagination.total"
      layout="total, sizes, prev, pager, next"
      @current-change="loadData"
      @size-change="loadData"
      class="pagination"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, Edit, Delete } from '@element-plus/icons-vue'
import { useI18n } from '@/composables/useI18n'
import { useFieldControl } from '@/composables/useFieldControl'
import { useUserStore } from '@/stores/user'
import { useGeneralizedCode, type GcOption } from '@/composables/useGeneralizedCode'
import GcSelect from '@/components/GcSelect.vue'
import { getTraceList, deleteTrace } from '@/api/pmProjectTrace'
import type { PmProjectTrace } from '@/api/pmProjectTrace'

const router = useRouter()
const { t } = useI18n()
const userStore = useUserStore()
const { isVisible } = useFieldControl('PM', 'ProductTraceList')
const { getOptions } = useGeneralizedCode()

const loading = ref(false)
const tableData = ref<PmProjectTrace[]>([])
const searchForm = reactive({
  keyword: '',
  status: undefined as number | undefined,
})
const statusOptions = ref<GcOption[]>([])

const statusFallback = [
  { code: '0', label: t('pmTrace.statusDraft'), tagType: 'info' },
  { code: '1', label: t('pmTrace.statusRunning'), tagType: 'warning' },
  { code: '2', label: t('pmTrace.statusCompleted'), tagType: 'success' }
]
const pagination = reactive({
  page: 1,
  pageSize: 20,
  total: 0,
})

function formatDate(date?: string) {
  if (!date) return '-'
  return new Date(date).toLocaleDateString()
}

const getStatusOption = (status: number) => statusOptions.value.find(o => o.value === String(status))

function statusType(status: number) {
  return getStatusOption(status)?.tagType || 'info'
}

function statusLabel(status: number) {
  return getStatusOption(status)?.label || '-'
}

async function loadData() {
  loading.value = true
  try {
    const res = await getTraceList({
      keyword: searchForm.keyword || undefined,
      status: searchForm.status,
      page: pagination.page,
      pageSize: pagination.pageSize,
    })
    tableData.value = res.data.list
    pagination.total = res.data.total
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

function handleCreate() {
  router.push('/pm/trace/create')
}

function handleEdit(row: PmProjectTrace) {
  router.push(`/pm/trace/edit/${row.id}`)
}

async function handleDelete(row: PmProjectTrace) {
  try {
    await ElMessageBox.confirm(t('common.confirmDelete'), t('common.tip'), { type: 'warning' })
    await deleteTrace(row.id!)
    ElMessage.success(t('common.deleteSuccess'))
    loadData()
  } catch {
    // cancelled
  }
}

onMounted(async () => {
  statusOptions.value = await getOptions('PM_TRACE_STATUS', statusFallback)
  loadData()
})
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
.search-card {
  margin-bottom: 16px;
}
.pagination {
  margin-top: 16px;
  justify-content: flex-end;
}
</style>
