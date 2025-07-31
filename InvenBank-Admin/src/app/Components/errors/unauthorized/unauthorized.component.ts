import { Component } from '@angular/core';

@Component({
  standalone: true,
  selector: 'app-unauthorized',
  template: `
    <div style="text-align:center; padding: 5rem;">
      <h1>🚫 Acceso Denegado</h1>
      <p>No tienes permisos para acceder a esta página.</p>
      <button mat-raised-button color="primary" routerLink="http://localhost:4200/login">
        Volver al login
      </button>
    </div>
  `,
  styles: []
})
export class UnauthorizedComponent {}
