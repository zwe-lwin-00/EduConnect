import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { UserRole } from './core/models/user.model';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: 'admin',
    loadComponent: () => import('./features/admin/pages/dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent),
    canActivate: [authGuard, roleGuard([UserRole.Admin])]
  },
  {
    path: 'teacher',
    loadComponent: () => import('./features/teacher/pages/dashboard/teacher-dashboard.component').then(m => m.TeacherDashboardComponent),
    canActivate: [authGuard, roleGuard([UserRole.Teacher])]
  },
  {
    path: 'parent',
    loadComponent: () => import('./features/parent/pages/dashboard/parent-dashboard.component').then(m => m.ParentDashboardComponent),
    canActivate: [authGuard, roleGuard([UserRole.Parent])]
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [authGuard]
  },
  {
    path: '',
    redirectTo: '/auth/login',
    pathMatch: 'full'
  },
  {
    path: '**',
    redirectTo: '/auth/login'
  }
];
