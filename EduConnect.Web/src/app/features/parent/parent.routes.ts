import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';
import { UserRole } from '../../core/models/user.model';
import { ParentLayoutComponent } from './parent-layout.component';

export const PARENT_ROUTES: Routes = [
  {
    path: '',
    component: ParentLayoutComponent,
    canActivate: [authGuard, roleGuard([UserRole.Parent])],
    children: [
      { path: '', loadComponent: () => import('./pages/dashboard/parent-dashboard.component').then(m => m.ParentDashboardComponent) },
      { path: 'student/:studentId', loadComponent: () => import('./pages/student-learning/parent-student-learning.component').then(m => m.ParentStudentLearningComponent) }
    ]
  }
];
