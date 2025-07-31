import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { ProductService } from '../../services/product.service';
import { SupplierService } from '../../services/supplier.service';
import { CategoryService } from '../../services/category.service';

@Component({
  selector: 'app-overview',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  templateUrl: './overview.component.html',
  styleUrls: ['./overview.component.scss']
})
export class OverviewComponent implements OnInit {
  totalProducts = 0;
  totalSuppliers = 0;
  totalCategories = 0;
  topSellingProduct = '';
  topCategory = '';
  isLoading = true;

  constructor(
    private productService: ProductService,
    private supplierService: SupplierService,
    private categoryService: CategoryService
  ) {}

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.isLoading = true;

    this.productService.getStats().subscribe({
      next: (response) => {
        if (response.success) {
          this.totalProducts = response.data.totalProducts;
          this.topSellingProduct = response.data.topSellingProduct;
        }
      },
      error: (err) => console.error('Product stats error', err)
    });

    this.supplierService.getAllSuppliers().subscribe({
      next: (res) => {
        if (res.success) this.totalSuppliers = res.data.length;
      },
      error: (err) => console.error('Supplier stats error', err)
    });

    this.categoryService.getAllCategories().subscribe({
      next: (res) => {
        if (res.success) {
          this.totalCategories = res.data.length;
          this.topCategory = res.data[0]?.name || '';
        }
      },
      error: (err) => console.error('Category stats error', err),
      complete: () => {
        this.isLoading = false;
      }
    });
  }
}
