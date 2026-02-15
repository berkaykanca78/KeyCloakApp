import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { OrderService } from '../../core/api/order.service';
import type { Order } from '../../core/api/api-types';

interface CustomerRow {
  customerName: string;
  orderCount: number;
  lastOrderAt: string;
}

@Component({
  selector: 'app-musteriler-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe],
  templateUrl: './musteriler-page.component.html',
  styleUrl: './musteriler-page.component.scss',
})
export class MusterilerPageComponent {
  private readonly orderService = inject(OrderService);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly customers = signal<CustomerRow[]>([]);
  protected readonly hasCustomers = computed(() => this.customers().length > 0);

  constructor() {
    this.orderService.getAll().subscribe({
      next: (orders) => {
        const map = new Map<string, { count: number; lastAt: string }>();
        for (const o of orders) {
          const key = o.customer ? `${o.customer.firstName} ${o.customer.lastName}`.trim() || o.customerId : o.customerId;
          const existing = map.get(key);
          const createdAt = o.createdAt;
          if (!existing) {
            map.set(key, { count: 1, lastAt: createdAt });
          } else {
            existing.count += 1;
            if (createdAt > existing.lastAt) existing.lastAt = createdAt;
          }
        }
        const rows: CustomerRow[] = Array.from(map.entries()).map(([customerName, v]) => ({
          customerName,
          orderCount: v.count,
          lastOrderAt: v.lastAt,
        }));
        rows.sort((a, b) => b.orderCount - a.orderCount);
        this.customers.set(rows);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Müşteri listesi yüklenemedi.');
        this.loading.set(false);
      },
    });
  }
}
