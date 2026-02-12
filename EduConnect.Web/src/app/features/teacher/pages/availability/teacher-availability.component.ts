import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TeacherService } from '../../../../core/services/teacher.service';
import { TeacherAvailabilityDto } from '../../../../core/models/teacher.model';
import { MessageService } from 'primeng/api';

const DAYS = [
  { value: 0, label: 'Sunday' },
  { value: 1, label: 'Monday' },
  { value: 2, label: 'Tuesday' },
  { value: 3, label: 'Wednesday' },
  { value: 4, label: 'Thursday' },
  { value: 5, label: 'Friday' },
  { value: 6, label: 'Saturday' }
];

export interface AvailabilityRow extends TeacherAvailabilityDto {
  startTimeModel?: Date;
  endTimeModel?: Date;
}

@Component({
  selector: 'app-teacher-availability',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, CardModule, MessageModule, InputTextModule, TableModule, CalendarModule, DropdownModule, CheckboxModule, ProgressSpinnerModule],
  templateUrl: './teacher-availability.component.html',
  styleUrl: './teacher-availability.component.css'
})
export class TeacherAvailabilityComponent implements OnInit {
  availabilities: AvailabilityRow[] = [];
  loading = true;
  saving = false;
  error = '';
  days = DAYS;

  static strToTime(s: string): Date {
    if (!s?.trim()) return new Date(0, 0, 0, 9, 0);
    const [h, m] = s.trim().split(':').map(Number);
    const d = new Date();
    d.setHours(isNaN(h) ? 9 : h, isNaN(m) ? 0 : m, 0, 0);
    return d;
  }
  static timeToStr(d: Date | undefined): string {
    if (!d || !(d instanceof Date)) return '09:00';
    return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
  }

  constructor(
    private teacherService: TeacherService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.teacherService.getAvailability().subscribe({
      next: (data) => {
        this.availabilities = data.length
          ? data.map(a => ({
              ...a,
              startTime: this.toTimeString(a.startTime),
              endTime: this.toTimeString(a.endTime),
              startTimeModel: TeacherAvailabilityComponent.strToTime(a.startTime),
              endTimeModel: TeacherAvailabilityComponent.strToTime(a.endTime)
            }))
          : this.getDefaultSlots();
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load';
        this.availabilities = this.getDefaultSlots();
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
  }

  getDefaultSlots(): AvailabilityRow[] {
    return DAYS.map(d => ({
      dayOfWeek: d.value,
      startTime: '09:00',
      endTime: '17:00',
      isAvailable: true,
      startTimeModel: TeacherAvailabilityComponent.strToTime('09:00'),
      endTimeModel: TeacherAvailabilityComponent.strToTime('17:00')
    }));
  }

  addSlot(): void {
    this.availabilities.push({
      dayOfWeek: 1,
      startTime: '09:00',
      endTime: '17:00',
      isAvailable: true,
      startTimeModel: TeacherAvailabilityComponent.strToTime('09:00'),
      endTimeModel: TeacherAvailabilityComponent.strToTime('17:00')
    });
  }

  removeSlot(index: number): void {
    this.availabilities.splice(index, 1);
  }

  save(): void {
    const toSend = this.availabilities
      .filter(a => a.isAvailable)
      .map(a => ({
        dayOfWeek: a.dayOfWeek,
        startTime: TeacherAvailabilityComponent.timeToStr(a.startTimeModel) || a.startTime,
        endTime: TeacherAvailabilityComponent.timeToStr(a.endTimeModel) || a.endTime,
        isAvailable: true
      }));
    this.saving = true;
    this.error = '';
    this.teacherService.updateAvailability(toSend).subscribe({
      next: () => {
        this.saving = false;
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Availability saved.' });
        this.load();
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to save';
        this.saving = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
  }

  getDayLabel(dayOfWeek: number): string {
    return DAYS.find(d => d.value === dayOfWeek)?.label ?? '';
  }

  toTimeString(t: string): string {
    if (!t) return '09:00';
    if (t.length >= 5) return t.slice(0, 5);
    return t;
  }
}
