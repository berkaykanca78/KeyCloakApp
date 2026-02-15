import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { InventoryService } from '../../core/api/inventory.service';
import { OrderService } from '../../core/api/order.service';
import { AuthService } from '../../core/auth/auth.service';
import type { InventoryPublicItem } from '../../core/api/api-types';
import { BasketActions } from '../../features/basket/state/basket.actions';

@Component({
  selector: 'app-product-detail-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './product-detail-page.component.html',
  styleUrl: './product-detail-page.component.scss',
})
export class ProductDetailPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly inventory = inject(InventoryService);
  private readonly orderService = inject(OrderService);
  protected readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly store = inject(Store);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly item = signal<InventoryPublicItem | null>(null);
  protected readonly orderSuccess = signal(false);
  protected readonly orderError = signal<string | null>(null);
  protected readonly orderSending = signal(false);

  protected id = computed(() => this.route.snapshot.paramMap.get('id') ?? '');
  protected form = this.fb.nonNullable.group({
    quantity: [1, [Validators.required, Validators.min(1)]],
  });

  constructor() {
    this.inventory.getPublic().subscribe({
      next: (res) => {
        const id = this.id();
        const found = res.items.find((i) => i.id === id) ?? null;
        this.item.set(found);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Ürün bilgisi alınamadı.');
        this.loading.set(false);
      },
    });
  }

  protected imageUrl(): string {
    return this.inventory.getImageUrl(this.id());
  }

  protected addToBasket(): void {
    const product = this.item();
    if (!product) return;
    const qty = this.form.getRawValue().quantity;
    this.store.dispatch(
      BasketActions.addToBasket({
        request: {
          productId: product.productId,
          productName: product.productName,
          inventoryItemId: product.id,
          quantity: qty,
          unitPrice: product.unitPrice ?? product.priceAfterDiscount ?? undefined,
          currency: product.currency ?? undefined,
        },
      })
    );
  }

  protected onSubmit(): void {
    const product = this.item();
    if (!product || this.form.invalid || this.orderSending()) return;
    this.orderError.set(null);
    this.orderSending.set(true);
    this.orderService.getCustomerMeOrCreate().subscribe({
      next: (customer) => {
        if (!customer) {
          this.orderSending.set(false);
          this.orderError.set('Müşteri kaydı bulunamadı. Kayıt olup adres bilgilerinizi tamamlayın.');
          return;
        }
        this.orderService
          .create({
            customerId: customer.id,
            productId: product.productId,
            quantity: this.form.getRawValue().quantity,
          })
          .subscribe({
            next: (res) => {
              this.orderSending.set(false);
              if (res.isSuccess && res.data) {
                this.orderSuccess.set(true);
                this.form.reset({ quantity: 1 });
              } else {
                this.orderError.set(res.message ?? 'Sipariş oluşturulamadı.');
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
