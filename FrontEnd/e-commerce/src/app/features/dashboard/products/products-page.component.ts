import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
  OnInit,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../../../core/api/inventory.service';
import type { InventoryPublicItem } from '../../../core/api/api-types';

@Component({
  selector: 'app-products-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './products-page.component.html',
  styleUrl: './products-page.component.scss',
})
export class ProductsPageComponent implements OnInit {
  private readonly inventory = inject(InventoryService);
  private readonly fb = inject(FormBuilder);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly items = signal<InventoryPublicItem[]>([]);
  protected readonly hasItems = computed(() => this.items().length > 0);
  protected readonly products = signal<{ id: string; name: string }[]>([]);
  protected readonly discountSaving = signal(false);
  protected readonly discountError = signal<string | null>(null);
  protected readonly discountSuccess = signal(false);

  private static toDatetimeLocal(d: Date): string {
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }

  protected discountForm = this.fb.nonNullable.group({
    productId: ['', Validators.required],
    discountPercent: [10, [Validators.required, Validators.min(1), Validators.max(100)]],
    startAt: [ProductsPageComponent.toDatetimeLocal(new Date()), Validators.required],
    endAt: [
      ProductsPageComponent.toDatetimeLocal(new Date(Date.now() + 7 * 24 * 60 * 60 * 1000)),
      Validators.required,
    ],
    name: [''],
  });

  ngOnInit(): void {
    this.loadList();
    this.inventory.getProducts().subscribe((list) => this.products.set(list));
  }

  protected loadList(): void {
    this.loading.set(true);
    this.error.set(null);
    this.inventory.getPublic().subscribe({
      next: (res) => {
        this.items.set(res.items);
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

  protected formatPrice(unitPrice?: number | null, currency?: string | null): string {
    if (unitPrice == null) return '–';
    const cur = currency ?? 'TRY';
    return `${Number(unitPrice).toLocaleString('tr-TR')} ${cur}`;
  }

  protected formatDiscountPrice(item: InventoryPublicItem): string {
    if (item.priceAfterDiscount != null) return this.formatPrice(item.priceAfterDiscount, item.currency);
    return this.formatPrice(item.unitPrice, item.currency);
  }

  protected hasDiscount(item: InventoryPublicItem): boolean {
    return item.discountPercent != null && item.discountPercent > 0;
  }

  protected onAddDiscount(): void {
    if (this.discountForm.invalid || this.discountSaving()) return;
    this.discountError.set(null);
    this.discountSuccess.set(false);
    this.discountSaving.set(true);
    const v = this.discountForm.getRawValue();
    const startAt = new Date(v.startAt);
    const endAt = new Date(v.endAt);
    if (endAt <= startAt) {
      this.discountError.set('Bitiş tarihi başlangıçtan sonra olmalı.');
      this.discountSaving.set(false);
      return;
    }
    this.inventory
      .createProductDiscount({
        productId: v.productId,
        discountPercent: v.discountPercent,
        startAt: startAt.toISOString(),
        endAt: endAt.toISOString(),
        name: v.name || undefined,
      })
      .subscribe({
        next: (res) => {
          this.discountSaving.set(false);
          if (res.isSuccess) {
            this.discountSuccess.set(true);
            const now = new Date();
            const weekLater = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);
            this.discountForm.reset({
              productId: '',
              discountPercent: 10,
              startAt: ProductsPageComponent.toDatetimeLocal(now),
              endAt: ProductsPageComponent.toDatetimeLocal(weekLater),
              name: '',
            });
            this.loadList();
          } else {
            this.discountError.set(res.message ?? 'İndirim eklenemedi.');
          }
        },
        error: (err) => {
          this.discountSaving.set(false);
          this.discountError.set(err?.error?.message ?? 'İndirim eklenemedi.');
        },
      });
  }
}
