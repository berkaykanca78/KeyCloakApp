import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';
import type {
  ResultDto,
  InventoryPublicItem,
  InventoryPublicResponse,
  InventoryItem,
  UpdateQuantityRequest,
  CreateInventoryRequest,
} from './api-types';

const DEFAULT_BASE = 'https://localhost:5001';

@Injectable({ providedIn: 'root' })
export class InventoryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = DEFAULT_BASE;

  /** Herkese açık ürün listesi (token gerekmez) */
  getPublic(): Observable<{ message: string; items: InventoryPublicItem[] }> {
    return this.http
      .get<ResultDto<InventoryPublicResponse>>(`${this.baseUrl}/inventory/public`)
      .pipe(
        map((res) => ({
          message: res.data?.message ?? res.message ?? '',
          items: res.data?.items ?? [],
        })),
        catchError(() => of({ message: 'Liste yüklenemedi.', items: [] }))
      );
  }

  /** Tüm stok listesi (Admin token gerekir) */
  getAll(): Observable<InventoryItem[]> {
    return this.http
      .get<ResultDto<InventoryItem[]>>(`${this.baseUrl}/inventory`)
      .pipe(
        map((res) => (res.data ?? [])),
        catchError(() => of([]))
      );
  }

  /** Tek stok kalemi detayı (Admin/User token gerekir) */
  getById(id: string): Observable<InventoryItem | null> {
    return this.http
      .get<ResultDto<InventoryItem>>(`${this.baseUrl}/inventory/${id}`)
      .pipe(
        map((res) => res.data ?? null),
        catchError(() => of(null))
      );
  }

  /** Stok miktarı güncelle (Admin token gerekir) */
  updateQuantity(id: string, request: UpdateQuantityRequest): Observable<ResultDto<InventoryItem>> {
    return this.http.put<ResultDto<InventoryItem>>(
      `${this.baseUrl}/inventory/${id}`,
      request
    );
  }

  /** Stoka yeni ürün ekle (Admin token gerekir) */
  create(request: CreateInventoryRequest): Observable<ResultDto<InventoryItem>> {
    return this.http.post<ResultDto<InventoryItem>>(`${this.baseUrl}/inventory`, request);
  }

  /** Stok kalemi resmi yükle – FormData, alan adı: file */
  uploadImage(inventoryId: string, file: File): Observable<ResultDto<{ imageKey: string }>> {
    const form = new FormData();
    form.append('file', file, file.name || 'image.jpg');
    return this.http.post<ResultDto<{ imageKey: string }>>(
      `${this.baseUrl}/inventory/${inventoryId}/image`,
      form
    );
  }

  /** Stok kalemi resmi için proxy URL */
  getImageUrl(inventoryId: string): string {
    return `${this.baseUrl}/inventory/${inventoryId}/image`;
  }

  /** Presigned URL (opsiyonel) */
  getImagePresignedUrl(inventoryId: string, expirySeconds = 3600): Observable<ResultDto<{ url: string }>> {
    return this.http.get<ResultDto<{ url: string }>>(
      `${this.baseUrl}/inventory/${inventoryId}/image/url`,
      { params: { expirySeconds } }
    );
  }

  /** Ürün listesi (Admin, stok eklerken dropdown) */
  getProducts(): Observable<{ id: string; name: string }[]> {
    return this.http
      .get<ResultDto<{ id: string; name: string }[]>>(`${this.baseUrl}/inventory/products`)
      .pipe(map((res) => res.data ?? []), catchError(() => of([])));
  }

  /** Depo listesi (Admin, stok eklerken dropdown) */
  getWarehouses(): Observable<{ id: string; name: string; code?: string | null }[]> {
    return this.http
      .get<ResultDto<{ id: string; name: string; code?: string | null }[]>>(`${this.baseUrl}/inventory/warehouses`)
      .pipe(map((res) => res.data ?? []), catchError(() => of([])));
  }

  /** Yeni depo ekle (Admin) – önce depo, sonra ürün eklenir */
  createWarehouse(name: string, code?: string): Observable<ResultDto<{ id: string; name: string; code?: string | null }>> {
    return this.http.post<ResultDto<{ id: string; name: string; code?: string | null }>>(
      `${this.baseUrl}/inventory/warehouses`,
      { name, code: code || null }
    );
  }

  /** Depo resmi yükle (Admin) – MinIO'ya kaydedilir */
  uploadWarehouseImage(warehouseId: string, file: File): Observable<ResultDto<{ imageKey: string }>> {
    const form = new FormData();
    form.append('file', file, file.name || 'image.jpg');
    return this.http.post<ResultDto<{ imageKey: string }>>(
      `${this.baseUrl}/inventory/warehouses/${warehouseId}/image`,
      form
    );
  }

  /** Depo resmi için URL (img src – stream endpoint) */
  getWarehouseImageUrl(warehouseId: string): string {
    return `${this.baseUrl}/inventory/warehouses/${warehouseId}/image/stream`;
  }

  /** Yeni ürün ekle (Admin) – en az bir depo seçilmeli */
  createProduct(request: {
    name: string;
    imageKey?: string;
    warehouseIds: string[];
    initialQuantity?: number;
  }): Observable<ResultDto<{ id: string; name: string }>> {
    return this.http.post<ResultDto<{ id: string; name: string }>>(
      `${this.baseUrl}/inventory/products`,
      {
        name: request.name,
        imageKey: request.imageKey ?? null,
        warehouseIds: request.warehouseIds,
        initialQuantity: request.initialQuantity ?? 0,
      }
    );
  }
}
