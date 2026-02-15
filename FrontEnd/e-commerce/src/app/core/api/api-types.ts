/** Backend ResultDto<T> uyumlu API yanıtı */
export interface ResultDto<T> {
  data: T | null;
  isSuccess: boolean;
  message: string;
  errors?: readonly string[];
}

/** Herkese açık ürün özeti (GET /api/inventory/public) – fiyat ve indirim dahil */
export interface InventoryPublicItem {
  id: string;
  productId: string;
  productName: string;
  warehouseName: string;
  inStock: boolean;
  quantity: number;
  unitPrice?: number;
  currency?: string;
  discountPercent?: number | null;
  priceAfterDiscount?: number | null;
}

/** Public endpoint yanıtı (Items + Message + Time) */
export interface InventoryPublicResponse {
  message: string;
  items: InventoryPublicItem[];
  time: string;
}

/** Tam stok kalemi (GET /api/inventory, GET /api/inventory/:id) */
export interface InventoryItem {
  id: string;
  productId: string;
  warehouseId: string;
  quantity: number;
  product?: {
    id: string;
    name: string;
    imageKey?: string | null;
    unitPrice?: number;
    currency?: string;
  };
  warehouse?: { id: string; name: string; code?: string | null };
}

/** Stok miktarı güncelleme isteği (PUT /api/inventory/:id) */
export interface UpdateQuantityRequest {
  quantity: number;
}

/** Stoka yeni ürün ekleme isteği (POST /api/inventory) */
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

/** Sipariş (Ordering.API) */
export interface Order {
  id: string;
  customerId: string;
  productId: string;
  quantity: number;
  createdBy: string;
  createdAt: string;
  customer?: { id: string; firstName: string; lastName: string };
}

/** Sipariş oluşturma isteği (POST /api/orders) */
export interface CreateOrderRequest {
  customerId: string;
  productId: string;
  quantity: number;
}

/** Sepet kalemi (Basket.API) */
export interface BasketItemDto {
  productId: string;
  productName: string;
  inventoryItemId?: string | null;
  quantity: number;
  unitPrice?: number | null;
  currency?: string | null;
}

/** Müşteri sepeti (Basket.API) */
export interface CustomerBasketDto {
  buyerId: string;
  items: BasketItemDto[];
}

/** Sepete ekleme isteği (POST /api/basket/items) */
export interface AddBasketItemRequest {
  productId: string;
  productName: string;
  inventoryItemId?: string | null;
  quantity: number;
  unitPrice?: number | null;
  currency?: string | null;
}
