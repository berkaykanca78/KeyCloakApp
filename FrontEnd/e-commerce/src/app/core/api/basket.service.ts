import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, map, catchError, of, throwError } from 'rxjs';
import type { CustomerBasketDto, AddBasketItemRequest } from './api-types';

const DEFAULT_BASE = 'https://localhost:5001';
const BASKET_ID_KEY = 'basketId';

@Injectable({ providedIn: 'root' })
export class BasketService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = DEFAULT_BASE;

  getOrCreateBasketId(): string {
    let id = localStorage.getItem(BASKET_ID_KEY);
    if (!id) {
      id = crypto.randomUUID().replace(/-/g, '');
      localStorage.setItem(BASKET_ID_KEY, id);
    }
    return id;
  }

  private basketHeaders(): HttpHeaders {
    return new HttpHeaders({ 'X-Basket-Id': this.getOrCreateBasketId() });
  }

  private storeBasketIdFromResponse(headers: HttpHeaders): void {
    const id = headers.get('X-Basket-Id');
    if (id) localStorage.setItem(BASKET_ID_KEY, id);
  }

  getBasket(): Observable<CustomerBasketDto> {
    const buyerId = this.getOrCreateBasketId();
    const url = `${this.baseUrl}/api/basket?buyerId=${encodeURIComponent(buyerId)}`;
    return this.http
      .get<CustomerBasketDto>(url, {
        headers: this.basketHeaders(),
        observe: 'response',
      })
      .pipe(
        map((res) => {
          this.storeBasketIdFromResponse(res.headers);
          return res.body ?? { buyerId, items: [] };
        }),
        catchError(() => of({ buyerId, items: [] }))
      );
  }

  addItem(request: AddBasketItemRequest): Observable<CustomerBasketDto> {
    return this.http
      .post<CustomerBasketDto>(`${this.baseUrl}/api/basket/items`, request, {
        headers: this.basketHeaders(),
        observe: 'response',
      })
      .pipe(
        map((res) => {
          this.storeBasketIdFromResponse(res.headers);
          return res.body ?? { buyerId: this.getOrCreateBasketId(), items: [] };
        }),
        catchError((err) => throwError(() => err))
      );
  }

  updateQuantity(productId: string, quantity: number): Observable<CustomerBasketDto> {
    const buyerId = this.getOrCreateBasketId();
    const url = `${this.baseUrl}/api/basket/items/${encodeURIComponent(productId)}?buyerId=${encodeURIComponent(buyerId)}`;
    return this.http
      .put<CustomerBasketDto>(url, { quantity }, { headers: this.basketHeaders(), observe: 'response' })
      .pipe(
        map((res) => {
          this.storeBasketIdFromResponse(res.headers);
          return res.body ?? { buyerId, items: [] };
        }),
        catchError((err) => throwError(() => err))
      );
  }

  removeItem(productId: string): Observable<CustomerBasketDto> {
    const buyerId = this.getOrCreateBasketId();
    const url = `${this.baseUrl}/api/basket/items/${encodeURIComponent(productId)}?buyerId=${encodeURIComponent(buyerId)}`;
    return this.http
      .delete<CustomerBasketDto>(url, { headers: this.basketHeaders(), observe: 'response' })
      .pipe(
        map((res) => {
          this.storeBasketIdFromResponse(res.headers);
          return res.body ?? { buyerId, items: [] };
        }),
        catchError((err) => throwError(() => err))
      );
  }

  clear(): Observable<void> {
    return this.http
      .delete<void>(`${this.baseUrl}/api/basket`, { headers: this.basketHeaders() })
      .pipe(catchError(() => of(undefined)));
  }
}
