// ===============================================
// 🔔 MODELOS PARA NOTIFICACIONES
// ===============================================

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  duration?: number;
  timestamp: Date;
}
