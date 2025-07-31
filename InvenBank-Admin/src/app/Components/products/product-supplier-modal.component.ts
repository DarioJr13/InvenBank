import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ProductSupplierService } from '../../services/product-supplier.service';
import { SupplierService } from '../../services/supplier.service';
import { NotificationService } from '../../services/notification.service';
import { ProductSupplier, Supplier, CreateProductSupplierRequest, UpdateProductSupplierRequest } from '../../models';

export interface ProductSupplierModalData {
  productId: number;
  productName: string;
  productSupplier?: ProductSupplier;
}

@Component({
  selector: 'app-product-supplier-modal',
  standalone: true,
  templateUrl: './product-supplier-modal.component.html',
  styleUrls: ['./product-supplier-modal.component.scss'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ]
})
export class ProductSupplierModalComponent implements OnInit {
  form: FormGroup;
  suppliers: Supplier[] = [];
  isLoading = false;
  isSaving = false;
  isEditMode = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ProductSupplierModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ProductSupplierModalData,
    private productSupplierService: ProductSupplierService,
    private supplierService: SupplierService,
    private notificationService: NotificationService
  ) {
    this.isEditMode = !!data.productSupplier;
    this.form = this.createForm();
  }

  ngOnInit(): void {
    this.loadSuppliers();
    if (this.isEditMode) {
      this.populateForm();
    }
  }

  private createForm(): FormGroup {
    return this.fb.group({
      supplierId: [null, [Validators.required]],
      price: [0, [Validators.required, Validators.min(0.01)]],
      stock: [0, [Validators.required, Validators.min(0)]],
      batchNumber: ['', [Validators.required]],
      supplierSku: ['', [Validators.required]]
    });
  }

  private populateForm(): void {
    if (!this.data.productSupplier) return;

    const ps = this.data.productSupplier;
    this.form.patchValue({
      supplierId: ps.supplierId,
      price: ps.price,
      stock: ps.stock,
      batchNumber: ps.batchNumber,
      supplierSku: ps.supplierSku
    });

    // Disable supplier selection in edit mode
    this.form.get('supplierId')?.disable();
  }

  private loadSuppliers(): void {
   this.isLoading = true;

  this.supplierService.getAllSuppliers().subscribe({
    next: (response) => {
      if (response.success && response.data) {
        this.suppliers = response.data; // Si deseas, puedes filtrar aquí por isActive
      }
      this.isLoading = false;
    },
    error: (error) => {
      console.error('Error loading suppliers:', error);
      this.notificationService.error('Error', 'No se pudieron cargar los proveedores');
      this.isLoading = false;
    }
  });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isSaving = true;
    const formValue = this.form.getRawValue();

    if (this.isEditMode) {
      this.updateProductSupplier(formValue);
    } else {
      this.createProductSupplier(formValue);
    }
  }

  private createProductSupplier(formValue: any): void {
    const request: CreateProductSupplierRequest = {
      productId: this.data.productId,
      supplierId: formValue.supplierId,
      price: formValue.price,
      stock: formValue.stock,
      batchNumber: formValue.batchNumber,
      supplierSku: formValue.supplierSku
    };

    this.productSupplierService.create(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.notificationService.success('Éxito', 'Proveedor agregado al producto');
          this.dialogRef.close(true);
        } else {
          this.notificationService.error('Error', 'No se pudo agregar el proveedor');
        }
        this.isSaving = false;
      },
      error: (error) => {
        console.error('Error creating product supplier:', error);
        this.notificationService.error('Error', 'Error al agregar proveedor');
        this.isSaving = false;
      }
    });
  }

  private updateProductSupplier(formValue: any): void {
    if (!this.data.productSupplier) return;

    const request: UpdateProductSupplierRequest = {
      price: formValue.price,
      stock: formValue.stock,
      batchNumber: formValue.batchNumber,
      supplierSku: formValue.supplierSku,
      isActive: this.data.productSupplier.isActive
    };

    this.productSupplierService.update(
      this.data.productId,
      this.data.productSupplier.id,
      request
    ).subscribe({
      next: (response) => {
        if (response.success) {
          this.notificationService.success('Éxito', 'Proveedor actualizado');
          this.dialogRef.close(true);
        } else {
          this.notificationService.error('Error', 'No se pudo actualizar el proveedor');
        }
        this.isSaving = false;
      },
      error: (error) => {
        console.error('Error updating product supplier:', error);
        this.notificationService.error('Error', 'Error al actualizar proveedor');
        this.isSaving = false;
      }
    });
  }

  private markFormGroupTouched(): void {
    Object.keys(this.form.controls).forEach(key => {
      const control = this.form.get(key);
      control?.markAsTouched();
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  getErrorMessage(fieldName: string): string {
    const control = this.form.get(fieldName);
    if (!control?.errors || !control.touched) return '';

    const errors = control.errors;

    if (errors['required']) return `${this.getFieldDisplayName(fieldName)} es requerido`;
    if (errors['min']) return `${this.getFieldDisplayName(fieldName)} debe ser mayor a ${errors['min'].min}`;

    return 'Campo inválido';
  }

  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      supplierId: 'Proveedor',
      price: 'Precio',
      stock: 'Stock',
      batchNumber: 'Número de lote',
      supplierSku: 'SKU del proveedor'
    };
    return displayNames[fieldName] || fieldName;
  }

  get dialogTitle(): string {
    return this.isEditMode ? 'Editar Proveedor' : 'Agregar Proveedor';
  }

  get submitButtonText(): string {
    if (this.isSaving) return this.isEditMode ? 'Actualizando...' : 'Agregando...';
    return this.isEditMode ? 'Actualizar' : 'Agregar';
  }
}
