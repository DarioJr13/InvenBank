// ===============================================
// 📂 MODELOS DE CATEGORÍAS
// ===============================================

export interface Category {
  id: number;
  name: string;
  description: string;
  isActive?: boolean;
  createdAt?: string;
  updatedAt?: string;
  productsCount?: number;
}

export interface CreateCategoryRequest {
  name: string;
  description: string;
}

export interface UpdateCategoryRequest extends CreateCategoryRequest {
  isActive: boolean;
}
