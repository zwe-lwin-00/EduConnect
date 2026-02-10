import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { InputTextModule } from 'primeng/inputtext';
import { AdminService } from '../../../../core/services/admin.service';
import {
  AdminGroupClassDto,
  AdminGroupClassEnrollmentDto,
  AdminCreateGroupClassRequest,
  AdminUpdateGroupClassRequest,
  Teacher,
  ContractDto
} from '../../../../core/models/admin.model';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-admin-group-classes',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, TableModule, DialogModule, ButtonModule, MessageModule, InputTextModule],
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

  readonly DAY_LABELS: { value: number; label: string }[] = [
    { value: 1, label: 'Mon' }, { value: 2, label: 'Tue' }, { value: 3, label: 'Wed' },
    { value: 4, label: 'Thu' }, { value: 5, label: 'Fri' }, { value: 6, label: 'Sat' }, { value: 7, label: 'Sun' }
  ];
  newName = '';
  newTeacherId: number | null = null;
  newStartTime = '';
  newEndTime = '';
  dayOptions: { value: number; label: string; checked: boolean }[] = [];
  creating = false;

  selectedClass: AdminGroupClassDto | null = null;
  editName = '';
  editTeacherId: number | null = null;
  editIsActive = true;
  editStartTime = '';
  editEndTime = '';
  editDayOptions: { value: number; label: string; checked: boolean }[] = [];
  updating = false;

  enrollContractId: number | null = null;
  enrolling = false;

  constructor(
    private adminService: AdminService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService
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
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
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
    this.newStartTime = '';
    this.newEndTime = '';
    this.dayOptions = this.DAY_LABELS.map(d => ({ ...d, checked: false }));
    this.showCreate = true;
  }

  onNewDayChange(): void {}

  onEditDayChange(): void {}

  getNewDaysOfWeek(): string | null {
    const checked = this.dayOptions.filter(d => d.checked).map(d => d.value).sort((a, b) => a - b);
    return checked.length ? checked.join(',') : null;
  }

  getEditDaysOfWeek(): string | null {
    const checked = this.editDayOptions.filter(d => d.checked).map(d => d.value).sort((a, b) => a - b);
    return checked.length ? checked.join(',') : null;
  }

  formatSchedule(gc: AdminGroupClassDto): string {
    if (!gc.daysOfWeek && !gc.startTime && !gc.endTime) return '—';
    const days = gc.daysOfWeek ? gc.daysOfWeek.split(',').map(n => this.DAY_LABELS[+n - 1]?.label || n).join(', ') : '';
    const time = (gc.startTime || gc.endTime) ? `${gc.startTime || '?'}–${gc.endTime || '?'}` : '';
    return [days, time].filter(Boolean).join(' · ') || '—';
  }

  create(): void {
    const name = this.newName?.trim();
    if (!name) { this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Enter a name.' }); return; }
    if (this.newTeacherId == null) { this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Select a teacher.' }); return; }
    this.creating = true;
    const request: AdminCreateGroupClassRequest = {
      name,
      teacherId: this.newTeacherId,
      daysOfWeek: this.getNewDaysOfWeek(),
      startTime: this.newStartTime?.trim() || null,
      endTime: this.newEndTime?.trim() || null
    };
    this.adminService.createGroupClass(request).subscribe({
      next: () => {
        this.creating = false;
        this.showCreate = false;
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Group class created.' });
        this.load();
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Create failed';
        this.creating = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
  }

  openEdit(gc: AdminGroupClassDto): void {
    this.selectedClass = gc;
    this.editName = gc.name;
    this.editTeacherId = gc.teacherId;
    this.editIsActive = gc.isActive;
    this.editStartTime = gc.startTime ?? '';
    this.editEndTime = gc.endTime ?? '';
    const daySet = new Set((gc.daysOfWeek ?? '').split(',').map(s => +s).filter(n => n >= 1 && n <= 7));
    this.editDayOptions = this.DAY_LABELS.map(d => ({ ...d, checked: daySet.has(d.value) }));
    this.showEdit = true;
  }

  update(): void {
    if (!this.selectedClass || this.editTeacherId == null) return;
    this.updating = true;
    const request: AdminUpdateGroupClassRequest = {
      name: this.editName?.trim() || this.selectedClass.name,
      teacherId: this.editTeacherId,
      isActive: this.editIsActive,
      daysOfWeek: this.getEditDaysOfWeek(),
      startTime: this.editStartTime?.trim() || null,
      endTime: this.editEndTime?.trim() || null
    };
    this.adminService.updateGroupClass(this.selectedClass.id, request).subscribe({
      next: () => {
        this.updating = false;
        this.showEdit = false;
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
        this.adminService.unenrollFromGroupClass(enrollmentId).subscribe({
          next: () => {
            if (this.selectedClass) this.openEnrollments(this.selectedClass);
            this.load();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message || 'Failed' })
        });
      }
    });
  }
}
