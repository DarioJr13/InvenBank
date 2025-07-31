export interface SalesReport {
  period: string;
  totalSales: number;
  totalProducts: number;
  topProducts: Array<{
    productName: string;
    quantitySold: number;
    revenue: number;
  }>;
  topSuppliers: Array<{
    supplierName: string;
    totalRevenue: number;
    productsCount: number;
  }>;
}

export interface InventoryReport {
  totalProducts: number;
  totalStock: number;
  totalValue: number;
  lowStockProducts: Array<{
    productName: string;
    currentStock: number;
    minStock: number;
    supplierName: string;
  }>;
  categoryDistribution: Array<{
    categoryName: string;
    productsCount: number;
    totalStock: number;
    percentage: number;
  }>;
}
