<template>
  <q-page class="q-pa-lg">
    <div class="row items-center justify-between q-mb-md">
      <div class="page-title">Expenses</div>
      <q-btn color="negative" icon="add" label="Add Expense" @click="openAddDialog" />
    </div>

    <q-table
      :rows="expenses"
      :columns="columns"
      :loading="loading"
      row-key="id"
      flat
      bordered
      :no-data-label="'No expenses yet. Click \'Add Expense\' to get started.'"
    >
      <template #body-cell-amount="props">
        <q-td :props="props" class="text-negative text-weight-medium">
          ${{ props.row.amount.toFixed(2) }}
        </q-td>
      </template>

      <template #body-cell-date="props">
        <q-td :props="props">
          {{ formatDate(props.row.date) }}
        </q-td>
      </template>

      <template #body-cell-category="props">
        <q-td :props="props">
          <q-badge :color="categoryColor(props.row.category)" :label="props.row.category" />
        </q-td>
      </template>

      <template #body-cell-actions="props">
        <q-td :props="props">
          <q-btn flat round dense icon="edit" color="primary" @click="openEditDialog(props.row)" />
          <q-btn flat round dense icon="delete" color="negative" @click="confirmDelete(props.row)" />
        </q-td>
      </template>
    </q-table>

    <!-- Add/Edit Dialog -->
    <q-dialog v-model="showDialog" persistent>
      <q-card style="min-width: 420px">
        <q-card-section class="row items-center">
          <div class="text-h6">{{ isEditing ? 'Edit Expense' : 'Add Expense' }}</div>
          <q-space />
          <q-btn icon="close" flat round dense @click="showDialog = false" />
        </q-card-section>

        <q-separator />

        <q-card-section>
          <q-form @submit.prevent="saveExpense" class="q-gutter-md">
            <q-input
              v-model.number="form.amount"
              label="Amount ($)"
              type="number"
              step="0.01"
              min="0.01"
              outlined
              :rules="[val => val > 0 || 'Amount must be greater than 0']"
            />
            <q-input
              v-model="form.date"
              label="Date"
              type="date"
              outlined
              :rules="[val => !!val || 'Date is required', val => !isFutureDate(val) || 'Date cannot be in the future']"
            />
            <q-select
              v-model="form.category"
              :options="categoryOptions"
              label="Category"
              outlined
              :rules="[val => !!val || 'Category is required']"
            />
            <q-input
              v-model="form.description"
              label="Description"
              type="textarea"
              rows="3"
              outlined
              maxlength="500"
            />
            <div class="row q-gutter-sm justify-end">
              <q-btn label="Cancel" flat @click="showDialog = false" />
              <q-btn label="Save" type="submit" color="negative" :loading="saving" />
            </div>
          </q-form>
        </q-card-section>
      </q-card>
    </q-dialog>

    <!-- Delete Confirmation -->
    <q-dialog v-model="showDeleteDialog">
      <q-card>
        <q-card-section>
          <div class="text-h6">Confirm Delete</div>
        </q-card-section>
        <q-card-section>
          Are you sure you want to delete this expense for <strong>${{ currentExpense?.amount.toFixed(2) }}</strong>?
        </q-card-section>
        <q-card-actions align="right">
          <q-btn label="Cancel" flat @click="showDeleteDialog = false" />
          <q-btn label="Delete" color="negative" :loading="saving" @click="deleteExpense" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { expensesApi, type Expense, type ExpenseRequest } from 'src/services/apiService'
import { useQuasar } from 'quasar'

const $q = useQuasar()

const expenses = ref<Expense[]>([])
const loading = ref(false)
const saving = ref(false)
const showDialog = ref(false)
const showDeleteDialog = ref(false)
const isEditing = ref(false)
const currentExpense = ref<Expense | null>(null)

const categoryOptions = ['Operating Costs', 'Marketing', 'Salaries', 'Inventory', 'Utilities', 'Other']

const categoryColors: Record<string, string> = {
  'Operating Costs': 'blue-7',
  'Marketing': 'purple-7',
  'Salaries': 'teal-7',
  'Inventory': 'orange-7',
  'Utilities': 'cyan-7',
  'Other': 'grey-7',
}

function categoryColor(cat: string) {
  return categoryColors[cat] ?? 'grey-7'
}

const form = ref<ExpenseRequest>({
  amount: 0,
  date: todayString(),
  category: '',
  description: '',
})

const columns = [
  { name: 'date', label: 'Date', field: 'date', sortable: true, align: 'left' as const },
  { name: 'amount', label: 'Amount', field: 'amount', sortable: true, align: 'left' as const },
  { name: 'category', label: 'Category', field: 'category', sortable: true, align: 'left' as const },
  { name: 'description', label: 'Description', field: 'description', align: 'left' as const },
  { name: 'actions', label: 'Actions', field: 'actions', align: 'center' as const },
]

function todayString() {
  return new Date().toISOString().split('T')[0]
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString()
}

function isFutureDate(dateStr: string) {
  return new Date(dateStr) > new Date()
}

async function fetchExpenses() {
  loading.value = true
  try {
    const response = await expensesApi.getAll()
    expenses.value = response.data
  } catch {
    $q.notify({ type: 'negative', message: 'Failed to load expenses' })
  } finally {
    loading.value = false
  }
}

function openAddDialog() {
  isEditing.value = false
  currentExpense.value = null
  form.value = { amount: 0, date: todayString(), category: '', description: '' }
  showDialog.value = true
}

function openEditDialog(expense: Expense) {
  isEditing.value = true
  currentExpense.value = expense
  form.value = {
    amount: expense.amount,
    date: expense.date.split('T')[0],
    category: expense.category,
    description: expense.description,
  }
  showDialog.value = true
}

async function saveExpense() {
  saving.value = true
  try {
    if (isEditing.value && currentExpense.value) {
      await expensesApi.update(currentExpense.value.id, form.value)
      $q.notify({ type: 'positive', message: 'Expense updated successfully' })
    } else {
      await expensesApi.create(form.value)
      $q.notify({ type: 'positive', message: 'Expense created successfully' })
    }
    showDialog.value = false
    await fetchExpenses()
  } catch {
    $q.notify({ type: 'negative', message: 'Failed to save expense' })
  } finally {
    saving.value = false
  }
}

function confirmDelete(expense: Expense) {
  currentExpense.value = expense
  showDeleteDialog.value = true
}

async function deleteExpense() {
  if (!currentExpense.value) return
  saving.value = true
  try {
    await expensesApi.delete(currentExpense.value.id)
    $q.notify({ type: 'positive', message: 'Expense deleted' })
    showDeleteDialog.value = false
    await fetchExpenses()
  } catch {
    $q.notify({ type: 'negative', message: 'Failed to delete expense' })
  } finally {
    saving.value = false
  }
}

onMounted(fetchExpenses)
</script>
