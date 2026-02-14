import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

const API_BASE = 'https://localhost:5001';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const isApiRequest = req.url.startsWith(API_BASE);
  const token = auth.getAccessToken();

  const reqWithAuth =
    isApiRequest && token
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(reqWithAuth).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && isApiRequest && auth.getAccessToken()) {
        return auth.refresh().pipe(
          switchMap((res) => {
            const newToken = res?.access_token ?? auth.getAccessToken();
            if (newToken) {
              const retry = req.clone({
                setHeaders: { Authorization: `Bearer ${newToken}` },
              });
              return next(retry);
            }
            auth.logout();
            router.navigate(['/login'], {
              queryParams: { returnUrl: router.url },
            });
            return throwError(() => err);
          }),
          catchError(() => {
            auth.logout();
            router.navigate(['/login'], {
              queryParams: { returnUrl: router.url },
            });
            return throwError(() => err);
          })
        );
      }
      return throwError(() => err);
    })
  );
};
