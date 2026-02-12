import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumberModule } from 'primeng/inputnumber';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AdminService } from '../../../../core/services/admin.service';
import { DailyReportDto, MonthlyReportDto } from '../../../../core/models/admin.model';
import { appConfig } from '../../../../core/constants/app-config';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-admin-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, TableModule, InputTextModule, CalendarModule, DropdownModule, InputNumberModule, ProgressSpinnerModule],
  templateUrl: './admin-reports.component.html',
  styleUrl: './admin-reports.component.css'
})
export class AdminReportsComponent implements OnInit {
  dailyReport: DailyReportDto | null = null;
  monthlyReport: MonthlyReportDto | null = null;
  /** Today in app timezone as YYYY-MM-DD */
  dailyDate = new Date().toLocaleDateString('en-CA', { timeZone: appConfig.timeZone });
  /** For p-calendar binding */
  get dailyDateModel(): Date {
    const [y, m, d] = this.dailyDate.split('-').map(Number);
    return new Date(y, (m ?? 1) - 1, d ?? 1);
  }
  set dailyDateModel(v: Date) {
    if (v && v instanceof Date) {
      this.dailyDate = `${v.getFullYear()}-${String(v.getMonth() + 1).padStart(2, '0')}-${String(v.getDate()).padStart(2, '0')}`;
    }
  }
  reportYear = new Date().getFullYear();
  reportMonth = new Date().getMonth() + 1;
  loadingDaily = false;
  loadingMonthly = false;

  get monthOptions(): { label: string; value: number }[] {
    return [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12].map(m => ({ label: this.monthName(m), value: m }));
  }

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
