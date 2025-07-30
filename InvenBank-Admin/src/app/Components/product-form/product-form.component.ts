import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { SupplierService } from '../../services/supplier.service';
import { ProductSupplierService } from '../../services/product-supplier.service';
import { NotificationService } from '../../services/notification.service';
import { Product, Category, Supplier, ProductSupplier, CreateProductRequest, UpdateProductRequest } from '../../models';

@Component({
  selector: 'app-product-form',
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.scss']
})
export class ProductFormComponent implements OnInit {
  productForm: FormGroup;
  supplierForm: FormGroup;

  isEditMode = false;
  isLoading = false;
  isSaving = false;
  productId: number | null = null;

  product: Product | null = null;
  categories: Category[] = [];
  suppliers: Supplier[] = [];
  productSuppliers: ProductSupplier[] = [];

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private categoryService: CategoryService,
    private supplierService: SupplierService,
    private productSupplierService: ProductSupplierService,
    private notificationService: NotificationService
  ) {
    this.productForm = this.createProductForm();
    this.supplierForm = this.createSupplierForm();
  }

  ngOnInit(): void {
    this.loadInitialData();
    this.checkEditMode();
  }

  /**
   * Crear formulario de producto
   */
  private createProductForm(): FormGroup {
    return this.formBuilder.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
      sku: ['', [Validators.required, Validators.pattern(/^[A-Z0-9-]+$/)]],
      brand: ['', [Validators.required, Validators.minLength(2)]],
      categoryId: ['', [Validators.required]],
      imageUrl: [''],
      isActive: [true]
    });
  }

  /**
   * Crear formulario de proveedor
   */
  private createSupplierForm(): FormGroup {
    return this.formBuilder.group({
      supplierId: ['', [Validators.required]],
      price: ['', [Validators.required, Validators.min(0.01)]],
      stock: ['', [Validators.required, Validators.min(0)]],
      batchNumber: ['', [Validators.required]],
      supplierSku: ['', [Validators.required]]
    });
  }

  /**
   * Cargar datos iniciales
   */
  private loadInitialData(): void {
    this.loadCategories();
    this.loadSuppliers();
  }

  /**
   * Verificar si está en modo edición
   */
  private checkEditMode(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEditMode = true;
      this.productId = parseInt(id);
      this.loadProductData();
    }
  }

  /**
   * Cargar categorías
   */
  private loadCategories(): void {
    this.categoryService.getAllCategories().subscribe({
      next: (response) => {
        if (response.success) {
          this.categories = response.data.filter(c => c.isActive);
        }
      },
      error: (error) => {
        this.notificationService.error('Error', 'No se pudieron cargar las categorías');
      }
    });
  }

  /**
   * Cargar proveedores
   */
  private loadSuppliers(): void {
    this.supplierService.getAllSuppliers().subscribe({
      next: (response) => {
        if (response.success) {
          this.suppliers = response.data.filter(s => s.isActive);
        }
      },
      error: (error) => {
        this.notificationService.error('Error', 'No se pudieron cargar los proveedores');
      }
    });
  }

  /**
   * Cargar datos del producto (modo edición)
   */
  private loadProductData(): void {
    if (!this.productId) return;

    this.isLoading = true;

    this.productService.getProductById(this.productId).subscribe({
      next: (response) => {
        if (response.success) {
          this.product = response.data;
          this.populateForm();
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.notificationService.error('Error', 'No se pudo cargar el producto');
        this.router.navigate(['/products']);
        this.isLoading = false;
      }
    });
  }

  /**
   * Poblar formulario con datos del producto
   */
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

    this.productSuppliers = this.product.suppliers || [];
  }

  /**
   * Obtener controles del formulario de producto
   */
  get pf() {
    return this.productForm.controls;
  }

  /**
   * Obtener controles del formulario de proveedor
   */
  get sf() {
    return this.supplierForm.controls;
  }

  /**
   * Verificar si un campo tiene errores
   */
  hasError(formGroup: FormGroup, fieldName: string, errorType: string): boolean {
    const field = formGroup.get(fieldName);
    return field ? field.hasError(errorType) && (field.dirty || field.touched) : false;
  }

  /**
   * Obtener mensaje de error
   */
  getErrorMessage(formGroup: FormGroup, fieldName: string): string {
    const field = formGroup.get(fieldName);

    if (field?.hasError('required')) {
      return `${this.getFieldLabel(fieldName)} es requerido`;
    }

    if (field?.hasError('minlength')) {
      const minLength = field.errors?.['minlength'].requiredLength;
      return `${this.getFieldLabel(fieldName)} debe tener al menos ${minLength} caracteres`;
    }

    if (field?.hasError('pattern')) {
      return `${this.getFieldLabel(fieldName)} tiene un formato inválido`;
    }

    if (field?.hasError('min')) {
      return `${this.getFieldLabel(fieldName)} debe ser mayor que 0`;
    }

    return '';
  }

  /**
   * Obtener etiqueta del campo
   */
  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      name: 'Nombre',
      description: 'Descripción',
      sku: 'SKU',
      brand: 'Marca',
      categoryId: 'Categoría',
      supplierId: 'Proveedor',
      price: 'Precio',
      stock: 'Stock',
      batchNumber: 'Número de Lote',
      supplierSku: 'SKU del Proveedor'
    };

    return labels[fieldName] || fieldName;
  }

  /**
   * Generar SKU automáticamente
   */
  generateSku(): void {
    const name = this.pf['name'].value;
    const brand = this.pf['brand'].value;

    if (name && brand) {
      const timestamp = Date.now().toString().slice(-6);
      const sku = `${brand.slice(0, 3).toUpperCase()}-${name.slice(0, 3).toUpperCase()}-${timestamp}`;
      this.pf['sku'].setValue(sku);
    }
  }

  /**
   * Agregar proveedor al producto
   */
  addSupplier(): void {
    if (this.supplierForm.invalid) {
      Object.keys(this.supplierForm.controls).forEach(key => {
        this.supplierForm.get(key)?.markAsTouched();
      });
      return;
    }

    const supplierData = this.supplierForm.value;
    const supplier = this.suppliers.find(s => s.id === supplierData.supplierId);

    if (!supplier) return;

    // Verificar si el proveedor ya está agregado
    const exists = this.productSuppliers.some(ps => ps.supplierId === supplierData.supplierId);
    if (exists) {
      this.notificationService.warning('Proveedor duplicado', 'Este proveedor ya está agregado al producto');
      return;
    }

    // Crear objeto de proveedor para mostrar en la tabla
    const newProductSupplier: ProductSupplier = {
      id: 0, // Temporal, se asignará cuando se guarde
      productId: this.productId || 0,
      supplierId: supplierData.supplierId,
      supplierName: supplier.name,
      price: supplierData.price,
      stock: supplierData.stock,
      batchNumber: supplierData.batchNumber,
      supplierSku: supplierData.supplierSku,
      isActive: true,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    };

    this.productSuppliers.push(newProductSupplier);
    this.supplierForm.reset();

    this.notificationService.success('Proveedor agregado', `${supplier.name} ha sido agregado al producto`);
  }

  /**
   * Remover proveedor del producto
   */
  removeSupplier(index: number): void {
    const supplier = this.productSuppliers[index];
    this.productSuppliers.splice(index, 1);
    this.notificationService.success('Proveedor removido', `${supplier.supplierName} ha sido removido del producto`);
  }

  /**
   * Obtener nombre del proveedor por ID
   */
  getSupplierName(supplierId: number): string {
    const supplier = this.suppliers.find(s => s.id === supplierId);
    return supplier ? supplier.name : 'Desconocido';
  }

  /**
   * Guardar producto
   */
  save(): void {
    if (this.productForm.invalid) {
      Object.keys(this.productForm.controls).forEach(key => {
        this.productForm.get(key)?.markAsTouched();
      });
      return;
    }

    if (this.productSuppliers.length === 0) {
      this.notificationService.warning('Proveedores requeridos', 'Debes agregar al menos un proveedor al producto');
      return;
    }

    this.isSaving = true;

    if (this.isEditMode) {
      this.updateProduct();
    } else {
      this.createProduct();
    }
  }

  /**
   * Crear nuevo producto
   */
  private createProduct(): void {
    const productData: CreateProductRequest = {
      name: this.pf['name'].value,
      description: this.pf['description'].value,
      sku: this.pf['sku'].value,
      brand: this.pf['brand'].value,
      categoryId: this.pf['categoryId'].value,
      imageUrl: this.pf['imageUrl'].value
    };

    this.productService.createProduct(productData).subscribe({
      next: (response) => {
        if (response.success) {
          this.notificationService.success('Producto creado', `${productData.name} ha sido creado exitosamente`);
          this.router.navigate(['/products']);
        } else {
          this.notificationService.error('Error', response.message || 'No se pudo crear el producto');
        }
        this.isSaving = false;
      },
      error: (error) => {
        this.notificationService.error('Error', 'No se pudo crear el producto');
        this.isSaving = false;
      }
    });
  }

  /**
   * Actualizar producto existente
   */
  private updateProduct(): void {
    if (!this.productId) return;

    const productData: UpdateProductRequest = {
      name: this.pf['name'].value,
      description: this.pf['description'].value,
      sku: this.pf['sku'].value,
      brand: this.pf['brand'].value,
      categoryId: this.pf['categoryId'].value,
      imageUrl: this.pf['imageUrl'].value,
      isActive: this.pf['isActive'].value
    };

    this.productService.updateProduct(this.productId, productData).subscribe({
      next: (response) => {
        if (response.success) {
          this.notificationService.success('Producto actualizado', `${productData.name} ha sido actualizado exitosamente`);
          this.router.navigate(['/products']);
        } else {
          this.notificationService.error('Error', response.message || 'No se pudo actualizar el producto');
        }
        this.isSaving = false;
      },
      error: (error) => {
        this.notificationService.error('Error', 'No se pudo actualizar el producto');
        this.isSaving = false;
      }
    });
  }

  /**
   * Cancelar y volver
   */
  cancel(): void {
    this.router.navigate(['/products']);
  }
}
