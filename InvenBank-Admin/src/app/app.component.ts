import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Observable } from 'rxjs';
import { LoadingService } from './services/loading.service';

@Component({
  standalone: true, // ✅ Ahora es standalone
  selector: 'app-root',
  imports: [
    CommonModule,
    RouterOutlet,
    MatProgressSpinnerModule
  ],
  template: `
    <!-- Loading global -->
    <div *ngIf="loading$ | async" class="global-loading">
      <div class="loading-content">
        <mat-spinner diameter="60"></mat-spinner>
        <p>Cargando...</p>
      </div>
    </div>

    <!-- Contenido principal -->
    <router-outlet></router-outlet>
  `,
  styles: [`
    .global-loading {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(255, 255, 255, 0.9);
      z-index: 9999;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .loading-content {
      display: flex;
      flex-direction: column;
      align-items: center;
      color: #c2185b;
    }

    .loading-content p {
      margin-top: 1rem;
      font-weight: 500;
    }
  `]
})
export class AppComponent implements OnInit {
  title = 'InvenBank Admin';
  loading$: Observable<boolean>;

  constructor(private loadingService: LoadingService) {
    this.loading$ = this.loadingService.loading$;
  }

  ngOnInit(): void {}
}
