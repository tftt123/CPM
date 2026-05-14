<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header-section">
      <el-page-header @back="$router.push('/home')">
        <template #content>
          <div class="page-header-content">
            <h1 class="page-title">{{ t('product.pageTitle') }}</h1>
            <span class="page-subtitle">{{ t('product.productName') }}</span>
          </div>
        </template>
        <template #extra>
          <el-button type="primary" :icon="Plus" @click="handleAdd">
            {{ t('common.add') }}
          </el-button>
        </template>
      </el-page-header>
    </div>

    <!-- Search Area -->
    <div class="search-area">
      <el-form :model="query" inline class="search-form">
        <el-form-item :label="t('common.search')">
          <el-input
            v-model="query.keyword"
            :placeholder="t('common.pleaseInput')"
            clearable
            style="width: 320px"
            :prefix-icon="Search"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :icon="Search" @click="handleSearch">{{ t('common.search') }}</el-button>
          <el-button :icon="RefreshRight" @click="handleReset">{{ t('common.reset') }}</el-button>
        </el-form-item>
      </el-form>
    </div>

    <!-- Data Table -->
    <div class="content-card" style="padding: 0;">
      <el-table :data="tableData" v-loading="loading" stripe style="width: 100%">
        <el-table-column type="index" label="#" width="50" align="center" />
        <el-table-column prop="productCode" :label="t('product.productCode')" width="200">
          <template #default="{ row }">
            <span class="code-link">{{ row.productCode }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="productName" :label="t('product.productName')" min-width="180">
          <template #default="{ row }">
            <div class="product-name-cell">
              <el-avatar :size="28" :icon="Box" class="product-avatar" />
              <span class="product-name">{{ row.productName }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="material" :label="t('product.material')" min-width="120">
          <template #default="{ row }">
            <span class="badge badge-success">{{ row.material || '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="surfaceTreatment" :label="t('product.surfaceTreatment')" width="140">
          <template #default="{ row }">
            <div class="surface-cell">
              <el-icon size="14"><Brush /></el-icon>
              <span>{{ row.surfaceTreatment || '-' }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column :label="t('common.action')" min-width="150" align="right" fixed="right">
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

      <div class="table-footer">
        <span class="table-info">{{ t('common.total') }} {{ total }}</span>
        <el-pagination
          v-model:current-page="query.pageNum"
          v-model:page-size="query.pageSize"
          :total="total"
          layout="prev, pager, next, sizes"
          :page-sizes="[10, 20, 50]"
          @change="loadData"
        />
      </div>
    </div>

    <ProductForm
      v-model:visible="dialogVisible"
      :data="editData"
      @success="loadData"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { productApi } from '@/api/product'
import ProductForm from './ProductForm.vue'
import { useI18n } from '@/composables/useI18n'
import {
  Plus,
  Search,
  RefreshRight,
  Edit,
  Delete,
  Box,
  Brush
} from '@element-plus/icons-vue'

const { t } = useI18n()
const loading = ref(false)
const dialogVisible = ref(false)
const editData = ref<any>(null)
const tableData = ref([])
const total = ref(0)
const query = ref({ pageNum: 1, pageSize: 10, keyword: '' })

const loadData = async () => {
  loading.value = true
  const res = await productApi.list(query.value)
  tableData.value = res.data.list
  total.value = res.data.total
  loading.value = false
}

const handleSearch = () => { query.value.pageNum = 1; loadData() }
const handleReset = () => { query.value = { pageNum: 1, pageSize: 10, keyword: '' }; loadData() }
const handleAdd = () => { editData.value = null; dialogVisible.value = true }
const handleEdit = (row: any) => { editData.value = row; dialogVisible.value = true }

const handleDelete = async (row: any) => {
  await ElMessageBox.confirm(t('message.deleteConfirm'), t('common.tip'), { type: 'warning' })
  await productApi.delete(row.id)
  ElMessage.success(t('message.deleteSuccess'))
  loadData()
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

.search-area {
  background: var(--slds-bg-card);
  padding: var(--slds-spacing-md) var(--slds-spacing-lg);
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
  margin-bottom: var(--slds-spacing-md);
}

.search-form :deep(.el-form-item) {
  margin-bottom: 0;
  margin-right: var(--slds-spacing-md);
}

.search-form :deep(.el-form-item__label) {
  font-size: var(--slds-font-size-sm);
  font-weight: 500;
  color: var(--slds-text-secondary);
}

.content-card {
  background: var(--slds-bg-card);
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
  box-shadow: var(--slds-shadow-card);
  overflow: hidden;
}

.code-link {
  color: var(--slds-brand-primary);
  font-weight: 600;
  font-family: 'SF Mono', Monaco, monospace;
  font-size: 13px;
}

.product-name-cell {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
}

.product-avatar {
  background: #EDF7EE;
  color: var(--slds-success);
}

.product-name {
  font-weight: 600;
  color: var(--slds-text-primary);
}

.surface-cell {
  display: flex;
  align-items: center;
  gap: 4px;
  color: var(--slds-text-secondary);
  font-size: 13px;
}

.badge {
  display: inline-flex;
  align-items: center;
  padding: 2px 10px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
}

.badge-success {
  background: #EDF7EE;
  color: var(--slds-success);
}

.table-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--slds-spacing-md) var(--slds-spacing-lg);
  border-top: 1px solid var(--slds-border-color-light);
}

.table-info {
  font-size: var(--slds-font-size-sm);
  color: var(--slds-text-secondary);
}
</style>
