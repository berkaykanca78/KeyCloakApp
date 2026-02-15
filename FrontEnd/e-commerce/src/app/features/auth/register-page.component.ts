import {
  Component,
  ChangeDetectionStrategy,
  inject,
  signal,
  OnInit,
  computed,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService, type RegisterPayload } from '../../core/auth/auth.service';
import { CitiesApi, type CityDto, type DistrictDto } from '../../core/api/cities-api';

@Component({
  selector: 'app-register-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register-page.component.html',
  styleUrl: './register-page.component.scss',
})
export class RegisterPageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly citiesApi = inject(CitiesApi);

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly cities = signal<CityDto[]>([]);
  protected readonly districts = signal<DistrictDto[]>([]);

  protected form = this.fb.nonNullable.group({
    username: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    firstName: ['', [Validators.required]],
    lastName: ['', [Validators.required]],
    address: [''],
    cityId: [0 as number, []],
    districtId: [0 as number, []],
    cardLast4: ['', [Validators.maxLength(4), Validators.pattern(/^\d{0,4}$/)]],
  });

  protected cityId = computed(() => this.form.controls.cityId.value);

  ngOnInit(): void {
    this.citiesApi.getCities().subscribe({
      next: (list) => this.cities.set(list),
      error: () => this.cities.set([]),
    });
  }

  protected onCityChange(): void {
    const id = this.form.controls.cityId.value;
    this.form.patchValue({ districtId: 0 });
    if (!id) {
      this.districts.set([]);
      return;
    }
    this.citiesApi.getDistricts(id).subscribe({
      next: (list) => this.districts.set(list),
      error: () => this.districts.set([]),
    });
  }

  protected onSubmit(): void {
    this.error.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const payload: RegisterPayload = {
      username: raw.username,
      email: raw.email,
      password: raw.password,
      firstName: raw.firstName,
      lastName: raw.lastName,
      address: raw.address || undefined,
      cityId: raw.cityId || undefined,
      districtId: raw.districtId || undefined,
      cardLast4: raw.cardLast4 || undefined,
    };
    this.loading.set(true);
    this.auth.register(payload).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res) {
          this.router.navigate(['/giris'], { queryParams: { registered: '1' } });
        } else {
          this.error.set('Kayıt başarısız. Kullanıcı adı veya e-posta kullanımda olabilir.');
        }
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Bağlantı hatası. Lütfen tekrar deneyin.');
      },
    });
  }
}
