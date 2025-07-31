export const API_BASE_URL = 'http://localhost:5207/api';

export const ENDPOINTS = {
  AUTH: {
    LOGIN: '/auth/login',
  },
  PRODUCTS: {
    SEARCH: '/products/search',
    DETAIL: '/products',
    CATEGORIES: '/categories',
  },
  WISHLIST: {
    GET: '/wishlist',
    ADD: '/wishlist',
    REMOVE: '/wishlist',
  },
  ORDERS: {
    CREATE: '/orders',
    GET: '/orders',
  },
  HEALTH: {
    CHECK: '/health',
  }
} as const;     

export const STORAGE_KEYS = {
  TOKEN: 'invenbank_token',
  REFRESH_TOKEN: 'invenbank_refresh_token',
  USER: 'invenbank_user',
} as const;

export const DEFAULT_PAGE_SIZE = 20;