import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { NotificationService } from '../../services/notification.service';
import { User } from '../../models';

@Component({
  selector: 'app-user-list',
  template: `
    <div class="user-list-container">
      <!-- Header -->
      <div class="page-header">
        <div class="header-content">
          <h1>Usuarios</h1>
          <p>Gestiona los usuarios del sistema</p>
        </div>
        <button mat-raised-button color="primary" class="create-button">
          <mat-icon>add</mat-icon>
          Nuevo Usuario
        </button>
      </div>

      <!-- Loading -->
      <div *ngIf="isLoading" class="loading-container">
        <mat-spinner diameter="50"></mat-spinner>
        <p>Cargando usuarios...</p>
      </div>

      <!-- Tabla de usuarios -->
      <mat-card *ngIf="!isLoading" class="users-table-card">
        <mat-card-content>
          <table mat-table [dataSource]="users" class="users-table">

            <ng-container matColumnDef="name">
              <th mat-header-cell *matHeaderCellDef>Nombre</th>
              <td mat-cell *matCellDef="let user">
                <div class="user-info">
                  <div class="user-avatar">{{ getUserInitials(user) }}</div>
                  <div class="user-details">
                    <div class="user-name">{{ user.firstName }} {{ user.lastName }}</div>
                    <div class="user-email">{{ user.email }}</div>
                  </div>
                </div>
              </td>
            </ng-container>

            <ng-container matColumnDef="role">
              <th mat-header-cell *matHeaderCellDef>Rol</th>
              <td mat-cell *matCellDef="let user">
                <mat-chip [class]="user.role === 'Admin' ? 'role-admin' : 'role-customer'">
                  {{ user.role }}
                </mat-chip>
              </td>
            </ng-container>

            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>Estado</th>
              <td mat-cell *matCellDef="let user">
                <mat-chip [class]="user.isActive ? 'status-active' : 'status-inactive'">
                  {{ user.isActive ? 'Activo' : 'Inactivo' }}
                </mat-chip>
              </td>
            </ng-container>

            <ng-container matColumnDef="lastLogin">
              <th mat-header-cell *matHeaderCellDef>Último Acceso</th>
              <td mat-cell *matCellDef="let user">
                {{ user.lastLoginAt ? (user.lastLoginAt | date:'short') : 'Nunca' }}
              </td>
            </ng-container>

            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>Acciones</th>
              <td mat-cell *matCellDef="let user">
                <div class="actions-container">
                  <button mat-icon-button matTooltip="Editar">
                    <mat-icon>edit</mat-icon>
                  </button>
                  <button mat-icon-button matTooltip="Cambiar contraseña">
                    <mat-icon>lock</mat-icon>
                  </button>
                  <button mat-icon-button matTooltip="Eliminar" color="warn">
                    <mat-icon>delete</mat-icon>
                  </button>
                </div>
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="['name', 'role', 'status', 'lastLogin', 'actions']"></tr>
            <tr mat-row *matRowDef="let row; columns: ['name', 'role', 'status', 'lastLogin', 'actions'];"></tr>
          </table>
        </mat-card-content>
      </mat-card>

      <!-- Sin datos -->
      <div *ngIf="!isLoading && users.length === 0" class="no-data">
        <mat-icon>people</mat-icon>
        <p>No hay usuarios registrados</p>
        <button mat-raised-button color="primary">
          Crear primer usuario
        </button>
      </div>
    </div>
  `,
  styles: [`
    .user-list-container {
      .page-header {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        margin-bottom: 2rem;

        .header-content {
          h1 {
            font-size: 2rem;
            color: #333;
            margin: 0 0 0.5rem 0;
          }

          p {
            color: #666;
            margin: 0;
          }
        }

        .create-button {
          display: flex;
          align-items: center;
          gap: 0.5rem;
          font-weight: 600;
        }
      }

      .loading-container {
        display: flex;
        flex-direction: column;
        align-items: center;
        padding: 4rem;
        color: #666;

        p {
          margin-top: 1rem;
        }
      }

      .users-table-card {
        .users-table {
          width: 100%;

          .user-info {
            display: flex;
            align-items: center;
            gap: 1rem;

            .user-avatar {
              width: 40px;
              height: 40px;
              border-radius: 50%;
              background: linear-gradient(135deg, #c2185b 0%, #e91e63 100%);
              color: white;
              display: flex;
              align-items: center;
              justify-content: center;
              font-weight: 600;
              font-size: 0.9rem;
            }

            .user-details {
              .user-name {
                font-weight: 600;
                color: #333;
              }

              .user-email {
                font-size: 0.8rem;
                color: #666;
              }
            }
          }

          .role-admin {
            background: rgba(194, 24, 91, 0.1);
            color: #c2185b;
          }

          .role-customer {
            background: rgba(0, 123, 255, 0.1);
            color: #007bff;
          }

          .status-active {
            background: #e8f5e8;
            color: #2e7d32;
          }

          .status-inactive {
            background: #ffebee;
            color: #d32f2f;
          }

          .actions-container {
            display: flex;
            gap: 0.25rem;
          }
        }
      }

      .no-data {
        text-align: center;
        padding: 4rem;
        color: #666;

        mat-icon {
          font-size: 4rem;
          width: 4rem;
          height: 4rem;
          margin-bottom: 1rem;
          opacity: 0.5;
        }

        p {
          font-size: 1.1rem;
          margin-bottom: 1.5rem;
        }
      }
    }
  `]
})
export class UserListComponent implements OnInit {
  users: User[] = [];
  isLoading = false;

  constructor(
    private userService: UserService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;

    // Simular datos mientras se implementa el endpoint
    setTimeout(() => {
      this.users = [
        {
          id: 1,
          firstName: 'Alejandro',
          lastName: 'Pérez',
          email: 'alejandro@example.com',
          role: 'Admin',
          isActive: true,
          createdAt: '2024-01-01',
          lastLoginAt: '2024-01-15T10:30:00Z'
        },
        {
          id: 2,
          firstName: 'María',
          lastName: 'Gómez',
          email: 'maria@example.com',
          role: 'Admin',
          isActive: true,
          createdAt: '2024-01-02',
          lastLoginAt: '2024-01-14T15:45:00Z'
        },
        {
          id: 3,
          firstName: 'Juan',
          lastName: 'Rodríguez',
          email: 'juan@example.com',
          role: 'Customer',
          isActive: true,
          createdAt: '2024-01-03',
          lastLoginAt: '2024-01-13T09:20:00Z'
        }
      ];
      this.isLoading = false;
    }, 1000);
  }

  getUserInitials(user: User): string {
    return `${user.firstName[0]}${user.lastName[0]}`.toUpperCase();
  }
}
