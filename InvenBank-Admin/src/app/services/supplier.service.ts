import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';
import {
  Supplier,
  CreateSupplierRequest,
  UpdateSupplierRequest,
  ApiResponse,
  PagedResponse
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class SupplierService {
  private endpoint = '/admin/suppliers';

  constructor(private http: HttpService) {}

  /**
   * Obtener todos los proveedores
   */
  getAllSuppliers(): Observable<ApiResponse<Supplier[]>> {
    return this.http.get<ApiResponse<Supplier[]>>(this.endpoint);
  }

  /**
   * Búsqueda paginada de proveedores
   */
  searchSuppliers(params: any): Observable<PagedResponse<Supplier>> {
    return this.http.getPaged<Supplier>(`${this.endpoint}/search`, params);
  }

  /**
   * Obtener proveedor por ID
   */
  getSupplierById(id: number): Observable<ApiResponse<Supplier>> {
    return this.http.get<ApiResponse<Supplier>>(`${this.endpoint}/${id}`);
  }

  /**
   * Crear nuevo proveedor
   */
  createSupplier(supplier: CreateSupplierRequest): Observable<ApiResponse<Supplier>> {
    return this.http.post<ApiResponse<Supplier>>(this.endpoint, supplier);
  }

  /**
   * Actualizar proveedor
   */
  updateSupplier(id: number, supplier: UpdateSupplierRequest): Observable<ApiResponse<Supplier>> {
    return this.http.put<ApiResponse<Supplier>>(`${this.endpoint}/${id}`, supplier);
  }

  /**
   * Eliminar proveedor
   */
  deleteSupplier(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.endpoint}/${id}`);
  }

  /**
   * Activar/Desactivar proveedor
   */
  toggleSupplierStatus(id: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.endpoint}/${id}/toggle-status`, {});
  }

  /**
   * Obtener estadísticas del proveedor
   */
  getSupplierStats(id: number): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.endpoint}/${id}/stats`);
  }
}
