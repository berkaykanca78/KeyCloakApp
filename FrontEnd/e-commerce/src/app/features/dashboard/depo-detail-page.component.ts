import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
  viewChild,
} from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InventoryService } from '../../core/api/inventory.service';
import type { InventoryItem } from '../../core/api/api-types';

@Component({
  selector: 'app-depo-detail-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './depo-detail-page.component.html',
  styleUrl: './depo-detail-page.component.scss',
})
export class DepoDetailPageComponent {
  private readonly route = inject(ActivatedRoute);
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
  protected readonly fileInput = viewChild<HTMLInputElement>('fileInput');
  /** Resim güncellendikten sonra cache kırımı için */
  protected readonly imageVersion = signal(0);

  protected readonly id = computed(() => this.route.snapshot.paramMap.get('id') ?? '');
  protected readonly imageUrl = computed(() => {
    const it = this.item();
    const id = it?.id ?? this.id();
    const base = id ? this.inventory.getImageUrl(id) : '';
    return base ? `${base}?v=${this.imageVersion()}` : '';
  });

  protected form = this.fb.nonNullable.group({
    quantity: [0, [Validators.required, Validators.min(0)]],
  });

  constructor() {
    const id = this.id();
    if (!id) {
      this.error.set('Geçersiz stok kimliği.');
      this.loading.set(false);
      return;
    }
    this.inventory.getById(id).subscribe({
      next: (data) => {
        this.item.set(data);
        if (data) this.form.patchValue({ quantity: data.quantity });
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Stok detayı yüklenemedi.');
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
    const currentItem = this.item();
    const id = currentItem?.id ?? this.route.snapshot.paramMap.get('id') ?? '';
    const file = this.selectedImageFile();
    if (!id || !file || !currentItem) return;
    this.imageError.set(null);
    this.imageSuccess.set(false);
    this.uploadingImage.set(true);
    this.inventory.uploadImage(id, file).subscribe({
      next: () => {
        this.imageSuccess.set(true);
        this.selectedImageFile.set(null);
        this.uploadingImage.set(false);
        this.resetFileInput();
        this.imageVersion.update((v) => v + 1);
        this.inventory.getById(id).subscribe({
          next: (updated) => updated && this.item.set(updated),
        });
      },
      error: (err: { error?: { message?: string }; status?: number }) => {
        const msg =
          err?.error?.message ??
          (err?.status === 413
            ? 'Dosya çok büyük (max 5 MB).'
            : err?.status === 401
              ? 'Oturum süresi doldu. Tekrar giriş yapın.'
              : err?.status === 404
                ? 'API veya ürün bulunamadı.'
                : 'Resim yüklenemedi. MinIO çalışıyor mu?');
        this.imageError.set(msg);
        this.uploadingImage.set(false);
      },
    });
  }

  private resetFileInput(): void {
    const input = this.fileInput();
    if (input) input.value = '';
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
        this.saveError.set(err?.error?.message ?? 'Güncelleme başarısız.');
        this.saving.set(false);
      },
    });
  }
}
