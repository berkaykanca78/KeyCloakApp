import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { InventoryService } from '../../core/api/inventory.service';
import type { InventoryItem } from '../../core/api/api-types';

@Component({
  selector: 'app-urunler-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  templateUrl: './urunler-page.component.html',
  styleUrl: './urunler-page.component.scss',
})
export class UrunlerPageComponent {
  private readonly inventory = inject(InventoryService);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly items = signal<InventoryItem[]>([]);
  protected readonly hasItems = computed(() => this.items().length > 0);

  constructor() {
    this.inventory.getAll().subscribe({
      next: (list) => {
        this.items.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Ürün listesi yüklenemedi.');
        this.loading.set(false);
      },
    });
  }

  protected imageUrl(id: string): string {
    return this.inventory.getImageUrl(id);
  }
}
