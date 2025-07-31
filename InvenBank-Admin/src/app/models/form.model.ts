// ===============================================
// 📋 MODELOS PARA FORMULARIOS
// ===============================================

export interface FormField {
  name: string;
  label: string;
  type: 'text' | 'email' | 'password' | 'number' | 'select' | 'textarea';
  required: boolean;
  placeholder?: string;
  options?: { value: any; label: string }[];
  validation?: {
    minLength?: number;
    maxLength?: number;
    min?: number;
    max?: number;
    pattern?: string;
  };
}
