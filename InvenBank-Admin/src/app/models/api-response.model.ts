// ===============================================
// 🌐 MODELOS DE RESPUESTA DE API
// ===============================================

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
  timestamp: string;
}

export interface ApiError {
  message: string;
  field?: string;
  code?: string;
}
