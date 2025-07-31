import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';
import { Category, ApiResponse, PagedResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private endpoint = '/admin/categories';

  constructor(private http: HttpService) {}

  getAllCategories(): Observable<ApiResponse<Category[]>> {
    return this.http.get<ApiResponse<Category[]>>(this.endpoint);
  }

  searchCategories(params: any): Observable<PagedResponse<Category>> {
    return this.http.getPaged<Category>(`${this.endpoint}/search`, params);
  }

  getCategoryById(id: number): Observable<ApiResponse<Category>> {
    return this.http.get<ApiResponse<Category>>(`${this.endpoint}/${id}`);
  }

  createCategory(category: any): Observable<ApiResponse<Category>> {
    return this.http.post<ApiResponse<Category>>(this.endpoint, category);
  }

  updateCategory(id: number, category: any): Observable<ApiResponse<Category>> {
    return this.http.put<ApiResponse<Category>>(`${this.endpoint}/${id}`, category);
  }

  deleteCategory(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.endpoint}/${id}`);
  }

  toggleCategoryStatus(id: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.endpoint}/${id}/toggle-status`, {});
  }
}
