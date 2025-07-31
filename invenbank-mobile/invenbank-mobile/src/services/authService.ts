import { apiClient } from './api';
import { ENDPOINTS, STORAGE_KEYS } from '../utils/constants';
import { LoginRequest, LoginResponse, User } from '../types/auth.types';
import { ApiResponse } from '../types/api.types';

export class AuthService {
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>(
      ENDPOINTS.AUTH.LOGIN,
      credentials
    );
    
    if (response.success && response.data) {
      this.saveAuthData(response.data);
      return response.data;
    }
    
    throw new Error(response.message || 'Login failed');
  }

  private saveAuthData(authData: LoginResponse): void {
    localStorage.setItem(STORAGE_KEYS.TOKEN, authData.token);
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, authData.refreshToken);
    localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(authData.user));
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEYS.TOKEN);
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.USER);
  }

  getStoredUser(): User | null {
    const userStr = localStorage.getItem(STORAGE_KEYS.USER);
    return userStr ? JSON.parse(userStr) : null;
  }

  getStoredToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.TOKEN);
  }

  isAuthenticated(): boolean {
    const token = this.getStoredToken();
    return !!token;
  }
}

export const authService = new AuthService();