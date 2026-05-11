<template>
  <div class="dashboard">
    <!-- Welcome Section -->
    <div class="welcome-section">
      <div class="welcome-content">
        <h1 class="welcome-title">
          {{ t('layout.appName') }}, {{ userStore.userInfo?.realName || userStore.userInfo?.username || t('common.user') }}
        </h1>
        <p class="welcome-subtitle">{{ t('login.desc') }}</p>
      </div>
      <div class="welcome-actions">
        <el-button type="primary" :icon="Plus" @click="$router.push('/customer')">
          {{ t('common.add') }}
        </el-button>
        <el-button :icon="Refresh" @click="refreshData">
          {{ t('common.refresh') }}
        </el-button>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="stats-grid">
      <div class="stat-card dashboard-stat">
        <div class="stat-icon" style="background: #F0F8FF; color: #0176D3;">
          <el-icon size="24"><UserFilled /></el-icon>
        </div>
        <div class="stat-info">
          <div class="stat-value">{{ stats.customers }}</div>
          <div class="stat-label">{{ t('nav.customer') }}</div>
        </div>
        <div class="stat-trend trend-up">
          <el-icon><ArrowUp /></el-icon>
          <span>+12%</span>
        </div>
      </div>

      <div class="stat-card dashboard-stat">
        <div class="stat-icon" style="background: #EDF7EE; color: #2E844A;">
          <el-icon size="24"><Box /></el-icon>
        </div>
        <div class="stat-info">
          <div class="stat-value" style="color: var(--slds-success);">{{ stats.products }}</div>
          <div class="stat-label">{{ t('nav.product') }}</div>
        </div>
        <div class="stat-trend trend-up">
          <el-icon><ArrowUp /></el-icon>
          <span>+8%</span>
        </div>
      </div>

      <div class="stat-card dashboard-stat">
        <div class="stat-icon" style="background: #FEF3E8; color: #E67A1F;">
          <el-icon size="24"><TrendCharts /></el-icon>
        </div>
        <div class="stat-info">
          <div class="stat-value" style="color: var(--slds-warning);">{{ stats.deals }}</div>
          <div class="stat-label">{{ t('nav.salesReport') }}</div>
        </div>
        <div class="stat-trend trend-down">
          <el-icon><ArrowDown /></el-icon>
          <span>-3%</span>
        </div>
      </div>

      <div class="stat-card dashboard-stat">
        <div class="stat-icon" style="background: #FCEEED; color: #C23A31;">
          <el-icon size="24"><Timer /></el-icon>
        </div>
        <div class="stat-info">
          <div class="stat-value" style="color: var(--slds-error);">{{ stats.pending }}</div>
          <div class="stat-label">{{ t('common.pending') }}</div>
        </div>
        <div class="stat-trend trend-up">
          <el-icon><ArrowUp /></el-icon>
          <span>+5</span>
        </div>
      </div>
    </div>

    <!-- Quick Actions + Recent Activity -->
    <div class="dashboard-grid">
      <!-- Quick Entry -->
      <div class="content-card quick-actions">
        <div class="card-header">
          <h3 class="card-title">
            <el-icon><Grid /></el-icon>
            {{ t('common.quickActions') }}
          </h3>
        </div>
        <div class="action-grid">
          <div class="action-item" @click="$router.push('/customer')">
            <div class="action-icon" style="background: #F0F8FF; color: #0176D3;">
              <el-icon size="20"><UserFilled /></el-icon>
            </div>
            <span class="action-label">{{ t('nav.customer') }}</span>
          </div>
          <div class="action-item" @click="$router.push('/product')">
            <div class="action-icon" style="background: #EDF7EE; color: #2E844A;">
              <el-icon size="20"><Box /></el-icon>
            </div>
            <span class="action-label">{{ t('nav.product') }}</span>
          </div>
          <div class="action-item">
            <div class="action-icon" style="background: #FEF3E8; color: #E67A1F;">
              <el-icon size="20"><Document /></el-icon>
            </div>
            <span class="action-label">{{ t('nav.salesReport') }}</span>
          </div>
          <div class="action-item">
            <div class="action-icon" style="background: #FCEEED; color: #C23A31;">
              <el-icon size="20"><Setting /></el-icon>
            </div>
            <span class="action-label">{{ t('common.systemSettings') }}</span>
          </div>
        </div>
      </div>

      <!-- Recent Activity -->
      <div class="content-card recent-activity">
        <div class="card-header">
          <h3 class="card-title">
            <el-icon><Clock /></el-icon>
            {{ t('common.recentActivity') }}
          </h3>
          <el-link type="primary" :underline="false" class="view-all">{{ t('common.viewAll') }}</el-link>
        </div>
        <div class="activity-list">
          <div v-for="(item, index) in recentActivities" :key="index" class="activity-item">
            <div class="activity-dot" :style="{ background: item.color }"></div>
            <div class="activity-content">
              <p class="activity-text">{{ item.text }}</p>
              <span class="activity-time">{{ item.time }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- System Status -->
    <div class="content-card system-status">
      <div class="card-header">
        <h3 class="card-title">
          <el-icon><Monitor /></el-icon>
          {{ t('common.systemStatus') }}
        </h3>
      </div>
      <div class="status-grid">
        <div class="status-item">
          <el-icon size="18" color="#2E844A"><CircleCheckFilled /></el-icon>
          <span class="status-label">{{ t('common.backendService') }}</span>
          <span class="status-value status-online">{{ t('common.running') }}</span>
        </div>
        <div class="status-item">
          <el-icon size="18" color="#2E844A"><CircleCheckFilled /></el-icon>
          <span class="status-label">{{ t('common.databaseConnection') }}</span>
          <span class="status-value status-online">{{ t('common.connected') }}</span>
        </div>
        <div class="status-item">
          <el-icon size="18" color="#0176D3"><InfoFilled /></el-icon>
          <span class="status-label">{{ t('common.systemVersion') }}</span>
          <span class="status-value">v1.0.0</span>
        </div>
        <div class="status-item">
          <el-icon size="18" color="#0176D3"><Calendar /></el-icon>
          <span class="status-label">{{ t('common.currentDate') }}</span>
          <span class="status-value">{{ currentDate }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useUserStore } from '@/stores/user'
import request from '@/api/request'
import { useI18n } from '@/composables/useI18n'
import {
  Plus,
  Refresh,
  UserFilled,
  Box,
  TrendCharts,
  Timer,
  Grid,
  Document,
  Setting,
  Clock,
  Monitor,
  CircleCheckFilled,
  InfoFilled,
  Calendar,
  ArrowUp,
  ArrowDown
} from '@element-plus/icons-vue'

const userStore = useUserStore()
const { t } = useI18n()
const currentDate = ref(new Date().toLocaleDateString())

const stats = ref({
  customers: 0,
  products: 0,
  deals: 0,
  pending: 0
})

const recentActivities = ref([
  { text: 'New customer "Huawei Technologies"', time: '2 hours ago', color: '#0176D3' },
  { text: 'Updated product "SUS304 Stainless Steel Sheet"', time: '4 hours ago', color: '#2E844A' },
  { text: 'Deleted customer "Test Customer 001"', time: 'Yesterday', color: '#C23A31' },
  { text: 'System backup completed', time: 'Yesterday', color: '#706E6B' },
  { text: 'New product "AL6061 Aluminum Profile"', time: '3 days ago', color: '#0176D3' }
])

const refreshData = async () => {
  try {
    const [customerRes, productRes] = await Promise.all([
      request.get('/customer/list', { params: { pageNum: 1, pageSize: 1 } }),
      request.get('/product/list', { params: { pageNum: 1, pageSize: 1 } })
    ])
    stats.value.customers = customerRes?.data?.total || 0
    stats.value.products = productRes?.data?.total || 0
    stats.value.deals = 0
    stats.value.pending = 0
  } catch (e) {
    console.log('Dashboard data load failed:', e)
  }
}

onMounted(() => {
  refreshData()
})
</script>

<style scoped>
.dashboard {
  padding: var(--slds-spacing-lg);
}

.welcome-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--slds-spacing-xl);
  padding-bottom: var(--slds-spacing-lg);
  border-bottom: 1px solid var(--slds-border-color);
}

