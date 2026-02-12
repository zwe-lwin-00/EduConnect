import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TeacherService } from '../../../../core/services/teacher.service';
import {
  GroupClassDto,
  GroupClassEnrollmentDto,
  TeacherAssignedStudentDto,
  UpdateGroupClassRequest,
  EnrollInGroupClassRequest
} from '../../../../core/models/teacher.model';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-teacher-group-classes',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, CardModule, DialogModule, ButtonModule, MessageModule, InputTextModule, TagModule, DropdownModule, CheckboxModule, ProgressSpinnerModule],
  templateUrl: './teacher-group-classes.component.html',
  styleUrl: './teacher-group-classes.component.css'
})
export class TeacherGroupClassesComponent implements OnInit {
  groupClasses: GroupClassDto[] = [];
  assignedStudents: TeacherAssignedStudentDto[] = [];
  enrollments: GroupClassEnrollmentDto[] = [];
  loading = true;
  error = '';
  selectedGroupClass: GroupClassDto | null = null;
  showEnrollModal = false;
  /** Selected contract (one row from assignedStudents = one contract). */
  selectedEnrollContract: TeacherAssignedStudentDto | null = null;
  enrolling = false;
  editName = '';
  editZoomUrl = '';
  editIsActive = true;
  updating = false;

  constructor(
    private teacherService: TeacherService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
  ) {}

  /** Assigned contracts whose student is not already enrolled (backend allows one enrollment per student per group). */
  get assignableContracts(): TeacherAssignedStudentDto[] {
    if (!this.selectedGroupClass) return this.assignedStudents;
    const enrolledStudentIds = new Set(this.enrollments.map(e => e.studentId));
    return this.assignedStudents.filter(s => !enrolledStudentIds.has(s.studentId));
  }

  get enrollContractOptions(): { label: string; value: TeacherAssignedStudentDto }[] {
    return this.assignableContracts.map(s => ({ label: `${s.studentName} – ${s.contractIdDisplay}`, value: s }));
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
            this.editZoomUrl = updated.zoomJoinUrl ?? '';
            this.editIsActive = updated.isActive;
          }
        }
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load';
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
  }

  viewEnrollments(gc: GroupClassDto): void {
    this.selectedGroupClass = gc;
    this.editName = gc.name;
    this.editZoomUrl = gc.zoomJoinUrl ?? '';
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
    const request: UpdateGroupClassRequest = { name: this.editName?.trim() || this.selectedGroupClass.name, isActive: this.editIsActive, zoomJoinUrl: this.editZoomUrl?.trim() || null };
    this.teacherService.updateGroupClass(this.selectedGroupClass.id, request).subscribe({
      next: () => {
        this.updating = false;
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Group class updated.' });
        this.load();
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Update failed';
        this.updating = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
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
        this.selectedEnrollContract = null;
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Student enrolled.' });
        this.viewEnrollments(this.selectedGroupClass!);
        this.load();
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Enroll failed';
        this.enrolling = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
  }

  unenroll(enrollmentId: number): void {
    this.confirmationService.confirm({
      message: 'Remove this student from the group class?',
      header: 'Remove enrollment',
      icon: 'pi pi-user-minus',
      accept: () => {
        this.teacherService.unenrollFromGroupClass(enrollmentId).subscribe({
          next: () => {
            if (this.selectedGroupClass) this.viewEnrollments(this.selectedGroupClass);
            this.load();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message || 'Failed' })
        });
      }
    });
  }

}
