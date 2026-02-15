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
      { path: '', pathMatch: 'full', redirectTo: 'depo' },
      {
        path: 'depo',
        loadComponent: () =>
          import('./features/dashboard/depo-page.component').then((m) => m.DepoPageComponent),
      },
      {
        path: 'depo/yeni',
        loadComponent: () =>
          import('./features/dashboard/warehouse-create-page.component').then((m) => m.WarehouseCreatePageComponent),
      },
      {
        path: 'depo/ekle',
        loadComponent: () =>
          import('./features/dashboard/depo-create-page.component').then((m) => m.DepoCreatePageComponent),
      },
      {
        path: 'depo/:id',
        loadComponent: () =>
          import('./features/dashboard/depo-detail-page.component').then((m) => m.DepoDetailPageComponent),
      },
      {
        path: 'urunler',
        loadComponent: () =>
          import('./features/dashboard/urunler-page.component').then((m) => m.UrunlerPageComponent),
      },
      {
        path: 'urunler/ekle',
        loadComponent: () =>
          import('./features/dashboard/product-create-page.component').then((m) => m.ProductCreatePageComponent),
      },
      {
        path: 'siparisler',
        loadComponent: () =>
          import('./features/dashboard/siparisler-page.component').then((m) => m.SiparislerPageComponent),
      },
      {
        path: 'musteriler',
        loadComponent: () =>
          import('./features/dashboard/musteriler-page.component').then((m) => m.MusterilerPageComponent),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
