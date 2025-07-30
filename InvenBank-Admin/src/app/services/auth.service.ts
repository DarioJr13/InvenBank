import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { LoginRequest, LoginResponse, User, TokenPayload } from '../models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl;
  private jwtHelper = new JwtHelperService();

  // Subjects para manejar el estado de autenticación
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);

  // Observables públicos
  public currentUser$ = this.currentUserSubject.asObservable();
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    // Verificar si hay un token almacenado al inicializar
    this.initializeAuth();
  }

  // ===============================================
  // 🔐 MÉTODOS DE AUTENTICACIÓN
  // ===============================================

  /**
   * Inicializar autenticación desde localStorage
   */
  private initializeAuth(): void {
    const token = this.getToken();
    if (token && !this.jwtHelper.isTokenExpired(token)) {
      const user = this.getUserFromToken(token);
      if (user) {
        this.currentUserSubject.next(user);
        this.isAuthenticatedSubject.next(true);
      }
    } else {
      this.logout();
    }
  }

  /**
   * Login del usuario
   */
  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(
        tap(response => {
          if (response.success && response.data) {
            this.setSession(response.data);
          }
        }),
        catchError(this.handleError)
      );
  }

  /**
   * Logout del usuario
   */
  logout(): void {
    // Limpiar localStorage
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user_data');

    // Actualizar subjects
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);

    // Redirigir al login
    this.router.navigate(['/login']);
  }

  /**
   * Refresh token
   */
  refreshToken(): Observable<LoginResponse> {
    const refreshToken = localStorage.getItem('refresh_token');

    if (!refreshToken) {
      this.logout();
      return throwError('No refresh token available');
    }

    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/refresh`, {
      refreshToken: refreshToken
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
  // 🔑 MÉTODOS DE TOKEN
  // ===============================================

  /**
   * Obtener token actual
   */
  getToken(): string | null {
    return localStorage.getItem('access_token');
  }

  /**
   * Verificar si el usuario está autenticado
   */
  isAuthenticated(): boolean {
    const token = this.getToken();
    return token ? !this.jwtHelper.isTokenExpired(token) : false;
  }

  /**
   * Verificar si el usuario es admin
   */
  isAdmin(): boolean {
    const user = this.currentUserSubject.value;
    return user?.role === 'Admin';
  }

  /**
   * Obtener usuario actual
   */
  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  /**
   * Obtener información del usuario desde el token
   */
  private getUserFromToken(token: string): User | null {
  try {
    const decodedToken = this.jwtHelper.decodeToken(token) as TokenPayload | null;

    if (!decodedToken) {
      throw new Error('Token inválido o no se pudo decodificar');
    }

    return {
      id: parseInt(decodedToken.sub),
      firstName: decodedToken.firstName,
      lastName: decodedToken.lastName,
      email: decodedToken.email,
      role: decodedToken.role as 'Admin' | 'Customer',
      isActive: true,
      createdAt: new Date().toISOString()
    };
  } catch (error) {
    console.error('Error decoding token:', error);
    return null;
  }
}


  // ===============================================
  // 💾 MÉTODOS DE SESIÓN
  // ===============================================

  /**
   * Establecer sesión del usuario
   */
  private setSession(authData: LoginResponse['data']): void {
    // Guardar tokens
    localStorage.setItem('access_token', authData.token);
    localStorage.setItem('refresh_token', authData.refreshToken);
    localStorage.setItem('user_data', JSON.stringify(authData.user));

    // Actualizar subjects
    this.currentUserSubject.next(authData.user);
    this.isAuthenticatedSubject.next(true);
  }

  // ===============================================
  // ⚠️ MANEJO DE ERRORES
  // ===============================================

  private handleError(error: any): Observable<never> {
    let errorMessage = 'Ocurrió un error inesperado';

    if (error?.error?.message) {
      errorMessage = error.error.message;
    } else if (error?.message) {
      errorMessage = error.message;
    } else if (error?.status) {
      switch (error.status) {
        case 401:
          errorMessage = 'Credenciales inválidas';
          break;
        case 403:
          errorMessage = 'No tienes permisos para esta acción';
          break;
        case 404:
          errorMessage = 'Recurso no encontrado';
          break;
        case 500:
          errorMessage = 'Error interno del servidor';
          break;
        default:
          errorMessage = `Error ${error.status}: ${error.statusText}`;
      }
    }

    console.error('AuthService Error:', error);
    return throwError(errorMessage);
  }
}
