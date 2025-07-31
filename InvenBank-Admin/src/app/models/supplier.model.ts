export interface Supplier {
  id: number;
  name: string;
  contactName: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  country: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  productsCount: number;
  totalStock: number;
  averagePrice: number;
}

export interface CreateSupplierRequest {
  name: string;
  contactName: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  country: string;
}

export interface UpdateSupplierRequest extends CreateSupplierRequest {
  isActive: boolean;
}
