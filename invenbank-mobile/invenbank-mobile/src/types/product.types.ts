export interface Product {
  id: number;
  name: string;
  description: string;
  sku: string;
  barcode: string;
  categoryId: number;
  categoryName: string;
  imageUrl: string;
  minPrice: number;
  maxPrice: number;
  totalStock: number;
  supplierCount: number;
  isAvailable: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ProductDetail extends Product {
  suppliers: ProductSupplier[];
}

export interface ProductSupplier {
  id: number;
  supplierId: number;
  supplierName: string;
  supplierCode: string;
  price: number;
  stock: number;
  isPreferred: boolean;
  isActive: boolean;
}

export interface ProductsState {
  products: Product[];
  currentProduct: ProductDetail | null;
  loading: boolean;
  error: string | null;
  searchTerm: string;
  filters: ProductFilters;
  pagination: {
    pageNumber: number;
    pageSize: number;
    totalRecords: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
  };
}

export interface ProductFilters {
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  inStock?: boolean;
}

export interface SearchProductsParams {
  searchTerm?: string;
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  inStock?: boolean;
  pageNumber?: number;
  pageSize?: number;
}
