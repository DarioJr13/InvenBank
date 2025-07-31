import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';

import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';
import { LoadingService } from '../../services/loading.service';

@Component({
  standalone: true,
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCardModule
  ]
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
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';

    if (this.authService.isAuthenticated()) {
      this.router.navigateByUrl(this.returnUrl);
    }
  }

  get f() {
    return this.loginForm.controls;
  }

  hasError(fieldName: string, errorType: string): boolean {
    const field = this.loginForm.get(fieldName);
    return field ? field.hasError(errorType) && (field.dirty || field.touched) : false;
  }

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

  onSubmit(): void {
    if (this.loginForm.invalid) {
      Object.keys(this.loginForm.controls).forEach(key => {
        this.loginForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.isLoading = true;

    const { email, password } = this.loginForm.value;

    this.authService.login({ email, password }).subscribe({
      next: (res) => {
        this.isLoading = false;

        if (res.success && res.data?.token) {
          this.notificationService.success(
            '¡Bienvenido!',
            `Hola ${res.data.user?.firstName || 'usuario'}, has iniciado sesión exitosamente`
          );

          this.router.navigateByUrl(this.returnUrl);
        } else {
          this.notificationService.error(
            'Error de autenticación',
            res.message || 'Credenciales inválidas'
          );
        }
      },
      error: (error) => {
        this.isLoading = false;
        console.error('❌ Error en login:', error);
        this.notificationService.error(
          'Error de conexión',
          error?.error?.message || 'No se pudo conectar con el servidor'
        );
      }
    });
  }

  togglePasswordVisibility(): void {
    this.hidePassword = !this.hidePassword;
  }
}
