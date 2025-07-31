// src/app/services/product-supplier.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';

export interface ProductSupplier {
  id?: number;
  productId: number;
  supplierId: number;
  price: number;
  stock: number;
  isActive: boolean;
  supplierName?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProductSupplierService {
  private apiUrl = 'http://localhost:5207/api/admin/product-suppliers';

  constructor(private http: HttpClient) { }

  /**
   * Obtener proveedores de un producto específico
   */
  getProductSuppliers(productId: number): Observable<ProductSupplier[]> {
    // Datos temporales mientras no esté el backend
    const mockProductSuppliers: ProductSupplier[] = [
      {
        id: 1,
        productId: productId,
        supplierId: 1,
        price: 250.00,
        stock: 10,
        isActive: true,
        supplierName: 'TechCorp SA'
      },
      {
        id: 2,
        productId: productId,
        supplierId: 2,
        price: 245.00,
        stock: 15,
        isActive: true,
        supplierName: 'ElectroMax'
      }
    ];

    return of(mockProductSuppliers);
    // return this.http.get<ProductSupplier[]>(`${this.apiUrl}/product/${productId}`); // ← Usar cuando esté el backend
  }

  /**
   * Agregar proveedor a un producto
   */
  addProductSupplier(productSupplier: ProductSupplier): Observable<ProductSupplier> {
    // Simulación temporal
    const newProductSupplier = {
      ...productSupplier,
      id: Math.floor(Math.random() * 1000) + 100
    };

    return of(newProductSupplier);
    // return this.http.post<ProductSupplier>(this.apiUrl, productSupplier); // ← Usar cuando esté el backend
  }

  /**
   * Actualizar proveedor de producto
   */
  updateProductSupplier(id: number, productSupplier: ProductSupplier): Observable<ProductSupplier> {
    return this.http.put<ProductSupplier>(`${this.apiUrl}/${id}`, productSupplier);
  }

  /**
   * Eliminar proveedor de producto
   */
  removeProductSupplier(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Obtener todos los proveedores de productos
   */
  getAllProductSuppliers(): Observable<ProductSupplier[]> {
    return this.http.get<ProductSupplier[]>(this.apiUrl);
  }

  /**
   * Obtener precio más bajo de un producto
   */
  getLowestPrice(productId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/product/${productId}/lowest-price`);
  }

  /**
   * Obtener stock total de un producto
   */
  getTotalStock(productId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/product/${productId}/total-stock`);
  }
}
