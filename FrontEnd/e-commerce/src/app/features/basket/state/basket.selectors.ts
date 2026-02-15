import { createFeatureSelector, createSelector } from '@ngrx/store';
import type { BasketState } from './basket.reducer';

export const selectBasketState = createFeatureSelector<BasketState>('basket');

export const selectBasket = createSelector(selectBasketState, (state) => state.basket);

export const selectBasketItems = createSelector(selectBasket, (basket) => basket?.items ?? []);

export const selectBasketCount = createSelector(selectBasketItems, (items) =>
  items.reduce((sum, i) => sum + i.quantity, 0)
);

export const selectBasketLoading = createSelector(selectBasketState, (state) => state.loading);

export const selectBasketError = createSelector(selectBasketState, (state) => state.error);
