import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  signal,
  computed,
} from '@angular/core';
import { Store } from '@ngrx/store';
import { Router, RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { forkJoin } from 'rxjs';
import { BasketActions } from './state/basket.actions';
import {
  selectBasketItems,
  selectBasketLoading,
  selectBasketError,
} from './state/basket.selectors';
import { InventoryService } from '../../core/api/inventory.service';
import { AuthService } from '../../core/auth/auth.service';
import { OrderService } from '../../core/api/order.service';

@Component({
  selector: 'app-basket-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, DecimalPipe],
  templateUrl: './basket-page.component.html',
  styleUrl: './basket-page.component.scss',
})
export class BasketPageComponent implements OnInit {
  private readonly store = inject(Store);
  private readonly inventory = inject(InventoryService);
  protected readonly auth = inject(AuthService);
  private readonly orderService = inject(OrderService);
  private readonly router = inject(Router);

  protected readonly items = this.store.selectSignal(selectBasketItems);
  protected readonly loading = this.store.selectSignal(selectBasketLoading);
  protected readonly error = this.store.selectSignal(selectBasketError);
  protected readonly isEmpty = computed(() => (this.items() ?? []).length === 0);
  protected readonly orderSending = signal(false);
  protected readonly orderError = signal<string | null>(null);
  protected readonly orderSuccess = signal(false);

  ngOnInit(): void {
    this.store.dispatch(BasketActions.loadBasket());
  }

  protected imageUrl(inventoryItemId: string | null | undefined): string {
    if (!inventoryItemId) return '';
    return this.inventory.getImageUrl(inventoryItemId);
  }

  protected updateQty(productId: string, quantity: number): void {
    if (quantity < 1) return;
    this.store.dispatch(BasketActions.updateQuantity({ productId, quantity }));
  }

  protected remove(productId: string): void {
    this.store.dispatch(BasketActions.removeFromBasket({ productId }));
  }

  protected clear(): void {
    this.store.dispatch(BasketActions.clearBasket());
  }

  protected placeOrder(): void {
    const itemList = this.items() ?? [];
    if (!this.auth.isAuthenticated() || itemList.length === 0 || this.orderSending()) return;
    this.orderError.set(null);
    this.orderSuccess.set(false);
    this.orderSending.set(true);
    this.orderService.getCustomerMeOrCreate().subscribe({
      next: (customer) => {
        if (!customer) {
          this.orderSending.set(false);
          this.orderError.set('Müşteri kaydı bulunamadı. Giriş yapıp adres bilgilerinizi tamamlayın.');
          return;
        }
        const requests = itemList.map((item) =>
          this.orderService.create({
            customerId: customer.id,
            productId: item.productId,
            quantity: item.quantity,
          })
        );
        forkJoin(requests).subscribe({
          next: (results) => {
            this.orderSending.set(false);
            const allOk = results.every((r) => r?.isSuccess);
            if (allOk) {
              this.orderSuccess.set(true);
              this.store.dispatch(BasketActions.clearBasket());
              setTimeout(() => this.router.navigate(['/siparislerim']), 1500);
            } else {
              this.orderError.set(results.find((r) => !r?.isSuccess)?.message ?? 'Sipariş oluşturulamadı.');
            }
          },
          error: (err) => {
            this.orderSending.set(false);
            this.orderError.set(err?.error?.message ?? 'Sipariş gönderilemedi.');
          },
        });
      },
      error: () => {
        this.orderSending.set(false);
        this.orderError.set('Müşteri bilgisi alınamadı. Giriş yapıp tekrar deneyin.');
      },
    });
  }
}
