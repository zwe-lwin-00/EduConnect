import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../core/services/auth.service';
import { NotificationBellComponent } from '../../shared/components/notifications/notification-bell.component';
import { AppBreadcrumbComponent } from '../../shared/components/breadcrumb/app-breadcrumb.component';

@Component({
  selector: 'app-teacher-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, ToolbarModule, ButtonModule, NotificationBellComponent, AppBreadcrumbComponent],
  templateUrl: './teacher-layout.component.html',
  styleUrl: './teacher-layout.component.css'
})
export class TeacherLayoutComponent {
  sidebarOpen = true;

  readonly teacherSegmentLabels: Record<string, string> = {
    availability: 'Availability',
    students: 'Students',
    sessions: 'Sessions',
    'group-classes': 'Group',
    calendar: 'Calendar',
    'homework-grades': 'Homework & Grades',
    profile: 'Profile'
  };

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
  }

  closeSidebar(): void {
    this.sidebarOpen = false;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}
