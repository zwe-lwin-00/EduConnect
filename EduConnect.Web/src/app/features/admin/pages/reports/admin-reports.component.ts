import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { AdminService } from '../../../../core/services/admin.service';
import { DailyReportDto, MonthlyReportDto } from '../../../../core/models/admin.model';
import { appConfig } from '../../../../core/constants/app-config';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-admin-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, TableModule, InputTextModule],
  templateUrl: './admin-reports.component.html',
  styleUrl: './admin-reports.component.css'
})
export class AdminReportsComponent implements OnInit {
  dailyReport: DailyReportDto | null = null;
  monthlyReport: MonthlyReportDto | null = null;
  /** Today in app timezone as YYYY-MM-DD (from environment.timeZone) */
  dailyDate = new Date().toLocaleDateString('en-CA', { timeZone: appConfig.timeZone });
  reportYear = new Date().getFullYear();
  reportMonth = new Date().getMonth() + 1;
  loadingDaily = false;
  loadingMonthly = false;

  constructor(
    private adminService: AdminService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadDaily();
    this.loadMonthly();
  }

  loadDaily(): void {
    this.loadingDaily = true;
    this.adminService.getDailyReport(this.dailyDate).subscribe({
      next: (data) => { this.dailyReport = data; this.loadingDaily = false; },
      error: (err) => {
        this.loadingDaily = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message || 'Failed to load daily report.' });
      }
    });
  }

  loadMonthly(): void {
    this.loadingMonthly = true;
    this.adminService.getMonthlyReport(this.reportYear, this.reportMonth).subscribe({
      next: (data) => { this.monthlyReport = data; this.loadingMonthly = false; },
      error: (err) => {
        this.loadingMonthly = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message || 'Failed to load monthly report.' });
      }
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
