import { apiClient } from './api';
import { ENDPOINTS } from '../utils/constants';

export const wishlistService = {
async get() {
  return apiClient.get(ENDPOINTS.MOBILE.WISHLIST.GET);
},

  async add(productId: number) {
    return apiClient.post(ENDPOINTS.MOBILE.WISHLIST.ADD, { productId });
  },

  async remove(productId: number) {
    return apiClient.post(ENDPOINTS.MOBILE.WISHLIST.REMOVE, { productId });
  }
};
