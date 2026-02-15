import { createActionGroup, emptyProps, props } from '@ngrx/store';
import type { CustomerBasketDto, AddBasketItemRequest } from '../../../core/api/api-types';

export const BasketActions = createActionGroup({
  source: 'Basket',
  events: {
    'Load basket': emptyProps(),
    'Load basket success': props<{ basket: CustomerBasketDto }>(),
    'Load basket failure': props<{ error: string }>(),
    'Add to basket': props<{ request: AddBasketItemRequest }>(),
    'Add to basket success': props<{ basket: CustomerBasketDto }>(),
    'Add to basket failure': props<{ error: string }>(),
    'Update quantity': props<{ productId: string; quantity: number }>(),
    'Update quantity success': props<{ basket: CustomerBasketDto }>(),
    'Update quantity failure': props<{ error: string }>(),
    'Remove from basket': props<{ productId: string }>(),
    'Remove from basket success': props<{ basket: CustomerBasketDto }>(),
    'Remove from basket failure': props<{ error: string }>(),
    'Clear basket': emptyProps(),
    'Clear basket success': emptyProps(),
  },
});
