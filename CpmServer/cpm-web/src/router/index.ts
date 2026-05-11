import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // 登录页 - 公开访问
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/login/LoginView.vue'),
      meta: { public: true }
    },
    // 主布局 - 需要登录
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
          meta: { title: '客户管理' }
        },
        {
          path: 'product',
          name: 'product',
          component: () => import('@/views/product/ProductList.vue'),
          meta: { title: '产品管理' }
        },
        {
          path: 'opportunity',
          name: 'opportunity',
          component: () => import('@/views/quotation/OpportunityList.vue'),
          meta: { title: '商机管理' }
        },
        {
          path: 'quotation/list',
          name: 'quotationList',
          component: () => import('@/views/quotation/QuotationList.vue'),
          meta: { title: '报价单管理' }
        },
        {
          path: 'quotation/detail/:id',
          name: 'quotationDetail',
          component: () => import('@/views/quotation/QuotationDetail.vue'),
          meta: { title: '报价单详情' }
        },
        {
          path: 'system/settings',
          name: 'systemSettings',
          component: () => import('@/views/system/SystemSettings.vue'),
          meta: { title: '系统设置', adminOnly: true }
        },
        {
          path: 'profile',
          name: 'profile',
          component: () => import('@/views/profile/ProfileView.vue'),
          meta: { title: '个人资料' }
        },
        {
          path: 'mfg/process',
          name: 'mfgProcess',
          component: () => import('@/views/mfg/MfgProcessManage.vue'),
          meta: { title: '工艺管理' }
        },
        // 旧路由重定向到系统设置
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

// 路由守卫
router.beforeEach((to) => {
  const token = localStorage.getItem('token')
  if (!to.meta.public && !token) {
    return '/login'
  }
  if (to.meta.adminOnly && token) {
    const userInfo = JSON.parse(localStorage.getItem('userInfo') || '{}')
    const roles = userInfo.roles || []
    if (!roles.some((r: string) => r.toUpperCase() === 'ADMIN')) {
      return '/home'
    }
  }
})

export default router
