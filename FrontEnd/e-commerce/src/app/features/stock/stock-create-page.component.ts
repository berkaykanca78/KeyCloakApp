import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  OnInit,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../../core/api/inventory.service';
import type { CreateInventoryRequest } from '../../core/api/api-types';

@Component({
  selector: 'app-stock-create-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './stock-create-page.component.html',
  styleUrl: './stock-create-page.component.scss',
})
export class StockCreatePageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly inventory = inject(InventoryService);
  private readonly fb = inject(FormBuilder);

  protected readonly saving = signal(false);
  protected readonly createError = signal<string | null>(null);
  protected readonly products = signal<{ id: string; name: string }[]>([]);
  protected readonly warehouses = signal<{ id: string; name: string }[]>([]);

  protected form = this.fb.nonNullable.group({
    productId: ['', [Validators.required]],
    warehouseId: ['', [Validators.required]],
    quantity: [0, [Validators.required, Validators.min(0)]],
  });

  ngOnInit(): void {
    this.inventory.getProducts().subscribe((list) => this.products.set(list));
    this.inventory.getWarehouses().subscribe((list) => this.warehouses.set(list));
  }

  protected onSubmit(): void {
    const raw = this.form.getRawValue();
    if (this.form.invalid || this.saving()) return;
    this.createError.set(null);
    this.saving.set(true);

    const request: CreateInventoryRequest = {
      productId: raw.productId,
      warehouseId: raw.warehouseId,
      quantity: raw.quantity,
    };

    this.inventory.create(request).subscribe({
      next: (res) => {
        if (!res.data) {
          this.createError.set(res.message ?? 'Stok kalemi eklenemedi.');
          this.saving.set(false);
          return;
        }
        this.saving.set(false);
        this.router.navigate(['/stock', res.data.id]);
      },
      error: (err) => {
        this.createError.set(err?.error?.message ?? 'Stok kalemi eklenemedi.');
        this.saving.set(false);
      },
    });
  }
}
