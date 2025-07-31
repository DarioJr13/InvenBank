import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Category } from '../models/category.model';

// export interface Category {
//   id: number;
//   name: string;
//   description: string;
//   isActive?: boolean;
//   createdAt? : string;Category
//   updatedAt?: string;
//   productsCount?: number;
// }

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private apiUrl = 'http://localhost:5207/api/admin/categories';

  constructor(private http: HttpClient) { }

  // ✅ MÉTODO CORRECTO: getAllCategories (como lo llaman los componentes)
  getAllCategories(): Observable<Category[]> {
    // Datos temporales mientras no esté el backend
    const mockCategories: Category[] = [
      { id: 1, name: 'Electrónica', description: 'Productos electrónicos', isActive: true },
      { id: 2, name: 'Ropa', description: 'Artículos de vestir', isActive: true },
      { id: 3, name: 'Hogar', description: 'Productos para el hogar', isActive: true },
      { id: 4, name: 'Deportes', description: 'Artículos deportivos', isActive: true }
    ];

    return of(mockCategories);
    // return this.http.get<Category[]>(this.apiUrl); // ← Usar cuando esté el backend
  }

  // ✅ MÉTODO ALTERNATIVO: getCategories (alias)
  getCategories(): Observable<Category[]> {
    return this.getAllCategories();
  }

  getCategory(id: number): Observable<Category> {
    return this.http.get<Category>(`${this.apiUrl}/${id}`);
  }

  createCategory(category: Category): Observable<Category> {
    return this.http.post<Category>(this.apiUrl, category);
  }

  updateCategory(id: number, category: Category): Observable<Category> {
    return this.http.put<Category>(`${this.apiUrl}/${id}`, category);
  }

  deleteCategory(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
// import { Injectable } from '@angular/core';
// import { Observable } from 'rxjs';
// import { HttpService } from './http.service';
// import {
//   Category,
//   CreateCategoryRequest,
//   UpdateCategoryRequest,
//   ApiResponse
// } from '../models';

// @Injectable({
//   providedIn: 'root'
// })
// export class CategoryService {
//   private endpoint = '/admin/categories';

//   constructor(private http: HttpService) {}

//   /**
//    * Obtener todas las categorías
//    */
//   getAllCategories(): Observable<ApiResponse<Category[]>> {
//     return this.http.get<ApiResponse<Category[]>>(this.endpoint);
//   }

//   /**
//    * Obtener categoría por ID
//    */
//   getCategoryById(id: number): Observable<ApiResponse<Category>> {
//     return this.http.get<ApiResponse<Category>>(`${this.endpoint}/${id}`);
//   }

//   /**
//    * Crear nueva categoría
//    */
//   createCategory(category: CreateCategoryRequest): Observable<ApiResponse<Category>> {
//     return this.http.post<ApiResponse<Category>>(this.endpoint, category);
//   }

//   /**
//    * Actualizar categoría
//    */
//   updateCategory(id: number, category: UpdateCategoryRequest): Observable<ApiResponse<Category>> {
//     return this.http.put<ApiResponse<Category>>(`${this.endpoint}/${id}`, category);
//   }

//   /**
//    * Eliminar categoría
//    */
//   deleteCategory(id: number): Observable<ApiResponse<boolean>> {
//     return this.http.delete<ApiResponse<boolean>>(`${this.endpoint}/${id}`);
//   }

//   /**
//    * Activar/Desactivar categoría
//    */
//   toggleCategoryStatus(id: number): Observable<ApiResponse<boolean>> {
//     return this.http.put<ApiResponse<boolean>>(`${this.endpoint}/${id}/toggle-status`, {});
//   }
// }
