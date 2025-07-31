// app.routes.ts
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
    canActivate: [AuthGuard, AdminGuard]
  },
  {
    path: 'products',
    loadComponent: () => import('./Components/products/product-list.component').then(m => m.ProductListComponent),
    canActivate: [AuthGuard]
  },
{
  path: 'unauthorized',
  loadComponent: () => import('./Components/errors/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent)
}
  // {
  //   path: '**',
  //   loadComponent: () => import('./Components/errors/not-found.component').then(m => m.NotFoundComponent)
  // }
];




// import { Routes } from '@angular/router';
// import { AuthGuard } from './guards/auth.guard';
// import { AdminGuard } from './guards/admin.guard';

// export const routes: Routes = [
//   {
//     path: 'login',
//     loadComponent: () => import('./Components/login/login.component').then(m => m.LoginComponent)
//   },
//  {
//     path: 'dashboard',
//     loadComponent: () => import('./Components/dashboard/dashboard.component').then(m => m.DashboardComponent),
//     canActivate: [AuthGuard, AdminGuard]
//   },
//   {
//     path: 'products',
//     loadComponent: () => import('./Components/products/product-list.component').then(m => m.ProductListComponent)
//   },
//   {
//     path: 'suppliers',
//     loadComponent: () => import('./Components/suppliers/supplier-list.component').then(m => m.SupplierListComponent),
//     canActivate: [AuthGuard]
//   },
//   {
//     path: 'categories',
//     loadComponent: () => import('./Components/categories/list/category-list.component').then(m => m.CategoryListComponent),
//     canActivate: [AuthGuard]
//   },
//   // {
//   //   path: 'users',
//   //   loadComponent: () => import('./Components/users/user-list.component').then(m => m.UserListComponent),
//   //   canActivate: [import('./guards/auth.guard').then(m => m.AuthGuard)]
//   // },
// {
//   path: 'unauthorized',
//   loadComponent: () => import('./Components/errors/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent)
// },
//   // {
//   //   path: '**',
//   //   loadComponent: () => import('./Components/errors/not-found.component').then(m => m.NotFoundComponent)
//   // },
//   {
//     path: '',
//     redirectTo: 'login',
//     pathMatch: 'full'
//   }
// ];
