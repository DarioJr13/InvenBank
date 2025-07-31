// ===============================================
// ⚙️ MODELOS DE CONFIGURACIÓN
// ===============================================

export interface AppConfig {
  apiUrl: string;
  appName: string;
  version: string;
  environment: 'development' | 'production';
  features: {
    enableNotifications: boolean;
    enableAnalytics: boolean;
    enableLogging: boolean;
  };
}
