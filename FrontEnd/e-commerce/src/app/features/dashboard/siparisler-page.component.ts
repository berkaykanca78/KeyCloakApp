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

@Component({
  selector: 'app-siparisler-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe],
  templateUrl: './siparisler-page.component.html',
  styleUrl: './siparisler-page.component.scss',
})
export class SiparislerPageComponent {
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
