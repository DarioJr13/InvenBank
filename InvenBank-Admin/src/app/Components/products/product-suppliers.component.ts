import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';

import { ProductSupplierService } from '../../services/product-supplier.service';
import { ProductService } from '../../services/product.service';
import { NotificationService } from '../../services/notification.service';
import { ProductSupplier, Product } from '../../models';
import { ProductSupplierModalComponent, ProductSupplierModalData } from './product-supplier-modal.component';

@Component({
  selector: 'app-product-suppliers',
  standalone: true,
  templateUrl: './product-suppliers.component.html',
  styleUrls: ['./product-suppliers.component.scss'],
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatToolbarModule,
    MatMenuModule
  ]
})
export class ProductSuppliersComponent implements OnInit {
  productId!: number;
  product: Product | null = null;
  dataSource = new MatTableDataSource<ProductSupplier>();
  displayedColumns = ['supplierName', 'price', 'stock', 'batchNumber', 'supplierSku', 'isActive', 'actions'];
  isLoading = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private dialog: MatDialog,
    private productSupplierService: ProductSupplierService,
    private productService: ProductService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.productId = +this.route.snapshot.paramMap.get('id')!;
    this.loadProduct();
    this.loadProductSuppliers();
  }

  private loadProduct(): void {
    this.productService.getProductById(this.productId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.product = response.data;
        }
      },
      error: (error) => {
        console.error('Error loading product:', error);
        this.notificationService.error('Error', 'No se pudo cargar el producto');
        this.goBack();
      }
    });
  }

  private loadProductSuppliers(): void {
    this.isLoading = true;
    this.productSupplierService.getByProductId(this.productId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.dataSource.data = response.data;
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading product suppliers:', error);
        this.notificationService.error('Error', 'No se pudieron cargar los proveedores');
        this.isLoading = false;
      }
    });
  }

  addSupplier(): void {
    if (!this.product) return;

    const dialogData: ProductSupplierModalData = {
      productId: this.productId,
      productName: this.product.name
    };

    const dialogRef = this.dialog.open(ProductSupplierModalComponent, {
      width: '500px',
      data: dialogData,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadProductSuppliers();
      }
    });
  }

  editSupplier(productSupplier: ProductSupplier): void {
    if (!this.product) return;

    const dialogData: ProductSupplierModalData = {
      productId: this.productId,
      productName: this.product.name,
      productSupplier
    };

    const dialogRef = this.dialog.open(ProductSupplierModalComponent, {
      width: '500px',
      data: dialogData,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadProductSuppliers();
      }
    });
  }

  async toggleSupplierStatus(productSupplier: ProductSupplier): Promise<void> {
    this.productSupplierService.delete(this.productId, productSupplier.id).subscribe({
      next: (response) => {
        if (response.success) {
          productSupplier.isActive = !productSupplier.isActive;
          this.notificationService.success(
            'Estado actualizado',
            `Proveedor ${productSupplier.isActive ? 'activado' : 'desactivado'}`
          );
        }
      },
      error: (error) => {
        console.error('Error toggling supplier status:', error);
        this.notificationService.error('Error', 'No se pudo actualizar el estado');
      }
    });
  }

  async deleteSupplier(productSupplier: ProductSupplier): Promise<void> {
    const confirmed = await this.notificationService.confirm(
      'Confirmar eliminación',
      `¿Eliminar proveedor ${productSupplier.supplierName}?`,
      'Eliminar',
      'Cancelar'
    );

    if (confirmed) {
      this.productSupplierService.delete(this.productId, productSupplier.id).subscribe({
        next: (response) => {
          if (response.success) {
            this.loadProductSuppliers();
            this.notificationService.success('Eliminado', 'Proveedor eliminado del producto');
          }
        },
        error: (error) => {
          console.error('Error deleting supplier:', error);
          this.notificationService.error('Error', 'No se pudo eliminar el proveedor');
        }
      });
    }
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(price);
  }

  getStatusColor(isActive: boolean): 'primary' | 'warn' {
    return isActive ? 'primary' : 'warn';
  }

  goBack(): void {
    this.router.navigate(['/products']);
  }
}
