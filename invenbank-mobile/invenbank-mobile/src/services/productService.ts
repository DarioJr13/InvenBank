import { apiClient } from './api';
import { ENDPOINTS } from '../utils/constants';
import { Product, ProductDetail, SearchProductsParams } from '../types/product.types';
import { ApiResponse } from '../types/api.types';

export class ProductService {
  async searchProducts(params: SearchProductsParams): Promise<ApiResponse<Product[]>> {
    return await apiClient.get<Product[]>(ENDPOINTS.MOBILE.CATALOG.SEARCH, params);
  }

  async getProductById(id: number): Promise<ProductDetail> {
    const response = await apiClient.get<ProductDetail>(`${ENDPOINTS.MOBILE.CATALOG.DETAIL}/${id}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Product not found');
  }

  async getCategories(): Promise<any[]> {
    const response = await apiClient.get<any[]>(ENDPOINTS.MOBILE.CATALOG.CATEGORIES);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    return [];
  }
}

export const productService = new ProductService();