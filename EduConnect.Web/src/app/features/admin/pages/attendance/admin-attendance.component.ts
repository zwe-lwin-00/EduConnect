import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DxButtonModule } from 'devextreme-angular';
import { AdminService } from '../../../../core/services/admin.service';
import { TodaySessionDto } from '../../../../core/models/admin.model';

@Component({
  selector: 'app-admin-attendance',
  standalone: true,
  imports: [CommonModule, FormsModule, DxButtonModule],
  templateUrl: './admin-attendance.component.html',
  styleUrl: './admin-attendance.component.css'
})
export class AdminAttendanceComponent implements OnInit {
  sessions: TodaySessionDto[] = [];
  loading = true;
  adjustingId: number | null = null;
  adjustHours = 0;
  adjustReason = '';

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.adminService.getTodaySessions().subscribe({
      next: (data) => {
        this.sessions = data;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  overrideCheckIn(s: TodaySessionDto): void {
    if (!confirm(`Override check-in for ${s.teacherName} – ${s.studentName}?`)) return;
    this.adminService.overrideCheckIn(s.id).subscribe({
      next: () => this.load(),
      error: (err) => alert('Error: ' + (err.error?.error || err.message))
    });
  }

  overrideCheckOut(s: TodaySessionDto): void {
    if (!confirm(`Override check-out for ${s.teacherName} – ${s.studentName}?`)) return;
    this.adminService.overrideCheckOut(s.id).subscribe({
      next: () => this.load(),
      error: (err) => alert('Error: ' + (err.error?.error || err.message))
    });
  }

  openAdjust(s: TodaySessionDto): void {
    this.adjustingId = s.id;
    this.adjustHours = 0;
    this.adjustReason = '';
  }

  closeAdjust(): void {
    this.adjustingId = null;
  }

  submitAdjust(): void {
    if (this.adjustingId == null || !this.adjustReason.trim()) return;
    this.adminService.adjustSessionHours(this.adjustingId, { hours: this.adjustHours, reason: this.adjustReason }).subscribe({
      next: () => { this.closeAdjust(); this.load(); },
      error: (err) => alert('Error: ' + (err.error?.error || err.message))
    });
  }
}
