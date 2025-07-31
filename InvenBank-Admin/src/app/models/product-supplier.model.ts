export interface ProductSupplier {
  id: number;
  productId: number;
  supplierId: number;
  supplierName: string;
  price: number;
  stock: number;
  batchNumber: string;
  supplierSku: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProductSupplierRequest {
  productId: number;
  supplierId: number;
  price: number;
  stock: number;
  batchNumber: string;
  supplierSku: string;
}

export interface UpdateProductSupplierRequest {
  price: number;
  stock: number;
  batchNumber: string;
  supplierSku: string;
  isActive: boolean;}
