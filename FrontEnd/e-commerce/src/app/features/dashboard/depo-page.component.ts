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

export interface WarehouseItem {
  id: string;
  name: string;
  code?: string | null;
}

@Component({
  selector: 'app-depo-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  templateUrl: './depo-page.component.html',
  styleUrl: './depo-page.component.scss',
})
export class DepoPageComponent {
  private readonly inventory = inject(InventoryService);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly warehouses = signal<WarehouseItem[]>([]);
  protected readonly items = signal<InventoryPublicItem[]>([]);
  protected readonly hasWarehouses = computed(() => this.warehouses().length > 0);
  protected readonly hasItems = computed(() => this.items().length > 0);

  constructor() {
    let warehousesDone = false;
    let itemsDone = false;
    const checkDone = () => {
      if (warehousesDone && itemsDone) this.loading.set(false);
    };

    this.inventory.getWarehouses().subscribe({
      next: (list) => {
        this.warehouses.set(list);
        warehousesDone = true;
        checkDone();
      },
      error: () => {
        this.warehouses.set([]);
        warehousesDone = true;
        checkDone();
      },
    });

    this.inventory.getPublic().subscribe({
      next: (res) => {
        this.items.set(res.items);
        itemsDone = true;
        checkDone();
      },
      error: () => {
        this.items.set([]);
        itemsDone = true;
        checkDone();
      },
    });
  }

  protected warehouseImageUrl(id: string): string {
    return this.inventory.getWarehouseImageUrl(id);
  }

  protected imageUrl(id: string): string {
    return this.inventory.getImageUrl(id);
  }
}
