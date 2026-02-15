import { inject } from '@angular/core';
import { createEffect, Actions, ofType } from '@ngrx/effects';
import { switchMap, map, catchError, of } from 'rxjs';
import { BasketService } from '../../../core/api/basket.service';
import { BasketActions } from './basket.actions';

export const loadBasketEffect = createEffect(
  (actions = inject(Actions), basketService = inject(BasketService)) =>
    actions.pipe(
      ofType(BasketActions.loadBasket),
      switchMap(() =>
        basketService.getBasket().pipe(
          map((basket) => BasketActions.loadBasketSuccess({ basket })),
          catchError((err) => of(BasketActions.loadBasketFailure({ error: err?.message ?? 'Sepet yüklenemedi' })))
        )
      )
    ),
  { functional: true }
);

export const addToBasketEffect = createEffect(
  (actions = inject(Actions), basketService = inject(BasketService)) =>
    actions.pipe(
      ofType(BasketActions.addToBasket),
      switchMap(({ request }) =>
        basketService.addItem(request).pipe(
          map((basket) => BasketActions.addToBasketSuccess({ basket })),
          catchError((err) => of(BasketActions.addToBasketFailure({ error: err?.message ?? 'Sepete eklenemedi' })))
        )
      )
    ),
  { functional: true }
);

export const updateQuantityEffect = createEffect(
  (actions = inject(Actions), basketService = inject(BasketService)) =>
    actions.pipe(
      ofType(BasketActions.updateQuantity),
      switchMap(({ productId, quantity }) =>
        basketService.updateQuantity(productId, quantity).pipe(
          map((basket) => BasketActions.updateQuantitySuccess({ basket })),
          catchError((err) => of(BasketActions.updateQuantityFailure({ error: err?.message ?? 'Güncellenemedi' })))
        )
      )
    ),
  { functional: true }
);

export const removeFromBasketEffect = createEffect(
  (actions = inject(Actions), basketService = inject(BasketService)) =>
    actions.pipe(
      ofType(BasketActions.removeFromBasket),
      switchMap(({ productId }) =>
        basketService.removeItem(productId).pipe(
          map((basket) => BasketActions.removeFromBasketSuccess({ basket })),
          catchError((err) => of(BasketActions.removeFromBasketFailure({ error: err?.message ?? 'Kaldırılamadı' })))
        )
      )
    ),
  { functional: true }
);

export const clearBasketEffect = createEffect(
  (actions = inject(Actions), basketService = inject(BasketService)) =>
    actions.pipe(
      ofType(BasketActions.clearBasket),
      switchMap(() =>
        basketService.clear().pipe(
          map(() => BasketActions.clearBasketSuccess()),
          catchError(() => of(BasketActions.clearBasketSuccess()))
        )
      )
    ),
  { functional: true }
);

export const BasketEffects = {
  loadBasketEffect,
  addToBasketEffect,
  updateQuantityEffect,
  removeFromBasketEffect,
  clearBasketEffect,
};
