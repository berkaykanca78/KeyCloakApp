import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
} from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../../core/api/inventory.service';
import type { InventoryItem } from '../../core/api/api-types';

@Component({
  selector: 'app-stock-detail-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './stock-detail-page.component.html',
  styleUrl: './stock-detail-page.component.scss',
})
export class StockDetailPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly inventory = inject(InventoryService);
  private readonly fb = inject(FormBuilder);

  protected readonly item = signal<InventoryItem | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly saveSuccess = signal(false);
  protected readonly saveError = signal<string | null>(null);
  protected readonly saving = signal(false);
  protected readonly imageSuccess = signal(false);
  protected readonly imageError = signal<string | null>(null);
  protected readonly uploadingImage = signal(false);
  protected readonly selectedImageFile = signal<File | null>(null);

  protected readonly id = computed(() => this.route.snapshot.paramMap.get('id') ?? '');

  protected readonly imageUrl = computed(() => {
    const id = this.id();
    return id ? this.inventory.getImageUrl(id) : '';
  });

  protected form = this.fb.nonNullable.group({
    quantity: [0, [Validators.required, Validators.min(0)]],
  });

  constructor() {
    const id = this.id();
    if (!id) {
      this.error.set('Geçersiz ürün kimliği.');
      this.loading.set(false);
      return;
    }
    this.inventory.getById(id).subscribe({
      next: (data) => {
        this.item.set(data);
        if (data) {
          this.form.patchValue({ quantity: data.quantity });
        }
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Stok detayı yüklenemedi. Giriş yapmanız gerekebilir.');
        this.loading.set(false);
      },
    });
  }

  protected onImageFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedImageFile.set(input.files?.[0] ?? null);
    this.imageError.set(null);
  }

  protected onImageSubmit(): void {
    const id = this.id();
    const file = this.selectedImageFile();
    if (!id || !file || this.item() == null) return;
    this.imageError.set(null);
    this.imageSuccess.set(false);
    this.uploadingImage.set(true);
    this.inventory.uploadImage(id, file).subscribe({
      next: (res) => {
        this.imageSuccess.set(true);
        this.selectedImageFile.set(null);
        this.uploadingImage.set(false);
        this.inventory.getById(id).subscribe({
          next: (updated) => {
            if (updated) this.item.set(updated);
          },
        });
      },
      error: (err) => {
        const msg =
          err?.error?.message ??
          (err?.status === 404
            ? 'API bulunamadı. InventoryApi bu adreste çalışıyor mu?'
            : err?.status === 401
              ? 'Oturum süresi dolmuş olabilir; tekrar giriş yapın.'
              : 'Resim yüklenemedi. MinIO çalışıyor mu? (docker-compose)');
        this.imageError.set(msg);
        this.uploadingImage.set(false);
      },
    });
  }

  protected onSubmit(): void {
    const id = this.id();
    const raw = this.form.getRawValue();
    if (!id || this.item() == null) return;
    this.saveError.set(null);
    this.saving.set(true);
    this.inventory.updateQuantity(id, { quantity: raw.quantity }).subscribe({
      next: (res) => {
        if (res.data) {
          this.item.set(res.data);
          this.form.patchValue({ quantity: res.data.quantity });
          this.saveSuccess.set(true);
          this.saveError.set(null);
        }
        this.saving.set(false);
      },
      error: (err) => {
        this.saveError.set(err?.error?.message ?? 'Güncelleme başarısız. Yetkiniz olmayabilir.');
        this.saving.set(false);
      },
    });
  }
}
