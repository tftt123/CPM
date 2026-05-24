<template>
  <div class="navigation-config">
    <div class="toolbar">
      <el-button type="primary" :icon="Plus" @click="handleAdd">{{ t('navConfig.add') }}</el-button>
      <el-button type="primary" :loading="saving" :icon="Check" @click="handleSave" style="margin-left: 12px;">
        {{ t('navConfig.saveAll') }}
      </el-button>
      <el-button v-if="configs.length === 0" type="warning" :loading="initing" @click="handleInit" style="margin-left: auto;">
        {{ t('navConfig.initDefault') }}
      </el-button>
    </div>

    <el-table v-if="configs.length > 0" border :data="configs" v-loading="loading" stripe size="small">
      <el-table-column type="index" label="#" width="45" align="center" />
      <el-table-column prop="navCode" :label="t('navConfig.navCode')" width="120" />
      <el-table-column :label="t('navConfig.moduleCode')" width="120">
        <template #default="{ row }">
          <el-input v-model="row.moduleCode" size="small" />
        </template>
      </el-table-column>
      <el-table-column :label="t('navConfig.moduleLabel')" min-width="120">
        <template #default="{ row }">
          <el-input v-model="row.moduleLabel" size="small" :placeholder="getModuleFallback(row, 'zh')" />
        </template>
      </el-table-column>
      <el-table-column :label="t('navConfig.moduleLabelEn')" min-width="120">
        <template #default="{ row }">
          <el-input v-model="row.moduleLabelEn" size="small" :placeholder="getModuleFallback(row, 'en')" />
        </template>
      </el-table-column>
      <el-table-column :label="t('navConfig.navLabel')" min-width="120">
        <template #default="{ row }">
          <el-input v-model="row.navLabel" size="small" :placeholder="getNavFallback(row, 'zh')" />
        </template>
      </el-table-column>
      <el-table-column :label="t('navConfig.navLabelEn')" min-width="120">
        <template #default="{ row }">
          <el-input v-model="row.navLabelEn" size="small" :placeholder="getNavFallback(row, 'en')" />
        </template>
      </el-table-column>
      <el-table-column :label="t('navConfig.icon')" width="130">
        <template #default="{ row }">
          <el-select v-model="row.iconName" size="small" filterable style="width: 115px">
            <el-option v-for="icon in iconOptions" :key="icon" :label="icon" :value="icon" />
          </el-select>
        </template>
      </el-table-column>
      <el-table-column :label="t('navConfig.sortOrder')" width="80" align="center">
        <template #default="{ row }">
          <el-input-number v-model="row.sortOrder" size="small" :min="0" :max="999" :controls="false" style="width: 55px" />
        </template>
      </el-table-column>
      <el-table-column :label="t('navConfig.isVisible')" width="70" align="center">
        <template #default="{ row }">
          <el-switch v-model="row.isVisible" size="small" />
        </template>
      </el-table-column>
      <el-table-column :label="t('navConfig.routePath')" width="140">
        <template #default="{ row }">
          <el-tooltip :content="row.routePath" placement="top">
            <span class="route-path">{{ row.routePath }}</span>
          </el-tooltip>
        </template>
      </el-table-column>
      <el-table-column :label="t('common.action')" width="70" align="center" fixed="right">
        <template #default="{ row, $index }">
          <el-button link type="danger" size="small" :icon="Delete" @click="handleRemove($index)">
            {{ t('navConfig.delete') }}
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-empty v-else :description="t('navConfig.emptyTip')" />

    <!-- Add/Edit Dialog -->
    <el-dialog
      :title="isEdit ? t('navConfig.editTitle') : t('navConfig.addTitle')"
      v-model="dialogVisible"
      width="520px"
      :close-on-click-modal="false"
    >
      <el-form :model="form" label-width="100px" ref="formRef" :rules="rules">
        <el-form-item :label="t('navConfig.navCode')" prop="navCode">
          <el-input v-model="form.navCode" placeholder="e.g. myModule" :disabled="isEdit" />
        </el-form-item>
        <el-form-item :label="t('navConfig.moduleCode')" prop="moduleCode">
          <el-input v-model="form.moduleCode" placeholder="e.g. rfq, pm, custom" />
        </el-form-item>
        <el-form-item :label="t('navConfig.moduleLabel')">
          <el-input v-model="form.moduleLabel" placeholder="模块中文名" />
        </el-form-item>
        <el-form-item :label="t('navConfig.moduleLabelEn')">
          <el-input v-model="form.moduleLabelEn" placeholder="Module English Name" />
        </el-form-item>
        <el-form-item :label="t('navConfig.navLabel')">
          <el-input v-model="form.navLabel" placeholder="栏目中文名" />
        </el-form-item>
        <el-form-item :label="t('navConfig.navLabelEn')">
          <el-input v-model="form.navLabelEn" placeholder="Item English Name" />
        </el-form-item>
        <el-form-item :label="t('navConfig.routePath')" prop="routePath">
          <el-input v-model="form.routePath" placeholder="e.g. /my-page" />
        </el-form-item>
        <el-form-item :label="t('navConfig.icon')">
          <el-select v-model="form.iconName" filterable style="width: 100%">
            <el-option v-for="icon in iconOptions" :key="icon" :label="icon" :value="icon" />
          </el-select>
        </el-form-item>
        <el-form-item :label="t('navConfig.sortOrder')">
          <el-input-number v-model="form.sortOrder" :min="0" :max="999" />
        </el-form-item>
        <el-form-item :label="t('navConfig.isVisible')">
          <el-switch v-model="form.isVisible" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">{{ t('common.cancel') }}</el-button>
        <el-button type="primary" @click="handleSubmit">{{ t('common.confirm') }}</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useI18n } from '@/composables/useI18n'
