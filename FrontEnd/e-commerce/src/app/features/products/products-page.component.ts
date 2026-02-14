import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { InventoryService } from '../../core/api/inventory.service';
import type { InventoryPublicItem } from '../../core/api/api-types';

@Component({
  selector: 'app-products-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  templateUrl: './products-page.component.html',
  styleUrl: './products-page.component.scss',
})
export class ProductsPageComponent {
  private readonly inventory = inject(InventoryService);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly message = signal('');
  protected readonly items = signal<InventoryPublicItem[]>([]);

  protected readonly hasItems = computed(() => this.items().length > 0);

  constructor() {
    this.inventory.getPublic().subscribe({
      next: (res) => {
        this.message.set(res.message);
        this.items.set(res.items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Ürün listesi yüklenirken bir hata oluştu.');
        this.loading.set(false);
      },
    });
  }
}
