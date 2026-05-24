<template>
  <div class="dashboard">
    <!-- Welcome Section -->
    <div class="welcome-section">
      <div class="welcome-content">
        <h1 class="welcome-title">
          {{ t('app.cpm') }}, {{ userStore.userInfo?.realName || userStore.userInfo?.username || t('common.user') }}
        </h1>
        <p class="welcome-subtitle">{{ t('login.desc') }}</p>
      </div>
      <div class="welcome-actions">
        <el-button :icon="Refresh" @click="refreshData">
          {{ t('common.refresh') }}
        </el-button>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="stats-grid">
      <div class="stat-card dashboard-stat stat-clickable" @click="$router.push('/approval/center')">
        <div class="stat-icon" style="background: var(--cpm-error-bg); color: var(--cpm-error);">
          <el-icon size="24"><Timer /></el-icon>
        </div>
        <div class="stat-info">
          <div class="stat-value" style="color: var(--cpm-error);">{{ stats.pending }}</div>
          <div class="stat-label">{{ t('common.pending') }}</div>
        </div>
      </div>

      <div class="stat-card dashboard-stat">
        <div class="stat-icon" style="background: var(--cpm-info-bg); color: var(--cpm-info);">
          <el-icon size="24"><UserFilled /></el-icon>
        </div>
        <div class="stat-info">
          <div class="stat-value">{{ stats.customers }}</div>
          <div class="stat-label">{{ t('nav.customer') }}</div>
        </div>
      </div>

      <div class="stat-card dashboard-stat">
        <div class="stat-icon" style="background: var(--cpm-success-bg); color: var(--cpm-success);">
          <el-icon size="24"><Box /></el-icon>
        </div>
        <div class="stat-info">
          <div class="stat-value" style="color: var(--cpm-success);">{{ stats.products }}</div>
          <div class="stat-label">{{ t('nav.product') }}</div>
        </div>
      </div>

      <div class="stat-card dashboard-stat">
        <div class="stat-icon" style="background: var(--cpm-warning-bg); color: var(--cpm-warning);">
          <el-icon size="24"><TrendCharts /></el-icon>
        </div>
        <div class="stat-info">
          <div class="stat-value" style="color: var(--cpm-warning);">{{ stats.deals }}</div>
          <div class="stat-label">{{ t('nav.salesReport') }}</div>
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
            <div class="action-icon" style="background: var(--cpm-info-bg); color: var(--cpm-info);">
              <el-icon size="20"><UserFilled /></el-icon>
            </div>
            <span class="action-label">{{ t('nav.customer') }}</span>
          </div>
          <div class="action-item" @click="$router.push('/product')">
            <div class="action-icon" style="background: var(--cpm-success-bg); color: var(--cpm-success);">
              <el-icon size="20"><Box /></el-icon>
            </div>
            <span class="action-label">{{ t('nav.product') }}</span>
          </div>
          <div class="action-item">
            <div class="action-icon" style="background: var(--cpm-warning-bg); color: var(--cpm-warning);">
              <el-icon size="20"><Document /></el-icon>
            </div>
            <span class="action-label">{{ t('nav.salesReport') }}</span>
          </div>
          <div class="action-item">
            <div class="action-icon" style="background: var(--cpm-error-bg); color: var(--cpm-error);">
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
          <el-icon size="18" color="var(--cpm-success)"><CircleCheckFilled /></el-icon>
          <span class="status-label">{{ t('common.backendService') }}</span>
          <span class="status-value status-online">{{ t('common.running') }}</span>
        </div>
        <div class="status-item">
          <el-icon size="18" color="var(--cpm-success)"><CircleCheckFilled /></el-icon>
          <span class="status-label">{{ t('common.databaseConnection') }}</span>
          <span class="status-value status-online">{{ t('common.connected') }}</span>
        </div>
        <div class="status-item">
          <el-icon size="18" color="var(--cpm-info)"><InfoFilled /></el-icon>
          <span class="status-label">{{ t('common.systemVersion') }}</span>
          <span class="status-value">v1.0.0</span>
        </div>
        <div class="status-item">
          <el-icon size="18" color="var(--cpm-info)"><Calendar /></el-icon>
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
import { getMyPendingTasks } from '@/api/approval'
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
  { text: 'New customer "Huawei Technologies"', time: '2 hours ago', color: 'var(--cpm-info)' },
  { text: 'Updated product "SUS304 Stainless Steel Sheet"', time: '4 hours ago', color: 'var(--cpm-success)' },
  { text: 'Deleted customer "Test Customer 001"', time: 'Yesterday', color: 'var(--cpm-error)' },
  { text: 'System backup completed', time: 'Yesterday', color: 'var(--cpm-text-muted)' },
  { text: 'New product "AL6061 Aluminum Profile"', time: '3 days ago', color: 'var(--cpm-info)' }
])

