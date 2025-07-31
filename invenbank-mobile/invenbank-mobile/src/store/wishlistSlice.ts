import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { WishlistState } from '../types/wishlist.types';
import { wishlistService } from '../services/wishlistService';

const initialState: WishlistState = {
  items: [],
  loading: false,
  error: null,
};

// Async Thunks
export const getWishlistAsync = createAsyncThunk(
  'wishlist/get',
  async (_, { rejectWithValue }) => {
    try {
      const items = await wishlistService.getWishlist();
      return items;
    } catch (error: any) {
      return rejectWithValue(error.message || 'Failed to get wishlist');
    }
  }
);

export const addToWishlistAsync = createAsyncThunk(
  'wishlist/add',
  async (productId: number, { rejectWithValue }) => {
    try {
      await wishlistService.addToWishlist({ productId });
      return productId;
    } catch (error: any) {
      return rejectWithValue(error.message || 'Failed to add to wishlist');
    }
  }
);

export const removeFromWishlistAsync = createAsyncThunk(
  'wishlist/remove',
  async (productId: number, { rejectWithValue }) => {
    try {
      await wishlistService.removeFromWishlist(productId);
      return productId;
    } catch (error: any) {
      return rejectWithValue(error.message || 'Failed to remove from wishlist');
    }
  }
);

const wishlistSlice = createSlice({
  name: 'wishlist',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Get wishlist
      .addCase(getWishlistAsync.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(getWishlistAsync.fulfilled, (state, action) => {
        state.loading = false;
        state.items = action.payload;
      })
      .addCase(getWishlistAsync.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Add to wishlist
      .addCase(addToWishlistAsync.fulfilled, (state) => {
        // Refrescar la lista después de agregar
        state.error = null;
      })
      .addCase(addToWishlistAsync.rejected, (state, action) => {
        state.error = action.payload as string;
      })
      // Remove from wishlist
      .addCase(removeFromWishlistAsync.fulfilled, (state, action) => {
        state.items = state.items.filter(item => item.productId !== action.payload);
        state.error = null;
      })
      .addCase(removeFromWishlistAsync.rejected, (state, action) => {
        state.error = action.payload as string;
      });
  },
});

export const { clearError } = wishlistSlice.actions;
export default wishlistSlice.reducer;