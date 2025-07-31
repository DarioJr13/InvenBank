import { HttpInterceptorFn } from '@angular/common/http';
import { HttpRequest, HttpHandlerFn, HttpEvent } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { JwtHelperService } from '@auth0/angular-jwt';

const jwtHelper = new JwtHelperService();

export const AuthInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> => {
  let token = localStorage.getItem('access_token');

  // ⚠️ Validar expiración antes de enviar
  if (token) {
    if (jwtHelper.isTokenExpired(token)) {
      console.warn('[AuthInterceptor] Token expirado. Cerrando sesión...');
      localStorage.clear();
      window.location.href = '/login';  // ⛔ Cierra sesión y redirige
      return throwError(() => new Error('Token expirado. Redirigiendo...'));
    }

    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req).pipe(
    catchError(error => {
      // 👀 Manejo adicional si el backend responde 401 por token vencido
      if (error?.status === 401 && jwtHelper.isTokenExpired(token || '')) {
        console.warn('[AuthInterceptor] Expiración detectada en respuesta. Cerrando sesión...');
        localStorage.clear();
        window.location.href = '/login';
      }

      return throwError(() => error);
    })
  );
};