const refreshData = async () => {
  try {
    const [customerRes, productRes, taskRes] = await Promise.all([
      request.get('/customer/list', { params: { pageNum: 1, pageSize: 1 } }),
      request.get('/product/list', { params: { pageNum: 1, pageSize: 1 } }),
      getMyPendingTasks()
    ])
    stats.value.customers = customerRes?.data?.total || 0
    stats.value.products = productRes?.data?.total || 0
    stats.value.deals = 0
    stats.value.pending = taskRes?.data?.length || 0
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
  padding: var(--cpm-space-6);
}

.welcome-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--cpm-space-8);
  padding-bottom: var(--cpm-space-6);
  border-bottom: 1px solid var(--cpm-border);
}

.welcome-title {
  font-size: var(--cpm-text-display);
  font-weight: 700;
  color: var(--cpm-text-primary);
  margin: 0 0 var(--cpm-space-1);
  letter-spacing: -0.5px;
  line-height: 1.2;
}

.welcome-subtitle {
  font-size: var(--cpm-text-body);
  color: var(--cpm-text-secondary);
  margin: 0;
}

.welcome-actions {
  display: flex;
  gap: var(--cpm-space-2);
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--cpm-space-4);
  margin-bottom: var(--cpm-space-6);
}

.dashboard-stat {
  display: flex;
  align-items: center;
  gap: var(--cpm-space-4);
  padding: var(--cpm-space-6);
}

.stat-clickable {
  cursor: pointer;
  transition: transform var(--cpm-duration-normal) var(--cpm-easing-default), box-shadow var(--cpm-duration-normal) var(--cpm-easing-default);
}

.stat-clickable:hover {
  transform: translateY(-2px);
  box-shadow: var(--cpm-shadow-md);
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--cpm-radius-md);
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
  font-size: var(--cpm-text-small);
  font-weight: 600;
  padding: 2px 8px;
  border-radius: var(--cpm-radius-full);
}

.trend-up {
  background: var(--cpm-success-bg);
  color: var(--cpm-success);
}

.trend-down {
  background: var(--cpm-error-bg);
  color: var(--cpm-error);
}

.dashboard-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--cpm-space-4);
  margin-bottom: var(--cpm-space-6);
}

.content-card {
  background: var(--cpm-bg-card);
  border-radius: var(--cpm-radius-md);
  border: 1px solid var(--cpm-border-light);
  box-shadow: var(--cpm-shadow-sm);
  padding: var(--cpm-space-6);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--cpm-space-4);
}

.card-title {
  font-size: var(--cpm-text-body);
  font-weight: 600;
  color: var(--cpm-text-primary);
  margin: 0;
  display: flex;
  align-items: center;
  gap: var(--cpm-space-2);
}

.view-all {
  font-size: 13px;
  font-weight: 500;
}

.action-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--cpm-space-4);
}

.action-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--cpm-space-2);
  padding: var(--cpm-space-6);
  border-radius: var(--cpm-radius-md);
  cursor: pointer;
  transition: background var(--cpm-duration-normal) var(--cpm-easing-default);
  border: 1px solid transparent;
}

.action-item:hover {
  background: var(--cpm-bg-hover);
  border-color: var(--cpm-border-light);
}

.action-icon {
  width: 44px;
  height: 44px;
  border-radius: var(--cpm-radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
}

.action-label {
  font-size: var(--cpm-text-small);
  font-weight: 500;
  color: var(--cpm-text-secondary);
}

.activity-list {
  display: flex;
  flex-direction: column;
  gap: var(--cpm-space-4);
}

.activity-item {
  display: flex;
  align-items: flex-start;
  gap: var(--cpm-space-2);
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
  font-size: var(--cpm-text-body);
  color: var(--cpm-text-primary);
  margin: 0 0 2px;
  line-height: 1.4;
}

.activity-time {
  font-size: var(--cpm-text-small);
  color: var(--cpm-text-secondary);
}

.system-status {
  margin-top: var(--cpm-space-6);
}

.status-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--cpm-space-6);
}

.status-item {
  display: flex;
  align-items: center;
  gap: var(--cpm-space-2);
  padding: var(--cpm-space-4);
  background: var(--cpm-bg-page);
  border-radius: var(--cpm-radius-md);
}

.status-label {
  font-size: var(--cpm-text-small);
  color: var(--cpm-text-secondary);
  flex: 1;
}

.status-value {
  font-size: var(--cpm-text-small);
  font-weight: 600;
  color: var(--cpm-text-primary);
}

.status-online {
  color: var(--cpm-success);
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
    gap: var(--cpm-space-4);
  }
  .status-grid {
    grid-template-columns: 1fr;
  }
}
</style>
