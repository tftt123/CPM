import { defineStore } from 'pinia'
import { ref, watch } from 'vue'

export const useUiControlStore = defineStore('uiControl', () => {
  // Table action buttons
  const buttonWidth = ref<number>(parseInt(localStorage.getItem('ui_buttonWidth') || '64', 10))
  const buttonHeight = ref<number>(parseInt(localStorage.getItem('ui_buttonHeight') || '24', 10))
  const buttonFontSize = ref<number>(parseInt(localStorage.getItem('ui_buttonFontSize') || '12', 10))
  const baseFontSize = ref<number>(parseInt(localStorage.getItem('ui_baseFontSize') || '14', 10))

  // Header dropdown triggers
  const headerTriggerWidth = ref<number>(parseInt(localStorage.getItem('ui_headerTriggerWidth') || '80', 10))
  const headerTriggerHeight = ref<number>(parseInt(localStorage.getItem('ui_headerTriggerHeight') || '34', 10))
  const headerTriggerFontSize = ref<number>(parseInt(localStorage.getItem('ui_headerTriggerFontSize') || '12', 10))

  const applyButtonWidth = () => {
    document.documentElement.style.setProperty('--cpm-btn-action-width', `${buttonWidth.value}px`)
  }

  const applyButtonHeight = () => {
    document.documentElement.style.setProperty('--cpm-btn-action-height', `${buttonHeight.value}px`)
  }

  const applyButtonFontSize = () => {
    document.documentElement.style.setProperty('--cpm-btn-action-font-size', `${buttonFontSize.value}px`)
  }

  const applyBaseFontSize = () => {
    const scale = baseFontSize.value / 14
    document.documentElement.style.setProperty('--cpm-font-scale', String(scale))
    document.documentElement.style.setProperty('--cpm-text-display', `${Math.round(28 * scale)}px`)
    document.documentElement.style.setProperty('--cpm-text-h1', `${Math.round(22 * scale)}px`)
    document.documentElement.style.setProperty('--cpm-text-h2', `${Math.round(18 * scale)}px`)
    document.documentElement.style.setProperty('--cpm-text-h3', `${Math.round(16 * scale)}px`)
    document.documentElement.style.setProperty('--cpm-text-body', `${Math.round(14 * scale)}px`)
    document.documentElement.style.setProperty('--cpm-text-small', `${Math.round(12 * scale)}px`)
    document.documentElement.style.setProperty('--cpm-text-label', `${Math.round(11 * scale)}px`)
  }

  const applyHeaderTriggerWidth = () => {
    document.documentElement.style.setProperty('--cpm-header-trigger-width', `${headerTriggerWidth.value}px`)
  }

  const applyHeaderTriggerHeight = () => {
    document.documentElement.style.setProperty('--cpm-header-trigger-height', `${headerTriggerHeight.value}px`)
  }

  const applyHeaderTriggerFontSize = () => {
    document.documentElement.style.setProperty('--cpm-header-trigger-font-size', `${headerTriggerFontSize.value}px`)
  }

  const setButtonWidth = (width: number) => {
    buttonWidth.value = width
    localStorage.setItem('ui_buttonWidth', String(width))
    applyButtonWidth()
  }

  const setButtonHeight = (height: number) => {
    buttonHeight.value = height
    localStorage.setItem('ui_buttonHeight', String(height))
    applyButtonHeight()
  }

  const setButtonFontSize = (size: number) => {
    buttonFontSize.value = size
    localStorage.setItem('ui_buttonFontSize', String(size))
    applyButtonFontSize()
  }

  const setBaseFontSize = (size: number) => {
    baseFontSize.value = size
    localStorage.setItem('ui_baseFontSize', String(size))
    applyBaseFontSize()
  }

  const setHeaderTriggerWidth = (width: number) => {
    headerTriggerWidth.value = width
    localStorage.setItem('ui_headerTriggerWidth', String(width))
    applyHeaderTriggerWidth()
  }

  const setHeaderTriggerHeight = (height: number) => {
    headerTriggerHeight.value = height
    localStorage.setItem('ui_headerTriggerHeight', String(height))
    applyHeaderTriggerHeight()
  }

  const setHeaderTriggerFontSize = (size: number) => {
    headerTriggerFontSize.value = size
    localStorage.setItem('ui_headerTriggerFontSize', String(size))
    applyHeaderTriggerFontSize()
  }

  const applyAll = () => {
    applyButtonWidth()
    applyButtonHeight()
    applyButtonFontSize()
    applyBaseFontSize()
    applyHeaderTriggerWidth()
    applyHeaderTriggerHeight()
    applyHeaderTriggerFontSize()
  }

  watch([buttonWidth, buttonHeight, buttonFontSize, baseFontSize, headerTriggerWidth, headerTriggerHeight, headerTriggerFontSize], applyAll, { immediate: true })

  return {
    buttonWidth, buttonHeight, buttonFontSize, baseFontSize,
    headerTriggerWidth, headerTriggerHeight, headerTriggerFontSize,
    setButtonWidth, setButtonHeight, setButtonFontSize, setBaseFontSize,
    setHeaderTriggerWidth, setHeaderTriggerHeight, setHeaderTriggerFontSize,
    applyAll,
  }
})
