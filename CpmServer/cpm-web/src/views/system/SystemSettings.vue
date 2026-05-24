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
      <el-tab-pane v-if="hasTabPerm('users.manage')" :label="t('system.userListTitle')" name="users">
        <UserList embedded />
      </el-tab-pane>
      <el-tab-pane v-if="hasTabPerm('roles.manage')" :label="t('system.roleListTitle')" name="roles">
        <RoleList embedded />
      </el-tab-pane>
      <el-tab-pane v-if="hasTabPerm('approval.templates.manage')" :label="t('approval.steps')" name="approval">
        <ApprovalConfig embedded />
      </el-tab-pane>
      <el-tab-pane v-if="hasTabPerm('system.manage')" :label="t('system.moduleTypeConfig')" name="moduleTypes">
        <ModuleTypeConfig />
      </el-tab-pane>
      <el-tab-pane v-if="hasTabPerm('system.manage')" :label="t('mail.title')" name="mail">
        <MailConfig />
      </el-tab-pane>
      <el-tab-pane v-if="hasTabPerm('system.manage')" :label="t('uiControl.title')" name="ui">
        <UiControl />
      </el-tab-pane>
      <el-tab-pane v-if="hasTabPerm('system.manage')" :label="t('siteSetup.title')" name="site">
        <SiteSetup />
      </el-tab-pane>
      <el-tab-pane v-if="hasTabPerm('system.manage')" :label="t('sequenceRule.title')" name="sequence">
        <SequenceRuleConfig />
      </el-tab-pane>
      <el-tab-pane v-if="hasTabPerm('email.templates.manage')" :label="t('emailTemplate.title')" name="emailTemplates">
        <EmailTemplateManage />
      </el-tab-pane>
      <el-tab-pane v-if="hasTabPerm('alerts.manage')" :label="t('alertRecipient.title')" name="alertRecipients">
        <AlertRecipientManage />
      </el-tab-pane>
      <el-tab-pane v-if="hasTabPerm('fieldcontrol.manage')" :label="t('fieldControl.title')" name="fieldControl">
        <FieldControlConfig />
      </el-tab-pane>
      <el-tab-pane v-if="hasTabPerm('i18n.manage')" :label="t('system.translationTitle')" name="translations">
        <TranslationManage />
      </el-tab-pane>
      <el-tab-pane v-if="hasTabPerm('nav.manage')" :label="t('navConfig.title')" name="navigation">
        <NavigationConfig />
      </el-tab-pane>
      <el-tab-pane v-if="hasTabPerm('gc.manage')" :label="t('gc.title')" name="generalizedCode">
        <GeneralizedCodeManage />
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from '@/composables/useI18n'
import { useUserStore } from '@/stores/user'
import UserList from './UserList.vue'
import RoleList from './RoleList.vue'
import ApprovalConfig from '../approval/ApprovalConfig.vue'
import ModuleTypeConfig from './ModuleTypeConfig.vue'
import MailConfig from './MailConfig.vue'
import UiControl from './UiControl.vue'
import SiteSetup from './SiteSetup.vue'
import SequenceRuleConfig from './SequenceRuleConfig.vue'
import EmailTemplateManage from './EmailTemplateManage.vue'
import AlertRecipientManage from './AlertRecipientManage.vue'
import FieldControlConfig from './FieldControlConfig.vue'
import TranslationManage from './TranslationManage.vue'
import NavigationConfig from './NavigationConfig.vue'
import GeneralizedCodeManage from './GeneralizedCodeManage.vue'

const { t } = useI18n()
const userStore = useUserStore()
const activeTab = ref('users')

const tabs = [
  { name: 'users', perm: 'users.manage' },
  { name: 'roles', perm: 'roles.manage' },
  { name: 'approval', perm: 'approval.templates.manage' },
  { name: 'moduleTypes', perm: 'system.manage' },
  { name: 'mail', perm: 'system.manage' },
  { name: 'ui', perm: 'system.manage' },
  { name: 'site', perm: 'system.manage' },
  { name: 'sequence', perm: 'system.manage' },
  { name: 'emailTemplates', perm: 'email.templates.manage' },
  { name: 'alertRecipients', perm: 'alerts.manage' },
  { name: 'fieldControl', perm: 'fieldcontrol.manage' },
  { name: 'translations', perm: 'i18n.manage' },
  { name: 'navigation', perm: 'nav.manage' },
  { name: 'generalizedCode', perm: 'gc.manage' }
]

const visibleTabs = computed(() => tabs.filter(tab => userStore.hasPermission(tab.perm)))

const hasTabPerm = (perm: string) => userStore.hasPermission(perm)

onMounted(() => {
  const first = visibleTabs.value[0]
  if (first) activeTab.value = first.name
})
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
