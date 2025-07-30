import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import Swal from 'sweetalert2';
import { Notification } from '../models';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);

  constructor() {}

  get notifications$(): Observable<Notification[]> {
    return this.notificationsSubject.asObservable();
  }

  /**
   * Mostrar notificación de éxito
   */
  success(title: string, message: string = ''): void {
    Swal.fire({
      icon: 'success',
      title: title,
      text: message,
      confirmButtonColor: '#c2185b',
      timer: 3000,
      timerProgressBar: true
    });
  }

  /**
   * Mostrar notificación de error
   */
  error(title: string, message: string = ''): void {
    Swal.fire({
      icon: 'error',
      title: title,
      text: message,
      confirmButtonColor: '#c2185b'
    });
  }

  /**
   * Mostrar notificación de advertencia
   */
  warning(title: string, message: string = ''): void {
    Swal.fire({
      icon: 'warning',
      title: title,
      text: message,
      confirmButtonColor: '#c2185b'
    });
  }

  /**
   * Mostrar notificación de información
   */
  info(title: string, message: string = ''): void {
    Swal.fire({
      icon: 'info',
      title: title,
      text: message,
      confirmButtonColor: '#c2185b'
    });
  }

  /**
   * Mostrar diálogo de confirmación
   */
  confirm(title: string, message: string = '', confirmText: string = 'Sí', cancelText: string = 'No'): Promise<boolean> {
    return Swal.fire({
      icon: 'question',
      title: title,
      text: message,
      showCancelButton: true,
      confirmButtonColor: '#c2185b',
      cancelButtonColor: '#6c757d',
      confirmButtonText: confirmText,
      cancelButtonText: cancelText
    }).then((result) => {
      return result.isConfirmed;
    });
  }

  /**
   * Mostrar loading con mensaje
   */
  showLoading(message: string = 'Cargando...'): void {
    Swal.fire({
      title: message,
      allowOutsideClick: false,
      allowEscapeKey: false,
      showConfirmButton: false,
      didOpen: () => {
        Swal.showLoading();
      }
    });
  }

  /**
   * Cerrar loading
   */
  hideLoading(): void {
    Swal.close();
  }
}
