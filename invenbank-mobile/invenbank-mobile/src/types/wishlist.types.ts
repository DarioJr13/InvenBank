export interface WishlistItem {
  WishlistId: number;
  ProductId: number;
  ProductName: string;
  Description: string;
  ImageUrl: string | null;
  Category: string;
  MinPrice: number;
  TotalStock: number;
  CreatedAt: string;
}

export interface WishlistState {
  items: WishlistItem[];
  loading: boolean;
  error: string | null;
}

export interface AddToWishlistRequest {
  productId: number;
}