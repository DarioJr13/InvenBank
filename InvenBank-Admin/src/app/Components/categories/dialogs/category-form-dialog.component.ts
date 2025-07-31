import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { CategoryService } from '../../../services/category.service';
import { NotificationService } from '../../../services/notification.service';

@Component({
  selector: 'app-category-form-dialog',
  standalone: true,
  template: `
    <h2 mat-dialog-title>Nueva Categoría</h2>
    <form [formGroup]="form" (ngSubmit)="submit()" class="form-container">
      <mat-form-field appearance="outline">
        <mat-label>Nombre</mat-label>
        <input matInput formControlName="name" required />
      </mat-form-field>

      <mat-form-field appearance="outline">
        <mat-label>Descripción</mat-label>
        <textarea matInput formControlName="description" rows="3"></textarea>
      </mat-form-field>

      <mat-checkbox formControlName="isActive">Activa</mat-checkbox>

      <div class="form-actions">
        <button mat-button type="button" (click)="close()">Cancelar</button>
        <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid">Guardar</button>
      </div>
    </form>
  `,
  styles: [`
    .form-container {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      padding: 1rem;
    }
    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.5rem;
    }
  `],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule
  ]
})
export class CategoryFormDialogComponent {
  private dialogRef = inject(MatDialogRef<CategoryFormDialogComponent>);
  private categoryService = inject(CategoryService);
  private notificationService = inject(NotificationService);
  private fb = inject(FormBuilder);

  form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    description: [''],
    isActive: [true]
  });

  submit(): void {
    if (this.form.valid) {
      // Simulación de guardado
      const newCategory = {
        id: Math.floor(Math.random() * 1000),
        ...this.form.value,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        productsCount: 0
      };

      // Retornar resultado al cerrar
      this.dialogRef.close({ success: true, data: newCategory });
    }
  }

  close(): void {
    this.dialogRef.close();
  }
}
