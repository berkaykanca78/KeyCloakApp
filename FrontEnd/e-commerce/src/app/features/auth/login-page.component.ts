import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  computed,
} from '@angular/core';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.scss',
})
export class LoginPageComponent {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  protected form = this.fb.nonNullable.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]],
  });

  protected returnUrl = computed(() => {
    const q = this.route.snapshot.queryParamMap.get('returnUrl');
    return q ?? '/urunler';
  });

  protected onSubmit(): void {
    this.error.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { username, password } = this.form.getRawValue();
    this.loading.set(true);
    this.auth.login(username, password).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res) {
          this.router.navigateByUrl(this.returnUrl());
        } else {
          this.error.set('Giriş başarısız. Kullanıcı adı veya şifre hatalı.');
        }
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Bağlantı hatası. Lütfen tekrar deneyin.');
      },
    });
  }
}
