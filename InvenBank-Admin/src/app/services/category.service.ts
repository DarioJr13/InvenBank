import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';
import {
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest,
  ApiResponse
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private endpoint = '/admin/categories';

  constructor(private http: HttpService) {}

  /**
   * Obtener todas las categorías
   */
  getAllCategories(): Observable<ApiResponse<Category[]>> {
    return this.http.get<ApiResponse<Category[]>>(this.endpoint);
  }

  /**
   * Obtener categoría por ID
   */
  getCategoryById(id: number): Observable<ApiResponse<Category>> {
    return this.http.get<ApiResponse<Category>>(`${this.endpoint}/${id}`);
  }

  /**
   * Crear nueva categoría
   */
  createCategory(category: CreateCategoryRequest): Observable<ApiResponse<Category>> {
    return this.http.post<ApiResponse<Category>>(this.endpoint, category);
  }

  /**
   * Actualizar categoría
   */
  updateCategory(id: number, category: UpdateCategoryRequest): Observable<ApiResponse<Category>> {
    return this.http.put<ApiResponse<Category>>(`${this.endpoint}/${id}`, category);
  }

  /**
   * Eliminar categoría
   */
  deleteCategory(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.endpoint}/${id}`);
  }

  /**
   * Activar/Desactivar categoría
   */
  toggleCategoryStatus(id: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.endpoint}/${id}/toggle-status`, {});
  }
}
