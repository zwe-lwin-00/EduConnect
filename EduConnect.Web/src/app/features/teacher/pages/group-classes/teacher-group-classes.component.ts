import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TeacherService } from '../../../../core/services/teacher.service';
import {
  GroupClassDto,
  GroupClassEnrollmentDto,
  TeacherAssignedStudentDto,
  CreateGroupClassRequest,
  UpdateGroupClassRequest,
  EnrollInGroupClassRequest
} from '../../../../core/models/teacher.model';

@Component({
  selector: 'app-teacher-group-classes',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './teacher-group-classes.component.html',
  styleUrl: './teacher-group-classes.component.css'
})
export class TeacherGroupClassesComponent implements OnInit {
  groupClasses: GroupClassDto[] = [];
  assignedStudents: TeacherAssignedStudentDto[] = [];
  enrollments: GroupClassEnrollmentDto[] = [];
  loading = true;
  error = '';
  newClassName = '';
  creating = false;
  selectedGroupClass: GroupClassDto | null = null;
  showEnrollModal = false;
  /** Selected contract (one row from assignedStudents = one contract). */
  selectedEnrollContract: TeacherAssignedStudentDto | null = null;
  enrolling = false;
  editName = '';
  editIsActive = true;
  updating = false;

  constructor(private teacherService: TeacherService) {}

  /** Assigned contracts whose student is not already enrolled (backend allows one enrollment per student per group). */
  get assignableContracts(): TeacherAssignedStudentDto[] {
    if (!this.selectedGroupClass) return this.assignedStudents;
    const enrolledStudentIds = new Set(this.enrollments.map(e => e.studentId));
    return this.assignedStudents.filter(s => !enrolledStudentIds.has(s.studentId));
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.teacherService.getGroupClasses().subscribe({
      next: (data) => {
        this.groupClasses = data;
        this.loading = false;
        if (this.selectedGroupClass) {
          const updated = data.find(c => c.id === this.selectedGroupClass!.id);
          if (updated) {
            this.selectedGroupClass = updated;
            this.editName = updated.name;
            this.editIsActive = updated.isActive;
          }
        }
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load';
        this.loading = false;
      }
    });
  }

  createGroupClass(): void {
    const name = this.newClassName?.trim();
    if (!name) { this.error = 'Enter a name for the group class.'; return; }
    this.creating = true;
    this.teacherService.createGroupClass({ name }).subscribe({
      next: () => {
        this.newClassName = '';
        this.creating = false;
        this.load();
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to create';
        this.creating = false;
      }
    });
  }

  viewEnrollments(gc: GroupClassDto): void {
    this.selectedGroupClass = gc;
    this.editName = gc.name;
    this.editIsActive = gc.isActive;
    this.enrollments = [];
    this.teacherService.getGroupClassEnrollments(gc.id).subscribe({
      next: (data) => this.enrollments = data,
      error: () => {}
    });
  }

  updateGroupClass(): void {
    if (!this.selectedGroupClass) return;
    this.updating = true;
    const request: UpdateGroupClassRequest = { name: this.editName?.trim() || this.selectedGroupClass.name, isActive: this.editIsActive };
    this.teacherService.updateGroupClass(this.selectedGroupClass.id, request).subscribe({
      next: () => {
        this.updating = false;
        this.load();
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Update failed';
        this.updating = false;
      }
    });
  }

  closeEnrollments(): void {
    this.selectedGroupClass = null;
    this.showEnrollModal = false;
  }

  openEnrollModal(): void {
    if (!this.selectedGroupClass) return;
    this.teacherService.getAssignedStudents().subscribe({
      next: (data) => this.assignedStudents = data,
      error: () => {}
    });
    this.selectedEnrollContract = null;
    this.showEnrollModal = true;
  }

  enroll(): void {
    if (!this.selectedGroupClass || !this.selectedEnrollContract) return;
    this.enrolling = true;
    const request: EnrollInGroupClassRequest = { studentId: this.selectedEnrollContract.studentId, contractId: this.selectedEnrollContract.contractId };
    this.teacherService.enrollInGroupClass(this.selectedGroupClass.id, request).subscribe({
      next: () => {
        this.enrolling = false;
        this.showEnrollModal = false;
        this.viewEnrollments(this.selectedGroupClass!);
        this.load();
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Enroll failed';
        this.enrolling = false;
      }
    });
  }

  unenroll(enrollmentId: number): void {
    if (!confirm('Remove this student from the group class?')) return;
    this.teacherService.unenrollFromGroupClass(enrollmentId).subscribe({
      next: () => {
        if (this.selectedGroupClass) this.viewEnrollments(this.selectedGroupClass);
        this.load();
      },
      error: (err) => this.error = err.error?.error || err.message || 'Failed'
    });
  }

}
