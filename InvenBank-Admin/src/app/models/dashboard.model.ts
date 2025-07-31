// ===============================================
// 📊 MODELOS PARA DASHBOARD
// ===============================================

export interface DashboardStats {
  totalProducts: number;
  totalSuppliers: number;
  totalCategories: number;
  totalUsers: number;
  lowStockProducts: number;
  mostSoldProduct: string;
  totalRevenue: number;
  monthlyGrowth: number;
}
