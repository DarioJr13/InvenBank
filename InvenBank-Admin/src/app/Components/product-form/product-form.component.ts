import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { NotificationService } from '../../services/notification.service';
import { Product, Category, CreateProductRequest, UpdateProductRequest } from '../../models';

@Component({
  selector: 'app-product-form',
  standalone: true,
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.scss'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatToolbarModule,
    MatSlideToggleModule
  ]
})
export class ProductFormComponent implements OnInit {
  productForm: FormGroup;

  isEditMode = false;
  isLoading = false;
  isSaving = false;
  productId: number | null = null;

  categories: Category[] = [];
  product: Product | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private categoryService: CategoryService,
    private notificationService: NotificationService
  ) {
    this.productForm = this.createForm();
  }

  ngOnInit(): void {
    this.checkEditMode();
    this.loadCategories();
  }

  private createForm(): FormGroup {
    return this.formBuilder.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: [''],
      sku: ['', [Validators.required, Validators.maxLength(50)]],
      brand: ['', [Validators.maxLength(100)]],
      categoryId: [null, [Validators.required]],
      imageUrl: ['', [Validators.maxLength(500)]],
      isActive: [true]
    });
  }

  private checkEditMode(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'create') {
      this.isEditMode = true;
      this.productId = +id;
      this.loadProduct();
    }
  }

  private loadProduct(): void {
    if (!this.productId) return;

    this.isLoading = true;
    this.productService.getProductById(this.productId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.product = response.data;
          this.populateForm();
        } else {
          this.notificationService.error('Error', 'No se pudo cargar el producto');
          this.goBack();
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading product:', error);
        this.notificationService.error('Error', 'No se pudo cargar el producto');
        this.goBack();
        this.isLoading = false;
      }
    });
  }

  private populateForm(): void {
    if (!this.product) return;

    this.productForm.patchValue({
      name: this.product.name,
      description: this.product.description,
      sku: this.product.sku,
      brand: this.product.brand,
      categoryId: this.product.categoryId,
      imageUrl: this.product.imageUrl,
      isActive: this.product.isActive
    });
  }

  private loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.categories = response.data;
        }
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.notificationService.error('Error', 'No se pudieron cargar las categorías');
      }
    });
  }

  onSubmit(): void {
    if (this.productForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isSaving = true;
    const formValue = this.productForm.value;

    if (this.isEditMode) {
      this.updateProduct(formValue);
    } else {
      this.createProduct(formValue);
    }
  }

  private createProduct(formValue: any): void {
    const request: CreateProductRequest = {
      name: formValue.name,
      description: formValue.description,
      sku: formValue.sku,
      brand: formValue.brand,
      categoryId: formValue.categoryId,
      imageUrl: formValue.imageUrl
    };

    this.productService.createProduct(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.notificationService.success('Éxito', 'Producto creado correctamente');
          this.goBack();
        } else {
          this.notificationService.error('Error', 'No se pudo crear el producto');
        }
        this.isSaving = false;
      },
      error: (error) => {
        console.error('Error creating product:', error);
        this.notificationService.error('Error', 'Error al crear el producto');
        this.isSaving = false;
      }
    });
  }

  private updateProduct(formValue: any): void {
    if (!this.productId) return;

    const request: UpdateProductRequest = {
      name: formValue.name,
      description: formValue.description,
      sku: formValue.sku,
      brand: formValue.brand,
      categoryId: formValue.categoryId,
      imageUrl: formValue.imageUrl,
      isActive: formValue.isActive
    };

    this.productService.updateProduct(this.productId, request).subscribe({
      next: (response) => {
        if (response.success) {
          this.notificationService.success('Éxito', 'Producto actualizado correctamente');
          this.goBack();
        } else {
          this.notificationService.error('Error', 'No se pudo actualizar el producto');
        }
        this.isSaving = false;
      },
      error: (error) => {
        console.error('Error updating product:', error);
        this.notificationService.error('Error', 'Error al actualizar el producto');
        this.isSaving = false;
      }
    });
  }

  private markFormGroupTouched(): void {
    Object.keys(this.productForm.controls).forEach(key => {
      const control = this.productForm.get(key);
      control?.markAsTouched();
    });
  }

  goBack(): void {
    this.router.navigate(['/products']);
  }

  getErrorMessage(fieldName: string): string {
    const control = this.productForm.get(fieldName);
    if (!control?.errors || !control.touched) return '';

    const errors = control.errors;

    if (errors['required']) return `${this.getFieldDisplayName(fieldName)} es requerido`;
    if (errors['maxlength']) return `${this.getFieldDisplayName(fieldName)} excede la longitud máxima`;
    if (errors['email']) return 'Email inválido';

    return 'Campo inválido';
  }

  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      name: 'Nombre',
      description: 'Descripción',
      sku: 'SKU',
      brand: 'Marca',
      categoryId: 'Categoría',
      imageUrl: 'URL de imagen'
    };
    return displayNames[fieldName] || fieldName;
  }

  get pageTitle(): string {
    return this.isEditMode ? 'Editar Producto' : 'Crear Producto';
  }

  get submitButtonText(): string {
    if (this.isSaving) return this.isEditMode ? 'Actualizando...' : 'Creando...';
    return this.isEditMode ? 'Actualizar' : 'Crear';
  }
}
