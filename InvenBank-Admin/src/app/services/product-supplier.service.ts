import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';
import {
  ProductSupplier,
  CreateProductSupplierRequest,
  UpdateProductSupplierRequest,
  ApiResponse
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class ProductSupplierService {
  private endpoint = '/admin/products';

  constructor(private http: HttpService) {}

  /**
   * Obtener proveedores de un producto
   */
  getProductSuppliers(productId: number): Observable<ApiResponse<ProductSupplier[]>> {
    return this.http.get<ApiResponse<ProductSupplier[]>>(`${this.endpoint}/${productId}/suppliers`);
  }

  /**
   * Crear asociación producto-proveedor
   */
  createProductSupplier(productId: number, data: CreateProductSupplierRequest): Observable<ApiResponse<ProductSupplier>> {
    return this.http.post<ApiResponse<ProductSupplier>>(`${this.endpoint}/${productId}/suppliers`, data);
  }

  /**
   * Actualizar asociación producto-proveedor
   */
  updateProductSupplier(productId: number, id: number, data: UpdateProductSupplierRequest): Observable<ApiResponse<ProductSupplier>> {
    return this.http.put<ApiResponse<ProductSupplier>>(`${this.endpoint}/${productId}/suppliers/${id}`, data);
  }

  /**
   * Eliminar asociación producto-proveedor
   */
  deleteProductSupplier(productId: number, id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.endpoint}/${productId}/suppliers/${id}`);
  }
}
