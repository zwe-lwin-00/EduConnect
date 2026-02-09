import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../../core/services/admin.service';
import {
  AdminGroupClassDto,
  AdminGroupClassEnrollmentDto,
  AdminCreateGroupClassRequest,
  AdminUpdateGroupClassRequest,
  Teacher,
  ContractDto
} from '../../../../core/models/admin.model';
import { ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-admin-group-classes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-group-classes.component.html',
  styleUrl: './admin-group-classes.component.css'
})
export class AdminGroupClassesComponent implements OnInit {
  groupClasses: AdminGroupClassDto[] = [];
  teachers: Teacher[] = [];
  contracts: ContractDto[] = [];
  enrollments: AdminGroupClassEnrollmentDto[] = [];
  loading = true;
  error = '';

  showCreate = false;
  showEdit = false;
  showEnrollments = false;
  showEnrollModal = false;

  newName = '';
  newTeacherId: number | null = null;
  newZoomUrl = '';
  creating = false;

  selectedClass: AdminGroupClassDto | null = null;
  editName = '';
  editTeacherId: number | null = null;
  editIsActive = true;
  editZoomUrl = '';
  updating = false;

  enrollContractId: number | null = null;
  enrolling = false;

  constructor(
    private adminService: AdminService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.load();
    this.loadTeachers();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.adminService.getGroupClasses().subscribe({
      next: (data) => {
        this.groupClasses = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load';
        this.loading = false;
      }
    });
  }

  loadTeachers(): void {
    this.adminService.getTeachers().subscribe({
      next: (data) => {
        this.teachers = Array.isArray(data) ? data : (data as { items: Teacher[] }).items ?? [];
      },
      error: () => {}
    });
  }

  openCreate(): void {
    this.newName = '';
    this.newTeacherId = null;
    this.newZoomUrl = '';
    this.showCreate = true;
  }

  create(): void {
    const name = this.newName?.trim();
    if (!name) { this.error = 'Enter a name.'; return; }
    if (this.newTeacherId == null) { this.error = 'Select a teacher.'; return; }
    this.creating = true;
    const request: AdminCreateGroupClassRequest = { name, teacherId: this.newTeacherId, zoomJoinUrl: this.newZoomUrl?.trim() || null };
    this.adminService.createGroupClass(request).subscribe({
      next: () => {
        this.creating = false;
        this.showCreate = false;
        this.load();
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Create failed';
        this.creating = false;
      }
    });
  }

  openEdit(gc: AdminGroupClassDto): void {
    this.selectedClass = gc;
    this.editName = gc.name;
    this.editTeacherId = gc.teacherId;
    this.editIsActive = gc.isActive;
    this.editZoomUrl = gc.zoomJoinUrl ?? '';
    this.showEdit = true;
  }

  update(): void {
    if (!this.selectedClass || this.editTeacherId == null) return;
    this.updating = true;
    const request: AdminUpdateGroupClassRequest = {
      name: this.editName?.trim() || this.selectedClass.name,
      teacherId: this.editTeacherId,
      isActive: this.editIsActive,
      zoomJoinUrl: this.editZoomUrl?.trim() || null
    };
    this.adminService.updateGroupClass(this.selectedClass.id, request).subscribe({
      next: () => {
        this.updating = false;
        this.showEdit = false;
        this.load();
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Update failed';
        this.updating = false;
      }
    });
  }

  openEnrollments(gc: AdminGroupClassDto): void {
    this.selectedClass = gc;
    this.enrollments = [];
    this.showEnrollments = true;
    this.adminService.getGroupClassEnrollments(gc.id).subscribe({
      next: (data) => this.enrollments = data,
      error: () => {}
    });
    this.adminService.getContracts(gc.teacherId, undefined, 1).subscribe({
      next: (data) => this.contracts = data,
      error: () => {}
    });
  }

  closeEnrollments(): void {
    this.showEnrollments = false;
    this.showEnrollModal = false;
    this.selectedClass = null;
  }

  get assignableContracts(): ContractDto[] {
    if (!this.selectedClass) return this.contracts;
    const enrolledStudentIds = new Set(this.enrollments.map(e => e.studentId));
    return this.contracts.filter(c => !enrolledStudentIds.has(c.studentId));
  }

  openEnrollModal(): void {
    this.enrollContractId = null;
    this.showEnrollModal = true;
  }

  enroll(): void {
    if (!this.selectedClass || this.enrollContractId == null) return;
    const contract = this.contracts.find(c => c.id === this.enrollContractId);
    if (!contract) return;
    this.enrolling = true;
    this.adminService.enrollInGroupClass(this.selectedClass.id, { studentId: contract.studentId, contractId: contract.id }).subscribe({
      next: () => {
        this.enrolling = false;
        this.enrollContractId = null;
        this.openEnrollments(this.selectedClass!);
        this.load();
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Enroll failed';
        this.enrolling = false;
      }
    });
  }

  unenroll(enrollmentId: number): void {
    this.confirmationService.confirm({
      message: 'Remove this student from the group class?',
      header: 'Remove enrollment',
      icon: 'pi pi-user-minus',
      accept: () => {
        this.adminService.unenrollFromGroupClass(enrollmentId).subscribe({
          next: () => {
            if (this.selectedClass) this.openEnrollments(this.selectedClass);
            this.load();
          },
          error: (err) => this.error = err.error?.error || err.message || 'Failed'
        });
      }
    });
  }
}
