import { ProductSupplier } from './product-supplier.model';

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
