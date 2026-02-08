import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';
import { UserRole } from '../../core/models/user.model';
import { TeacherLayoutComponent } from './teacher-layout.component';

export const TEACHER_ROUTES: Routes = [
  {
    path: '',
    component: TeacherLayoutComponent,
    canActivate: [authGuard, roleGuard([UserRole.Teacher])],
    children: [
      { path: '', loadComponent: () => import('./pages/dashboard/teacher-dashboard.component').then(m => m.TeacherDashboardComponent) },
      { path: 'availability', loadComponent: () => import('./pages/availability/teacher-availability.component').then(m => m.TeacherAvailabilityComponent) },
      { path: 'students', loadComponent: () => import('./pages/students/teacher-students.component').then(m => m.TeacherStudentsComponent) },
      { path: 'sessions', loadComponent: () => import('./pages/sessions/teacher-sessions.component').then(m => m.TeacherSessionsComponent) },
      { path: 'homework-grades', loadComponent: () => import('./pages/homework-grades/teacher-homework-grades.component').then(m => m.TeacherHomeworkGradesComponent) },
      { path: 'profile', loadComponent: () => import('./pages/profile/teacher-profile.component').then(m => m.TeacherProfileComponent) }
    ]
  }
];
