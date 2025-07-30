import { Component, OnInit } from '@angular/core';
import { SupplierService } from '../../services/supplier.service';
import { NotificationService } from '../../services/notification.service';
import { Supplier } from '../../models';

@Component({
  selector: 'app-supplier-list',
  template: `
    <div class="supplier-list-container">
      <!-- Header -->
      <div class="page-header">
        <div class="header-content">
          <h1>Proveedores</h1>
          <p>Gestiona los proveedores del sistema</p>
        </div>
        <button mat-raised-button color="primary" class="create-button">
          <mat-icon>add</mat-icon>
          Nuevo Proveedor
        </button>
      </div>

      <!-- Loading -->
      <div *ngIf="isLoading" class="loading-container">
        <mat-spinner diameter="50"></mat-spinner>
        <p>Cargando proveedores...</p>
      </div>

      <!-- Lista de proveedores -->
      <div *ngIf="!isLoading" class="suppliers-grid">
        <mat-card *ngFor="let supplier of suppliers" class="supplier-card">
          <mat-card-header>
            <mat-card-title>{{ supplier.name }}</mat-card-title>
            <mat-card-subtitle>{{ supplier.contactName }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <div class="supplier-info">
              <div class="info-item">
                <mat-icon>email</mat-icon>
                <span>{{ supplier.email }}</span>
              </div>
              <div class="info-item">
                <mat-icon>phone</mat-icon>
                <span>{{ supplier.phone }}</span>
              </div>
              <div class="info-item">
                <mat-icon>location_on</mat-icon>
                <span>{{ supplier.city }}, {{ supplier.country }}</span>
              </div>
              <div class="supplier-stats">
                <div class="stat">
                  <span class="stat-value">{{ supplier.productsCount }}</span>
                  <span class="stat-label">Productos</span>
                </div>
                <div class="stat">
                  <span class="stat-value">{{ supplier.totalStock }}</span>
                  <span class="stat-label">Stock Total</span>
                </div>
              </div>
            </div>
          </mat-card-content>
          <mat-card-actions>
            <button mat-button color="primary">
              <mat-icon>edit</mat-icon>
              Editar
            </button>
            <button mat-button color="warn">
              <mat-icon>delete</mat-icon>
              Eliminar
            </button>
          </mat-card-actions>
        </mat-card>
      </div>

      <!-- Sin datos -->
      <div *ngIf="!isLoading && suppliers.length === 0" class="no-data">
        <mat-icon>business</mat-icon>
        <p>No hay proveedores registrados</p>
        <button mat-raised-button color="primary">
          Crear primer proveedor
        </button>
      </div>
    </div>
  `,
  styles: [`
    .supplier-list-container {
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

      .suppliers-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
        gap: 2rem;

        .supplier-card {
          .supplier-info {
            .info-item {
              display: flex;
              align-items: center;
              gap: 0.5rem;
              margin-bottom: 0.5rem;
              color: #666;

              mat-icon {
                font-size: 1.2rem;
                width: 1.2rem;
                height: 1.2rem;
              }
            }

            .supplier-stats {
              display: flex;
              gap: 2rem;
              margin-top: 1rem;
              padding-top: 1rem;
              border-top: 1px solid #eee;

              .stat {
                text-align: center;

                .stat-value {
                  display: block;
                  font-size: 1.5rem;
                  font-weight: 600;
                  color: #c2185b;
                }

                .stat-label {
                  font-size: 0.8rem;
                  color: #666;
                }
              }
            }
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

    @media (max-width: 768px) {
      .supplier-list-container {
        .suppliers-grid {
          grid-template-columns: 1fr;
        }
      }
    }
  `]
})
export class SupplierListComponent implements OnInit {
  suppliers: Supplier[] = [];
  isLoading = false;

  constructor(
    private supplierService: SupplierService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadSuppliers();
  }

  loadSuppliers(): void {
    this.isLoading = true;

    // Simular datos mientras se implementa el endpoint
    setTimeout(() => {
      this.suppliers = [
        {
          id: 1,
          name: 'TechSupply Corp',
          contactName: 'Juan Pérez',
          email: 'contacto@techsupply.com',
          phone: '+1-555-0123',
          address: '123 Tech Street',
          city: 'Miami',
          country: 'USA',
          isActive: true,
          createdAt: '2024-01-01',
          updatedAt: '2024-01-01',
          productsCount: 15,
          totalStock: 250,
          averagePrice: 350.50
        },
        {
          id: 2,
          name: 'ElectroMax Solutions',
          contactName: 'María González',
          email: 'ventas@electromax.com',
          phone: '+1-555-0124',
          address: '456 Electronic Ave',
          city: 'New York',
          country: 'USA',
          isActive: true,
          createdAt: '2024-01-02',
          updatedAt: '2024-01-02',
          productsCount: 12,
          totalStock: 180,
          averagePrice: 275.00
        }
      ];
      this.isLoading = false;
    }, 1000);
  }
}
