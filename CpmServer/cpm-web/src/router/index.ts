import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/login/LoginView.vue'),
      meta: { public: true }
    },
    {
      path: '/',
      component: () => import('@/views/layout/MainLayout.vue'),
      redirect: '/home',
      children: [
        {
          path: 'home',
          name: 'home',
          component: () => import('@/views/HomeView.vue')
        },
        {
          path: 'customer',
          name: 'customer',
          component: () => import('@/views/customer/CustomerList.vue'),
          meta: { title: 'Customer' }
        },
        {
          path: 'product',
          name: 'product',
          component: () => import('@/views/product/ProductList.vue'),
          meta: { title: 'Product' }
        },
        {
          path: 'opportunity',
          name: 'opportunity',
          component: () => import('@/views/quotation/OpportunityList.vue'),
          meta: { title: 'Opportunity' }
        },
        {
          path: 'quotation/list',
          name: 'quotationList',
          component: () => import('@/views/quotation/QuotationList.vue'),
          meta: { title: 'Quotation' }
        },
        {
          path: 'quotation/detail/:id',
          name: 'quotationDetail',
          component: () => import('@/views/quotation/QuotationDetail.vue'),
          meta: { title: 'Quotation Detail' }
        },
        {
          path: 'system/settings',
          name: 'systemSettings',
          component: () => import('@/views/system/SystemSettings.vue'),
          meta: { title: 'System Settings', permission: 'settings.view' }
        },

        {
          path: 'profile',
          name: 'profile',
          component: () => import('@/views/profile/ProfileView.vue'),
          meta: { title: 'Profile' }
        },
        {
          path: 'mfg/process',
          name: 'mfgProcess',
          component: () => import('@/views/mfg/MfgProcessManage.vue'),
          meta: { title: 'Mfg Process' }
        },
        {
          path: 'pm/trace',
          name: 'productTrace',
          component: () => import('@/views/pm/ProductTraceList.vue'),
          meta: { title: 'Product Trace' }
        },
        {
          path: 'pm/trace/create',
          name: 'productTraceCreate',
          component: () => import('@/views/pm/ProductTraceDetail.vue'),
          meta: { title: 'Create Product Trace' }
        },
        {
          path: 'pm/trace/edit/:id',
          name: 'productTraceEdit',
          component: () => import('@/views/pm/ProductTraceDetail.vue'),
          meta: { title: 'Edit Product Trace' }
        },
        {
          path: 'pm/actual-cycle-time',
          name: 'actualCycleTime',
          component: () => import('@/views/pm/ActualCycleTimeManage.vue'),
          meta: { title: 'Actual Cycle Time' }
        },
        {
          path: 'approval/center',
          name: 'approvalCenter',
          component: () => import('@/views/approval/ApprovalCenter.vue'),
          meta: { title: 'Approval Center' }
        },
        {
          path: 'approval/config',
          redirect: '/system/settings'
        },
        {
          path: 'system/user',
          redirect: '/system/settings'
        }
      ]
    }
  ],
})

let cachedUserInfo: any = null
let cachedUserInfoStr = ''

function getCachedUserInfo() {
  const raw = localStorage.getItem('userInfo') || '{}'
  if (raw !== cachedUserInfoStr) {
    cachedUserInfoStr = raw
    try {
      cachedUserInfo = JSON.parse(raw)
    } catch {
      cachedUserInfo = {}
    }
  }
  return cachedUserInfo
}

router.beforeEach((to) => {
  const token = localStorage.getItem('token')
  if (!to.meta.public && !token) {
    return '/login'
  }
  if (to.meta.permission && token) {
    const userInfo = getCachedUserInfo()
    const roles = userInfo?.roles || []
    const permissions = userInfo?.permissions || []
    const required = to.meta.permission as string
    const isAdmin = roles.some((r: string) => r.toUpperCase() === 'ADMIN')
    if (!isAdmin && !permissions.includes(required)) {
      return '/home'
    }
  }
})

// Aggressive cleanup after each route change to mitigate Element Plus
// el-table ResizeObserver leaks (known issue in 2.13.x)
router.afterEach(() => {
  requestAnimationFrame(() => {
    document.querySelectorAll('.el-table').forEach((el) => {
      const table = el as any
      // Disconnect any ResizeObserver instances Element Plus may have left behind
      const obs = table.__resizeObserver__ || table.resizeObserver || table._resizeObserver
      if (obs && typeof obs.disconnect === 'function') {
        try { obs.disconnect() } catch {}
      }
    })
  })
})

export default router

