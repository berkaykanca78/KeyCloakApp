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
  selector: 'app-home-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss',
})
export class HomePageComponent {
  private readonly inventory = inject(InventoryService);

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
}
