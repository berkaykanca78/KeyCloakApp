import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { OrderService } from '../../../core/api/order.service';
import type { Order } from '../../../core/api/api-types';

@Component({
  selector: 'app-orders-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe],
  templateUrl: './orders-page.component.html',
  styleUrl: './orders-page.component.scss',
})
export class OrdersPageComponent {
  private readonly orderService = inject(OrderService);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly orders = signal<Order[]>([]);
  protected readonly hasOrders = computed(() => this.orders().length > 0);

  constructor() {
    this.orderService.getAll().subscribe({
      next: (list) => {
        this.orders.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Siparişler yüklenemedi.');
        this.loading.set(false);
      },
    });
  }
}
