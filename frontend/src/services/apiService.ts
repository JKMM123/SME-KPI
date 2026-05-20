import { api } from './api'

export interface Sale {
  id: string
  productId?: string
  productName?: string
  amount: number
  date: string
  description: string
  createdAt: string
}

export interface SaleRequest {
  productId: string
  date: string
  description: string
}

export interface Expense {
  id: string
  amount: number
  date: string
  category: string
  description: string
  createdAt: string
}

export interface ExpenseRequest {
  amount: number
  date: string
  category: string
  description: string
}

export interface Product {
  id: string
  name: string
  price: number
  stockQuantity: number
  createdAt: string
}

export interface ProductRequest {
  name: string
  price: number
  stockQuantity: number
}

export interface Customer {
  id: string
  name: string
  email: string
  phone: string
  createdAt: string
}

export interface CustomerRequest {
  name: string
  email: string
  phone: string
}

export interface KpiSummary {
  totalRevenue: number
  totalExpenses: number
  netProfit: number
  growthPercentage: number
  topSellingProductName?: string
  lowStockAlertCount: number
}

export interface ChartData {
  monthlyData: {
    month: string
    sales: number
    expenses: number
    profit: number
  }[]
}

export interface ExpenseCategory {
  category: string
  total: number
}

export interface MonthlyReport {
  year: number
  month: number
  totalSales: number
  totalExpenses: number
  netProfit: number
  expensesByCategory: ExpenseCategory[]
}

export const salesApi = {
  getAll: () => api.get<Sale[]>('/sales'),
  getById: (id: string) => api.get<Sale>(`/sales/${id}`),
  create: (data: SaleRequest) => api.post<Sale>('/sales', data),
  update: (id: string, data: SaleRequest) => api.put<Sale>(`/sales/${id}`, data),
  delete: (id: string) => api.delete(`/sales/${id}`),
}

export const expensesApi = {
  getAll: () => api.get<Expense[]>('/expenses'),
  getById: (id: string) => api.get<Expense>(`/expenses/${id}`),
  create: (data: ExpenseRequest) => api.post<Expense>('/expenses', data),
  update: (id: string, data: ExpenseRequest) => api.put<Expense>(`/expenses/${id}`, data),
  delete: (id: string) => api.delete(`/expenses/${id}`),
}

export const productsApi = {
  getAll: () => api.get<Product[]>('/products'),
  getById: (id: string) => api.get<Product>(`/products/${id}`),
  create: (data: ProductRequest) => api.post<Product>('/products', data),
  update: (id: string, data: ProductRequest) => api.put<Product>(`/products/${id}`, data),
  delete: (id: string) => api.delete(`/products/${id}`),
}

export const customersApi = {
  getAll: () => api.get<Customer[]>('/customers'),
  getById: (id: string) => api.get<Customer>(`/customers/${id}`),
  create: (data: CustomerRequest) => api.post<Customer>('/customers', data),
  update: (id: string, data: CustomerRequest) => api.put<Customer>(`/customers/${id}`, data),
  delete: (id: string) => api.delete(`/customers/${id}`),
}

export const dashboardApi = {
  getKpiSummary: () => api.get<KpiSummary>('/dashboard/kpi-summary'),
  getChartData: () => api.get<ChartData>('/dashboard/chart-data'),
  getExpenseDistribution: () => api.get<ExpenseCategory[]>('/dashboard/expense-distribution'),
}

export const reportsApi = {
  getMonthlyReport: (year: number, month: number) =>
    api.get<MonthlyReport>(`/reports/monthly?year=${year}&month=${month}`),
}
