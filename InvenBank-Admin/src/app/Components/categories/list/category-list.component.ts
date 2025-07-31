import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { CategoryService } from '../../../services/category.service';
import { NotificationService } from '../../../services/notification.service';
import { Category } from '../../../models';
import { CategoryFormDialogComponent } from '../dialogs/category-form-dialog.component';

@Component({
  selector: 'app-category-list',
  standalone: true,
  templateUrl: '../list/category-list.component.html',
  styleUrls: ['../list/category-list.component.scss'],
  imports: [
    CommonModule,
    MatCardModule,
    MatChipsModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatDialogModule,
  ]
})
export class CategoryListComponent implements OnInit {
  private categoryService = inject(CategoryService);
  private notificationService = inject(NotificationService);
  private dialog = inject(MatDialog);

  categories: Category[] = [];
  isLoading = false;

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading = true;

    // Simulación de carga de datos
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

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(CategoryFormDialogComponent, {
      width: '400px',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result?.success) {
        this.notificationService.success('Categoría creada', 'La nueva categoría ha sido registrada');
        this.loadCategories();
      }
    });
  }
}
