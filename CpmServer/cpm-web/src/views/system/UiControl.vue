<template>
  <div class="ui-control-container">
    <!-- Header Dropdown Trigger Controls -->
    <div class="control-section">
      <h3 class="section-title">{{ t('uiControl.headerTriggerTitle') }}</h3>
      <p class="section-desc">{{ t('uiControl.headerTriggerDesc') }}</p>

      <!-- Header Trigger Width -->
      <div class="slider-row">
        <span class="slider-label">{{ t('uiControl.headerTriggerWidth') }}</span>
        <el-slider
          v-model="uiStore.headerTriggerWidth"
          :min="40"
          :max="100"
          :step="5"
          show-stops
          style="flex: 1; max-width: 400px;"
          @change="uiStore.setHeaderTriggerWidth"
        />
        <span class="slider-value">{{ uiStore.headerTriggerWidth }}px</span>
      </div>

      <!-- Header Trigger Height -->
      <div class="slider-row">
        <span class="slider-label">{{ t('uiControl.headerTriggerHeight') }}</span>
        <el-slider
          v-model="uiStore.headerTriggerHeight"
          :min="10"
          :max="35"
          :step="1"
          show-stops
          style="flex: 1; max-width: 400px;"
          @change="uiStore.setHeaderTriggerHeight"
        />
        <span class="slider-value">{{ uiStore.headerTriggerHeight }}px</span>
      </div>

      <!-- Header Trigger Font Size -->
      <div class="slider-row">
        <span class="slider-label">{{ t('uiControl.headerTriggerFontSize') }}</span>
        <el-slider
          v-model="uiStore.headerTriggerFontSize"
          :min="8"
          :max="18"
          :step="1"
          show-stops
          style="flex: 1; max-width: 400px;"
          @change="uiStore.setHeaderTriggerFontSize"
        />
        <span class="slider-value">{{ uiStore.headerTriggerFontSize }}px</span>
      </div>

      <div class="preview-section">
        <p class="preview-label">{{ t('uiControl.preview') }}</p>
        <div class="preview-row">
          <div class="preview-trigger" :style="{ width: uiStore.headerTriggerWidth + 'px', height: uiStore.headerTriggerHeight + 'px', fontSize: uiStore.headerTriggerFontSize + 'px' }">
            <el-icon size="14"><OfficeBuilding /></el-icon>
            <span>NT01</span>
            <el-icon size="12"><ArrowDown /></el-icon>
          </div>
          <div class="preview-trigger" :style="{ width: uiStore.headerTriggerWidth + 'px', height: uiStore.headerTriggerHeight + 'px', fontSize: uiStore.headerTriggerFontSize + 'px' }">
            <el-icon size="14"><MapLocation /></el-icon>
            <span>CH</span>
            <el-icon size="12"><ArrowDown /></el-icon>
          </div>
        </div>
      </div>
    </div>

    <!-- Table Action Buttons -->
    <div class="control-section">
      <h3 class="section-title">{{ t('uiControl.tableActionTitle') }}</h3>
      <p class="section-desc">{{ t('uiControl.tableActionDesc') }}</p>

      <!-- Button Width -->
      <div class="slider-row">
        <span class="slider-label">{{ t('uiControl.buttonWidth') }}</span>
        <el-slider
          v-model="uiStore.buttonWidth"
          :min="40"
          :max="120"
          :step="4"
          show-stops
          style="flex: 1; max-width: 400px;"
          @change="uiStore.setButtonWidth"
        />
        <span class="slider-value">{{ uiStore.buttonWidth }}px</span>
      </div>

      <!-- Button Height -->
      <div class="slider-row">
        <span class="slider-label">{{ t('uiControl.buttonHeight') }}</span>
        <el-slider
          v-model="uiStore.buttonHeight"
          :min="20"
          :max="40"
          :step="2"
          show-stops
          style="flex: 1; max-width: 400px;"
          @change="uiStore.setButtonHeight"
        />
        <span class="slider-value">{{ uiStore.buttonHeight }}px</span>
      </div>

      <!-- Button Font Size -->
      <div class="slider-row">
        <span class="slider-label">{{ t('uiControl.buttonFontSize') }}</span>
        <el-slider
          v-model="uiStore.buttonFontSize"
          :min="5"
          :max="16"
          :step="1"
          show-stops
          style="flex: 1; max-width: 400px;"
          @change="uiStore.setButtonFontSize"
        />
        <span class="slider-value">{{ uiStore.buttonFontSize }}px</span>
      </div>

      <div class="preview-section">
        <p class="preview-label">{{ t('uiControl.preview') }}</p>
        <div class="preview-row">
          <el-button link type="primary" size="small">{{ t('common.edit') }}</el-button>
          <el-button link type="success" size="small">{{ t('common.create') }}</el-button>
          <el-button link type="danger" size="small">{{ t('common.delete') }}</el-button>
          <el-button link type="info" size="small">{{ t('common.detail') }}</el-button>
        </div>
      </div>
    </div>

    <!-- Font Size -->
    <div class="control-section">
      <h3 class="section-title">{{ t('uiControl.fontSize') }}</h3>
      <p class="section-desc">{{ t('uiControl.fontSizeDesc') }}</p>
      <div class="slider-row">
        <span class="slider-label">{{ t('uiControl.baseFontSize') }}</span>
        <el-slider
          v-model="uiStore.baseFontSize"
          :min="12"
          :max="18"
          :step="1"
          show-stops
          style="flex: 1; max-width: 400px;"
          @change="uiStore.setBaseFontSize"
        />
        <span class="slider-value">{{ uiStore.baseFontSize }}px</span>
      </div>
      <div class="preview-section">
        <p class="preview-label">{{ t('uiControl.preview') }}</p>
        <div class="preview-font" :style="{ fontSize: uiStore.baseFontSize + 'px' }">
          {{ t('uiControl.fontPreviewText') }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useUiControlStore } from '@/stores/uiControl'
