import { Component, OnInit } from '@angular/core';
import { CategoryService } from '../../services/category.service';
import { NotificationService } from '../../services/notification.service';
import { Category } from '../../models';

@Component({
  selector: 'app-category-list',
  template: `
    <div class="category-list-container">
      <!-- Header -->
      <div class="page-header">
        <div class="header-content">
          <h1>Categorías</h1>
          <p>Gestiona las categorías de productos</p>
        </div>
        <button mat-raised-button color="primary" class="create-button">
          <mat-icon>add</mat-icon>
          Nueva Categoría
        </button>
      </div>

      <!-- Loading -->
      <div *ngIf="isLoading" class="loading-container">
        <mat-spinner diameter="50"></mat-spinner>
        <p>Cargando categorías...</p>
      </div>

      <!-- Lista de categorías -->
      <mat-card *ngIf="!isLoading" class="categories-card">
        <mat-card-content>
          <div class="categories-grid">
            <div *ngFor="let category of categories" class="category-item">
              <div class="category-info">
                <h3>{{ category.name }}</h3>
                <p>{{ category.description }}</p>
                <div class="category-stats">
                  <mat-chip class="products-count">
                    {{ category.productsCount }} productos
                  </mat-chip>
                  <mat-chip [class]="category.isActive ? 'status-active' : 'status-inactive'">
                    {{ category.isActive ? 'Activa' : 'Inactiva' }}
                  </mat-chip>
                </div>
              </div>
              <div class="category-actions">
                <button mat-icon-button matTooltip="Editar">
                  <mat-icon>edit</mat-icon>
                </button>
                <button mat-icon-button matTooltip="Eliminar" color="warn">
                  <mat-icon>delete</mat-icon>
                </button>
              </div>
            </div>
          </div>
        </mat-card-content>
      </mat-card>

      <!-- Sin datos -->
      <div *ngIf="!isLoading && categories.length === 0" class="no-data">
        <mat-icon>category</mat-icon>
        <p>No hay categorías registradas</p>
        <button mat-raised-button color="primary">
          Crear primera categoría
        </button>
      </div>
    </div>
  `,
  styles: [`
    .category-list-container {
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

      .categories-card {
        .categories-grid {
          display: grid;
          gap: 1.5rem;

          .category-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 1.5rem;
            border: 1px solid #eee;
            border-radius: 8px;
            transition: box-shadow 0.3s;

            &:hover {
              box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            }

            .category-info {
              flex: 1;

              h3 {
                margin: 0 0 0.5rem 0;
                color: #333;
              }

              p {
                margin: 0 0 1rem 0;
                color: #666;
              }

              .category-stats {
                display: flex;
                gap: 0.5rem;

                .products-count {
                  background: rgba(194, 24, 91, 0.1);
                  color: #c2185b;
                }

                .status-active {
                  background: #e8f5e8;
                  color: #2e7d32;
                }

                .status-inactive {
                  background: #ffebee;
                  color: #d32f2f;
                }
              }
            }

            .category-actions {
              display: flex;
              gap: 0.5rem;
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
  `]
})
export class CategoryListComponent implements OnInit {
  categories: Category[] = [];
  isLoading = false;

  constructor(
    private categoryService: CategoryService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading = true;

    // Simular datos mientras se implementa el endpoint
    setTimeout(() => {
      this.categories = [
        {
          id: 1,
          name: 'Electrónicos',
          description: 'Dispositivos electrónicos y gadgets',
          isActive: true,
          createdAt: '2024-01-01',
          updatedAt: '2024-01-01',
          productsCount: 45
        },
        {
          id: 2,
          name: 'Computadoras',
          description: 'Laptops, desktops y accesorios',
          isActive: true,
          createdAt: '2024-01-01',
          updatedAt: '2024-01-01',
          productsCount: 28
        },
        {
          id: 3,
          name: 'Móviles',
          description: 'Smartphones y accesorios móviles',
          isActive: true,
          createdAt: '2024-01-01',
          updatedAt: '2024-01-01',
          productsCount: 35
        }
      ];
      this.isLoading = false;
    }, 1000);
  }
}
