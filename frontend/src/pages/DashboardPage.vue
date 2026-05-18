<template>
  <q-page class="q-pa-lg">
    <div class="row items-center justify-between q-mb-lg">
      <div class="page-title">Dashboard</div>
      <q-btn
        flat
        round
        icon="refresh"
        color="primary"
        :loading="kpiLoading || chartLoading"
        @click="fetchData"
      >
        <q-tooltip>Refresh</q-tooltip>
      </q-btn>
    </div>

    <!-- KPI Cards Row 1 -->
    <div class="row q-col-gutter-md q-mb-md">
      <div class="col-12 col-sm-6 col-md-3">
        <q-card flat bordered class="metric-card">
          <q-card-section>
            <div class="text-caption text-grey-6 q-mb-xs">Total Revenue</div>
            <div class="text-h5 text-weight-bold text-positive">{{ formatCurrency(kpi.totalRevenue) }}</div>
            <div class="row items-center q-mt-sm">
              <q-icon name="trending_up" size="sm" color="positive" />
              <span class="text-caption text-grey-6 q-ml-xs">All time</span>
            </div>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-sm-6 col-md-3">
        <q-card flat bordered class="metric-card">
          <q-card-section>
            <div class="text-caption text-grey-6 q-mb-xs">Total Expenses</div>
            <div class="text-h5 text-weight-bold text-negative">{{ formatCurrency(kpi.totalExpenses) }}</div>
            <div class="row items-center q-mt-sm">
              <q-icon name="receipt_long" size="sm" color="negative" />
              <span class="text-caption text-grey-6 q-ml-xs">All time</span>
            </div>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-sm-6 col-md-3">
        <q-card flat bordered class="metric-card">
          <q-card-section>
            <div class="text-caption text-grey-6 q-mb-xs">Net Profit</div>
            <div
              class="text-h5 text-weight-bold"
              :class="kpi.netProfit >= 0 ? 'text-primary' : 'text-negative'"
            >
              {{ formatCurrency(kpi.netProfit) }}
            </div>
            <div class="row items-center q-mt-sm">
              <q-icon name="account_balance_wallet" size="sm" color="primary" />
              <span class="text-caption text-grey-6 q-ml-xs">Revenue − Expenses</span>
            </div>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-sm-6 col-md-3">
        <q-card flat bordered class="metric-card">
          <q-card-section>
            <div class="text-caption text-grey-6 q-mb-xs">Monthly Growth</div>
            <div
              class="text-h5 text-weight-bold"
              :class="kpi.growthPercentage >= 0 ? 'text-info' : 'text-negative'"
            >
              {{ kpi.growthPercentage >= 0 ? '+' : '' }}{{ kpi.growthPercentage.toFixed(2) }}%
            </div>
            <div class="row items-center q-mt-sm">
              <q-icon name="show_chart" size="sm" color="info" />
              <span class="text-caption text-grey-6 q-ml-xs">vs last month</span>
            </div>
          </q-card-section>
        </q-card>
      </div>
    </div>

    <!-- KPI Cards Row 2 -->
    <div class="row q-col-gutter-md q-mb-lg">
      <div class="col-12 col-sm-6">
        <q-card flat bordered class="metric-card">
          <q-card-section>
            <div class="text-caption text-grey-6 q-mb-xs">Top Selling Product</div>
            <div class="text-h6 text-weight-bold text-primary ellipsis">
              {{ kpi.topSellingProductName ?? 'No sales yet' }}
            </div>
            <div class="row items-center q-mt-sm">
              <q-icon name="emoji_events" size="sm" color="amber" />
              <span class="text-caption text-grey-6 q-ml-xs">By sales count</span>
            </div>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-sm-6">
        <q-card flat bordered class="metric-card">
          <q-card-section>
            <div class="text-caption text-grey-6 q-mb-xs">Low Stock Alert</div>
            <div
              class="text-h5 text-weight-bold"
              :class="kpi.lowStockAlertCount > 0 ? 'text-negative' : 'text-positive'"
            >
              {{ kpi.lowStockAlertCount }}
              <span class="text-caption text-weight-regular text-grey-6">
                {{ kpi.lowStockAlertCount === 1 ? 'product' : 'products' }}
              </span>
            </div>
            <div class="row items-center q-mt-sm">
              <q-icon name="inventory" size="sm" :color="kpi.lowStockAlertCount > 0 ? 'negative' : 'positive'" />
              <span class="text-caption text-grey-6 q-ml-xs">Stock &lt; 5 units</span>
            </div>
          </q-card-section>
        </q-card>
      </div>
    </div>

    <!-- Charts Row -->
    <div class="row q-col-gutter-md">
      <!-- Revenue Line Chart -->
      <div class="col-12 col-md-8">
        <q-card flat bordered>
          <q-card-section>
            <div class="text-subtitle1 text-weight-medium q-mb-md">Revenue Overview — Last 6 Months</div>
            <div v-if="chartLoading" class="flex flex-center q-pa-xl">
              <q-spinner size="3rem" color="primary" />
            </div>
            <canvas v-show="!chartLoading" ref="lineChartCanvas" style="max-height: 320px;" />
          </q-card-section>
        </q-card>
      </div>

      <!-- Expense Distribution Doughnut -->
      <div class="col-12 col-md-4">
        <q-card flat bordered>
          <q-card-section>
            <div class="text-subtitle1 text-weight-medium q-mb-md">Expense Distribution</div>
            <div v-if="chartLoading" class="flex flex-center q-pa-xl">
              <q-spinner size="3rem" color="primary" />
            </div>
            <div v-else-if="expenseDistribution.length === 0" class="flex flex-center q-pa-xl text-grey-5">
              <div class="text-center">
                <q-icon name="pie_chart" size="3rem" />
                <div class="q-mt-sm">No expense data</div>
              </div>
            </div>
            <canvas
              ref="doughnutChartCanvas"
              v-show="!chartLoading && expenseDistribution.length > 0"
              style="max-height: 280px;"
            />
          </q-card-section>
        </q-card>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, nextTick } from 'vue'
