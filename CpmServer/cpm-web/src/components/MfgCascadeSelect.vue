<template>
  <div class="cascade-select">
    <el-select
      v-model="selectedCategory"
      :placeholder="t('mfg.category')"
      filterable
      clearable
      style="width: 160px"
      @change="onCategoryChange"
    >
      <el-option
        v-for="cat in categoryList"
        :key="cat"
        :label="cat"
        :value="cat"
      />
    </el-select>

    <el-select
      v-model="selectedProcessId"
      :placeholder="t('mfg.process')"
      filterable
      clearable
      style="width: 180px; margin-left: 12px;"
      :disabled="!selectedCategory"
      @change="onProcessChange"
    >
      <el-option
        v-for="p in processOptions"
        :key="p.id"
        :label="p.label"
        :value="p.id"
      />
    </el-select>

    <el-select
      v-model="selectedSubCategoryId"
      :placeholder="t('mfg.subCategory')"
      filterable
      clearable
      style="width: 180px; margin-left: 12px;"
      :disabled="!selectedProcessId"
      @change="onSubCategoryChange"
    >
      <el-option
        v-for="s in subCategoryOptions"
        :key="s.id"
        :label="s.label"
        :value="s.id"
      />
    </el-select>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { mfgProcessApi, type MfgCascadeOption } from '@/api/mfgProcess'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()

const props = defineProps<{
  modelValue?: {
    category?: string
    processId?: number
    subCategoryId?: number
  }
}>()

const emit = defineEmits<['update:modelValue', 'change']>()

const categoryList = ref<string[]>([])
const processOptions = ref<MfgCascadeOption[]>([])
const subCategoryOptions = ref<MfgCascadeOption[]>([])

const selectedCategory = ref('')
const selectedProcessId = ref<number | undefined>(undefined)
const selectedSubCategoryId = ref<number | undefined>(undefined)

// Sync with v-model
watch(() => props.modelValue, (val) => {
  if (val) {
    selectedCategory.value = val.category || ''
    selectedProcessId.value = val.processId
    selectedSubCategoryId.value = val.subCategoryId
    if (selectedCategory.value) loadProcesses()
    if (selectedProcessId.value) loadSubCategories()
  }
}, { immediate: true, deep: true })

// Emit changes
watch([selectedCategory, selectedProcessId, selectedSubCategoryId], () => {
  emit('update:modelValue', {
    category: selectedCategory.value,
    processId: selectedProcessId.value,
    subCategoryId: selectedSubCategoryId.value
  } as any)
  emit('change', {
    category: selectedCategory.value,
    processId: selectedProcessId.value,
    subCategoryId: selectedSubCategoryId.value
  } as any)
})

const loadCategories = async () => {
  const res = await mfgProcessApi.getCategoryList()
  categoryList.value = res.data || []
}

const loadProcesses = async () => {
  if (!selectedCategory.value) {
    processOptions.value = []
    return
  }
  const res = await mfgProcessApi.getProcessOptions(selectedCategory.value)
  processOptions.value = res.data || []
}

const loadSubCategories = async () => {
  if (!selectedProcessId.value) {
    subCategoryOptions.value = []
    return
  }
  const res = await mfgProcessApi.getSubCategoryOptions(selectedProcessId.value)
  subCategoryOptions.value = res.data || []
}

const onCategoryChange = async () => {
  selectedProcessId.value = undefined
  selectedSubCategoryId.value = undefined
  processOptions.value = []
  subCategoryOptions.value = []
  await loadProcesses()
}

const onProcessChange = async () => {
  selectedSubCategoryId.value = undefined
  subCategoryOptions.value = []
  await loadSubCategories()
}

const onSubCategoryChange = () => {
  // subCategory selected
}

onMounted(loadCategories)
</script>

<style scoped>
.cascade-select {
  display: flex;
  align-items: center;
}
</style>
