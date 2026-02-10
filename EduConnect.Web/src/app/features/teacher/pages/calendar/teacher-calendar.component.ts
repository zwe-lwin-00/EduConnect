import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { TeacherService } from '../../../../core/services/teacher.service';
import { WeekSessionDto } from '../../../../core/models/teacher.model';
import { MessageService } from 'primeng/api';

const DAY_LABELS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

export interface CalendarDay {
  date: Date | null;
  dayOfMonth: number;
  isCurrentMonth: boolean;
  isToday: boolean;
}

@Component({
  selector: 'app-teacher-calendar',
  standalone: true,
  imports: [CommonModule, ButtonModule, CardModule, MessageModule],
  templateUrl: './teacher-calendar.component.html',
  styleUrl: './teacher-calendar.component.css'
})
export class TeacherCalendarComponent implements OnInit {
  currentMonth: Date = new Date();
  sessions: WeekSessionDto[] = [];
  holidays: { id: number; date: string; name: string; description?: string | null }[] = [];
  loading = true;
  error = '';

  constructor(
    private teacherService: TeacherService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  get monthLabel(): string {
    return this.currentMonth.toLocaleDateString(undefined, { month: 'long', year: 'numeric' });
  }

  get year(): number {
    return this.currentMonth.getFullYear();
  }

  get month(): number {
    return this.currentMonth.getMonth() + 1;
  }

  /** Build 6 rows × 7 days (Monday first). Each cell is either a day in the month or a padding day from prev/next month. */
  get calendarWeeks(): CalendarDay[][] {
    const y = this.currentMonth.getFullYear();
    const m = this.currentMonth.getMonth();
    const first = new Date(y, m, 1);
    const daysInMonth = new Date(y, m + 1, 0).getDate();
    const startWeekday = first.getDay();
    const mondayFirst = startWeekday === 0 ? 6 : startWeekday - 1;
    const weeks: CalendarDay[][] = [];
    let week: CalendarDay[] = [];
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    for (let i = 0; i < mondayFirst; i++) {
      const d = new Date(y, m, 1 - (mondayFirst - i));
      week.push({
        date: d,
        dayOfMonth: d.getDate(),
        isCurrentMonth: false,
        isToday: this.sameDay(d, today)
      });
    }
    for (let day = 1; day <= daysInMonth; day++) {
      const d = new Date(y, m, day);
      week.push({
        date: d,
        dayOfMonth: day,
        isCurrentMonth: true,
        isToday: this.sameDay(d, today)
      });
      if (week.length === 7) {
        weeks.push(week);
        week = [];
      }
    }
    if (week.length > 0) {
      let nextDay = new Date(y, m, daysInMonth);
      nextDay.setDate(nextDay.getDate() + 1);
      while (week.length < 7) {
        week.push({
          date: nextDay,
          dayOfMonth: nextDay.getDate(),
          isCurrentMonth: false,
          isToday: this.sameDay(nextDay, today)
        });
        nextDay = new Date(nextDay);
        nextDay.setDate(nextDay.getDate() + 1);
      }
      weeks.push(week);
    }
    return weeks;
  }

  get dayHeaders(): string[] {
    return DAY_LABELS;
  }

  sessionsOnDay(date: Date): WeekSessionDto[] {
    const ymd = this.formatYmd(date);
    return this.sessions.filter(s => {
      if (s.dateYmd) return s.dateYmd === ymd;
      if (s.date == null) return false;
      const d = typeof s.date === 'string'
        ? s.date.slice(0, 10)
        : (s.date as unknown as Date)?.toISOString?.()?.slice(0, 10) ?? '';
      return d === ymd;
    });
  }

  holidayOnDay(date: Date): { name: string; description?: string | null } | null {
    const ymd = this.formatYmd(date);
    const h = this.holidays.find(x => x.date.slice(0, 10) === ymd);
    return h ? { name: h.name, description: h.description } : null;
  }

  isCompleted(s: WeekSessionDto): boolean {
    const status = (s.status || '').toLowerCase();
    if (s.groupSessionId) return status === 'completed' || status === 'checkedout';
    return status === 'completed' || status === 'checkedout' || (s.attendanceLogId > 0 && status !== 'scheduled');
  }

  isUpcoming(s: WeekSessionDto): boolean {
    return (s.status || '').toLowerCase() === 'scheduled' || (s.attendanceLogId === 0 && !s.groupSessionId);
  }

  prevMonth(): void {
    this.currentMonth = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() - 1, 1);
    this.load();
  }

  nextMonth(): void {
    this.currentMonth = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() + 1, 1);
    this.load();
  }

  thisMonth(): void {
    this.currentMonth = new Date();
    this.load();
  }

  private load(): void {
    this.loading = true;
    this.error = '';
    const y = this.year;
    const m = this.month;
    this.teacherService.getCalendarMonth(y, m).subscribe({
      next: (list) => {
        this.sessions = list;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load';
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
    this.teacherService.getHolidays(y).subscribe({
      next: (list) => (this.holidays = list),
      error: () => {}
    });
  }

  private sameDay(a: Date, b: Date): boolean {
    return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
  }

  private formatYmd(d: Date): string {
    const y = d.getFullYear();
    const mo = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${mo}-${day}`;
  }
}
