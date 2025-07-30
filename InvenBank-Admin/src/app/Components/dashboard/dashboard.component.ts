import { Component, OnInit } from '@angular/core';
import { DashboardService } from '../../services/dashboard.service';
import { DashboardStats } from '../../models';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  stats: DashboardStats = {
    totalProducts: 0,
    totalSuppliers: 0,
    totalCategories: 0,
    totalUsers: 0,
    lowStockProducts: 0,
    mostSoldProduct: '',
    totalRevenue: 0,
    monthlyGrowth: 0
  };

  isLoading = true;

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  /**
   * Cargar datos del dashboard
   */
  loadDashboardData(): void {
    this.isLoading = true;

    // Simular datos mientras se implementa el endpoint
    setTimeout(() => {
      this.stats = {
        totalProducts: 120,
        totalSuppliers: 5,
        totalCategories: 8,
        totalUsers: 15,
        lowStockProducts: 3,
        mostSoldProduct: 'Laptop',
        totalRevenue: 125000,
        monthlyGrowth: 12.5
      };
      this.isLoading = false;
    }, 1000);

    /* Cuando el endpoint esté listo:
    this.dashboardService.getDashboardStats().subscribe({
      next: (response) => {
        if (response.success) {
          this.stats = response.data;
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading dashboard:', error);
        this.isLoading = false;
      }
    });
    */
  }
}
