export const API_BASE_URL = 'http://localhost:5207/api';

export const ENDPOINTS = {
  AUTH: {
    LOGIN: '/auth/login',
  },

  MOBILE: {
    CATALOG: {
      SEARCH: '/mobile/Catalog/search',
      DETAIL: '/mobile/Catalog',
      CATEGORIES: '/mobile/Catalog/categories',
    },

    ORDERS: {
      CREATE: '/mobile/Orders',              // POST para crear orden
      GET: '/mobile/Orders',                 // GET lista del usuario
      DETAIL: '/mobile/Orders',              // GET /{id}
    },

    WISHLIST: {
      ADD: '/mobile/wishlist',
      REMOVE: '/mobile/wishlist',
      GET: '/mobile/wishlist',
    },
  },

  HEALTH: {
    CHECK: '/health',
    DATABASE: '/health/database',
    SYSTEM: '/health/system',
  }
} as const;

export const STORAGE_KEYS = {
  TOKEN: 'invenbank_token',
  REFRESH_TOKEN: 'invenbank_refresh_token',
  USER: 'invenbank_user',
} as const;

export const DEFAULT_PAGE_SIZE = 20;
