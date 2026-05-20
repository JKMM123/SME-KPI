<template>
  <q-layout view="hHh lpR fFf">
    <q-page-container>
      <q-page class="auth-page flex flex-center">
        <div class="theme-toggle-corner">
          <ThemeToggle />
        </div>

        <q-card class="auth-card" flat bordered>
          <q-card-section class="text-center q-pb-sm">
            <div class="text-h5 text-weight-bold text-primary">SME KPI</div>
            <div class="text-caption text-grey-6">Dashboard</div>
          </q-card-section>

          <router-view />
        </q-card>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import ThemeToggle from 'src/components/ThemeToggle.vue'
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  position: relative;
}

.auth-card {
  width: 100%;
  max-width: 420px;
}

.theme-toggle-corner {
  position: fixed;
  top: 12px;
  right: 12px;
  z-index: 1000;
}
</style>
