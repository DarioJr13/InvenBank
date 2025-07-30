// ===============================================
// 🔐 MODELOS DE AUTENTICACIÓN
// ===============================================

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  data: {
    token: string;
    refreshToken: string;
    user: User;
  };
  errors: string[];
  timestamp: string;
}

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  role: 'Admin' | 'Customer';
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string;
}

// ===============================================
// 📦 MODELOS DE PRODUCTOS
// ===============================================

export interface Product {
  id: number;
  name: string;
  description: string;
  sku: string;
  brand: string;
  categoryId: number;
  categoryName: string;
  imageUrl?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  minPrice: number;
  maxPrice: number;
  totalStock: number;
  suppliersCount: number;
  suppliers: ProductSupplier[];
}

export interface CreateProductRequest {
  name: string;
  description: string;
  sku: string;
  brand: string;
  categoryId: number;
  imageUrl?: string;
}

export interface UpdateProductRequest extends CreateProductRequest {
  isActive: boolean;
}

export interface ProductSupplier {
  id: number;
  productId: number;
  supplierId: number;
  supplierName: string;
  price: number;
  stock: number;
  batchNumber: string;
  supplierSku: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

// ===============================================
// 🏪 MODELOS DE PROVEEDORES
// ===============================================

export interface Supplier {
  id: number;
  name: string;
  contactName: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  country: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  productsCount: number;
  totalStock: number;
  averagePrice: number;
}

export interface CreateSupplierRequest {
  name: string;
  contactName: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  country: string;
}

export interface UpdateSupplierRequest extends CreateSupplierRequest {
  isActive: boolean;
}

// ===============================================
// 📂 MODELOS DE CATEGORÍAS
// ===============================================

export interface Category {
  id: number;
  name: string;
  description: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  productsCount: number;
}

export interface CreateCategoryRequest {
  name: string;
  description: string;
}

export interface UpdateCategoryRequest extends CreateCategoryRequest {
  isActive: boolean;
}

// ===============================================
// 🔗 MODELOS DE PRODUCTO-PROVEEDOR
// ===============================================

export interface CreateProductSupplierRequest {
  productId: number;
  supplierId: number;
  price: number;
  stock: number;
  batchNumber: string;
  supplierSku: string;
}

export interface UpdateProductSupplierRequest {
  price: number;
  stock: number;
  batchNumber: string;
  supplierSku: string;
  isActive: boolean;
}

// ===============================================
// 📊 MODELOS PARA DASHBOARD
// ===============================================

export interface DashboardStats {
  totalProducts: number;
  totalSuppliers: number;
  totalCategories: number;
  totalUsers: number;
  lowStockProducts: number;
  mostSoldProduct: string;
  totalRevenue: number;
  monthlyGrowth: number;
}

// ===============================================
// 🔍 MODELOS DE BÚSQUEDA Y PAGINACIÓN
// ===============================================

export interface SearchRequest {
  searchTerm?: string;
  categoryId?: number;
  brand?: string;
  minPrice?: number;
  maxPrice?: number;
  inStock?: boolean;
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface PagedResponse<T> {
  success: boolean;
  message: string;
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  errors: string[];
  timestamp: string;
}

// ===============================================
// 🌐 MODELOS DE RESPUESTA DE API
// ===============================================

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
  timestamp: string;
}

export interface ApiError {
  message: string;
  field?: string;
  code?: string;
}

// ===============================================
// 📋 MODELOS PARA FORMULARIOS
// ===============================================

export interface FormField {
  name: string;
  label: string;
  type: 'text' | 'email' | 'password' | 'number' | 'select' | 'textarea';
  required: boolean;
  placeholder?: string;
  options?: { value: any; label: string }[];
  validation?: {
    minLength?: number;
    maxLength?: number;
    min?: number;
    max?: number;
    pattern?: string;
  };
}

// ===============================================
// 🔔 MODELOS PARA NOTIFICACIONES
// ===============================================

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  duration?: number;
  timestamp: Date;
}

// ===============================================
// 🎯 MODELOS PARA TABLAS
// ===============================================

export interface TableColumn {
  key: string;
  label: string;
  sortable?: boolean;
  type?: 'text' | 'number' | 'date' | 'currency' | 'boolean' | 'actions';
  width?: string;
}

export interface TableAction {
  label: string;
  icon: string;
  color: 'primary' | 'accent' | 'warn';
  action: (item: any) => void;
  disabled?: (item: any) => boolean;
}

// ===============================================
// 🔒 MODELOS PARA GUARDS Y INTERCEPTORS
// ===============================================

export interface TokenPayload {
  sub: string; // User ID
  email: string;
  role: string;
  firstName: string;
  lastName: string;
  exp: number;
  iat: number;
}

// ===============================================
// ⚙️ MODELOS DE CONFIGURACIÓN
// ===============================================

export interface AppConfig {
  apiUrl: string;
  appName: string;
  version: string;
  environment: 'development' | 'production';
  features: {
    enableNotifications: boolean;
    enableAnalytics: boolean;
    enableLogging: boolean;
  };
}

// ===============================================
// 📈 MODELOS PARA REPORTES
// ===============================================

export interface SalesReport {
  period: string;
  totalSales: number;
  totalProducts: number;
  topProducts: Array<{
    productName: string;
    quantitySold: number;
    revenue: number;
  }>;
  topSuppliers: Array<{
    supplierName: string;
    totalRevenue: number;
    productsCount: number;
  }>;
}

export interface InventoryReport {
  totalProducts: number;
  totalStock: number;
  totalValue: number;
  lowStockProducts: Array<{
    productName: string;
    currentStock: number;
    minStock: number;
    supplierName: string;
  }>;
  categoryDistribution: Array<{
    categoryName: string;
    productsCount: number;
    totalStock: number;
    percentage: number;
  }>;
}
