import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-parent-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './parent-layout.component.html',
  styleUrl: './parent-layout.component.css'
})
export class ParentLayoutComponent {
  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}
