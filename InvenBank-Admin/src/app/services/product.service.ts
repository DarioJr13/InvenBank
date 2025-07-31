// src/app/services/product.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Product, CreateProductRequest, UpdateProductRequest } from '../models/product.model';
import { ApiResponse } from '../models/api-response.model';


@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrl = 'http://localhost:5207/api/admin/products';

  constructor(private http: HttpClient) { }

  /**
   * Obtener todos los productos
   */
getProducts(): Observable<Product[]> {
  const mockProducts: Product[] = [
    {
      id: 1,
      name: 'Laptop MacBook Pro',
      description: 'MacBook Pro 14 pulgadas con chip M2',
      sku: 'MBP-14-M2-001',
      brand: 'Apple',
      categoryId: 1,
      categoryName: 'Electrónica',
      imageUrl: '',
      isActive: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-06-01T00:00:00Z',
      minPrice: 1999.99,
      maxPrice: 2299.99,
      totalStock: 5,
      suppliersCount: 2,
      suppliers: []
    },
    {
      id: 2,
      name: 'iPhone 15 Pro',
      description: 'iPhone 15 Pro con cámara avanzada',
      sku: 'IPH-15-PRO-001',
      brand: 'Apple',
      categoryId: 1,
      categoryName: 'Electrónica',
      imageUrl: '',
      isActive: true,
      createdAt: '2024-02-01T00:00:00Z',
      updatedAt: '2024-06-05T00:00:00Z',
      minPrice: 1199.99,
      maxPrice: 1299.99,
      totalStock: 12,
      suppliersCount: 3,
      suppliers: []
    },
    {
      id: 3,
      name: 'Samsung Galaxy S24',
      description: 'Samsung Galaxy S24 Ultra',
      sku: 'SGS-24-ULT-001',
      brand: 'Samsung',
      categoryId: 1,
      categoryName: 'Electrónica',
      imageUrl: '',
      isActive: true,
      createdAt: '2024-03-01T00:00:00Z',
      updatedAt: '2024-06-10T00:00:00Z',
      minPrice: 1099.99,
      maxPrice: 1199.99,
      totalStock: 8,
      suppliersCount: 2,
      suppliers: []
    }
  ];

  return of(mockProducts);
      // return this.http.get<Product[]>(this.apiUrl); // ← Usar cuando esté el backend
}

  /**
   * Obtener producto por ID
   */
getProduct(id: number): Observable<{ success: boolean; data: Product }> {
  const mockProduct: Product = {
    id: id,
    name: 'Producto de Ejemplo',
    description: 'Descripción del producto de ejemplo',
    sku: 'PROD-001',
    brand: 'Marca Ejemplo',
    categoryId: 1,
    categoryName: 'Categoría Ejemplo',
    imageUrl: '',
    isActive: true,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    minPrice: 15,
    maxPrice: 30,
    totalStock: 100,
    suppliersCount: 2,
    suppliers: []
  };

  return of({
    success: true,
    data: mockProduct
  });

  // return this.http.get<ApiResponse<Product>>(`${this.apiUrl}/${id}`);
}



  /**
   * Crear nuevo producto
   */
 // Simulado con estructura ApiResponse<Product>
  createProduct(product: CreateProductRequest): Observable<ApiResponse<Product>> {
    const newProduct: Product = {
      ...product,
      id: Math.floor(Math.random() * 1000),
      categoryName: 'Mock Category',
      isActive: true,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      minPrice: 0,
      maxPrice: 0,
      totalStock: 0,
      suppliersCount: 0,
      suppliers: []
    };

    return of({
      success: true,
      message: 'Producto creado exitosamente',
      data: newProduct,
      errors: [],
      timestamp: new Date().toISOString()
    });
      // Cuando esté el backend:
  // return this.http.post<ApiResponse<Product>>(this.apiUrl, product);
  }


  /**
   * Actualizar producto
   */
  updateProduct(id: number, product: UpdateProductRequest): Observable<ApiResponse<Product>> {
    const updatedProduct: Product = {
      ...product,
      id: id,
      categoryName: 'Mock Category',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      minPrice: 100,
      maxPrice: 300,
      totalStock: 15,
      suppliersCount: 2,
      suppliers: []
    };

    return of({
      success: true,
      message: 'Producto actualizado exitosamente',
      data: updatedProduct,
      errors: [],
      timestamp: new Date().toISOString()
    });
    // return this.http.put<Product>(`${this.apiUrl}/${id}`, product); // ← Usar cuando esté el backend
  }


  /**
   * Eliminar producto
   */
  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Buscar productos
   */
  searchProducts(searchTerm: string): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/search?q=${searchTerm}`);
  }

  /**
   * Obtener productos por categoría
   */
  getProductsByCategory(categoryId: number): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/category/${categoryId}`);
  }

  /**
   * Obtener productos con stock bajo
   */
  getLowStockProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/low-stock`);
  }
}
