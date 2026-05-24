import { ref } from 'vue'
import { useI18n } from './useI18n'
import { getFieldControls, initFieldControls, type FieldControl } from '@/api/fieldControl'
import { getFieldTemplateList } from '@/config/field-registry'

export function useFieldControl(moduleCode: string, pageCode: string) {
  const { t } = useI18n()
  const fields = ref<FieldControl[]>([])
  const loaded = ref(false)

  const load = async () => {
    const templates = getFieldTemplateList(moduleCode, pageCode)
    if (templates.length === 0) {
      loaded.value = true
      return
    }

    // Initialize if needed
    await initFieldControls(moduleCode, pageCode, templates.map(f => ({
      fieldCode: f.code,
      defaultRequired: f.defaultRequired
    })))

    // Load config
    const res = await getFieldControls(moduleCode, pageCode)
    fields.value = res.data || []
    loaded.value = true
  }

  const isVisible = (fieldCode: string) => {
    const f = fields.value.find(x => x.fieldCode === fieldCode)
    return f ? f.isVisible : true
  }

  const isRequired = (fieldCode: string) => {
    const f = fields.value.find(x => x.fieldCode === fieldCode)
    return f ? f.isRequired : false
  }

  const getLabel = (fieldCode: string) => {
    const templates = getFieldTemplateList(moduleCode, pageCode)
    const template = templates.find(f => f.code === fieldCode)
    if (template) {
      return t(template.i18nKey)
    }
    return fieldCode
  }

  return { fields, loaded, isVisible, isRequired, getLabel, load }
}
