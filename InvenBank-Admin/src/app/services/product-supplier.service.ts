import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';
import { ProductSupplier, CreateProductSupplierRequest, UpdateProductSupplierRequest, ApiResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class ProductSupplierService {
  private baseEndpoint = '/admin/products';

  constructor(private http: HttpService) {}

  /**
   * Obtener proveedores de un producto
   */
  getByProductId(productId: number): Observable<ApiResponse<ProductSupplier[]>> {
    return this.http.get<ApiResponse<ProductSupplier[]>>(`${this.baseEndpoint}/${productId}/suppliers`);
  }

  /**
   * Obtener una relación producto-proveedor específica
   */
  getById(productId: number, id: number): Observable<ApiResponse<ProductSupplier>> {
    return this.http.get<ApiResponse<ProductSupplier>>(`${this.baseEndpoint}/${productId}/suppliers/${id}`);
  }

  /**
   * Crear nueva relación producto-proveedor
   */
  create(productSupplier: CreateProductSupplierRequest): Observable<ApiResponse<ProductSupplier>> {
    const { productId } = productSupplier;
    return this.http.post<ApiResponse<ProductSupplier>>(
      `${this.baseEndpoint}/${productId}/suppliers`,
      productSupplier
    );
  }

  /**
   * Actualizar relación producto-proveedor
   */
  update(productId: number, id: number, productSupplier: UpdateProductSupplierRequest): Observable<ApiResponse<ProductSupplier>> {
    return this.http.put<ApiResponse<ProductSupplier>>(
      `${this.baseEndpoint}/${productId}/suppliers/${id}`,
      productSupplier
    );
  }

  /**
   * Eliminar relación producto-proveedor
   */
  delete(productId: number, id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseEndpoint}/${productId}/suppliers/${id}`);
  }

  /**
   * Actualizar precio de un proveedor para un producto
   */
  updatePrice(productId: number, id: number, price: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(
      `${this.baseEndpoint}/${productId}/suppliers/${id}/price`,
      { price }
    );
  }

  /**
   * Actualizar stock de un proveedor para un producto
   */
  updateStock(productId: number, id: number, stock: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(
      `${this.baseEndpoint}/${productId}/suppliers/${id}/stock`,
      { stock }
    );
  }
}
