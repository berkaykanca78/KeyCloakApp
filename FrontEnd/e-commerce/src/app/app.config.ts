import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';

import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { basketReducer } from './features/basket/state/basket.reducer';
import { BasketEffects } from './features/basket/state/basket.effects';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideStore({ basket: basketReducer }),
    provideEffects(BasketEffects),
  ],
};
