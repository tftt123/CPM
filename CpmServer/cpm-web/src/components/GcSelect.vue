<template>
  <el-select
    v-model="modelValue"
    :placeholder="placeholder"
    :clearable="clearable"
    :filterable="filterable"
    :disabled="disabled"
    @change="handleChange"
  >
    <el-option
      v-for="opt in options"
      :key="opt.value"
      :label="opt.label"
      :value="opt.value"
    >
      <span style="display: flex; align-items: center; gap: 8px;">
        <el-tag v-if="showTag && opt.tagType" :type="opt.tagType as any" size="small" effect="light">{{ opt.value }}</el-tag>
        <span>{{ opt.label }}</span>
      </span>
    </el-option>
  </el-select>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { useGeneralizedCode, type GcOption } from '@/composables/useGeneralizedCode'

interface FallbackOption {
  code: string
  label: string
  tagType?: string
}

const props = withDefaults(defineProps<{
  modelValue: string | number | null | undefined
  domain: string
  placeholder?: string
  clearable?: boolean
  filterable?: boolean
  disabled?: boolean
  showTag?: boolean
  fallback?: FallbackOption[]
}>(), {
  placeholder: '',
  clearable: false,
  filterable: false,
  disabled: false,
  showTag: false
})

const emit = defineEmits<{
  'update:modelValue': [value: string | number | null]
  change: [value: string | number | null]
}>()

const isNumberMode = computed(() => typeof props.modelValue === 'number')

const { getOptions } = useGeneralizedCode()
const options = ref<GcOption[]>([])

watch(() => props.domain, async () => {
  options.value = await getOptions(props.domain, props.fallback)
}, { immediate: true })

const modelValue = computed({
  get: () => {
    if (props.modelValue == null) return null
    return String(props.modelValue)
  },
  set: (v: string | null) => {
    const raw = isNumberMode.value && v !== null ? Number(v) : v
    emit('update:modelValue', raw)
  }
})

function handleChange(val: string | null) {
  const raw = isNumberMode.value && val !== null ? Number(val) : val
  emit('change', raw)
}
</script>
