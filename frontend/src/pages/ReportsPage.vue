<template>
  <q-page class="q-pa-lg">
    <div class="page-title q-mb-lg no-print">Reports</div>

    <!-- Month/Year Selector -->
    <q-card flat bordered class="q-mb-lg no-print">
      <q-card-section>
        <div class="text-subtitle1 text-weight-medium q-mb-md">Generate Monthly Report</div>
        <div class="row q-col-gutter-md items-center">
          <div class="col-12 col-sm-4 col-md-3">
            <q-select
              v-model="selectedMonth"
              :options="monthOptions"
              label="Month"
              outlined
              dense
              emit-value
              map-options
            />
          </div>
          <div class="col-12 col-sm-4 col-md-3">
            <q-input
              v-model.number="selectedYear"
              label="Year"
              type="number"
              outlined
              dense
              :rules="[val => (val >= 2000 && val <= 2100) || 'Enter a valid year (2000–2100)']"
            />
          </div>
          <div class="col-12 col-sm-4 col-md-auto">
            <q-btn
              color="primary"
              label="Generate Report"
              icon="bar_chart"
              unelevated
              no-wrap
              :loading="loading"
              @click="fetchReport"
            />
          </div>
        </div>
      </q-card-section>
    </q-card>

    <!-- Report Display -->
    <template v-if="report">
      <!-- Print header (only visible when printing) -->
      <div class="print-only print-header q-mb-md">
        <div class="text-h5 text-weight-bold">Monthly Financial Report</div>
      </div>

      <div class="row items-center justify-between q-mb-md">
        <div class="text-h6">
          {{ monthLabel }} {{ report.year }}
        </div>
        <q-btn
          class="no-print"
          color="primary"
          icon="print"
          label="Download / Print"
          outline
          @click="printReport"
        />
      </div>

      <!-- Summary Cards -->
      <div class="row q-col-gutter-md q-mb-lg">
        <div class="col-12 col-md-4">
          <q-card flat bordered>
            <q-card-section>
              <div class="text-caption text-grey-6">Total Sales</div>
              <div class="text-h5 text-positive text-weight-bold">{{ formatCurrency(report.totalSales) }}</div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-md-4">
          <q-card flat bordered>
            <q-card-section>
              <div class="text-caption text-grey-6">Total Expenses</div>
              <div class="text-h5 text-negative text-weight-bold">{{ formatCurrency(report.totalExpenses) }}</div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-md-4">
          <q-card flat bordered>
            <q-card-section>
              <div class="text-caption text-grey-6">Net Profit</div>
              <div
                class="text-h5 text-weight-bold"
                :class="report.netProfit >= 0 ? 'text-primary' : 'text-negative'"
              >
                {{ formatCurrency(report.netProfit) }}
              </div>
            </q-card-section>
          </q-card>
        </div>
      </div>

      <!-- Expenses by Category -->
      <q-card flat bordered>
        <q-card-section>
          <div class="text-subtitle1 text-weight-medium q-mb-md">Expenses by Category</div>
          <div v-if="report.expensesByCategory.length === 0" class="text-grey-5 text-center q-pa-md">
            No expenses recorded for this month.
          </div>
          <q-list v-else bordered separator>
            <q-item v-for="cat in report.expensesByCategory" :key="cat.category">
              <q-item-section>
                <q-item-label>{{ cat.category }}</q-item-label>
              </q-item-section>
              <q-item-section side>
                <q-item-label class="text-negative text-weight-medium">
                  {{ formatCurrency(cat.total) }}
                </q-item-label>
              </q-item-section>
            </q-item>
          </q-list>
        </q-card-section>
      </q-card>
    </template>

    <!-- Empty state -->
    <div v-else class="text-center q-pa-xl text-grey-5 no-print">
      <q-icon name="bar_chart" size="4rem" />
      <div class="q-mt-md">Select a month and year, then click "Generate Report"</div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { reportsApi, type MonthlyReport } from 'src/services/apiService'
import { useQuasar } from 'quasar'

const $q = useQuasar()

const selectedMonth = ref(new Date().getMonth() + 1)
const selectedYear = ref(new Date().getFullYear())
const report = ref<MonthlyReport | null>(null)
const loading = ref(false)

const monthOptions = [
  { label: 'January', value: 1 },
  { label: 'February', value: 2 },
  { label: 'March', value: 3 },
  { label: 'April', value: 4 },
  { label: 'May', value: 5 },
  { label: 'June', value: 6 },
  { label: 'July', value: 7 },
  { label: 'August', value: 8 },
  { label: 'September', value: 9 },
  { label: 'October', value: 10 },
  { label: 'November', value: 11 },
  { label: 'December', value: 12 },
]

const monthLabel = computed(
  () => monthOptions.find(m => m.value === selectedMonth.value)?.label ?? ''
)

function formatCurrency(value: number): string {
  return `$${value.toFixed(2)}`
}

function printReport() {
  window.print()
}

async function fetchReport() {
  loading.value = true
  try {
    const response = await reportsApi.getMonthlyReport(selectedYear.value, selectedMonth.value)
    report.value = response.data
  } catch (err: unknown) {
    const msg =
      (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
      'Failed to generate report'
    $q.notify({ type: 'negative', message: msg })
  } finally {
    loading.value = false
  }
}
</script>

<style>
@media print {
  .no-print {
    display: none !important;
  }

  .print-only {
    display: block !important;
  }

  body {
    background: white !important;
    color: black !important;
  }

  .q-card {
    border: 1px solid #ccc !important;
    box-shadow: none !important;
  }
}

@media screen {
  .print-only {
    display: none;
  }
}
</style>
