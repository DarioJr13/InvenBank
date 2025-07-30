import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { LayoutComponent } from './components/layout/layout.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ProductListComponent } from './components/products/product-list.component';
import { ProductFormComponent } from './components/products/product-form.component';
import { SupplierListComponent } from './components/suppliers/supplier-list.component';
import { CategoryListComponent } from './components/categories/category-list.component';
import { UserListComponent } from './components/users/user-list.component';
import { AuthGuard } from './guards/auth.guard';
import { AdminGuard } from './guards/admin.guard';
import { NoAuthGuard } from './guards/no-auth.guard';

const routes: Routes = [
  // Ruta por defecto - redirigir al dashboard
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },

  // Login - solo accesible si NO está autenticado
  {
    path: 'login',
    component: LoginComponent,
    canActivate: [NoAuthGuard]
  },

  // Rutas protegidas con layout
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard, AdminGuard],
    children: [
      // Dashboard
      {
        path: 'dashboard',
        component: DashboardComponent
      },

      // Productos
      {
        path: 'products',
        children: [
          { path: '', component: ProductListComponent },
          { path: 'new', component: ProductFormComponent },
          { path: 'edit/:id', component: ProductFormComponent },
          { path: 'view/:id', component: ProductFormComponent } // Reutilizar form en modo solo lectura
        ]
      },

      // Proveedores
      {
        path: 'suppliers',
        children: [
          { path: '', component: SupplierListComponent },
          // Agregar más rutas según necesidad
        ]
      },

      // Categorías
      {
        path: 'categories',
        children: [
          { path: '', component: CategoryListComponent },
          // Agregar más rutas según necesidad
        ]
      },

      // Usuarios
      {
        path: 'users',
        children: [
          { path: '', component: UserListComponent },
          // Agregar más rutas según necesidad
        ]
      }
    ]
  },

  // Página de no autorizado
  {
    path: 'unauthorized',
    template: `
      <div style="display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100vh; text-align: center;">
        <h1>Acceso Denegado</h1>
        <p>No tienes permisos para acceder a esta página.</p>
        <a routerLink="/login" style="color: #c2185b;">Volver al Login</a>
      </div>
    `
  },

  // Ruta wildcard - página no encontrada
  {
    path: '**',
    template: `
      <div style="display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100vh; text-align: center;">
        <h1>Página No Encontrada</h1>
        <p>La página que buscas no existe.</p>
        <a routerLink="/dashboard" style="color: #c2185b;">Ir al Dashboard</a>
      </div>
    `
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {
    enableTracing: false, // Cambiar a true para debug
    scrollPositionRestoration: 'top'
  })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
