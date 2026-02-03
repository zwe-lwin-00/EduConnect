import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DxButtonModule } from 'devextreme-angular';
import { TeacherService } from '../../../../core/services/teacher.service';
import { TeacherSessionItemDto } from '../../../../core/models/teacher.model';

@Component({
  selector: 'app-teacher-sessions',
  standalone: true,
  imports: [CommonModule, FormsModule, DxButtonModule],
  templateUrl: './teacher-sessions.component.html',
  styleUrl: './teacher-sessions.component.css'
})
export class TeacherSessionsComponent implements OnInit {
  todaySessions: TeacherSessionItemDto[] = [];
  upcomingSessions: TeacherSessionItemDto[] = [];
  loading = true;
  error = '';
  checkingInContractId: number | null = null;
  checkingOutSession: TeacherSessionItemDto | null = null;
  lessonNotes = '';

  constructor(private teacherService: TeacherService) {}

  ngOnInit(): void {
    this.load();
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
            this.loading = false;
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

  checkIn(contractId: number): void {
    if (!confirm('Start session (Check-in) for this contract?')) return;
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
}
