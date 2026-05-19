<template>
  <q-page class="q-pa-lg">
    <div class="row items-center justify-between q-mb-md">
      <div class="page-title">Products</div>
      <q-btn color="primary" icon="add" label="Add Product" @click="openAddDialog" />
    </div>

    <q-table
      :rows="products"
      :columns="columns"
      :loading="loading"
      row-key="id"
      flat
      bordered
      :no-data-label="'No products yet. Click \'Add Product\' to get started.'"
    >
      <template #body-cell-price="props">
        <q-td :props="props" class="text-weight-medium">
          ${{ props.row.price.toFixed(2) }}
        </q-td>
      </template>

      <template #body-cell-stockQuantity="props">
        <q-td :props="props">
          <span :class="props.row.stockQuantity < 10 ? 'text-negative text-weight-bold' : ''">
            {{ props.row.stockQuantity }}
          </span>
          <q-badge
            v-if="props.row.stockQuantity < 10"
            color="negative"
            label="Low Stock"
            class="q-ml-sm"
          />
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
          <div class="text-h6">{{ isEditing ? 'Edit Product' : 'Add Product' }}</div>
          <q-space />
          <q-btn icon="close" flat round dense @click="showDialog = false" />
        </q-card-section>

        <q-separator />

        <q-card-section>
          <q-form @submit.prevent="saveProduct" class="q-gutter-md">
            <q-input
              v-model="form.name"
              label="Product Name"
              outlined
              maxlength="200"
              :rules="[val => !!val || 'Name is required']"
            />
            <q-input
              v-model.number="form.price"
              label="Price ($)"
              type="number"
              step="0.01"
              min="0"
              outlined
              :rules="[val => val >= 0 || 'Price must be 0 or greater']"
            />
            <q-input
              v-model.number="form.stockQuantity"
              label="Stock Quantity"
              type="number"
              min="0"
              outlined
              :rules="[val => val >= 0 || 'Stock quantity must be 0 or greater']"
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
          Are you sure you want to delete <strong>{{ currentProduct?.name }}</strong>?
        </q-card-section>
        <q-card-actions align="right">
          <q-btn label="Cancel" flat @click="showDeleteDialog = false" />
          <q-btn label="Delete" color="negative" :loading="saving" @click="deleteProduct" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { productsApi, type Product, type ProductRequest } from 'src/services/apiService'
import { useQuasar } from 'quasar'

const $q = useQuasar()

const products = ref<Product[]>([])
const loading = ref(false)
const saving = ref(false)
const showDialog = ref(false)
const showDeleteDialog = ref(false)
const isEditing = ref(false)
const currentProduct = ref<Product | null>(null)

const form = ref<ProductRequest>({
  name: '',
  price: 0,
  stockQuantity: 0,
})

const columns = [
  { name: 'name', label: 'Name', field: 'name', sortable: true, align: 'left' as const },
  { name: 'price', label: 'Price', field: 'price', sortable: true, align: 'left' as const },
  { name: 'stockQuantity', label: 'Stock', field: 'stockQuantity', sortable: true, align: 'left' as const },
  { name: 'actions', label: 'Actions', field: 'actions', align: 'center' as const },
]

async function fetchProducts() {
  loading.value = true
  try {
    const response = await productsApi.getAll()
    products.value = response.data
  } catch {
    $q.notify({ type: 'negative', message: 'Failed to load products' })
  } finally {
    loading.value = false
  }
}

function openAddDialog() {
  isEditing.value = false
  currentProduct.value = null
  form.value = { name: '', price: 0, stockQuantity: 0 }
  showDialog.value = true
}

function openEditDialog(product: Product) {
  isEditing.value = true
  currentProduct.value = product
  form.value = {
    name: product.name,
    price: product.price,
    stockQuantity: product.stockQuantity,
  }
  showDialog.value = true
}

async function saveProduct() {
  saving.value = true
  try {
    if (isEditing.value && currentProduct.value) {
      await productsApi.update(currentProduct.value.id, form.value)
      $q.notify({ type: 'positive', message: 'Product updated successfully' })
    } else {
      await productsApi.create(form.value)
      $q.notify({ type: 'positive', message: 'Product created successfully' })
    }
    showDialog.value = false
    await fetchProducts()
  } catch {
    $q.notify({ type: 'negative', message: 'Failed to save product' })
  } finally {
    saving.value = false
  }
}

function confirmDelete(product: Product) {
  currentProduct.value = product
  showDeleteDialog.value = true
}

async function deleteProduct() {
  if (!currentProduct.value) return
  saving.value = true
  try {
    await productsApi.delete(currentProduct.value.id)
    $q.notify({ type: 'positive', message: 'Product deleted' })
    showDeleteDialog.value = false
    await fetchProducts()
  } catch {
    $q.notify({ type: 'negative', message: 'Failed to delete product' })
  } finally {
    saving.value = false
  }
}

onMounted(fetchProducts)
</script>
