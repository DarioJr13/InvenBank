import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';
import { User } from '../../models';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent implements OnInit {
  currentUser: User | null = null;
  sidenavOpened = true;

  menuItems = [
    {
      label: 'Dashboard',
      icon: 'dashboard',
      route: '/dashboard',
      active: true
    },
    {
      label: 'Productos',
      icon: 'inventory',
      route: '/products'
    },
    {
      label: 'Proveedores',
      icon: 'business',
      route: '/suppliers'
    },
    {
      label: 'Categorías',
      icon: 'category',
      route: '/categories'
    },
    {
      label: 'Usuarios',
      icon: 'people',
      route: '/users'
    }
  ];

  constructor(
    private authService: AuthService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    // Suscribirse al usuario actual
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
  }

  /**
   * Navegar a una ruta
   */
  navigateTo(route: string): void {
    // Actualizar estado activo
    this.menuItems.forEach(item => {
      item.active = item.route === route;
    });

    this.router.navigate([route]);
  }

  /**
   * Toggle del sidebar
   */
  toggleSidenav(): void {
    this.sidenavOpened = !this.sidenavOpened;
  }

  /**
   * Logout del usuario
   */
  async logout(): Promise<void> {
    const confirmed = await this.notificationService.confirm(
      '¿Cerrar sesión?',
      '¿Estás seguro que deseas cerrar sesión?',
      'Sí, cerrar sesión',
      'Cancelar'
    );

    if (confirmed) {
      this.authService.logout();
      this.notificationService.success('Sesión cerrada', 'Has cerrado sesión exitosamente');
    }
  }

  /**
   * Obtener iniciales del usuario
   */
  getUserInitials(): string {
    if (!this.currentUser) return 'U';
    return `${this.currentUser.firstName[0]}${this.currentUser.lastName[0]}`.toUpperCase();
  }
}
