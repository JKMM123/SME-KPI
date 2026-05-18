<template>
  <q-page class="q-pa-lg">
    <div class="row items-center justify-between q-mb-md">
      <div class="page-title">Customers</div>
      <q-btn color="primary" icon="person_add" label="Add Customer" @click="openAddDialog" />
    </div>

    <q-table
      :rows="customers"
      :columns="columns"
      :loading="loading"
      row-key="id"
      flat
      bordered
      :no-data-label="'No customers yet. Click \'Add Customer\' to get started.'"
    >
      <template #body-cell-email="props">
        <q-td :props="props">
          <a v-if="props.row.email" :href="`mailto:${props.row.email}`" class="text-primary">
            {{ props.row.email }}
          </a>
          <span v-else class="text-grey-5">—</span>
        </q-td>
      </template>

      <template #body-cell-phone="props">
        <q-td :props="props">
          <span v-if="props.row.phone">{{ props.row.phone }}</span>
          <span v-else class="text-grey-5">—</span>
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
          <div class="text-h6">{{ isEditing ? 'Edit Customer' : 'Add Customer' }}</div>
          <q-space />
          <q-btn icon="close" flat round dense @click="showDialog = false" />
        </q-card-section>

        <q-separator />

        <q-card-section>
          <q-form @submit.prevent="saveCustomer" class="q-gutter-md">
            <q-input
              v-model="form.name"
              label="Name"
              outlined
              maxlength="200"
              :rules="[val => !!val || 'Name is required']"
            />
            <q-input
              v-model="form.email"
              label="Email (optional)"
              type="email"
              outlined
              maxlength="255"
              :rules="[val => !val || isValidEmail(val) || 'Please enter a valid email']"
            />
            <q-input
              v-model="form.phone"
              label="Phone (optional)"
              outlined
              maxlength="20"
            />
            <div class="row q-gutter-sm justify-end">
              <q-btn label="Cancel" flat @click="showDialog = false" />
              <q-btn label="Save" type="submit" color="primary" :loading="saving" />
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
          Are you sure you want to delete customer <strong>{{ currentCustomer?.name }}</strong>?
        </q-card-section>
        <q-card-actions align="right">
          <q-btn label="Cancel" flat @click="showDeleteDialog = false" />
          <q-btn label="Delete" color="negative" :loading="saving" @click="deleteCustomer" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { customersApi, type Customer, type CustomerRequest } from 'src/services/apiService'
import { useQuasar } from 'quasar'

const $q = useQuasar()

const customers = ref<Customer[]>([])
const loading = ref(false)
const saving = ref(false)
const showDialog = ref(false)
const showDeleteDialog = ref(false)
const isEditing = ref(false)
const currentCustomer = ref<Customer | null>(null)

const form = ref<CustomerRequest>({
  name: '',
  email: '',
  phone: '',
})

const columns = [
  { name: 'name', label: 'Name', field: 'name', sortable: true, align: 'left' as const },
  { name: 'email', label: 'Email', field: 'email', align: 'left' as const },
  { name: 'phone', label: 'Phone', field: 'phone', align: 'left' as const },
  { name: 'actions', label: 'Actions', field: 'actions', align: 'center' as const },
]

function isValidEmail(email: string) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
}

async function fetchCustomers() {
  loading.value = true
  try {
    const response = await customersApi.getAll()
    customers.value = response.data
  } catch {
    $q.notify({ type: 'negative', message: 'Failed to load customers' })
  } finally {
    loading.value = false
  }
}

function openAddDialog() {
  isEditing.value = false
  currentCustomer.value = null
  form.value = { name: '', email: '', phone: '' }
  showDialog.value = true
}

function openEditDialog(customer: Customer) {
  isEditing.value = true
  currentCustomer.value = customer
  form.value = {
    name: customer.name,
    email: customer.email,
    phone: customer.phone,
  }
  showDialog.value = true
}

async function saveCustomer() {
  saving.value = true
  try {
    if (isEditing.value && currentCustomer.value) {
      await customersApi.update(currentCustomer.value.id, form.value)
      $q.notify({ type: 'positive', message: 'Customer updated successfully' })
    } else {
      await customersApi.create(form.value)
      $q.notify({ type: 'positive', message: 'Customer created successfully' })
    }
    showDialog.value = false
    await fetchCustomers()
  } catch {
    $q.notify({ type: 'negative', message: 'Failed to save customer' })
  } finally {
    saving.value = false
  }
}

function confirmDelete(customer: Customer) {
  currentCustomer.value = customer
  showDeleteDialog.value = true
}

async function deleteCustomer() {
  if (!currentCustomer.value) return
  saving.value = true
  try {
    await customersApi.delete(currentCustomer.value.id)
    $q.notify({ type: 'positive', message: 'Customer deleted' })
    showDeleteDialog.value = false
    await fetchCustomers()
  } catch {
    $q.notify({ type: 'negative', message: 'Failed to delete customer' })
  } finally {
    saving.value = false
  }
}

onMounted(fetchCustomers)
</script>
