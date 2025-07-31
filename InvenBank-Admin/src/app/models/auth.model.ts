// ===============================================
// 🔐 MODELOS DE AUTENTICACIÓN
// ===============================================
export interface User {
  id: number;
  firstName: string;
  lastName: string;
  fullName?: string;
  email: string;
  roleName: string;   // ✅ requerido
  roleId: number;     // ✅ requerido
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}


export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  data: {
    token: string;
    refreshToken: string;
    user: User;
  };
  errors: string[];
  timestamp: string;
}
