import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';

export interface Supplier {
  id: number;
  name: string;
  contactName: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  country: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  productsCount: number;
  totalStock: number;
  averagePrice: number;
}

@Injectable({
  providedIn: 'root'
})
export class SupplierService {
  private apiUrl = 'http://localhost:5207/api/admin/suppliers';

  constructor(private http: HttpClient) {}

  getAllSuppliers(): Observable<Supplier[]> {
    const mockSuppliers: Supplier[] = [
      {
        id: 1,
        name: 'TechCorp SA',
        contactName: 'Juan Pérez',
        email: 'juan@techcorp.com',
        phone: '+507 6000-1234',
        address: 'Calle 1, Ciudad de Panamá',
        city: 'Ciudad de Panamá',
        country: 'Panamá',
        isActive: true,
        createdAt: '2023-01-01T08:00:00Z',
        updatedAt: '2023-06-01T08:00:00Z',
        productsCount: 5,
        totalStock: 100,
        averagePrice: 120.5
      },
      {
        id: 2,
        name: 'ElectroMax',
        contactName: 'María García',
        email: 'maria@electromax.com',
        phone: '+507 6000-5678',
        address: 'Avenida Central, San Miguelito',
        city: 'San Miguelito',
        country: 'Panamá',
        isActive: true,
        createdAt: '2023-02-10T08:00:00Z',
        updatedAt: '2023-06-10T08:00:00Z',
        productsCount: 3,
        totalStock: 75,
        averagePrice: 95.0
      },
      {
        id: 3,
        name: 'CompuWorld',
        contactName: 'Carlos López',
        email: 'carlos@compuworld.com',
        phone: '+507 6000-9012',
        address: 'Plaza Comercial, La Chorrera',
        city: 'La Chorrera',
        country: 'Panamá',
        isActive: true,
        createdAt: '2023-03-15T08:00:00Z',
        updatedAt: '2023-06-15T08:00:00Z',
        productsCount: 7,
        totalStock: 150,
        averagePrice: 88.75
      }
    ];

    return of(mockSuppliers);
    // return this.http.get<Supplier[]>(this.apiUrl);
  }

  getSuppliers(): Observable<Supplier[]> {
    return this.getAllSuppliers();
  }

  getSupplier(id: number): Observable<Supplier> {
    return this.http.get<Supplier>(`${this.apiUrl}/${id}`);
  }

  createSupplier(supplier: Supplier): Observable<Supplier> {
    return this.http.post<Supplier>(this.apiUrl, supplier);
  }

  updateSupplier(id: number, supplier: Supplier): Observable<Supplier> {
    return this.http.put<Supplier>(`${this.apiUrl}/${id}`, supplier);
  }

  deleteSupplier(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