import { useLocaleStore } from '@/stores/locale'
import { navigationConfigApi, type NavigationConfig } from '@/api/navigationConfig'
import { Plus, Check, Delete } from '@element-plus/icons-vue'

const { t } = useI18n()
const localeStore = useLocaleStore()
const loading = ref(false)
const saving = ref(false)
const initing = ref(false)
const configs = ref<NavigationConfig[]>([])
const dialogVisible = ref(false)
const isEdit = ref(false)
const formRef = ref<any>(null)

const iconOptions = [
  'HomeFilled', 'UserFilled', 'Box', 'FolderOpened', 'Document', 'Setting',
  'Timer', 'TrendCharts', 'CircleCheck', 'DataAnalysis',
  'House', 'User', 'DocumentChecked', 'List', 'Edit', 'Delete',
  'Search', 'Plus', 'Minus', 'Link', 'Message', 'Bell',
  'Calendar', 'Clock', 'Money', 'Wallet', 'Shop', 'Goods',
  'Grid', 'Menu', 'OfficeBuilding', 'MapLocation', 'Compass',
  'StarFilled', 'FirstAidKit', 'Aim', 'Cpu', 'Monitor',
  'Tools', 'Service', 'Warning', 'InfoFilled', 'QuestionFilled'
]

const form = ref<NavigationConfig>({
  id: 0,
  navCode: '',
  moduleCode: '',
  moduleLabel: '',
  moduleLabelEn: '',
  navLabel: '',
  navLabelEn: '',
  routePath: '',
  iconName: 'Document',
  sortOrder: 0,
  isVisible: true,
  isActive: true
})

const rules = {
  navCode: [{ required: true, message: '请输入导航编码', trigger: 'blur' }],
  moduleCode: [{ required: true, message: '请输入模块编码', trigger: 'blur' }],
  routePath: [{ required: true, message: '请输入路由路径', trigger: 'blur' }]
}

const moduleI18nMap: Record<string, string> = {
  home: 'nav.home',
  rfq: 'nav.rfq',
  pm: 'nav.pm',
  approval: 'nav.approval',
  salesReport: 'nav.salesReport'
}

function getModuleFallback(row: NavigationConfig, locale: 'zh' | 'en'): string {
  const key = moduleI18nMap[row.moduleCode]
  if (key) {
    const msg = localeStore.getMessage(key, locale)
    if (msg !== key) return msg
  }
  return row.moduleCode
}

function getNavFallback(row: NavigationConfig, locale: 'zh' | 'en'): string {
  const key = `nav.${row.navCode}`
  const msg = localeStore.getMessage(key, locale)
  if (msg !== key) return msg
  return row.navCode
}

const loadData = async () => {
  loading.value = true
  try {
    const res = await navigationConfigApi.getList()
    configs.value = res.data || []
  } finally {
    loading.value = false
  }
}

const handleAdd = () => {
  isEdit.value = false
  form.value = {
    id: 0,
    navCode: '',
    moduleCode: '',
    moduleLabel: '',
    moduleLabelEn: '',
    navLabel: '',
    navLabelEn: '',
    routePath: '',
    iconName: 'Document',
    sortOrder: 0,
    isVisible: true,
    isActive: true
  }
  dialogVisible.value = true
}

const handleSubmit = async () => {
  if (!formRef.value) return
  await formRef.value.validate((valid: boolean) => {
    if (!valid) return

    // Check duplicate navCode
    const exists = configs.value.find(c => c.navCode === form.value.navCode)
    if (!isEdit.value && exists) {
      ElMessage.error('导航编码已存在')
      return
    }

    if (isEdit.value && exists) {
      // Update existing in array
      const idx = configs.value.findIndex(c => c.navCode === form.value.navCode)
      if (idx >= 0) {
        configs.value[idx] = { ...form.value }
      }
    } else {
      // Add new
      configs.value.push({ ...form.value })
    }

    dialogVisible.value = false
    ElMessage.success(isEdit.value ? t('message.updateSuccess') : t('message.createSuccess'))
  })
}

const handleRemove = async (index: number) => {
  try {
    await ElMessageBox.confirm(t('navConfig.deleteConfirm'), t('common.tip'), {
      confirmButtonText: t('common.confirm'),
      cancelButtonText: t('common.cancel'),
      type: 'warning'
    })
  } catch {
    return
  }
  configs.value.splice(index, 1)
}

const handleInit = async () => {
  try {
    await ElMessageBox.confirm(t('navConfig.initConfirm'), t('common.tip'), {
      confirmButtonText: t('common.confirm'),
      cancelButtonText: t('common.cancel'),
      type: 'warning'
    })
  } catch {
    return
  }

  initing.value = true
  try {
    const res = await navigationConfigApi.init()
    configs.value = res.data || []
    ElMessage.success(t('message.createSuccess'))
  } catch {
    ElMessage.error(t('common.failed'))
  } finally {
    initing.value = false
  }
}

const handleSave = async () => {
  saving.value = true
  try {
    await navigationConfigApi.batchUpdate(configs.value)
    ElMessage.success(t('message.saveSuccess'))
  } catch {
    ElMessage.error(t('common.failed'))
  } finally {
    saving.value = false
  }
}

onMounted(loadData)
</script>

<style scoped>
.navigation-config {
  padding: 0;
}
.toolbar {
  display: flex;
  align-items: center;
  margin-bottom: 16px;
}
.route-path {
  color: var(--el-text-color-secondary);
  font-size: 12px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  display: block;
  max-width: 120px;
}
</style>