.welcome-title {
  font-size: 28px;
  font-weight: 700;
  color: var(--slds-text-primary);
  margin: 0 0 4px;
}

.welcome-subtitle {
  font-size: var(--slds-font-size-md);
  color: var(--slds-text-secondary);
  margin: 0;
}

.welcome-actions {
  display: flex;
  gap: var(--slds-spacing-sm);
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--slds-spacing-md);
  margin-bottom: var(--slds-spacing-lg);
}

.dashboard-stat {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-md);
  padding: var(--slds-spacing-lg);
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-info {
  flex: 1;
}

.stat-trend {
  display: flex;
  align-items: center;
  gap: 2px;
  font-size: 12px;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 12px;
}

.trend-up {
  background: #EDF7EE;
  color: var(--slds-success);
}

.trend-down {
  background: #FCEEED;
  color: var(--slds-error);
}

.dashboard-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--slds-spacing-md);
  margin-bottom: var(--slds-spacing-lg);
}

.content-card {
  background: var(--slds-bg-card);
  border-radius: var(--slds-border-radius);
  border: 1px solid var(--slds-border-color-light);
  box-shadow: var(--slds-shadow-card);
  padding: var(--slds-spacing-lg);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--slds-spacing-md);
}

.card-title {
  font-size: var(--slds-font-size-md);
  font-weight: 600;
  color: var(--slds-text-primary);
  margin: 0;
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
}

