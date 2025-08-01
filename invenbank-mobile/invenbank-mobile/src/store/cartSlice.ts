// src/store/cartSlice.ts
import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface CartItem {
  productId: number;
  productName: string;
  supplierId: number;
  supplierName: string;
  price: number;
  quantity: number;
  total: number;
}

interface CartState {
  items: CartItem[];
}

const initialState: CartState = {
  items: []
};

export const cartSlice = createSlice({
  name: 'cart',
  initialState,
  reducers: {
    addToCart(state, action: PayloadAction<CartItem>) {
      const item = action.payload;
      const existingIndex = state.items.findIndex(
        (i) => i.productId === item.productId && i.supplierId === item.supplierId
      );

      if (existingIndex >= 0) {
        // Actualiza cantidad y total si ya existe
        state.items[existingIndex].quantity += item.quantity;
        state.items[existingIndex].total += item.total;
      } else {
        // Agrega nuevo producto al carrito
        state.items.push(item);
      }
    },
    removeFromCart(state, action: PayloadAction<{ productId: number; supplierId: number }>) {
      state.items = state.items.filter(
        (item) =>
          item.productId !== action.payload.productId ||
          item.supplierId !== action.payload.supplierId
      );
    },
    clearCart(state) {
      state.items = [];
    }
  }
});

export const { addToCart, removeFromCart, clearCart } = cartSlice.actions;
export default cartSlice.reducer;
