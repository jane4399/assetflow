import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'work-orders' },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'work-orders',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/work-orders/work-order-list.component').then((m) => m.WorkOrderListComponent)
  },
  {
    path: 'work-orders/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/work-orders/work-order-detail.component').then((m) => m.WorkOrderDetailComponent)
  },
  {
    path: 'work-orders/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/work-orders/work-order-detail.component').then((m) => m.WorkOrderDetailComponent)
  },
  {
    path: 'assets',
    canActivate: [authGuard],
    loadComponent: () => import('./features/assets/asset-list.component').then((m) => m.AssetListComponent)
  },
  { path: '**', redirectTo: 'work-orders' }
];
