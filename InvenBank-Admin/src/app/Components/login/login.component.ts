import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';
import { LoadingService } from '../../services/loading.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  hidePassword = true;
  returnUrl: string = '/dashboard';
  isLoading = false;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private notificationService: NotificationService,
    private loadingService: LoadingService
  ) {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    // Obtener URL de retorno de los query params
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || this.returnUrl;

    // Si ya está autenticado, redirigir
    if (this.authService.isAuthenticated()) {
      this.router.navigate([this.returnUrl]);
    }
  }

  /**
   * Obtener control del formulario
   */
  get f() {
    return this.loginForm.controls;
  }

  /**
   * Verificar si un campo tiene errores
   */
  hasError(fieldName: string, errorType: string): boolean {
    const field = this.loginForm.get(fieldName);
    return field ? field.hasError(errorType) && (field.dirty || field.touched) : false;
  }

  /**
   * Obtener mensaje de error para un campo
   */
  getErrorMessage(fieldName: string): string {
    const field = this.loginForm.get(fieldName);

    if (field?.hasError('required')) {
      return `${fieldName === 'email' ? 'Email' : 'Contraseña'} es requerido`;
    }

    if (field?.hasError('email')) {
      return 'Email debe tener un formato válido';
    }

    if (field?.hasError('minlength')) {
      return 'Contraseña debe tener al menos 6 caracteres';
    }

    return '';
  }

  /**
   * Enviar formulario de login
   */
  onSubmit(): void {
    if (this.loginForm.invalid) {
      // Marcar todos los campos como tocados para mostrar errores
      Object.keys(this.loginForm.controls).forEach(key => {
        this.loginForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.isLoading = true;

    const loginData = {
      email: this.f['email'].value,
      password: this.f['password'].value
    };

    this.authService.login(loginData).subscribe({
      next: (response) => {
        this.isLoading = false;

        if (response.success) {
          this.notificationService.success(
            '¡Bienvenido!',
            `Hola ${response.data.user.firstName}, has iniciado sesión exitosamente`
          );

          // Redirigir a la URL solicitada o al dashboard
          this.router.navigate([this.returnUrl]);
        } else {
          this.notificationService.error(
            'Error de autenticación',
            response.message || 'Credenciales inválidas'
          );
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.notificationService.error(
          'Error de conexión',
          error || 'No se pudo conectar con el servidor'
        );
      }
    });
  }

  /**
   * Toggle visibilidad de contraseña
   */
  togglePasswordVisibility(): void {
    this.hidePassword = !this.hidePassword;
  }
}
