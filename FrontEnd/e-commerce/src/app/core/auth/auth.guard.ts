import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (_, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) return true;
  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url || '/products' },
  });
};

/** Login sayfası için: zaten giriş yapmışsa returnUrl veya /products'a yönlendirir */
export const loginGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.isAuthenticated()) return true;
  const params = new URL(router.url, 'http://x').searchParams;
  const returnUrl = params.get('returnUrl') ?? '/products';
  const path = returnUrl.startsWith('/') ? returnUrl : `/${returnUrl}`;
  return router.createUrlTree([path]);
};