.view-all {
  font-size: 13px;
  font-weight: 500;
}

.action-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--slds-spacing-md);
}

.action-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--slds-spacing-sm);
  padding: var(--slds-spacing-lg);
  border-radius: var(--slds-border-radius);
  cursor: pointer;
  transition: background 0.2s;
  border: 1px solid transparent;
}

.action-item:hover {
  background: var(--slds-bg-hover);
  border-color: var(--slds-border-color-light);
}

.action-icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.action-label {
  font-size: var(--slds-font-size-sm);
  font-weight: 500;
  color: var(--slds-text-secondary);
}

.activity-list {
  display: flex;
  flex-direction: column;
  gap: var(--slds-spacing-md);
}

.activity-item {
  display: flex;
  align-items: flex-start;
  gap: var(--slds-spacing-sm);
}

.activity-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  margin-top: 6px;
  flex-shrink: 0;
}

.activity-content {
  flex: 1;
}

.activity-text {
  font-size: var(--slds-font-size-md);
  color: var(--slds-text-primary);
  margin: 0 0 2px;
  line-height: 1.4;
}

.activity-time {
  font-size: 12px;
  color: var(--slds-text-secondary);
}

.system-status {
  margin-top: var(--slds-spacing-lg);
}

.status-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--slds-spacing-lg);
}

.status-item {
  display: flex;
  align-items: center;
  gap: var(--slds-spacing-sm);
  padding: var(--slds-spacing-md);
  background: var(--slds-bg-page);
  border-radius: var(--slds-border-radius);
}

.status-label {
  font-size: var(--slds-font-size-sm);
  color: var(--slds-text-secondary);
  flex: 1;
}

.status-value {
  font-size: var(--slds-font-size-sm);
  font-weight: 600;
  color: var(--slds-text-primary);
}

.status-online {
  color: var(--slds-success);
}

@media (max-width: 1200px) {
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
  .status-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 768px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }
  .welcome-section {
    flex-direction: column;
    align-items: flex-start;
    gap: var(--slds-spacing-md);
  }
  .status-grid {
    grid-template-columns: 1fr;
  }
}
</style>
