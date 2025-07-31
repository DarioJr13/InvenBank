export interface WishlistItem {
  id: number;
  userId: number;
  productId: number;
//   product: Product;
  createdAt: string;
}

export interface WishlistState {
  items: WishlistItem[];
  loading: boolean;
  error: string | null;
}

export interface AddToWishlistRequest {
  productId: number;
}