import { useI18n } from '@/composables/useI18n'
import { OfficeBuilding, MapLocation, ArrowDown } from '@element-plus/icons-vue'

const uiStore = useUiControlStore()
const { t } = useI18n()
</script>

<style scoped>
.ui-control-container {
  display: flex;
  flex-direction: column;
  gap: var(--cpm-space-6);
}

.control-section {
  background: var(--cpm-bg-card);
  border: 1px solid var(--cpm-border-light);
  border-radius: var(--cpm-radius-md);
  padding: var(--cpm-space-5);
}

.section-title {
  font-size: var(--cpm-text-h2);
  font-weight: 600;
  color: var(--cpm-text-primary);
  margin: 0 0 var(--cpm-space-2);
}

.section-desc {
  font-size: var(--cpm-text-body);
  color: var(--cpm-text-secondary);
  margin: 0 0 var(--cpm-space-5);
}

.slider-row {
  display: flex;
  align-items: center;
  gap: var(--cpm-space-4);
  margin-bottom: var(--cpm-space-5);
}

.slider-label {
  font-size: var(--cpm-text-body);
  font-weight: 600;
  color: var(--cpm-text-primary);
  min-width: 100px;
}

.slider-value {
  font-size: var(--cpm-text-body);
  font-weight: 700;
  color: var(--cpm-brand-accent);
  min-width: 50px;
  text-align: right;
  font-variant-numeric: tabular-nums;
}

.preview-section {
  border-top: 1px solid var(--cpm-border-light);
  padding-top: var(--cpm-space-4);
}

.preview-label {
  font-size: var(--cpm-text-small);
  font-weight: 600;
  color: var(--cpm-text-secondary);
  letter-spacing: 0.3px;
  margin: 0 0 var(--cpm-space-3);
}

.preview-row {
  display: flex;
  gap: var(--cpm-space-3);
  align-items: center;
}

.preview-trigger {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  padding: var(--slds-spacing-sm) var(--slds-spacing-md);
  border-radius: var(--slds-border-radius);
  background: transparent;
  border: 1px solid var(--slds-border-color);
  cursor: pointer;
  color: var(--slds-text-primary);
  font-weight: 600;
}

.preview-font {
  color: var(--cpm-text-primary);
  line-height: 1.5;
}
</style>
