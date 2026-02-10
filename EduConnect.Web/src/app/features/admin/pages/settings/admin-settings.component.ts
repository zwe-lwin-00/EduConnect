import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import { AdminService } from '../../../../core/services/admin.service';
import {
  HolidayDto,
  CreateHolidayRequest,
  UpdateHolidayRequest,
  SystemSettingDto,
  UpsertSystemSettingRequest
} from '../../../../core/models/admin.model';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ToolbarModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    CardModule
  ],
  templateUrl: './admin-settings.component.html',
  styleUrl: './admin-settings.component.css'
})
export class AdminSettingsComponent implements OnInit {
  holidays: HolidayDto[] = [];
  systemSettings: SystemSettingDto[] = [];
  loadingHolidays = false;
  loadingSettings = false;
  holidayYear: number | null = new Date().getFullYear();
  readonly holidayYears = [2024, 2025, 2026, 2027];

  showHolidayDialog = false;
  holidayId: number | null = null;
  holidayDate = '';
  holidayName = '';
  holidayDescription = '';
  savingHoliday = false;

  showSettingDialog = false;
  settingKey = '';
  settingValue = '';
  settingDescription = '';
  editingSettingKey = '';
  savingSetting = false;

  constructor(
    private adminService: AdminService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadHolidays();
    this.loadSystemSettings();
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

  loadSystemSettings(): void {
    this.loadingSettings = true;
    this.adminService.getSystemSettings().subscribe({
      next: (data) => {
        this.systemSettings = data;
        this.loadingSettings = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to load settings' });
        this.loadingSettings = false;
      }
    });
  }

  onYearChange(): void {
    this.loadHolidays();
  }

  openAddHoliday(): void {
    this.holidayId = null;
    this.holidayDate = new Date().toISOString().slice(0, 10);
    this.holidayName = '';
    this.holidayDescription = '';
    this.showHolidayDialog = true;
  }

  openEditHoliday(h: HolidayDto): void {
    this.holidayId = h.id;
    this.holidayDate = h.date.slice(0, 10);
    this.holidayName = h.name;
    this.holidayDescription = h.description ?? '';
    this.showHolidayDialog = true;
  }

  saveHoliday(): void {
    const name = this.holidayName?.trim();
    if (!name) {
      this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Name is required' });
      return;
    }
    this.savingHoliday = true;
    if (this.holidayId != null) {
      const request: UpdateHolidayRequest = { date: this.holidayDate, name, description: this.holidayDescription?.trim() || null };
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
      const request: CreateHolidayRequest = { date: this.holidayDate, name, description: this.holidayDescription?.trim() || null };
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

  openAddSetting(): void {
    this.editingSettingKey = '';
    this.settingKey = '';
    this.settingValue = '';
    this.settingDescription = '';
    this.showSettingDialog = true;
  }

  openEditSetting(s: SystemSettingDto): void {
    this.editingSettingKey = s.key;
    this.settingKey = s.key;
    this.settingValue = s.value;
    this.settingDescription = s.description ?? '';
    this.showSettingDialog = true;
  }

  saveSetting(): void {
    const key = this.settingKey?.trim();
    if (!key) {
      this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Key is required' });
      return;
    }
    this.savingSetting = true;
    const request: UpsertSystemSettingRequest = {
      key,
      value: this.settingValue ?? '',
      description: this.settingDescription?.trim() || null
    };
    this.adminService.upsertSystemSetting(request).subscribe({
      next: () => {
        this.savingSetting = false;
        this.showSettingDialog = false;
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Setting saved' });
        this.loadSystemSettings();
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Save failed' });
        this.savingSetting = false;
      }
    });
  }

  formatDate(d: string): string {
    return d.slice(0, 10);
  }
}
