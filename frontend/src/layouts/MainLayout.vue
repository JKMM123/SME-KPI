<template>
  <q-layout view="lHh Lpr lFf">
    <q-header elevated>
      <q-toolbar>
        <q-btn flat round icon="menu" aria-label="Toggle menu" @click="drawerOpen = !drawerOpen" />
        <q-toolbar-title class="page-title">{{ currentPageTitle }}</q-toolbar-title>
        <ThemeToggle />
      </q-toolbar>
    </q-header>

    <q-drawer v-model="drawerOpen" show-if-above :width="240" :breakpoint="600" bordered>
      <q-scroll-area class="fit">
        <div class="q-pa-md q-pb-sm">
          <div class="text-h6 text-weight-bold text-primary">SME KPI</div>
          <div class="text-caption text-grey-6">Dashboard</div>
        </div>

        <q-separator />

        <q-list padding>
          <q-item
            v-for="link in navLinks"
            :key="link.to"
            v-ripple
            clickable
            :active="$route.path === link.to"
            active-class="text-primary"
            :to="link.to"
          >
            <q-item-section avatar>
              <q-icon :name="link.icon" />
            </q-item-section>
            <q-item-section>{{ link.label }}</q-item-section>
          </q-item>
        </q-list>

        <q-separator class="q-mt-auto" />

        <q-list padding>
          <q-item v-ripple clickable @click="handleLogout">
            <q-item-section avatar>
              <q-icon name="logout" />
            </q-item-section>
            <q-item-section>Logout</q-item-section>
          </q-item>
        </q-list>
      </q-scroll-area>
    </q-drawer>

    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ThemeToggle from 'src/components/ThemeToggle.vue'
import { useAuthStore } from 'src/stores/authStore'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const drawerOpen = ref(true)

const navLinks = [
  { to: '/dashboard', label: 'Dashboard', icon: 'dashboard' },
  { to: '/sales', label: 'Sales', icon: 'point_of_sale' },
  { to: '/expenses', label: 'Expenses', icon: 'receipt_long' },
  { to: '/products', label: 'Products', icon: 'inventory_2' },
  { to: '/customers', label: 'Customers', icon: 'people' },
  { to: '/reports', label: 'Reports', icon: 'bar_chart' }
]

const pageTitles: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/sales': 'Sales',
  '/expenses': 'Expenses',
  '/products': 'Products',
  '/customers': 'Customers',
  '/reports': 'Reports'
}

const currentPageTitle = computed(() => pageTitles[route.path] ?? 'SME KPI Dashboard')

async function handleLogout() {
  await authStore.logout()
  await router.push('/login')
}
</script>
