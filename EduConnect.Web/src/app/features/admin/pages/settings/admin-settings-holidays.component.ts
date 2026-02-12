import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { AdminService } from '../../../../core/services/admin.service';
import { HolidayDto, CreateHolidayRequest, UpdateHolidayRequest } from '../../../../core/models/admin.model';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-admin-settings-holidays',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ToolbarModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    CardModule,
    CalendarModule,
    DropdownModule
  ],
  templateUrl: './admin-settings-holidays.component.html',
  styleUrl: './admin-settings.component.css'
})
export class AdminSettingsHolidaysComponent implements OnInit {
  holidays: HolidayDto[] = [];
  loadingHolidays = false;
  holidayYear: number | null = new Date().getFullYear();
  readonly holidayYears = [2024, 2025, 2026, 2027];

  showHolidayDialog = false;
  holidayId: number | null = null;
  holidayDate: Date | null = null;
  holidayName = '';
  holidayDescription = '';
  savingHoliday = false;

  get holidayYearOptions(): { label: string; value: number | null }[] {
    return [{ label: 'All', value: null }, ...this.holidayYears.map(y => ({ label: String(y), value: y }))];
  }

  constructor(
    private adminService: AdminService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadHolidays();
  }

  loadHolidays(): void {
    this.loadingHolidays = true;
    this.adminService.getHolidays(this.holidayYear ?? undefined).subscribe({
      next: (data) => {
        this.holidays = data;
        this.loadingHolidays = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to load holidays' });
        this.loadingHolidays = false;
      }
    });
  }

  onYearChange(): void {
    this.loadHolidays();
  }

  openAddHoliday(): void {
    this.holidayId = null;
    this.holidayDate = new Date();
    this.holidayName = '';
    this.holidayDescription = '';
    this.showHolidayDialog = true;
  }

  openEditHoliday(h: HolidayDto): void {
    this.holidayId = h.id;
    this.holidayDate = new Date(h.date.slice(0, 10));
    this.holidayName = h.name;
    this.holidayDescription = h.description ?? '';
    this.showHolidayDialog = true;
  }

  getHolidayDateStr(): string {
    return this.holidayDate instanceof Date ? this.holidayDate.toISOString().slice(0, 10) : '';
  }

  saveHoliday(): void {
    const name = this.holidayName?.trim();
    if (!name) {
      this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Name is required' });
      return;
    }
    const dateStr = this.getHolidayDateStr();
    if (!dateStr) {
      this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Date is required' });
      return;
    }
    this.savingHoliday = true;
    if (this.holidayId != null) {
      const request: UpdateHolidayRequest = { date: dateStr, name, description: this.holidayDescription?.trim() || null };
      this.adminService.updateHoliday(this.holidayId, request).subscribe({
        next: () => {
          this.savingHoliday = false;
          this.showHolidayDialog = false;
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Holiday updated' });
          this.loadHolidays();
        },
        error: (err) => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Update failed' });
          this.savingHoliday = false;
        }
      });
    } else {
      const request: CreateHolidayRequest = { date: dateStr, name, description: this.holidayDescription?.trim() || null };
      this.adminService.createHoliday(request).subscribe({
        next: () => {
          this.savingHoliday = false;
          this.showHolidayDialog = false;
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Holiday added' });
          this.loadHolidays();
        },
        error: (err) => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Create failed' });
          this.savingHoliday = false;
        }
      });
    }
  }

  deleteHoliday(h: HolidayDto): void {
    this.confirmationService.confirm({
      message: `Delete holiday "${h.name}" (${h.date.slice(0, 10)})?`,
      header: 'Delete holiday',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.adminService.deleteHoliday(h.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Holiday deleted' });
            this.loadHolidays();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Delete failed' })
        });
      }
    });
  }

  formatDate(d: string): string {
    return d.slice(0, 10);
  }
}
