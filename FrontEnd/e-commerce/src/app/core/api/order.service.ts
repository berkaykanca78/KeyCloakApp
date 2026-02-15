import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';
import type { ResultDto, Order, CreateOrderRequest } from './api-types';

const DEFAULT_BASE = 'https://localhost:5001';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = DEFAULT_BASE;

  /** Herkese açık bilgi (token gerekmez) */
  getPublic(): Observable<{ message: string }> {
    return this.http
      .get<ResultDto<{ message: string; time: string }>>(`${this.baseUrl}/orders/public`)
      .pipe(
        map((res) => ({
          message: res.data?.message ?? res.message ?? '',
        })),
        catchError(() => of({ message: 'Bilgi alınamadı.' }))
      );
  }

  /** Giriş yapan kullanıcının siparişleri (Admin, User) */
  getMyOrders(): Observable<Order[]> {
    return this.http
      .get<ResultDto<Order[]>>(`${this.baseUrl}/orders/my`)
      .pipe(
        map((res) => (res.data ?? [])),
        catchError(() => of([]))
      );
  }

  /** Tüm siparişler (Admin) */
  getAll(): Observable<Order[]> {
    return this.http
      .get<ResultDto<Order[]>>(`${this.baseUrl}/orders`)
      .pipe(
        map((res) => (res.data ?? [])),
        catchError(() => of([]))
      );
  }

  /** Yeni sipariş oluştur (Admin, User) */
  create(request: CreateOrderRequest): Observable<ResultDto<Order>> {
    return this.http.post<ResultDto<Order>>(`${this.baseUrl}/orders`, request);
  }

  /** Giriş yapan kullanıcının müşteri kaydı (CustomerId için) */
  getCustomerMe(): Observable<{ id: string } | null> {
    return this.http
      .get<ResultDto<{ id: string }>>(`${this.baseUrl}/customers/me`)
      .pipe(
        map((res) => res.data ?? null),
        catchError(() => of(null))
      );
  }

  /** Müşteri kaydı yoksa JWT ile oluşturur, varsa döner. Sipariş öncesi kullanın. */
  getCustomerMeOrCreate(): Observable<{ id: string } | null> {
    return this.http.get<ResultDto<{ id: string }>>(`${this.baseUrl}/customers/me`).pipe(
      map((res) => res.data ?? null),
      catchError(() =>
        this.http.post<ResultDto<{ id: string }>>(`${this.baseUrl}/customers/me`, {}).pipe(
          map((res) => res.data ?? null),
          catchError(() => of(null))
        )
      )
    );
  }
}
