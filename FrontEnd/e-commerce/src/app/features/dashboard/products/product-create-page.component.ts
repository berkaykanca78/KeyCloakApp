import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  OnInit,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../../../core/api/inventory.service';

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
  protected readonly selectedImageFile = signal<File | null>(null);

  protected form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    unitPrice: [0, [Validators.required, Validators.min(0)]],
    currency: ['TRY', []],
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

  protected onImageFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedImageFile.set(input.files?.[0] ?? null);
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

    const { name, unitPrice, currency, initialQuantity } = this.form.getRawValue();
    const imageFile = this.selectedImageFile();

    this.inventory
      .createProduct({
        name,
        unitPrice: unitPrice ?? 0,
        currency: currency ?? 'TRY',
        warehouseIds: ids,
        initialQuantity: initialQuantity ?? 0,
      })
      .subscribe({
        next: (res) => {
          if (!res.data) {
            this.saving.set(false);
            this.createError.set(res.message ?? 'Ürün eklenemedi.');
            return;
          }
          const productId = res.data.id;
          if (imageFile) {
            this.inventory.getAll().subscribe({
              next: (items) => {
                const firstItem = items.find((i) => i.productId === productId);
                if (firstItem) {
                  this.inventory.uploadImage(firstItem.id, imageFile).subscribe({
                    next: () => {
                      this.saving.set(false);
                      this.router.navigate(['/dashboard/products']);
                    },
                    error: () => {
                      this.saving.set(false);
                      this.router.navigate(['/dashboard/products']);
                    },
                  });
                } else {
                  this.saving.set(false);
                  this.router.navigate(['/dashboard/products']);
                }
              },
              error: () => {
                this.saving.set(false);
                this.router.navigate(['/dashboard/products']);
              },
            });
          } else {
            this.saving.set(false);
            this.router.navigate(['/dashboard/products']);
          }
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
