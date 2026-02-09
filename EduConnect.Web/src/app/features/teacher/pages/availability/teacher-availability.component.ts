import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TeacherService } from '../../../../core/services/teacher.service';
import { TeacherAvailabilityDto } from '../../../../core/models/teacher.model';

const DAYS = [
  { value: 0, label: 'Sunday' },
  { value: 1, label: 'Monday' },
  { value: 2, label: 'Tuesday' },
  { value: 3, label: 'Wednesday' },
  { value: 4, label: 'Thursday' },
  { value: 5, label: 'Friday' },
  { value: 6, label: 'Saturday' }
];

@Component({
  selector: 'app-teacher-availability',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule],
  templateUrl: './teacher-availability.component.html',
  styleUrl: './teacher-availability.component.css'
})
export class TeacherAvailabilityComponent implements OnInit {
  availabilities: TeacherAvailabilityDto[] = [];
  loading = true;
  saving = false;
  error = '';
  days = DAYS;

  constructor(private teacherService: TeacherService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.teacherService.getAvailability().subscribe({
      next: (data) => {
        this.availabilities = data.length
          ? data.map(a => ({ ...a, startTime: this.toTimeString(a.startTime), endTime: this.toTimeString(a.endTime) }))
          : this.getDefaultSlots();
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load';
        this.availabilities = this.getDefaultSlots();
        this.loading = false;
      }
    });
  }

  getDefaultSlots(): TeacherAvailabilityDto[] {
    return DAYS.map(d => ({
      dayOfWeek: d.value,
      startTime: '09:00',
      endTime: '17:00',
      isAvailable: true
    }));
  }

  addSlot(): void {
    this.availabilities.push({
      dayOfWeek: 1,
      startTime: '09:00',
      endTime: '17:00',
      isAvailable: true
    });
  }

  removeSlot(index: number): void {
    this.availabilities.splice(index, 1);
  }

  save(): void {
    const toSend = this.availabilities
      .filter(a => a.isAvailable)
      .map(a => ({ dayOfWeek: a.dayOfWeek, startTime: a.startTime, endTime: a.endTime, isAvailable: true }));
    this.saving = true;
    this.error = '';
    this.teacherService.updateAvailability(toSend).subscribe({
      next: () => {
        this.saving = false;
        alert('Availability saved.');
        this.load();
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to save';
        this.saving = false;
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
