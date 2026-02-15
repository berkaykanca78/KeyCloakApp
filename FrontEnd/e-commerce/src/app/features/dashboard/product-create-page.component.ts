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

@Component({
  selector: 'app-product-create-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './product-create-page.component.html',
  styleUrl: './product-create-page.component.scss',
})
export class ProductCreatePageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly inventory = inject(InventoryService);
  private readonly fb = inject(FormBuilder);

  protected readonly saving = signal(false);
  protected readonly createError = signal<string | null>(null);
  protected readonly warehouses = signal<{ id: string; name: string }[]>([]);

  protected form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    warehouseIds: [[] as string[], [Validators.required]],
    initialQuantity: [0, [Validators.min(0)]],
  });

  ngOnInit(): void {
    this.inventory.getWarehouses().subscribe((list) =>
      this.warehouses.set(list.map((w) => ({ id: w.id, name: w.name })))
    );
  }

  protected toggleWarehouse(id: string): void {
    const current = this.form.controls.warehouseIds.value;
    const set = new Set(current);
    if (set.has(id)) set.delete(id);
    else set.add(id);
    this.form.controls.warehouseIds.setValue([...set]);
  }

  protected isSelected(id: string): boolean {
    return this.form.controls.warehouseIds.value.includes(id);
  }

  protected onSubmit(): void {
    if (this.form.invalid || this.saving()) return;
    const ids = this.form.controls.warehouseIds.value.filter(Boolean);
    if (ids.length === 0) {
      this.createError.set('En az bir depo seçmelisiniz.');
      return;
    }
    this.createError.set(null);
    this.saving.set(true);

    const { name, initialQuantity } = this.form.getRawValue();

    this.inventory
      .createProduct({
        name,
        warehouseIds: ids,
        initialQuantity: initialQuantity ?? 0,
      })
      .subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.data) {
            this.router.navigate(['/dashboard/urunler']);
            return;
          }
          this.createError.set(res.message ?? 'Ürün eklenemedi.');
        },
        error: (err) => {
          this.saving.set(false);
          this.createError.set(
            err?.error?.message ?? 'Ürün eklenemedi. Önce depo ekleyin.'
          );
        },
      });
  }
}
