import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { WishlistState } from '../types/wishlist.types';
import { wishlistService } from '../services/wishlistService';

const initialState: WishlistState = {
  items: [],
  loading: false,
  error: null,
};

export const getWishlistAsync = createAsyncThunk(
  'wishlist/get',
  async () => {
    const response = await wishlistService.get();
    return response.data;
  }
);

export const addToWishlistAsync = createAsyncThunk(
  'wishlist/add',
  async (productId: number) => {
    const response = await wishlistService.add(productId);
    return productId;
  }
);


export const removeFromWishlistAsync = createAsyncThunk(
  'wishlist/remove',
  async (productId: number, { rejectWithValue, dispatch }) => {
    try {
      await wishlistService.removeFromWishlist(productId);
      // ✅ Recargar wishlist después de eliminar para sincronización
      dispatch(getWishlistAsync());
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
      .addCase(addToWishlistAsync.rejected, (state, action) => {
        state.error = action.payload as string;
      })
      .addCase(removeFromWishlistAsync.fulfilled, (state, action) => {
        // ✅ Usar ProductId en lugar de productId
        state.items = state.items.filter(item => item.ProductId !== action.payload);
        state.error = null;
      })
      .addCase(removeFromWishlistAsync.rejected, (state, action) => {
        state.error = action.payload as string;
      });
  },
});

export const { clearError } = wishlistSlice.actions;
export default wishlistSlice.reducer;