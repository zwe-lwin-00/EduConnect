import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { MessageModule } from 'primeng/message';
import { TeacherService } from '../../../../core/services/teacher.service';
import { TeacherDashboardDto } from '../../../../core/models/teacher.model';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, CardModule, TagModule, ButtonModule, SkeletonModule, MessageModule],
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

  alertSeverity(type: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    if (type === 'InProgress') return 'info';
    if (type === 'MissingNotes') return 'warn';
    return 'secondary';
  }
}
