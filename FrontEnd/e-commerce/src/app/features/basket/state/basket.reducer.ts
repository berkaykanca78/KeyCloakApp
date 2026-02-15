import { createReducer, on } from '@ngrx/store';
import type { CustomerBasketDto } from '../../../core/api/api-types';
import { BasketActions } from './basket.actions';

export interface BasketState {
  basket: CustomerBasketDto | null;
  loading: boolean;
  error: string | null;
}

export const initialBasketState: BasketState = {
  basket: null,
  loading: false,
  error: null,
};

export const basketReducer = createReducer(
  initialBasketState,
  on(BasketActions.loadBasket, (state) => ({ ...state, loading: true, error: null })),
  on(BasketActions.loadBasketSuccess, (state, { basket }) => ({
    ...state,
    basket,
    loading: false,
    error: null,
  })),
  on(BasketActions.loadBasketFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),
  on(BasketActions.addToBasket, (state) => ({ ...state, loading: true, error: null })),
  on(BasketActions.addToBasketSuccess, (state, { basket }) => ({
    ...state,
    basket,
    loading: false,
    error: null,
  })),
  on(BasketActions.addToBasketFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),
  on(BasketActions.updateQuantity, (state) => ({ ...state, loading: true, error: null })),
  on(BasketActions.updateQuantitySuccess, (state, { basket }) => ({
    ...state,
    basket,
    loading: false,
    error: null,
  })),
  on(BasketActions.updateQuantityFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),
  on(BasketActions.removeFromBasket, (state) => ({ ...state, loading: true, error: null })),
  on(BasketActions.removeFromBasketSuccess, (state, { basket }) => ({
    ...state,
    basket,
    loading: false,
    error: null,
  })),
  on(BasketActions.removeFromBasketFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),
  on(BasketActions.clearBasket, (state) => ({ ...state, loading: true })),
  on(BasketActions.clearBasketSuccess, (state) => ({
    ...state,
    basket: state.basket ? { buyerId: state.basket.buyerId, items: [] } : null,
    loading: false,
    error: null,
  }))
);
