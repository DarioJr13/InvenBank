import { apiClient } from './api';
import { ENDPOINTS } from '../utils/constants';
import { CreateOrderRequest, Order } from '../types/order.types';
import { ApiResponse } from '../types/api.types';

export class OrderService {
  async createOrder(request: CreateOrderRequest): Promise<Order> {
    const response = await apiClient.post<Order>(ENDPOINTS.MOBILE.ORDERS.CREATE, request);

    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Failed to create order');
  }

  async getUserOrders(): Promise<Order[]> {
    const response = await apiClient.get<Order[]>(ENDPOINTS.MOBILE.ORDERS.GET);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    return [];
  }

  async getOrderById(id: number): Promise<Order> {
    const response = await apiClient.get<Order>(`${ENDPOINTS.MOBILE.ORDERS.DETAIL}/${id}`);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    throw new Error(response.message || 'Order not found');
  }
}

export const orderService = new OrderService();