import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (_, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) return true;
  return router.createUrlTree(['/giris'], {
    queryParams: { returnUrl: state.url || '/urunler' },
  });
};

/** Login sayfası için: zaten giriş yapmışsa returnUrl veya ana sayfaya yönlendirir */
export const loginGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.isAuthenticated()) return true;
  const params = new URL(router.url, 'http://x').searchParams;
  const returnUrl = params.get('returnUrl') ?? '/';
  const path = returnUrl.startsWith('/') ? returnUrl : `/${returnUrl}`;
  return router.createUrlTree([path]);
};

/** Sadece Admin rolü dashboard'a erişebilir */
export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.isAuthenticated())
    return router.createUrlTree(['/giris'], { queryParams: { returnUrl: '/dashboard' } });
  if (auth.isAdmin()) return true;
  return router.createUrlTree(['/']);
};