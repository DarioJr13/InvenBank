import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { SupplierService } from '../../services/supplier.service';
import { NotificationService } from '../../services/notification.service';
import { Supplier } from '../../models';

@Component({
  selector: 'app-supplier-list',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './supplier-list.component.html',
  styleUrls: ['./supplier-list.component.scss']
})
export class SupplierListComponent implements OnInit {
  suppliers: Supplier[] = [];
  isLoading = false;

  constructor(
    private supplierService: SupplierService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadSuppliers();
  }

  loadSuppliers(): void {
    this.isLoading = true;

    // Simulación temporal
    setTimeout(() => {
      this.suppliers = [
        {
          id: 1,
          name: 'TechSupply Corp',
          contactName: 'Juan Pérez',
          email: 'contacto@techsupply.com',
          phone: '+1-555-0123',
          address: '123 Tech Street',
          city: 'Miami',
          country: 'USA',
          isActive: true,
          createdAt: '2024-01-01',
          updatedAt: '2024-01-01',
          productsCount: 15,
          totalStock: 250,
          averagePrice: 350.50
        },
        {
          id: 2,
          name: 'ElectroMax Solutions',
          contactName: 'María González',
          email: 'ventas@electromax.com',
          phone: '+1-555-0124',
          address: '456 Electronic Ave',
          city: 'New York',
          country: 'USA',
          isActive: true,
          createdAt: '2024-01-02',
          updatedAt: '2024-01-02',
          productsCount: 12,
          totalStock: 180,
          averagePrice: 275.00
        }
      ];
      this.isLoading = false;
    }, 1000);
  }
}
