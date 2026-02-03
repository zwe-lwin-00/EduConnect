import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminService } from '../../core/services/admin.service';
import { DashboardDto } from '../../core/models/admin.model';

/**
 * Admin Dashboard (Home) — Master Doc B1.
 * Daily command center: Alerts, Today's Sessions, Pending Actions, Revenue Snapshot.
 */
@Component({
  selector: 'app-admin-dashboard-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-dashboard-home.component.html',
  styleUrl: './admin-dashboard-home.component.css'
})
export class AdminDashboardHomeComponent implements OnInit {
  dashboard: DashboardDto | null = null;
  loading = true;
  error = '';

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.adminService.getDashboard().subscribe({
      next: (data) => {
        this.dashboard = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load dashboard';
        this.loading = false;
      }
    });
  }

  alertClass(type: string): string {
    if (type === 'LowHours') return 'alert-warning';
    if (type === 'NoCheckIn') return 'alert-danger';
    if (type === 'ContractExpiring') return 'alert-info';
    return 'alert-default';
  }
}
