import { Routes } from '@angular/router';
import { authGuard, loginGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'products' },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login-page.component').then((m) => m.LoginPageComponent),
    canActivate: [loginGuard],
  },
  {
    path: 'products',
    loadComponent: () =>
      import('./features/products/products-page.component').then((m) => m.ProductsPageComponent),
  },
  {
    path: 'stock',
    loadComponent: () =>
      import('./features/stock/stock-page.component').then((m) => m.StockPageComponent),
  },
  {
    path: 'stock/:id',
    loadComponent: () =>
      import('./features/stock/stock-detail-page.component').then(
        (m) => m.StockDetailPageComponent
      ),
    canActivate: [authGuard],
  },
  {
    path: 'orders',
    loadComponent: () =>
      import('./features/orders/orders-page.component').then((m) => m.OrdersPageComponent),
    canActivate: [authGuard],
  },
];
