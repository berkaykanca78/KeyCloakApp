import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../../core/api/inventory.service';
@Component({
  selector: 'app-warehouse-create-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './warehouse-create-page.component.html',
  styleUrl: './warehouse-create-page.component.scss',
})
export class WarehouseCreatePageComponent {
  private readonly router = inject(Router);
  private readonly inventory = inject(InventoryService);
  private readonly fb = inject(FormBuilder);

  protected readonly saving = signal(false);
  protected readonly createError = signal<string | null>(null);
  protected readonly selectedImageFile = signal<File | null>(null);

  protected form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    code: [''],
  });

  protected onImageFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedImageFile.set(input.files?.[0] ?? null);
  }

  protected onSubmit(): void {
    if (this.form.invalid || this.saving()) return;
    this.createError.set(null);
    this.saving.set(true);

    const { name, code } = this.form.getRawValue();
    const file = this.selectedImageFile();

    this.inventory.createWarehouse(name, code || undefined).subscribe({
      next: (res) => {
        if (!res.data) {
          this.saving.set(false);
          this.createError.set(res.message ?? 'Depo eklenemedi.');
          return;
        }
        if (file) {
          this.inventory.uploadWarehouseImage(res.data.id, file).subscribe({
            next: () => {
              this.saving.set(false);
              this.router.navigate(['/dashboard/depo']);
            },
            error: (err) => {
              this.saving.set(false);
              this.createError.set(err?.error?.message ?? 'Depo eklendi ancak resim yüklenemedi.');
            },
          });
        } else {
          this.saving.set(false);
          this.router.navigate(['/dashboard/depo']);
        }
      },
      error: (err) => {
        this.saving.set(false);
        this.createError.set(err?.error?.message ?? 'Depo eklenemedi.');
      },
    });
  }
}
