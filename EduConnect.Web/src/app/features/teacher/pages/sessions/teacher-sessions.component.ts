import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TeacherService } from '../../../../core/services/teacher.service';
import { TeacherSessionItemDto, GroupClassDto, GroupSessionDto } from '../../../../core/models/teacher.model';
import { ConfirmationService } from 'primeng/api';
import { DisplayDatePipe } from '../../../../shared/pipes/display-date.pipe';

@Component({
  selector: 'app-teacher-sessions',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ButtonModule, DisplayDatePipe],
  templateUrl: './teacher-sessions.component.html',
  styleUrl: './teacher-sessions.component.css'
})
export class TeacherSessionsComponent implements OnInit {
  todaySessions: TeacherSessionItemDto[] = [];
  upcomingSessions: TeacherSessionItemDto[] = [];
  groupClasses: GroupClassDto[] = [];
  groupSessions: GroupSessionDto[] = [];
  loading = true;
  error = '';
  checkingInContractId: number | null = null;
  checkingOutSession: TeacherSessionItemDto | null = null;
  lessonNotes = '';
  selectedGroupClassId: number | null = null;
  checkingInGroupClassId: number | null = null;
  checkingOutGroupSession: GroupSessionDto | null = null;
  groupLessonNotes = '';

  constructor(
    private teacherService: TeacherService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  get inProgressGroupSession(): GroupSessionDto | null {
    return this.groupSessions.find(s => !s.checkOutTime) ?? null;
  }

  /** Active group classes that have at least one enrolled student (for start-session dropdown). */
  get startableGroupClasses(): GroupClassDto[] {
    return this.groupClasses.filter(c => c.isActive && c.enrolledCount > 0);
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.teacherService.getTodaySessions().subscribe({
      next: (today) => {
        this.todaySessions = today;
        this.teacherService.getUpcomingSessions().subscribe({
          next: (upcoming) => {
            this.upcomingSessions = upcoming;
            this.loadGroupData();
          },
          error: () => { this.loading = false; }
        });
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load';
        this.loading = false;
      }
    });
  }

  private loadGroupData(): void {
    this.teacherService.getGroupClasses().subscribe({
      next: (classes) => {
        this.groupClasses = classes;
        this.teacherService.getGroupSessions().subscribe({
          next: (sessions) => {
            this.groupSessions = sessions;
            this.loading = false;
          },
          error: () => { this.loading = false; }
        });
      },
      error: () => { this.loading = false; }
    });
  }

  checkIn(contractId: number): void {
    this.confirmationService.confirm({
      message: 'Start session (Check-in) for this One-To-One class?',
      header: 'Check-in',
      icon: 'pi pi-play',
      accept: () => {
        this.checkingInContractId = contractId;
        this.teacherService.checkIn({ contractId }).subscribe({
          next: () => {
            this.checkingInContractId = null;
            this.load();
          },
          error: (err) => {
            this.error = err.error?.error || err.message || 'Check-in failed';
            this.checkingInContractId = null;
          }
        });
      }
    });
  }

  openCheckOut(session: TeacherSessionItemDto): void {
    this.checkingOutSession = session;
    this.lessonNotes = '';
  }

  closeCheckOut(): void {
    this.checkingOutSession = null;
    this.lessonNotes = '';
  }

  submitCheckOut(): void {
    if (!this.checkingOutSession) return;
    if (!this.lessonNotes.trim()) {
      alert('Lesson notes are required to check out.');
      return;
    }
    this.teacherService.checkOut({
      sessionId: this.checkingOutSession.id,
      lessonNotes: this.lessonNotes.trim()
    }).subscribe({
      next: () => {
        this.closeCheckOut();
        this.load();
      },
      error: (err) => {
        alert(err.error?.error || err.message || 'Check-out failed');
      }
    });
  }

  startGroupSession(): void {
    const id = this.selectedGroupClassId ?? 0;
    if (!id) { this.error = 'Select a group class.'; return; }
    const gc = this.groupClasses.find(c => c.id === id);
    if (gc?.enrolledCount === 0) { this.error = 'Group class has no enrolled students.'; return; }
    this.confirmationService.confirm({
      message: `Start group session for "${gc?.name}"?`,
      header: 'Start group session',
      icon: 'pi pi-play',
      accept: () => {
        this.checkingInGroupClassId = id;
        this.teacherService.checkInGroup({ groupClassId: id }).subscribe({
          next: () => {
            this.checkingInGroupClassId = null;
            this.load();
          },
          error: (err) => {
            this.error = err.error?.error || err.message || 'Failed to start group session';
            this.checkingInGroupClassId = null;
          }
        });
      }
    });
  }

  openGroupCheckOut(gs: GroupSessionDto): void {
    this.checkingOutGroupSession = gs;
    this.groupLessonNotes = '';
  }

  closeGroupCheckOut(): void {
    this.checkingOutGroupSession = null;
    this.groupLessonNotes = '';
  }

  submitGroupCheckOut(): void {
    if (!this.checkingOutGroupSession) return;
    if (!this.groupLessonNotes.trim()) {
      alert('Lesson notes are required to check out.');
      return;
    }
    this.teacherService.checkOutGroup({
      groupSessionId: this.checkingOutGroupSession.id,
      lessonNotes: this.groupLessonNotes.trim()
    }).subscribe({
      next: () => {
        this.closeGroupCheckOut();
        this.load();
      },
      error: (err) => alert(err.error?.error || err.message || 'Check-out failed')
    });
  }
}
