import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumberModule } from 'primeng/inputnumber';
import { TextareaModule } from 'primeng/textarea';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TeacherService } from '../../../../core/services/teacher.service';
import {
  TeacherAssignedStudentDto,
  HomeworkDto,
  StudentGradeDto,
  CreateHomeworkRequest,
  UpdateHomeworkStatusRequest,
  CreateGradeRequest,
  HOMEWORK_STATUS
} from '../../../../core/models/teacher.model';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-teacher-homework-grades',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, DialogModule, ButtonModule, MessageModule, InputTextModule, TagModule, CalendarModule, DropdownModule, InputNumberModule, TextareaModule, ProgressSpinnerModule],
  templateUrl: './teacher-homework-grades.component.html',
  styleUrl: './teacher-homework-grades.component.css'
})
export class TeacherHomeworkGradesComponent implements OnInit {
  students: TeacherAssignedStudentDto[] = [];
  homeworks: HomeworkDto[] = [];
  grades: StudentGradeDto[] = [];
  loading = true;
  error = '';
  filterStudentId: number | null = null;
  dueDateFrom = '';
  dueDateTo = '';
  gradeDateFrom = '';
  gradeDateTo = '';

  showHomeworkForm = false;
  homeworkForm: CreateHomeworkRequest = {
    studentId: 0,
    title: '',
    description: '',
    dueDate: new Date().toISOString().slice(0, 10)
  };
  homeworkSubmitting = false;

  showGradeForm = false;
  gradeForm: CreateGradeRequest = {
    studentId: 0,
    title: '',
    gradeValue: '',
    maxValue: undefined,
    gradeDate: new Date().toISOString().slice(0, 10),
    notes: ''
  };
  gradeSubmitting = false;

  gradingHomeworkId: number | null = null;
  gradingFeedback = '';

  readonly HOMEWORK_STATUS = HOMEWORK_STATUS;

  /** Max date for "Since" (From) filter pickers: cannot select future dates. Set once to avoid PrimeNG overlay lifecycle issues. */
  maxDateForSince = new Date();

