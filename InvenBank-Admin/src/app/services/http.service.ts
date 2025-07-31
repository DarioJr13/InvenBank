import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { ApiResponse, PagedResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class HttpService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // ===============================================
  // 🌐 MÉTODOS HTTP GENÉRICOS
  // ===============================================

  /**
   * GET request genérico
   */
 get<T>(endpoint: string, params?: any): Observable<T> {
  let httpParams = new HttpParams();

  if (params) {
    Object.keys(params).forEach(key => {
      if (params[key] !== null && params[key] !== undefined) {
        httpParams = httpParams.set(key, params[key].toString());
      }
    });
  }

  // DEBUG: Verificar si interceptor funciona
  console.log('Making request to:', `${this.apiUrl}${endpoint}`);
  console.log('Token exists:', !!localStorage.getItem('access_token'));

  return this.http.get<T>(`${this.apiUrl}${endpoint}`, { params: httpParams })
    .pipe(
      catchError(this.handleError)
    );
}

  /**
   * POST request genérico
   */
  post<T>(endpoint: string, data: any): Observable<T> {
    return this.http.post<T>(`${this.apiUrl}${endpoint}`, data)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * PUT request genérico
   */
  put<T>(endpoint: string, data: any): Observable<T> {
    return this.http.put<T>(`${this.apiUrl}${endpoint}`, data)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * DELETE request genérico
   */
  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(`${this.apiUrl}${endpoint}`)
      .pipe(
        catchError(this.handleError)
      );
  }

  // ===============================================
  // 📄 MÉTODOS PARA RESPUESTAS PAGINADAS
  // ===============================================

  /**
   * GET con paginación
   */
  getPaged<T>(endpoint: string, params?: any): Observable<PagedResponse<T>> {
    return this.get<PagedResponse<T>>(endpoint, params);
  }

  // ===============================================
  // ⚠️ MANEJO DE ERRORES
  // ===============================================

  private handleError(error: any): Observable<never> {
    let errorMessage = 'Ocurrió un error inesperado';

    if (error?.error?.message) {
      errorMessage = error.error.message;
    } else if (error?.error?.errors && Array.isArray(error.error.errors)) {
      errorMessage = error.error.errors.join(', ');
    } else if (error?.message) {
      errorMessage = error.message;
    } else if (error?.status) {
      switch (error.status) {
        case 400:
          errorMessage = 'Datos inválidos';
          break;
        case 401:
          errorMessage = 'No autorizado';
          break;
        case 403:
          errorMessage = 'Sin permisos';
          break;
        case 404:
          errorMessage = 'Recurso no encontrado';
          break;
        case 500:
          errorMessage = 'Error interno del servidor';
          break;
        default:
          errorMessage = `Error ${error.status}`;
      }
    }

    console.error('HTTP Error:', error);
    return throwError(errorMessage);
  }
}
