import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { OrderService } from '../../core/api/order.service';
import type { Order } from '../../core/api/api-types';

@Component({
  selector: 'app-my-orders-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, DatePipe],
  templateUrl: './my-orders-page.component.html',
  styleUrl: './my-orders-page.component.scss',
})
export class MyOrdersPageComponent {
  private readonly orderService = inject(OrderService);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly orders = signal<Order[]>([]);
  protected readonly hasOrders = computed(() => this.orders().length > 0);

  constructor() {
    this.orderService.getMyOrders().subscribe({
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
