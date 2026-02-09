import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { NotificationBellComponent } from '../../shared/components/notifications/notification-bell.component';
import { AppBreadcrumbComponent } from '../../shared/components/breadcrumb/app-breadcrumb.component';

@Component({
  selector: 'app-parent-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, NotificationBellComponent, AppBreadcrumbComponent],
  templateUrl: './parent-layout.component.html',
  styleUrl: './parent-layout.component.css'
})
export class ParentLayoutComponent {
  sidebarOpen = true;

  readonly parentSegmentLabels: Record<string, string> = {
    student: 'Learning overview'
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
