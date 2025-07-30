import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Router } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { NotificationService } from '../../services/notification.service';
import { Product, Category, SearchRequest } from '../../models';

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss']
})
export class ProductListComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns: string[] = ['name', 'sku', 'brand', 'categoryName', 'totalStock', 'minPrice', 'maxPrice', 'isActive', 'actions'];
  dataSource = new MatTableDataSource<Product>();

  products: Product[] = [];
  categories: Category[] = [];
  isLoading = false;
  searchTerm = '';
  selectedCategory: number | null = null;
  selectedBrand = '';

  // Paginación
  totalRecords = 0;
  pageSize = 10;
  pageNumber = 1;

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
  }

  /**
   * Cargar categorías para el filtro
   */
  loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (response) => {
        if (response.success) {
          this.categories = response.data;
        }
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  /**
   * Cargar productos con filtros y paginación
   */
  loadProducts(): void {
    this.isLoading = true;

    const searchRequest: SearchRequest = {
      searchTerm: this.searchTerm || undefined,
      categoryId: this.selectedCategory || undefined,
      brand: this.selectedBrand || undefined,
      inStock: true,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      sortBy: 'Name',
      sortDescending: false
    };

    this.productService.searchProducts(searchRequest).subscribe({
      next: (response) => {
        if (response.success) {
          this.products = response.data;
          this.dataSource.data = this.products;
          this.totalRecords = response.totalRecords;
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.notificationService.error('Error', 'No se pudieron cargar los productos');
        this.isLoading = false;
      }
    });
  }

  /**
   * Aplicar filtros de búsqueda
   */
  applyFilter(): void {
    this.pageNumber = 1;
    this.loadProducts();
  }

  /**
   * Limpiar filtros
   */
  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCategory = null;
    this.selectedBrand = '';
    this.applyFilter();
  }

  /**
   * Cambiar página
   */
  onPageChange(event: any): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadProducts();
  }

  /**
   * Navegar a crear producto
   */
  createProduct(): void {
    this.router.navigate(['/products/new']);
  }

  /**
   * Navegar a editar producto
   */
  editProduct(product: Product): void {
    this.router.navigate(['/products/edit', product.id]);
  }

  /**
   * Ver detalles del producto
   */
  viewProduct(product: Product): void {
    this.router.navigate(['/products/view', product.id]);
  }

  /**
   * Eliminar producto
   */
  async deleteProduct(product: Product): Promise<void> {
    const confirmed = await this.notificationService.confirm(
      '¿Eliminar producto?',
      `¿Estás seguro que deseas eliminar "${product.name}"?`,
      'Sí, eliminar',
      'Cancelar'
    );

    if (confirmed) {
      this.productService.deleteProduct(product.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.notificationService.success('Producto eliminado', `${product.name} ha sido eliminado exitosamente`);
            this.loadProducts();
          } else {
            this.notificationService.error('Error', response.message || 'No se pudo eliminar el producto');
          }
        },
        error: (error) => {
          this.notificationService.error('Error', 'No se pudo eliminar el producto');
        }
      });
    }
  }

  /**
   * Toggle estado del producto
   */
  async toggleProductStatus(product: Product): Promise<void> {
    const action = product.isActive ? 'desactivar' : 'activar';
    const confirmed = await this.notificationService.confirm(
      `¿${action.charAt(0).toUpperCase() + action.slice(1)} producto?`,
      `¿Estás seguro que deseas ${action} "${product.name}"?`,
      `Sí, ${action}`,
      'Cancelar'
    );

    if (confirmed) {
      this.productService.toggleProductStatus(product.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.notificationService.success(
              `Producto ${product.isActive ? 'desactivado' : 'activado'}`,
              `${product.name} ha sido ${product.isActive ? 'desactivado' : 'activado'} exitosamente`
            );
            this.loadProducts();
          } else {
            this.notificationService.error('Error', response.message || `No se pudo ${action} el producto`);
          }
        },
        error: (error) => {
          this.notificationService.error('Error', `No se pudo ${action} el producto`);
        }
      });
    }
  }
}
