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
