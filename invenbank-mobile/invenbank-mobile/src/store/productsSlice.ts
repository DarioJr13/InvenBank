import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { ProductsState, SearchProductsParams } from '../types/product.types';
import { productService } from '../services/productService';
import { DEFAULT_PAGE_SIZE } from '../utils/constants';

const initialState: ProductsState = {
  products: [],
  currentProduct: null,
  loading: false,
  error: null,
  searchTerm: '',
  filters: {},
  pagination: {
    pageNumber: 1,
    pageSize: DEFAULT_PAGE_SIZE,
    totalRecords: 0,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false,
  },
};

// Async Thunks
export const searchProductsAsync = createAsyncThunk(
  'products/search',
  async (params: SearchProductsParams, { rejectWithValue }) => {
    try {
      const response = await productService.searchProducts(params);
      return response;
    } catch (error: any) {
      return rejectWithValue(error.message || 'Failed to search products');
    }
  }
);

export const getProductDetailAsync = createAsyncThunk(
  'products/getDetail',
  async (productId: number, { rejectWithValue }) => {
    try {
      const product = await productService.getProductById(productId);
      return product;
    } catch (error: any) {
      return rejectWithValue(error.message || 'Failed to get product detail');
    }
  }
);

const productsSlice = createSlice({
  name: 'products',
  initialState,
  reducers: {
    setSearchTerm: (state, action) => {
      state.searchTerm = action.payload;
    },
    setFilters: (state, action) => {
      state.filters = { ...state.filters, ...action.payload };
    },
    clearFilters: (state) => {
      state.filters = {};
      state.searchTerm = '';
    },
    clearCurrentProduct: (state) => {
      state.currentProduct = null;
    },
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Search products
      .addCase(searchProductsAsync.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(searchProductsAsync.fulfilled, (state, action) => {
        state.loading = false;
        state.products = action.payload.data || [];
        state.pagination = {
          pageNumber: action.payload.pageNumber || 1,
          pageSize: action.payload.pageSize || DEFAULT_PAGE_SIZE,
          totalRecords: action.payload.totalRecords || 0,
          totalPages: action.payload.totalPages || 0,
          hasNextPage: action.payload.hasNextPage || false,
          hasPreviousPage: action.payload.hasPreviousPage || false,
        };
      })
      .addCase(searchProductsAsync.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Get product detail
      .addCase(getProductDetailAsync.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(getProductDetailAsync.fulfilled, (state, action) => {
        state.loading = false;
        state.currentProduct = action.payload;
      })
      .addCase(getProductDetailAsync.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export const { 
  setSearchTerm, 
  setFilters, 
  clearFilters, 
  clearCurrentProduct, 
  clearError 
} = productsSlice.actions;
export default productsSlice.reducer;