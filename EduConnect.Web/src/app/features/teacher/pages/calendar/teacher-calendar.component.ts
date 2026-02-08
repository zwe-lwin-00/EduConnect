import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TeacherService } from '../../../../core/services/teacher.service';
import { WeekSessionDto } from '../../../../core/models/teacher.model';

const DAY_LABELS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

@Component({
  selector: 'app-teacher-calendar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './teacher-calendar.component.html',
  styleUrl: './teacher-calendar.component.css'
})
export class TeacherCalendarComponent implements OnInit {
  weekStart: Date = this.getMonday(new Date());
  sessions: WeekSessionDto[] = [];
  loading = true;
  error = '';

  constructor(private teacherService: TeacherService) {}

  ngOnInit(): void {
    this.load();
  }

  get weekStartStr(): string {
    return this.formatYmd(this.weekStart);
  }

  get weekLabel(): string {
    const end = new Date(this.weekStart);
    end.setDate(end.getDate() + 6);
    return `${this.formatShort(this.weekStart)} – ${this.formatShort(end)}`;
  }

  get dayColumns(): { date: Date; label: string; isToday: boolean }[] {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return Array.from({ length: 7 }, (_, i) => {
      const d = new Date(this.weekStart);
      d.setDate(d.getDate() + i);
      const isToday = d.getTime() === today.getTime();
      return { date: d, label: DAY_LABELS[i], isToday };
    });
  }

  sessionsOnDay(date: Date): WeekSessionDto[] {
    const ymd = this.formatYmd(date);
    return this.sessions.filter(s => {
      const d = s.date ? (typeof s.date === 'string' ? s.date.slice(0, 10) : '') : '';
      return d === ymd;
    });
  }

  prevWeek(): void {
    const d = new Date(this.weekStart);
    d.setDate(d.getDate() - 7);
    this.weekStart = d;
    this.load();
  }

  nextWeek(): void {
    const d = new Date(this.weekStart);
    d.setDate(d.getDate() + 7);
    this.weekStart = d;
    this.load();
  }

  thisWeek(): void {
    this.weekStart = this.getMonday(new Date());
    this.load();
  }

  private load(): void {
    this.loading = true;
    this.error = '';
    this.teacherService.getCalendarWeek(this.weekStartStr).subscribe({
      next: (list) => {
        this.sessions = list;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load';
        this.loading = false;
      }
    });
  }

  private getMonday(d: Date): Date {
    const x = new Date(d);
    x.setHours(0, 0, 0, 0);
    const day = x.getDay();
    const diff = (day === 0 ? -6 : 1) - day;
    x.setDate(x.getDate() + diff);
    return x;
  }

  private formatYmd(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  private formatShort(d: Date): string {
    return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
  }
}
