<template>
  <div class="page-container">
    <div class="page-header-section">
      <el-page-header @back="$router.push('/home')">
        <template #content>
          <div class="page-header-content">
            <h1 class="page-title">{{ t('system.settingsTitle') }}</h1>
            <span class="page-subtitle">{{ t('system.settingsSubtitle') }}</span>
          </div>
        </template>
      </el-page-header>
    </div>

    <el-tabs v-model="activeTab" type="border-card" class="settings-tabs">
      <el-tab-pane :label="t('system.userListTitle')" name="users">
        <UserList embedded />
      </el-tab-pane>
      <el-tab-pane :label="t('system.roleListTitle')" name="roles">
        <RoleList embedded />
      </el-tab-pane>
      <el-tab-pane :label="t('approval.steps')" name="approval">
        <ApprovalConfig embedded />
      </el-tab-pane>
      <el-tab-pane :label="t('system.moduleTypeConfig')" name="moduleTypes">
        <ModuleTypeConfig />
      </el-tab-pane>
      <el-tab-pane :label="t('mail.title')" name="mail">
        <MailConfig />
      </el-tab-pane>
      <el-tab-pane :label="t('uiControl.title')" name="ui">
        <UiControl />
      </el-tab-pane>
      <el-tab-pane :label="t('siteSetup.title')" name="site">
        <SiteSetup />
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from '@/composables/useI18n'
import UserList from './UserList.vue'
import RoleList from './RoleList.vue'
import ApprovalConfig from '../approval/ApprovalConfig.vue'
import ModuleTypeConfig from './ModuleTypeConfig.vue'
import MailConfig from './MailConfig.vue'
import UiControl from './UiControl.vue'
import SiteSetup from './SiteSetup.vue'

const { t } = useI18n()
const activeTab = ref('users')
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

.settings-tabs {
  background: var(--slds-bg-card);
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
  box-shadow: var(--slds-shadow-card);
  overflow: hidden;
}

.settings-tabs :deep(.el-tabs__header) {
  margin: 0;
  background: #FAFBFC;
  border-bottom: 1px solid var(--slds-border-color-light);
}

.settings-tabs :deep(.el-tabs__content) {
  padding: var(--slds-spacing-lg);
}
</style>


