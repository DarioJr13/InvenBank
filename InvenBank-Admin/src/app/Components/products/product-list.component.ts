import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';

import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { NotificationService } from '../../services/notification.service';
import { Product, Category } from '../../models';

@Component({
  selector: 'app-product-list',
  standalone: true,
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss'],
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    MatTableModule,
    MatCardModule,
    MatToolbarModule,
    MatDividerModule,
    MatMenuModule
  ]
})
export class ProductListComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  dataSource = new MatTableDataSource<Product>();
  displayedColumns: string[] = [
    'name',
    'sku',
    'brand',
    'categoryName',
    'minPrice',
    'totalStock',
    'suppliersCount',
    'isActive',
    'actions'
  ];

  products: Product[] = [];
  categories: Category[] = [];

  // Filtros
  searchTerm = '';
  selectedCategoryId: number | null = null;
  selectedBrand = '';

  // Estados
  isLoading = false;
  totalRecords = 0;
  pageSize = 10;

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    // this.loadInitialData();
     this.loadCategories();
      this.loadProducts();
  }

  /**
   * Cargar datos iniciales
   */
  // private async loadInitialData(): Promise<void> {
  //   await Promise.all([
  //     this.loadProducts(),
  //     this.loadCategories()
  //   ]);
  // }

  /**
   * Cargar productos
   */
  // private loadProducts(): void {
  //   this.isLoading = true;

  //   this.productService.getAllProducts().subscribe({
  //     next: (response) => {
  //       if (response.success && response.data) {
  //         this.products = response.data;
  //         this.dataSource.data = this.products;
  //         this.dataSource.paginator = this.paginator;
  //         this.totalRecords = this.products.length;
  //       } else {
  //         this.notificationService.error('Error', 'No se pudieron cargar los productos');
  //       }
  //       this.isLoading = false;
  //     },
  //     error: (error) => {
  //       console.error('Error loading products:', error);
  //       this.notificationService.error('Error', 'Error de conexión al cargar productos');
  //       this.isLoading = false;
  //     }
  //   });
  // }
   loadProducts(): void {
    this.isLoading = true;
    this.productService.getAllProducts().subscribe({
      next: res => {
        if (res.success) {
          this.products = res.data;
          this.applyFilter();
        }
        this.isLoading = false;
      },
      error: err => {
        console.error(err);
        this.notificationService.error('Error', 'No se pudieron cargar los productos');
        this.isLoading = false;
      }
    });
  }

  /**
   * Cargar categorías
   */
   loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: res => {
        if (res.success) {
          this.categories = res.data;
        }
      },
      error: err => {
        console.error(err);
        this.notificationService.error('Error', 'No se pudieron cargar las categorías');
      }
    });
  }
  // private loadCategories(): void {
  //   this.categoryService.getAllCategories().subscribe({
  //     next: (response) => {
  //       if (response.success && response.data) {
  //         this.categories = response.data;
  //       }
  //     },
  //     error: (error) => {
  //       console.error('Error loading categories:', error);
  //     }
  //   });
  // }

  /**
   * Aplicar filtros
   */
  applyFilter(): void {
    let filteredData = [...this.products];

    // Filtro por término de búsqueda
    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase().trim();
      filteredData = filteredData.filter(product =>
        product.name.toLowerCase().includes(term) ||
        product.sku.toLowerCase().includes(term) ||
        product.brand?.toLowerCase().includes(term) ||
        product.description?.toLowerCase().includes(term)
      );
    }

    // Filtro por categoría
    if (this.selectedCategoryId) {
      filteredData = filteredData.filter(product =>
        product.categoryId === this.selectedCategoryId
      );
    }

    // Filtro por marca
    if (this.selectedBrand.trim()) {
      const brand = this.selectedBrand.toLowerCase().trim();
      filteredData = filteredData.filter(product =>
        product.brand?.toLowerCase().includes(brand)
      );
    }

    this.dataSource.data = filteredData;
    this.totalRecords = filteredData.length;

    // Reset paginator
    if (this.paginator) {
      this.paginator.firstPage();
    }
  }

  /**
   * Limpiar filtros
   */
  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCategoryId = null;
    this.selectedBrand = '';
    this.dataSource.data = this.products;
    this.totalRecords = this.products.length;

    if (this.paginator) {
      this.paginator.firstPage();
    }
  }

  /**
   * Crear nuevo producto
   */
  createProduct(): void {
    this.router.navigate(['/products/create']);
  }

  /**
   * Ver detalles del producto
   */
  viewProduct(product: Product): void {
    this.router.navigate(['/products', product.id, 'view']);
  }

  /**
   * Editar producto
   */
  editProduct(product: Product): void {
    this.router.navigate(['/products', product.id, 'edit']);
  }

  /**
   * Alternar estado del producto
   */
  toggleProductStatus(product: Product): void {
    this.productService.toggleProductStatus(product.id).subscribe({
      next: (response) => {
        if (response.success) {
          product.isActive = !product.isActive;
          this.notificationService.success(
            'Estado actualizado',
            `Producto ${product.name} está ahora ${product.isActive ? 'activo' : 'inactivo'}`
          );
        } else {
          this.notificationService.error('Error', 'No se pudo actualizar el estado');
        }
      },
      error: (error) => {
        console.error('Error toggling product status:', error);
        this.notificationService.error('Error', 'Error al actualizar el estado');
      }
    });
  }

  /**
   * Eliminar producto
   */
  async deleteProduct(product: Product): Promise<void> {
    const confirmed = await this.notificationService.confirm(
      'Confirmar eliminación',
      `¿Estás seguro de que deseas eliminar el producto "${product.name}"?`,
      'Eliminar',
      'Cancelar'
    );

    if (confirmed) {
      this.productService.deleteProduct(product.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.loadProducts(); // Recargar lista
            this.notificationService.success('Eliminado', `Producto ${product.name} eliminado correctamente`);
          } else {
            this.notificationService.error('Error', 'No se pudo eliminar el producto');
          }
        },
        error: (error) => {
          console.error('Error deleting product:', error);
          this.notificationService.error('Error', 'Error al eliminar el producto');
        }
      });
    }
  }

  /**
   * Gestionar proveedores del producto
   */
  manageSuppliers(product: Product): void {
    this.router.navigate(['/products', product.id, 'suppliers']);
  }

  /**
   * Obtener color del chip de estado
   */
  getStatusColor(isActive: boolean): 'primary' | 'warn' {
    return isActive ? 'primary' : 'warn';
  }

  /**
   * Formatear precio
   */
  formatPrice(price: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(price);
  }

  onPageChange(event: any): void {
    this.pageSize = event.pageSize;
  }
}
