/** Backend ResultDto<T> uyumlu API yanıtı */
export interface ResultDto<T> {
  data: T | null;
  isSuccess: boolean;
  message: string;
  errors?: readonly string[];
}

/** Herkese açık ürün özeti (GET /inventory/public) */
export interface InventoryPublicItem {
  id: string;
  productId: string;
  productName: string;
  warehouseName: string;
  inStock: boolean;
  quantity: number;
}

/** Public endpoint yanıtı (Items + Message + Time) */
export interface InventoryPublicResponse {
  message: string;
  items: InventoryPublicItem[];
  time: string;
}

/** Tam stok kalemi (GET /inventory, GET /inventory/:id) */
export interface InventoryItem {
  id: string;
  productId: string;
  warehouseId: string;
  quantity: number;
  imageKey?: string | null;
  product?: { id: string; name: string; imageKey?: string | null };
  warehouse?: { id: string; name: string; code?: string | null };
}

/** Stok miktarı güncelleme isteği (PUT /inventory/:id) */
export interface UpdateQuantityRequest {
  quantity: number;
}

/** Stoka yeni ürün ekleme isteği (POST /inventory) */
export interface CreateInventoryRequest {
  productId: string;
  warehouseId: string;
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
  id: string;
  customerId: string;
  productId: string;
  quantity: number;
  createdBy: string;
  createdAt: string;
  customer?: { id: string; firstName: string; lastName: string };
}

/** Sipariş oluşturma isteği (POST /orders) */
export interface CreateOrderRequest {
  customerId: string;
  productId: string;
  quantity: number;
}
