import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';
import { UserRole } from '../../core/models/user.model';
import { AdminLayoutComponent } from './admin-layout.component';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    canActivate: [authGuard, roleGuard([UserRole.Admin])],
    children: [
      { path: '', loadComponent: () => import('./admin-dashboard-home.component').then(m => m.AdminDashboardHomeComponent) },
      { path: 'teachers', loadComponent: () => import('./pages/teachers/admin-teachers.component').then(m => m.AdminTeachersComponent) },
      { path: 'parents', loadComponent: () => import('./pages/parents/admin-parents.component').then(m => m.AdminParentsComponent) },
      { path: 'students', loadComponent: () => import('./pages/students/admin-students.component').then(m => m.AdminStudentsComponent) },
      { path: 'contracts', loadComponent: () => import('./pages/contracts/admin-contracts.component').then(m => m.AdminContractsComponent) },
      { path: 'group-classes', loadComponent: () => import('./pages/group-classes/admin-group-classes.component').then(m => m.AdminGroupClassesComponent) },
      { path: 'attendance', loadComponent: () => import('./pages/attendance/admin-attendance.component').then(m => m.AdminAttendanceComponent) },
      { path: 'payments', loadComponent: () => import('./pages/payments/admin-payments.component').then(m => m.AdminPaymentsComponent) },
      { path: 'reports', loadComponent: () => import('./pages/reports/admin-reports.component').then(m => m.AdminReportsComponent) },
      {
        path: 'settings',
        loadComponent: () => import('./pages/settings/admin-settings-layout.component').then(m => m.AdminSettingsLayoutComponent),
        children: [
          { path: '', redirectTo: 'holidays', pathMatch: 'full' },
          { path: 'holidays', loadComponent: () => import('./pages/settings/admin-settings-holidays.component').then(m => m.AdminSettingsHolidaysComponent) },
          { path: 'class-prices', loadComponent: () => import('./pages/settings/admin-settings-class-prices.component').then(m => m.AdminSettingsClassPricesComponent) }
        ]
      }
    ]
  }
];