  private static dateToStr(d: Date | null | undefined): string {
    if (!d || !(d instanceof Date)) return '';
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
  private static strToDate(s: string): Date | null {
    if (!s?.trim()) return null;
    const [y, m, d] = s.trim().split('-').map(Number);
    if (isNaN(y)) return null;
    return new Date(y, (m || 1) - 1, d || 1);
  }

  get dueDateFromModel(): Date | null { return TeacherHomeworkGradesComponent.strToDate(this.dueDateFrom); }
  set dueDateFromModel(v: Date | null) { this.dueDateFrom = TeacherHomeworkGradesComponent.dateToStr(v) ?? ''; }
  get dueDateToModel(): Date | null { return TeacherHomeworkGradesComponent.strToDate(this.dueDateTo); }
  set dueDateToModel(v: Date | null) { this.dueDateTo = TeacherHomeworkGradesComponent.dateToStr(v) ?? ''; }
  get gradeDateFromModel(): Date | null { return TeacherHomeworkGradesComponent.strToDate(this.gradeDateFrom); }
  set gradeDateFromModel(v: Date | null) { this.gradeDateFrom = TeacherHomeworkGradesComponent.dateToStr(v) ?? ''; }
  get gradeDateToModel(): Date | null { return TeacherHomeworkGradesComponent.strToDate(this.gradeDateTo); }
  set gradeDateToModel(v: Date | null) { this.gradeDateTo = TeacherHomeworkGradesComponent.dateToStr(v) ?? ''; }
  get homeworkDueDateModel(): Date | null { return TeacherHomeworkGradesComponent.strToDate(this.homeworkForm.dueDate); }
  set homeworkDueDateModel(v: Date | null) { this.homeworkForm.dueDate = TeacherHomeworkGradesComponent.dateToStr(v) ?? ''; }
  get gradeDateFormModel(): Date | null { return TeacherHomeworkGradesComponent.strToDate(this.gradeForm.gradeDate); }
  set gradeDateFormModel(v: Date | null) { this.gradeForm.gradeDate = TeacherHomeworkGradesComponent.dateToStr(v) ?? ''; }

  get studentOptions(): { label: string; value: number }[] {
    return this.students.map(s => ({ label: `${s.studentName} (${s.contractIdDisplay})`, value: s.studentId }));
  }
  get studentFilterOptions(): { label: string; value: number | null }[] {
    return [{ label: 'All students', value: null }, ...this.students.map(s => ({ label: `${s.studentName} (${s.contractIdDisplay})`, value: s.studentId }))];
  }

  constructor(
    private teacherService: TeacherService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadStudents();
    this.load();
  }

  loadStudents(): void {
    this.teacherService.getAssignedStudents().subscribe({
      next: (data) => this.students = data,
      error: () => {}
    });
  }

  load(): void {
    this.loading = true;
    this.error = '';
    const studentId = this.filterStudentId ?? undefined;
    const dueFrom = this.dueDateFrom || undefined;
    const dueTo = this.dueDateTo || undefined;
    const gradeFrom = this.gradeDateFrom || undefined;
    const gradeTo = this.gradeDateTo || undefined;
    this.teacherService.getHomeworks(studentId, dueFrom, dueTo).subscribe({
      next: (data) => {
        this.homeworks = data;
        this.teacherService.getGrades(studentId, gradeFrom, gradeTo).subscribe({
          next: (g) => {
            this.grades = g;
            this.loading = false;
          },
          error: (err) => {
            this.error = err.error?.error || err.message || 'Failed to load';
            this.loading = false;
            this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
          }
        });
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load';
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
  }

  loadGrades(): void {
    const studentId = this.filterStudentId ?? undefined;
    const gradeFrom = this.gradeDateFrom || undefined;
    const gradeTo = this.gradeDateTo || undefined;
    this.teacherService.getGrades(studentId, gradeFrom, gradeTo).subscribe({
      next: (data) => {
        this.grades = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load';
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
  }

  onFilterChange(): void {
    this.load();
  }

  clearDateFilters(): void {
    this.dueDateFrom = '';
    this.dueDateTo = '';
    this.gradeDateFrom = '';
    this.gradeDateTo = '';
    this.load();
  }

  openHomeworkForm(): void {
    this.homeworkForm = {
      studentId: this.students[0]?.studentId ?? 0,
      title: '',
      description: '',
      dueDate: new Date().toISOString().slice(0, 10)
    };
    this.showHomeworkForm = true;
  }

  closeHomeworkForm(): void {
    this.showHomeworkForm = false;
  }

  submitHomework(): void {
    if (!this.homeworkForm.title.trim()) return;
    this.homeworkSubmitting = true;
    this.teacherService.createHomework(this.homeworkForm).subscribe({
      next: () => {
        this.closeHomeworkForm();
        this.load();
        this.homeworkSubmitting = false;
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to create homework';
        this.homeworkSubmitting = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
  }

  openGradeForm(): void {
    this.gradeForm = {
      studentId: this.students[0]?.studentId ?? 0,
      title: '',
      gradeValue: '',
      maxValue: undefined,
      gradeDate: new Date().toISOString().slice(0, 10),
      notes: ''
    };
    this.showGradeForm = true;
  }

  closeGradeForm(): void {
    this.showGradeForm = false;
  }

  submitGrade(): void {
    if (!this.gradeForm.title.trim() || !this.gradeForm.gradeValue.trim()) return;
    this.gradeSubmitting = true;
    this.teacherService.createGrade(this.gradeForm).subscribe({
      next: () => {
        this.closeGradeForm();
        this.load();
        this.gradeSubmitting = false;
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to add grade';
        this.gradeSubmitting = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
  }

  openGrading(hw: HomeworkDto): void {
    this.gradingHomeworkId = hw.id;
    this.gradingFeedback = hw.teacherFeedback ?? '';
  }

  closeGrading(): void {
    this.gradingHomeworkId = null;
    this.gradingFeedback = '';
  }

  submitGrading(): void {
    if (this.gradingHomeworkId == null) return;
    this.teacherService.updateHomeworkStatus(this.gradingHomeworkId, {
      status: HOMEWORK_STATUS.Graded,
      teacherFeedback: this.gradingFeedback
    }).subscribe({
      next: () => {
        this.closeGrading();
        this.load();
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to update';
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
  }

  markSubmitted(hw: HomeworkDto): void {
    this.teacherService.updateHomeworkStatus(hw.id, { status: HOMEWORK_STATUS.Submitted }).subscribe({
      next: () => { this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Marked as submitted.' }); this.load(); },
      error: (err) => { this.error = err.error?.error || err.message || 'Failed'; this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error }); }
    });
  }
}
