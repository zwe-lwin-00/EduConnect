import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { ButtonModule } from 'primeng/button';
import { ParentService } from '../../../../core/services/parent.service';
import { StudentLearningOverviewDto } from '../../../../core/models/parent.model';
import { WeekSessionDto } from '../../../../core/models/teacher.model';
import { DisplayDatePipe } from '../../../../shared/pipes/display-date.pipe';
import { MessageService } from 'primeng/api';

const DAY_LABELS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

export interface CalendarDay {
  date: Date | null;
  dayOfMonth: number;
  isCurrentMonth: boolean;
  isToday: boolean;
}

@Component({
  selector: 'app-parent-student-learning',
  standalone: true,
  imports: [CommonModule, RouterModule, CardModule, MessageModule, ButtonModule, DisplayDatePipe],
  templateUrl: './parent-student-learning.component.html',
  styleUrl: './parent-student-learning.component.css'
})
export class ParentStudentLearningComponent implements OnInit {
  overview: StudentLearningOverviewDto | null = null;
  studentId: number | null = null;
  loading = true;
  error = '';

  weekStart: Date = this.getMonday(new Date());
  weekSessions: WeekSessionDto[] = [];
  weekLoading = false;

  currentMonth: Date = new Date();
  monthSessions: WeekSessionDto[] = [];
  holidays: { id: number; date: string; name: string; description?: string | null }[] = [];
  monthLoading = false;

  constructor(
    private route: ActivatedRoute,
    private parentService: ParentService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('studentId');
    const parsed = id ? +id : NaN;
    if (typeof parsed === 'number' && !Number.isNaN(parsed) && parsed >= 1 && Number.isInteger(parsed)) {
      this.studentId = parsed;
      this.load();
      this.loadWeek();
      this.loadMonth();
    } else {
      this.studentId = null;
      this.error = 'Invalid student';
      this.loading = false;
    }
  }

  load(): void {
    if (this.studentId == null) return;
    this.loading = true;
    this.error = '';
    this.parentService.getStudentLearningOverview(this.studentId).subscribe({
      next: (data) => {
        this.overview = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load';
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
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
    return this.weekSessions.filter(s => {
      const d = s.date ? (typeof s.date === 'string' ? s.date.slice(0, 10) : '') : '';
      return d === ymd;
    });
  }

  get monthLabel(): string {
    return this.currentMonth.toLocaleDateString(undefined, { month: 'long', year: 'numeric' });
  }

  get monthYear(): number {
    return this.currentMonth.getFullYear();
  }

  get monthNum(): number {
    return this.currentMonth.getMonth() + 1;
  }

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
      week.push({ date: d, dayOfMonth: d.getDate(), isCurrentMonth: false, isToday: this.sameDay(d, today) });
    }
    for (let day = 1; day <= daysInMonth; day++) {
      const d = new Date(y, m, day);
      week.push({ date: d, dayOfMonth: day, isCurrentMonth: true, isToday: this.sameDay(d, today) });
      if (week.length === 7) {
        weeks.push(week);
        week = [];
      }
    }
    if (week.length > 0) {
      let nextDay = new Date(y, m, daysInMonth);
      nextDay.setDate(nextDay.getDate() + 1);
      while (week.length < 7) {
        week.push({ date: nextDay, dayOfMonth: nextDay.getDate(), isCurrentMonth: false, isToday: this.sameDay(nextDay, today) });
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

  monthSessionsOnDay(date: Date): WeekSessionDto[] {
    const ymd = this.formatYmd(date);
    return this.monthSessions.filter(s => {
      if (s.dateYmd) return s.dateYmd === ymd;
      if (s.date == null) return false;
      const d = typeof s.date === 'string' ? s.date.slice(0, 10) : (s.date as unknown as Date)?.toISOString?.()?.slice(0, 10) ?? '';
      return d === ymd;
    });
  }

  holidayOnDay(date: Date): { name: string } | null {
    const ymd = this.formatYmd(date);
    const h = this.holidays.find(x => x.date.slice(0, 10) === ymd);
    return h ? { name: h.name } : null;
  }

  isMonthSessionCompleted(s: WeekSessionDto): boolean {
    const status = (s.status || '').toLowerCase();
    if (s.groupSessionId) return status === 'completed' || status === 'checkedout';
    return status === 'completed' || status === 'checkedout' || (s.attendanceLogId > 0 && status !== 'scheduled');
  }

  isMonthSessionUpcoming(s: WeekSessionDto): boolean {
    return (s.status || '').toLowerCase() === 'scheduled' || (s.attendanceLogId === 0 && !s.groupSessionId);
  }

  prevMonth(): void {
    this.currentMonth = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() - 1, 1);
    this.loadMonth();
  }

  nextMonth(): void {
    this.currentMonth = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() + 1, 1);
    this.loadMonth();
  }

  thisMonth(): void {
    this.currentMonth = new Date();
    this.loadMonth();
  }

  private loadMonth(): void {
    if (this.studentId == null) return;
    this.monthLoading = true;
    this.parentService.getStudentCalendarMonth(this.studentId, this.monthYear, this.monthNum).subscribe({
      next: (list) => {
        this.monthSessions = list;
        this.monthLoading = false;
      },
      error: () => { this.monthLoading = false; }
    });
    this.parentService.getHolidays(this.monthYear).subscribe({
      next: (list) => (this.holidays = list),
      error: () => {}
    });
  }

  private sameDay(a: Date, b: Date): boolean {
    return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
  }

  prevWeek(): void {
    const d = new Date(this.weekStart);
    d.setDate(d.getDate() - 7);
    this.weekStart = d;
    this.loadWeek();
  }

  nextWeek(): void {
    const d = new Date(this.weekStart);
    d.setDate(d.getDate() + 7);
    this.weekStart = d;
    this.loadWeek();
  }

  thisWeek(): void {
    this.weekStart = this.getMonday(new Date());
    this.loadWeek();
  }

  private loadWeek(): void {
    if (this.studentId == null) return;
    this.weekLoading = true;
    const weekStartStr = this.formatYmd(this.weekStart);
    this.parentService.getStudentCalendarWeek(this.studentId, weekStartStr).subscribe({
      next: (list) => {
        this.weekSessions = list;
        this.weekLoading = false;
      },
      error: (err) => {
        this.weekLoading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message || 'Failed to load week sessions.' });
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
