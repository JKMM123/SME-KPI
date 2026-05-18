<template>
  <q-page class="q-pa-lg">
    <div class="row items-center justify-between q-mb-md">
      <div class="page-title">Sales</div>
      <q-btn color="primary" icon="add" label="Add Sale" @click="openAddDialog" />
    </div>

    <q-table
      :rows="sales"
      :columns="columns"
      :loading="loading"
      row-key="id"
      flat
      bordered
      :no-data-label="'No sales yet. Click \'Add Sale\' to get started.'"
    >
      <template #body-cell-product="props">
        <q-td :props="props">
          {{ props.row.productName ?? '—' }}
        </q-td>
      </template>

      <template #body-cell-amount="props">
        <q-td :props="props" class="text-positive text-weight-medium">
          ${{ props.row.amount.toFixed(2) }}
        </q-td>
      </template>

      <template #body-cell-date="props">
        <q-td :props="props">
          {{ formatDate(props.row.date) }}
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
      <q-card style="min-width: 440px">
        <q-card-section class="row items-center">
          <div class="text-h6">{{ isEditing ? 'Edit Sale' : 'Add Sale' }}</div>
          <q-space />
          <q-btn icon="close" flat round dense @click="showDialog = false" />
        </q-card-section>

        <q-separator />

        <q-card-section>
          <q-form @submit.prevent="saveSale" class="q-gutter-md">
            <!-- Product select (only for new sales) -->
            <q-select
              v-if="!isEditing"
              v-model="selectedProduct"
              :options="productOptions"
              option-label="label"
              option-value="value"
              emit-value
              map-options
              label="Product *"
              outlined
              :rules="[val => !!val || 'Please select a product']"
              @update:model-value="onProductSelected"
            >
              <template #option="scope">
                <q-item v-bind="scope.itemProps">
                  <q-item-section>
                    <q-item-label>{{ scope.opt.label }}</q-item-label>
                    <q-item-label caption>
                      ${{ scope.opt.price.toFixed(2) }} · Stock: {{ scope.opt.stock }}
                    </q-item-label>
                  </q-item-section>
                </q-item>
              </template>
            </q-select>

            <!-- Price display (auto-filled, read-only) -->
            <q-input
              v-if="!isEditing"
              :model-value="selectedProductPrice ? `$${selectedProductPrice.toFixed(2)}` : ''"
              label="Unit Price (auto-filled)"
              outlined
              readonly
              :hint="selectedProductPrice ? 'Amount is set from the product price' : ''"
            />

            <!-- Product name display (edit mode) -->
            <q-input
              v-if="isEditing"
              :model-value="currentSale?.productName ?? '—'"
              label="Product"
              outlined
              readonly
            />

            <q-input
              v-model="form.date"
              label="Date *"
              type="date"
              outlined
              :rules="[val => !!val || 'Date is required', val => !isFutureDate(val) || 'Date cannot be in the future']"
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
          Are you sure you want to delete the sale for
          <strong>{{ currentSale?.productName ?? 'this item' }}</strong>
          (${{ currentSale?.amount.toFixed(2) }})?
        </q-card-section>
        <q-card-actions align="right">
          <q-btn label="Cancel" flat @click="showDeleteDialog = false" />
          <q-btn label="Delete" color="negative" :loading="saving" @click="deleteSale" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { salesApi, productsApi, type Sale, type SaleRequest, type Product } from 'src/services/apiService'
import { useQuasar } from 'quasar'

const $q = useQuasar()

const sales = ref<Sale[]>([])
const products = ref<Product[]>([])
const loading = ref(false)
const saving = ref(false)
const showDialog = ref(false)
const showDeleteDialog = ref(false)
const isEditing = ref(false)
const currentSale = ref<Sale | null>(null)
const selectedProduct = ref<string | null>(null)
const selectedProductPrice = ref<number | null>(null)

const form = ref<SaleRequest>({
  productId: '',
  date: todayString(),
  description: '',
})

const columns = [
  { name: 'product', label: 'Product', field: 'productName', sortable: true, align: 'left' as const },
  { name: 'amount', label: 'Amount', field: 'amount', sortable: true, align: 'left' as const },
  { name: 'date', label: 'Date', field: 'date', sortable: true, align: 'left' as const },
  { name: 'description', label: 'Description', field: 'description', align: 'left' as const },
  { name: 'actions', label: 'Actions', field: 'actions', align: 'center' as const },
]

const productOptions = computed(() =>
  products.value.map(p => ({
    label: p.name,
    value: p.id,
    price: p.price,
    stock: p.stockQuantity,
  }))
)

function todayString() {
  return new Date().toISOString().split('T')[0]
}

function formatDate(dateStr: string) {
  return new Date(dateStr).toLocaleDateString()
}

function isFutureDate(dateStr: string) {
  return new Date(dateStr) > new Date()
}

function onProductSelected(productId: string) {
  const product = products.value.find(p => p.id === productId)
  selectedProductPrice.value = product?.price ?? null
  form.value.productId = productId
}

async function fetchSales() {
  loading.value = true
  try {
    const response = await salesApi.getAll()
    sales.value = response.data
  } catch {
    $q.notify({ type: 'negative', message: 'Failed to load sales' })
  } finally {
    loading.value = false
  }
}

async function fetchProducts() {
  try {
    const response = await productsApi.getAll()
    products.value = response.data
  } catch {
    $q.notify({ type: 'negative', message: 'Failed to load products' })
  }
}

function openAddDialog() {
  isEditing.value = false
  currentSale.value = null
  selectedProduct.value = null
  selectedProductPrice.value = null
  form.value = { productId: '', date: todayString(), description: '' }
  showDialog.value = true
}

function openEditDialog(sale: Sale) {
  isEditing.value = true
  currentSale.value = sale
  form.value = {
    productId: sale.productId ?? '',
    date: sale.date.split('T')[0],
    description: sale.description,
  }
  showDialog.value = true
}

async function saveSale() {
  saving.value = true
  try {
    if (isEditing.value && currentSale.value) {
      await salesApi.update(currentSale.value.id, form.value)
      $q.notify({ type: 'positive', message: 'Sale updated successfully' })
    } else {
      await salesApi.create(form.value)
      $q.notify({ type: 'positive', message: 'Sale created successfully' })
    }
    showDialog.value = false
    await Promise.all([fetchSales(), fetchProducts()])
  } catch (err: unknown) {
    const msg =
      (err as { response?: { data?: { message?: string } } })?.response?.data?.message ??
      'Failed to save sale'
    $q.notify({ type: 'negative', message: msg })
  } finally {
    saving.value = false
  }
}

function confirmDelete(sale: Sale) {
  currentSale.value = sale
  showDeleteDialog.value = true
}

async function deleteSale() {
  if (!currentSale.value) return
  saving.value = true
  try {
    await salesApi.delete(currentSale.value.id)
    $q.notify({ type: 'positive', message: 'Sale deleted' })
    showDeleteDialog.value = false
    await fetchSales()
  } catch {
    $q.notify({ type: 'negative', message: 'Failed to delete sale' })
  } finally {
    saving.value = false
  }
}

onMounted(() => Promise.all([fetchSales(), fetchProducts()]))
</script>
