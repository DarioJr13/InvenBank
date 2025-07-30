import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';
import { User, ApiResponse, PagedResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private endpoint = '/admin/users';

  constructor(private http: HttpService) {}

  /**
   * Obtener todos los usuarios
   */
  getAllUsers(): Observable<ApiResponse<User[]>> {
    return this.http.get<ApiResponse<User[]>>(this.endpoint);
  }

  /**
   * Búsqueda paginada de usuarios
   */
  searchUsers(params: any): Observable<PagedResponse<User>> {
    return this.http.getPaged<User>(`${this.endpoint}/search`, params);
  }

  /**
   * Obtener usuario por ID
   */
  getUserById(id: number): Observable<ApiResponse<User>> {
    return this.http.get<ApiResponse<User>>(`${this.endpoint}/${id}`);
  }

  /**
   * Crear nuevo usuario
   */
  createUser(user: any): Observable<ApiResponse<User>> {
    return this.http.post<ApiResponse<User>>(this.endpoint, user);
  }

  /**
   * Actualizar usuario
   */
  updateUser(id: number, user: any): Observable<ApiResponse<User>> {
    return this.http.put<ApiResponse<User>>(`${this.endpoint}/${id}`, user);
  }

  /**
   * Eliminar usuario
   */
  deleteUser(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.endpoint}/${id}`);
  }

  /**
   * Activar/Desactivar usuario
   */
  toggleUserStatus(id: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.endpoint}/${id}/toggle-status`, {});
  }

  /**
   * Cambiar contraseña de usuario
   */
  changeUserPassword(id: number, newPassword: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.endpoint}/${id}/change-password`, { newPassword });
  }
}
