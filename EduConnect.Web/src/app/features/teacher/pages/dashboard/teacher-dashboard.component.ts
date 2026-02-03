import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TeacherService } from '../../../../core/services/teacher.service';
import { TeacherDashboardDto } from '../../../../core/models/teacher.model';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './teacher-dashboard.component.html',
  styleUrl: './teacher-dashboard.component.css'
})
export class TeacherDashboardComponent implements OnInit {
  dashboard: TeacherDashboardDto | null = null;
  loading = true;
  error = '';

  constructor(private teacherService: TeacherService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.teacherService.getDashboard().subscribe({
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
    if (type === 'InProgress') return 'alert-info';
    if (type === 'MissingNotes') return 'alert-warning';
    return 'alert-default';
  }
}
