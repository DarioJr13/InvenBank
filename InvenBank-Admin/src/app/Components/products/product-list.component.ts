import { Component, OnInit } from '@angular/core';
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
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';

import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { NotificationService } from '../../services/notification.service';
import { Product } from '../../models';

import { MatTableDataSource } from '@angular/material/table';

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
    MatTableModule
  ]
})
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  categories: string[] = [];
  dataSource = new MatTableDataSource<Product>();
  displayedColumns: string[] = [
    'name',
    'sku',
    'brand',
    'categoryName',
    'isActive',
    'actions'
  ];

  searchTerm = '';
  selectedCategory: string = '';
  selectedBrand: string = '';

  totalRecords = 0;
  pageSize = 10;
  currentPage = 0;
  isLoading = false;

  constructor(
    private productService: ProductService,
    private categoryService: CategoryService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.fetchProducts();
  }

  fetchProducts(): void {
    this.isLoading = true;

    this.productService.getProducts().subscribe({
      next: (products) => {
        this.products = products;
        this.dataSource.data = products;
        this.totalRecords = products.length;
        this.applyFilter();
      },
      error: () => {
        this.notificationService.error('Error', 'No se pudieron cargar los productos');
        this.isLoading = false;
      }
    });

    this.categoryService.getAllCategories().subscribe({
      next: (categories) => {
        this.categories = categories.map(c => c.name);
      },
      error: () => {
        this.notificationService.error('Error', 'No se pudieron cargar las categorías');
      }
    });
  }

  applyFilter(): void {
    let filtered = this.products;

    if (this.searchTerm) {
      const term = this.searchTerm.toLowerCase();
      filtered = filtered.filter(product =>
        product.name.toLowerCase().includes(term) ||
        product.brand.toLowerCase().includes(term)
      );
    }

    if (this.selectedCategory) {
      filtered = filtered.filter(product => product.categoryName === this.selectedCategory);
    }

    if (this.selectedBrand) {
      filtered = filtered.filter(product => product.brand === this.selectedBrand);
    }

    this.dataSource.data = filtered.slice(
      this.currentPage * this.pageSize,
      (this.currentPage + 1) * this.pageSize
    );
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedCategory = '';
    this.selectedBrand = '';
    this.applyFilter();
  }

  viewProduct(product: Product): void {
    this.router.navigate(['/products', product.id, 'view']);
  }

  editProduct(product: Product): void {
    this.router.navigate(['/products', product.id, 'edit']);
  }

  deleteProduct(product: Product): void {
    this.notificationService.success('Eliminado', `Producto ${product.name} eliminado`);
  }

  toggleProductStatus(product: Product): void {
    product.isActive = !product.isActive;
    this.notificationService.success(
      'Estado actualizado',
      `Producto ${product.name} está ahora ${product.isActive ? 'activo' : 'inactivo'}`
    );
  }

  onPageChange(event: any): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.applyFilter();
  }

  createProduct(id: number): void {
    this.router.navigate(['/products', id, 'edit']);
  }
}
