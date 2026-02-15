import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
} from '@angular/core';
import { Store } from '@ngrx/store';
import { RouterLink } from '@angular/router';
import { InventoryService } from '../../core/api/inventory.service';
import { AuthService } from '../../core/auth/auth.service';
import type { InventoryPublicItem } from '../../core/api/api-types';
import { BasketActions } from '../../features/basket/state/basket.actions';

@Component({
  selector: 'app-home-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss',
})
export class HomePageComponent {
  private readonly inventory = inject(InventoryService);
  private readonly store = inject(Store);
  protected readonly auth = inject(AuthService);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly items = signal<InventoryPublicItem[]>([]);
  protected readonly hasItems = computed(() => this.items().length > 0);

  constructor() {
    this.inventory.getPublic().subscribe({
      next: (res) => {
        this.items.set(res.items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Ürünler yüklenemedi.');
        this.loading.set(false);
      },
    });
  }

  protected imageUrl(id: string): string {
    return this.inventory.getImageUrl(id);
  }

  protected addToBasket(item: InventoryPublicItem): void {
    this.store.dispatch(
      BasketActions.addToBasket({
        request: {
          productId: item.productId,
          productName: item.productName,
          inventoryItemId: item.id,
          quantity: 1,
          unitPrice: item.unitPrice ?? item.priceAfterDiscount ?? undefined,
          currency: item.currency ?? undefined,
        },
      })
    );
  }
}
