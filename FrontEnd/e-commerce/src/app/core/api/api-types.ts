/** Backend ResultDto<T> uyumlu API yanıtı */
export interface ResultDto<T> {
  data: T | null;
  isSuccess: boolean;
  message: string;
  errors?: readonly string[];
}

/** Herkese açık ürün özeti (GET /inventory/public) */
export interface InventoryPublicItem {
  id: number;
  productName: string;
  inStock: boolean;
}

/** Public endpoint yanıtı (Items + Message + Time) */
export interface InventoryPublicResponse {
  message: string;
  items: InventoryPublicItem[];
  time: string;
}

/** Tam stok kalemi (GET /inventory, GET /inventory/:id) */
export interface InventoryItem {
  id: number;
  productName: string;
  quantity: number;
  location: string;
}

/** Stok miktarı güncelleme isteği (PUT /inventory/:id) */
export interface UpdateQuantityRequest {
  quantity: number;
}

/** Auth API – Keycloak token yanıtı (snake_case) */
export interface KeycloakTokenResponse {
  access_token: string;
  expires_in?: number;
  refresh_token?: string;
  refresh_expires_in?: number;
  token_type?: string;
}

/** Sipariş (OrderApi) */
export interface Order {
  id: number;
  productName: string;
  quantity: number;
  customerName: string;
  createdBy: string;
  createdAt: string;
}

/** Sipariş oluşturma isteği (POST /orders) */
export interface CreateOrderRequest {
  productName: string;
  quantity: number;
  customerName: string;
}
