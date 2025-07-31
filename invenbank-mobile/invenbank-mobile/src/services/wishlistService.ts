import { apiClient } from './api';
import { ENDPOINTS } from '../utils/constants';
import { WishlistItem, AddToWishlistRequest } from '../types/wishlist.types';
import { ApiResponse } from '../types/api.types';

export class WishlistService {
  async getWishlist(): Promise<WishlistItem[]> {
    const response = await apiClient.get<WishlistItem[]>(ENDPOINTS.MOBILE.WISHLIST.GET);
    
    if (response.success && response.data) {
      return response.data;
    }
    
    return [];
  }

  async addToWishlist(request: AddToWishlistRequest): Promise<void> {
    const response = await apiClient.post(ENDPOINTS.MOBILE.WISHLIST.ADD, request);
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to add to wishlist');
    }
  }

  async removeFromWishlist(productId: number): Promise<void> {
    const response = await apiClient.delete(`${ENDPOINTS.MOBILE.WISHLIST.REMOVE}/${productId}`);
    
    if (!response.success) {
      throw new Error(response.message || 'Failed to remove from wishlist');
    }
  }
}

export const wishlistService = new WishlistService();
