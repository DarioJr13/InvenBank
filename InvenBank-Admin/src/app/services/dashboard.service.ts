import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DashboardStats {
  totalProducts: number;
  totalSuppliers: number;
  totalUsers: number;
  totalOrders: number;
  mostSoldProduct: string;
  lowStockProducts: number;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = 'http://localhost:5207/api/admin/dashboard';

  constructor(private http: HttpClient) { }

  getStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/stats`);
  }

  getRecentActivity(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/recent-activity`);
  }
}
// import { Injectable } from '@angular/core';
// import { Observable } from 'rxjs';
// import { HttpService } from './http.service';
// import { DashboardStats, ApiResponse } from '../models';

// @Injectable({
//   providedIn: 'root'
// })
// export class DashboardService {
//   private endpoint = '/admin/dashboard';

//   constructor(private http: HttpService) {}

//   /**
//    * Obtener estadísticas del dashboard
//    */
//   getDashboardStats(): Observable<ApiResponse<DashboardStats>> {
//     return this.http.get<ApiResponse<DashboardStats>>(`${this.endpoint}/stats`);
//   }

//   /**
//    * Obtener productos más vendidos
//    */
//   getTopProducts(limit: number = 5): Observable<ApiResponse<any[]>> {
//     return this.http.get<ApiResponse<any[]>>(`${this.endpoint}/top-products`, { limit });
//   }

//   /**
//    * Obtener proveedores top
//    */
//   getTopSuppliers(limit: number = 5): Observable<ApiResponse<any[]>> {
//     return this.http.get<ApiResponse<any[]>>(`${this.endpoint}/top-suppliers`, { limit });
//   }

//   /**
//    * Obtener ventas por mes
//    */
//   getMonthlySales(year: number = new Date().getFullYear()): Observable<ApiResponse<any[]>> {
//     return this.http.get<ApiResponse<any[]>>(`${this.endpoint}/monthly-sales`, { year });
//   }

//   /**
//    * Obtener distribución por categorías
//    */
//   getCategoryDistribution(): Observable<ApiResponse<any[]>> {
//     return this.http.get<ApiResponse<any[]>>(`${this.endpoint}/category-distribution`);
//   }
// }
