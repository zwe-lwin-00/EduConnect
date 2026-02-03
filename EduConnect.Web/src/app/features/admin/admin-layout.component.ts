import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.css'
})
export class AdminLayoutComponent {
  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

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