import { dashboardApi, type KpiSummary, type ExpenseCategory } from 'src/services/apiService'
import { Chart, registerables } from 'chart.js'
import { useQuasar } from 'quasar'

Chart.register(...registerables)

const $q = useQuasar()

const lineChartCanvas = ref<HTMLCanvasElement | null>(null)
const doughnutChartCanvas = ref<HTMLCanvasElement | null>(null)
let lineChartInstance: Chart | null = null
let doughnutChartInstance: Chart | null = null

const kpiLoading = ref(true)
const chartLoading = ref(true)
const expenseDistribution = ref<ExpenseCategory[]>([])

const kpi = ref<KpiSummary>({
  totalRevenue: 0,
  totalExpenses: 0,
  netProfit: 0,
  growthPercentage: 0,
  lowStockAlertCount: 0,
})

// Stable per-category colors so slices never change color between renders
const CATEGORY_COLORS: Record<string, string> = {
  'Operating Costs': '#0f766e',
  'Marketing':       '#3b82f6',
  'Salaries':        '#f59e0b',
  'Inventory':       '#ef4444',
  'Utilities':       '#8b5cf6',
  'Other':           '#10b981',
}
const FALLBACK_COLORS = ['#f97316', '#6366f1', '#06b6d4', '#84cc16']

function formatCurrency(value: number): string {
  return `$${value.toFixed(2)}`
}

async function fetchData() {
  kpiLoading.value = true
  chartLoading.value = true
  try {
    const [kpiRes, chartRes, distRes] = await Promise.all([
      dashboardApi.getKpiSummary(),
      dashboardApi.getChartData(),
      dashboardApi.getExpenseDistribution(),
    ])
    kpi.value = kpiRes.data
    renderLineChart(chartRes.data.monthlyData)
    expenseDistribution.value = distRes.data
    // Ensure refs are resolved before drawing charts
    await nextTick()
    renderDoughnutChart(distRes.data)
  } catch {
    $q.notify({ type: 'negative', message: 'Failed to load dashboard data' })
  } finally {
    kpiLoading.value = false
    chartLoading.value = false
  }
}

function renderLineChart(
  monthlyData: { month: string; sales: number; expenses: number; profit: number }[]
) {
  if (!lineChartCanvas.value) return
  lineChartInstance?.destroy()

  const ctx = lineChartCanvas.value.getContext('2d')
  if (!ctx) return

  lineChartInstance = new Chart(ctx, {
    type: 'line',
    data: {
      labels: monthlyData.map(d => d.month),
      datasets: [
        {
          label: 'Sales',
          data: monthlyData.map(d => d.sales),
          borderColor: '#22c55e',
          backgroundColor: 'rgba(34, 197, 94, 0.1)',
          tension: 0.4,
          fill: true,
        },
        {
          label: 'Expenses',
          data: monthlyData.map(d => d.expenses),
          borderColor: '#ef4444',
          backgroundColor: 'rgba(239, 68, 68, 0.1)',
          tension: 0.4,
          fill: true,
        },
        {
          label: 'Profit',
          data: monthlyData.map(d => d.profit),
          borderColor: '#3b82f6',
          backgroundColor: 'rgba(59, 130, 246, 0.1)',
          tension: 0.4,
          fill: true,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: true,
      plugins: {
        legend: { position: 'top' },
        tooltip: {
          callbacks: {
            label: ctx => ` ${ctx.dataset.label}: $${(ctx.parsed.y as number).toFixed(2)}`,
          },
        },
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: { callback: val => `$${val}` },
        },
      },
    },
  })
}

function renderDoughnutChart(distribution: ExpenseCategory[]) {
  if (!doughnutChartCanvas.value || distribution.length === 0) return
  doughnutChartInstance?.destroy()

  const ctx = doughnutChartCanvas.value.getContext('2d')
  if (!ctx) return

  const total = distribution.reduce((sum, d) => sum + d.total, 0)

  doughnutChartInstance = new Chart(ctx, {
    type: 'doughnut',
    data: {
      labels: distribution.map(d => d.category),
      datasets: [
        {
          data: distribution.map(d => d.total),
          backgroundColor: distribution.map((d, i) =>
            CATEGORY_COLORS[d.category] ?? FALLBACK_COLORS[i % FALLBACK_COLORS.length]
          ),
          borderWidth: 2,
          borderColor: 'transparent',
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: true,
      plugins: {
        legend: { position: 'bottom', labels: { boxWidth: 12 } },
        tooltip: {
          callbacks: {
            label: ctx => {
              const pct = total > 0 ? ((ctx.parsed / total) * 100).toFixed(1) : '0.0'
              return ` ${ctx.label}: $${(ctx.parsed as number).toFixed(2)} (${pct}%)`
            },
          },
        },
      },
    },
  })
}

onMounted(fetchData)
onBeforeUnmount(() => {
  lineChartInstance?.destroy()
  doughnutChartInstance?.destroy()
})
</script>

<style scoped>
.metric-card {
  height: 100%;
}
</style>
