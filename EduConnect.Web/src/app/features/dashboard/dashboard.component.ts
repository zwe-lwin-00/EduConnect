import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  template: `
    <div class="dashboard">
      <h1>Welcome, {{ authService.currentUser()?.fullName }}!</h1>
      <p>Role: {{ getRoleName(authService.currentUser()?.role) }}</p>
      <router-outlet></router-outlet>
    </div>
  `,
  styles: [`
    .dashboard {
      padding: 2rem;
    }
  `]
})
export class DashboardComponent {
  constructor(public authService: AuthService) {}

  getRoleName(role?: number | string): string {
    if (typeof role === 'string') {
      return role;
    }
    switch (role) {
      case 1: return 'Admin';
      case 2: return 'Teacher';
      case 3: return 'Parent';
      default: return 'Unknown';
    }
  }
}
