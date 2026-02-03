import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../../core/services/admin.service';
import { DailyReportDto, MonthlyReportDto } from '../../../../core/models/admin.model';

@Component({
  selector: 'app-admin-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-reports.component.html',
  styleUrl: './admin-reports.component.css'
})
export class AdminReportsComponent implements OnInit {
  dailyReport: DailyReportDto | null = null;
  monthlyReport: MonthlyReportDto | null = null;
  dailyDate = new Date().toISOString().slice(0, 10);
  reportYear = new Date().getFullYear();
  reportMonth = new Date().getMonth() + 1;
  loadingDaily = false;
  loadingMonthly = false;

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadDaily();
    this.loadMonthly();
  }

  loadDaily(): void {
    this.loadingDaily = true;
    this.adminService.getDailyReport(this.dailyDate).subscribe({
      next: (data) => { this.dailyReport = data; this.loadingDaily = false; },
      error: () => { this.loadingDaily = false; }
    });
  }

  loadMonthly(): void {
    this.loadingMonthly = true;
    this.adminService.getMonthlyReport(this.reportYear, this.reportMonth).subscribe({
      next: (data) => { this.monthlyReport = data; this.loadingMonthly = false; },
      error: () => { this.loadingMonthly = false; }
    });
  }

  onDailyDateChange(): void {
    this.loadDaily();
  }

  onMonthChange(): void {
    this.loadMonthly();
  }

  monthName(m: number): string {
    const names = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
    return names[m - 1] ?? '';
  }
}
