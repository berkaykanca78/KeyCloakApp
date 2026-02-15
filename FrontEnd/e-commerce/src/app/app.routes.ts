import { Routes } from '@angular/router';
import { authGuard, loginGuard, adminGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layouts/shop-layout/shop-layout.component').then((m) => m.ShopLayoutComponent),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'urunler' },
      {
        path: 'urunler',
        loadComponent: () =>
          import('./features/shop/home-page.component').then((m) => m.HomePageComponent),
      },
      {
        path: 'urun/:id',
        loadComponent: () =>
          import('./features/shop/product-detail-page.component').then((m) => m.ProductDetailPageComponent),
      },
      {
        path: 'sepet',
        loadComponent: () =>
          import('./features/basket/basket-page.component').then((m) => m.BasketPageComponent),
      },
      {
        path: 'siparislerim',
        loadComponent: () =>
          import('./features/shop/my-orders-page.component').then((m) => m.MyOrdersPageComponent),
        canActivate: [authGuard],
      },
      {
        path: 'giris',
        loadComponent: () =>
          import('./features/auth/login-page.component').then((m) => m.LoginPageComponent),
        canActivate: [loginGuard],
      },
      {
        path: 'kayit',
        loadComponent: () =>
          import('./features/auth/register-page.component').then((m) => m.RegisterPageComponent),
        canActivate: [loginGuard],
      },
    ],
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./layouts/dashboard-layout/dashboard-layout.component').then((m) => m.DashboardLayoutComponent),
    canActivate: [adminGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'warehouse' },
      {
        path: 'warehouse',
        loadComponent: () =>
          import('./features/dashboard/warehouse/warehouse-page.component').then((m) => m.WarehousePageComponent),
      },
      {
        path: 'warehouse/new',
        loadComponent: () =>
          import('./features/dashboard/warehouse/warehouse-create-page.component').then((m) => m.WarehouseCreatePageComponent),
      },
      {
        path: 'warehouse/add',
        loadComponent: () =>
          import('./features/dashboard/warehouse/inventory-create-page.component').then((m) => m.InventoryCreatePageComponent),
      },
      {
        path: 'warehouse/:id',
        loadComponent: () =>
          import('./features/dashboard/warehouse/warehouse-detail-page.component').then((m) => m.WarehouseDetailPageComponent),
      },
      {
        path: 'products',
        loadComponent: () =>
          import('./features/dashboard/products/products-page.component').then((m) => m.ProductsPageComponent),
      },
      {
        path: 'products/add',
        loadComponent: () =>
          import('./features/dashboard/products/product-create-page.component').then((m) => m.ProductCreatePageComponent),
      },
      {
        path: 'orders',
        loadComponent: () =>
          import('./features/dashboard/orders/orders-page.component').then((m) => m.OrdersPageComponent),
      },
      {
        path: 'customers',
        loadComponent: () =>
          import('./features/dashboard/customer/customers-page.component').then((m) => m.CustomersPageComponent),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
