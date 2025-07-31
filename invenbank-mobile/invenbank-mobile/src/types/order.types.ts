export interface OrderItem {
  productId: number;
  supplierId: number;
  quantity: number;
  unitPrice: number;
}

export interface CreateOrderRequest {
  items: OrderItem[];
  notes?: string;
}

export interface Order {
  id: number;
  userId: number;
  orderNumber: string;
  totalAmount: number;
  status: 'Pending' | 'Confirmed' | 'Shipped' | 'Delivered' | 'Cancelled';
  notes: string;
  createdAt: string;
  items: OrderDetail[];
}

export interface OrderDetail {
  id: number;
  productId: number;
  productName: string;
  supplierId: number;
  supplierName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}