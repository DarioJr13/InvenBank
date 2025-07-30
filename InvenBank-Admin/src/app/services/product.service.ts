import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';
import {
  Product,
  CreateProductRequest,
  UpdateProductRequest,
  SearchRequest,
  ApiResponse,
  PagedResponse
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private endpoint = '/admin/products';

  constructor(private http: HttpService) {}

  /**
   * Obtener todos los productos
   */
  getAllProducts(): Observable<ApiResponse<Product[]>> {
    return this.http.get<ApiResponse<Product[]>>(this.endpoint);
  }

  /**
   * Búsqueda paginada de productos
   */
  searchProducts(searchRequest: SearchRequest): Observable<PagedResponse<Product>> {
    return this.http.getPaged<Product>(`${this.endpoint}/search`, searchRequest);
  }

  /**
   * Obtener producto por ID
   */
  getProductById(id: number): Observable<ApiResponse<Product>> {
    return this.http.get<ApiResponse<Product>>(`${this.endpoint}/${id}`);
  }

  /**
   * Crear nuevo producto
   */
  createProduct(product: CreateProductRequest): Observable<ApiResponse<Product>> {
    return this.http.post<ApiResponse<Product>>(this.endpoint, product);
  }

  /**
   * Actualizar producto
   */
  updateProduct(id: number, product: UpdateProductRequest): Observable<ApiResponse<Product>> {
    return this.http.put<ApiResponse<Product>>(`${this.endpoint}/${id}`, product);
  }

  /**
   * Eliminar producto
   */
  deleteProduct(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.endpoint}/${id}`);
  }

  /**
   * Activar/Desactivar producto
   */
  toggleProductStatus(id: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.endpoint}/${id}/toggle-status`, {});
  }

  /**
   * Obtener productos con stock bajo
   */
  getLowStockProducts(): Observable<ApiResponse<Product[]>> {
    return this.http.get<ApiResponse<Product[]>>(`${this.endpoint}/low-stock`);
  }
}
