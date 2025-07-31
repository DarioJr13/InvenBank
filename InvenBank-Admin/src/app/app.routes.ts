import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';
import { AdminGuard } from './guards/admin.guard';
import { NoAuthGuard } from './guards/no-auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./Components/login/login.component').then(m => m.LoginComponent),
    canActivate: [NoAuthGuard]
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./Components/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        redirectTo: 'overview',
        pathMatch: 'full'
      },
      {
        path: 'overview',
        loadComponent: () => import('./Components/overview/overview.component').then(m => m.OverviewComponent)
      },
      {
        path: 'products',
        loadComponent: () => import('./Components/products/product-list.component').then(m => m.ProductListComponent)
      },
      {
        path: 'suppliers',
        loadComponent: () => import('./Components/suppliers/supplier-list.component').then(m => m.SupplierListComponent)
      }
    ]
  },
  {
    path: 'products',
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./Components/products/product-list.component').then(m => m.ProductListComponent)
      },
      {
        path: 'create',
        loadComponent: () => import('./Components/product-form/product-form.component').then(m => m.ProductFormComponent)
      },
      {
        path: ':id/edit',
        loadComponent: () => import('./Components/product-form/product-form.component').then(m => m.ProductFormComponent)
      },
      {
        path: ':id/suppliers',
        loadComponent: () => import('./Components/products/product-suppliers.component').then(m => m.ProductSuppliersComponent)
      }
    ]
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('./Components/errors/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent)
  }
];





// {
  //   path: '**',
  //   loadComponent: () => import('./Components/errors/not-found.component').then(m => m.NotFoundComponent)
  // }


