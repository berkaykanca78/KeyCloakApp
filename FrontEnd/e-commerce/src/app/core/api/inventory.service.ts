import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';
import type {
  ResultDto,
  InventoryPublicItem,
  InventoryPublicResponse,
  InventoryItem,
  UpdateQuantityRequest,
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

  /** Tek ürün stok detayı (Admin/User token gerekir) */
  getById(id: number): Observable<InventoryItem | null> {
    return this.http
      .get<ResultDto<InventoryItem>>(`${this.baseUrl}/inventory/${id}`)
      .pipe(
        map((res) => res.data ?? null),
        catchError(() => of(null))
      );
  }

  /** Stok miktarı güncelle (Admin token gerekir) */
  updateQuantity(id: number, request: UpdateQuantityRequest): Observable<ResultDto<InventoryItem>> {
    return this.http.put<ResultDto<InventoryItem>>(
      `${this.baseUrl}/inventory/${id}`,
      request
    );
  }
}
