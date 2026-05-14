import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import './styles/salesforce-theme.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
// @ts-ignore
import zhCn from 'element-plus/dist/locale/zh-cn.mjs'
// @ts-ignore
import en from 'element-plus/dist/locale/en.mjs'

import App from './App.vue'
import router from './router'
import { useUiControlStore } from './stores/uiControl'

const app = createApp(App)

for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

const savedLocale = localStorage.getItem('locale') || 'zh'
const elementLocale = savedLocale === 'zh' ? zhCn : en

app.use(createPinia())
app.use(router)

// Initialize UI control settings
const uiStore = useUiControlStore()
uiStore.applyAll()

app.use(ElementPlus, {
  locale: elementLocale,
  zIndex: 3000,
  size: 'default',
  button: {
    autoInsertSpace: false,
  },
})

app.mount('#app')



