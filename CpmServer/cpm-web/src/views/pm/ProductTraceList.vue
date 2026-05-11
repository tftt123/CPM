<template>
  <div class="page-container">
    <div class="page-header">
      <h2>{{ t('pmTrace.pageTitle') }}</h2>
      <el-button type="primary" @click="handleCreate">
        <el-icon><Plus /></el-icon>
        {{ t('common.new') }}
      </el-button>
    </div>

    <el-card class="search-card">
      <el-form :inline="true" :model="searchForm">
        <el-form-item :label="t('common.keyword')">
          <el-input v-model="searchForm.keyword" :placeholder="t('pmTrace.searchPlaceholder')" clearable />
        </el-form-item>
        <el-form-item :label="t('common.status')">
          <el-select v-model="searchForm.status" clearable style="width: 120px">
            <el-option :label="t('pmTrace.statusDraft')" :value="0" />
            <el-option :label="t('pmTrace.statusRunning')" :value="1" />
            <el-option :label="t('pmTrace.statusCompleted')" :value="2" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="loadData">
            <el-icon><Search /></el-icon>
            {{ t('common.search') }}
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-table :data="tableData" v-loading="loading" stripe>
      <el-table-column prop="customerName" :label="t('pmTrace.customer')" min-width="140" />
      <el-table-column prop="productCode" :label="t('pmTrace.partNo')" min-width="140" />
      <el-table-column prop="productName" :label="t('pmTrace.productName')" min-width="160" />
      <el-table-column prop="plannedQty" :label="t('pmTrace.plannedQty')" width="100" align="center" />
      <el-table-column prop="projectStartDate" :label="t('pmTrace.startDate')" width="120">
        <template #default="{ row }">
          {{ formatDate(row.projectStartDate) }}
        </template>
      </el-table-column>
      <el-table-column prop="status" :label="t('common.status')" width="100">
        <template #default="{ row }">
          <el-tag :type="statusType(row.status)">{{ statusLabel(row.status) }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column :label="t('common.operation')" width="180" fixed="right">
        <template #default="{ row }">
          <el-button link type="primary" @click="handleEdit(row)">
            {{ t('common.edit') }}
          </el-button>
          <el-button link type="danger" @click="handleDelete(row)">
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
import { Plus, Search } from '@element-plus/icons-vue'
import { useI18n } from '@/composables/useI18n'
import { getTraceList, deleteTrace } from '@/api/pmProjectTrace'
import type { PmProjectTrace } from '@/api/pmProjectTrace'

const router = useRouter()
const { t } = useI18n()

const loading = ref(false)
const tableData = ref<PmProjectTrace[]>([])
const searchForm = reactive({
  keyword: '',
  status: undefined as number | undefined,
})
const pagination = reactive({
  page: 1,
  pageSize: 20,
  total: 0,
})

function formatDate(date?: string) {
  if (!date) return '-'
  return new Date(date).toLocaleDateString()
}

function statusType(status: number) {
  switch (status) {
    case 0: return 'info'
    case 1: return 'warning'
    case 2: return 'success'
    default: return 'info'
  }
}

function statusLabel(status: number) {
  switch (status) {
    case 0: return t('pmTrace.statusDraft')
    case 1: return t('pmTrace.statusRunning')
    case 2: return t('pmTrace.statusCompleted')
    default: return '-'
  }
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

onMounted(loadData)
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
