import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { LoginRequest, LoginResponse, User, TokenPayload } from '../models';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = environment.apiUrl;
  private jwtHelper = new JwtHelperService();

  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);

  public currentUser$ = this.currentUserSubject.asObservable();
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.initializeAuth();
  }

  private isBrowser(): boolean {
    return typeof window !== 'undefined';
  }

  // ===============================================
  // 🔐 MÉTODOS DE AUTENTICACIÓN
  // ===============================================

  private initializeAuth(): void {
    const token = this.getToken();

    if (token && !this.jwtHelper.isTokenExpired(token)) {
      const user = this.getStoredUser();
      if (user) {
        this.currentUserSubject.next(user);
        this.isAuthenticatedSubject.next(true);
      }
    } else {
      this.logout();
    }
  }

  login(data: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/auth/login', data).pipe(
      tap(response => {
        if (response.success) {
          this.setSession(response.data);
        }
      })
    );
  }

  logout(): void {
    if (this.isBrowser()) {
      localStorage.removeItem('access_token');
      localStorage.removeItem('refresh_token');
      localStorage.removeItem('user_data');
    }

    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);

    this.router.navigate(['/login']);
  }

  refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.isBrowser() ? localStorage.getItem('refresh_token') : null;

    if (!refreshToken) {
      this.logout();
      return throwError(() => 'No refresh token available');
    }

    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/refresh`, {
      refreshToken
    }).pipe(
      tap(response => {
        if (response.success && response.data) {
          this.setSession(response.data);
        }
      }),
      catchError(error => {
        this.logout();
        return this.handleError(error);
      })
    );
  }

  // ===============================================
  // 🔑 TOKEN Y SESIÓN
  // ===============================================

  getToken(): string | null {
    return this.isBrowser() ? localStorage.getItem('access_token') : null;
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  isAdmin(): boolean {
    const user = this.currentUserSubject.value || this.getStoredUser();
    return user?.roleName === 'Admin' || user?.roleId === 1;
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value || this.getStoredUser();
  }

  private getStoredUser(): User | null {
    if (!this.isBrowser()) return null;
    const raw = localStorage.getItem('user_data');
    try {
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }

  public setSession(authData: LoginResponse['data']): void {
    if (this.isBrowser()) {
      localStorage.setItem('access_token', authData.token);
      localStorage.setItem('refresh_token', authData.refreshToken);
      localStorage.setItem('user_data', JSON.stringify(authData.user));
    }

    this.currentUserSubject.next(authData.user);
    this.isAuthenticatedSubject.next(true);
  }

  // ===============================================
  // ⚠️ MANEJO DE ERRORES
  // ===============================================

  private handleError(error: any): Observable<never> {
    let msg = 'Ocurrió un error inesperado';

    if (error?.error?.message) {
      msg = error.error.message;
    } else if (error?.message) {
      msg = error.message;
    } else if (error?.status) {
      switch (error.status) {
        case 401: msg = 'Credenciales inválidas'; break;
        case 403: msg = 'No tienes permisos para esta acción'; break;
        case 404: msg = 'Recurso no encontrado'; break;
        case 500: msg = 'Error interno del servidor'; break;
        default: msg = `Error ${error.status}: ${error.statusText}`;
      }
    }

    console.error('AuthService Error:', error);
    return throwError(() => msg);
  }
}
