import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../core/services/auth.service';
import { NotificationBellComponent } from '../../shared/components/notifications/notification-bell.component';
import { AppBreadcrumbComponent } from '../../shared/components/breadcrumb/app-breadcrumb.component';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, ToolbarModule, ButtonModule, NotificationBellComponent, AppBreadcrumbComponent],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.css'
})
export class AdminLayoutComponent {
  sidebarOpen = true;

  readonly adminSegmentLabels: Record<string, string> = {
    teachers: 'Teachers',
    parents: 'Parents',
    students: 'Students',
    contracts: 'Contracts',
    'group-classes': 'Group classes',
    attendance: 'Attendance',
    payments: 'Payments',
    reports: 'Reports'
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

  isActive(path: string): boolean {
    const url = this.router.url;
    if (path === '' || path === 'dashboard') return url === '/admin' || url === '/admin/';
    return url.startsWith('/admin/' + path);
  }
}
