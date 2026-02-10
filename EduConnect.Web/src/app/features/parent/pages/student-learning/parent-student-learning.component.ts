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
