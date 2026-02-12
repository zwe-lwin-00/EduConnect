import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { InputTextModule } from 'primeng/inputtext';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { AdminService } from '../../../../core/services/admin.service';
import {
  AdminGroupClassDto,
  AdminGroupClassEnrollmentDto,
  AdminCreateGroupClassRequest,
  AdminUpdateGroupClassRequest,
  Teacher,
  ContractDto,
  SubscriptionDto,
  SubscriptionType
} from '../../../../core/models/admin.model';
import { ConfirmationService, MessageService } from 'primeng/api';
import { formatTime12h } from '../../../../shared/utils/time.utils';

@Component({
  selector: 'app-admin-group-classes',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, TableModule, DialogModule, ButtonModule, MessageModule, InputTextModule, CalendarModule, DropdownModule, CheckboxModule],
  templateUrl: './admin-group-classes.component.html',
  styleUrl: './admin-group-classes.component.css'
})
export class AdminGroupClassesComponent implements OnInit {
  groupClasses: AdminGroupClassDto[] = [];
  teachers: Teacher[] = [];
  contracts: ContractDto[] = [];
  groupSubscriptions: SubscriptionDto[] = [];
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
  newStartTime: Date | null = null;
  newEndTime: Date | null = null;
  newFromDate: Date | null = null;
  newToDate: Date | null = null;
  /** Cached min date for "from date" (today) — avoid getter for calendar. */
  createFromDateMin: Date = new Date();
  /** Cached min date for "to date" (from date or today). */
  createToDateMin: Date = new Date();
  dayOptions: { value: number; label: string; checked: boolean }[] = [];
  creating = false;

  selectedClass: AdminGroupClassDto | null = null;
  editName = '';
  editTeacherId: number | null = null;
  editIsActive = true;
  editStartTime: Date | null = null;
  editEndTime: Date | null = null;
  editFromDate: Date | null = null;
  editToDate: Date | null = null;
  editFromDateMin: Date = new Date();
  editToDateMin: Date = new Date();
  editDayOptions: { value: number; label: string; checked: boolean }[] = [];
  updating = false;

  static timeToStr(d: Date | null | undefined): string {
    if (!d || !(d instanceof Date)) return '';
    return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
  }
  static strToTime(s: string): Date | null {
    if (!s?.trim()) return null;
    const [h, m] = s.trim().split(':').map(Number);
    if (isNaN(h) || isNaN(m)) return null;
    const d = new Date();
    d.setHours(h, m, 0, 0);
    return d;
  }

  /** Format date as YYYY-MM-DD for API. */
  static formatDateForApi(d: Date | null | undefined): string | null {
    if (!d || !(d instanceof Date) || isNaN(d.getTime())) return null;
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  /** Parse ISO date string (YYYY-MM-DD) to Date at local midnight. */
  static parseDateFromApi(s: string | null | undefined): Date | null {
    if (!s?.trim()) return null;
    const parsed = new Date(s.trim() + 'T00:00:00');
    return isNaN(parsed.getTime()) ? null : parsed;
  }

  get teacherOptions(): { label: string; value: number }[] {
    return this.teachers.map(t => ({ label: t.fullName, value: t.id }));
  }
  get enrollSubscriptionOptions(): { label: string; value: number }[] {
    return this.assignableGroupSubscriptions.map(sub => ({ label: this.subscriptionOptionLabel(sub), value: sub.id }));
  }
  get enrollContractOptions(): { label: string; value: number }[] {
    return this.assignableContracts.map(c => ({ label: `${c.studentName} – ${c.contractId}`, value: c.id }));
  }

  enrollContractId: number | null = null;
  enrollSubscriptionId: number | null = null;
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
    this.newStartTime = null;
    this.newEndTime = null;
    this.newFromDate = null;
    this.newToDate = null;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    this.createFromDateMin = today;
    this.createToDateMin = today;
    this.dayOptions = this.DAY_LABELS.map(d => ({ ...d, checked: false }));
    this.showCreate = true;
  }

  onNewFromDateChange(): void {
    setTimeout(() => {
      if (this.newFromDate) {
        const d = new Date(this.newFromDate);
        d.setHours(0, 0, 0, 0);
        this.createToDateMin = d;
        if (this.newToDate && this.newToDate < d) this.newToDate = null;
      } else {
        this.createToDateMin = new Date();
        this.createToDateMin.setHours(0, 0, 0, 0);
      }
    }, 0);
  }

  onEditFromDateChange(): void {
    setTimeout(() => {
      if (this.editFromDate) {
        const d = new Date(this.editFromDate);
        d.setHours(0, 0, 0, 0);
        this.editToDateMin = d;
        if (this.editToDate && this.editToDate < d) this.editToDate = null;
      } else {
        this.editToDateMin = new Date();
        this.editToDateMin.setHours(0, 0, 0, 0);
      }
    }, 0);
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
    const parts: string[] = [];
    if (gc.daysOfWeek) parts.push(gc.daysOfWeek.split(',').map(n => this.DAY_LABELS[+n - 1]?.label || n).join(', '));
    if (gc.startTime || gc.endTime) parts.push(`${formatTime12h(gc.startTime) || '?'} – ${formatTime12h(gc.endTime) || '?'}`);
    if (gc.startDate || gc.endDate) {
      const from = gc.startDate ? new Date(gc.startDate + 'T00:00:00').toLocaleDateString() : '?';
      const to = gc.endDate ? new Date(gc.endDate + 'T00:00:00').toLocaleDateString() : '?';
      parts.push(`${from} – ${to}`);
    }
    return parts.length ? parts.join(' · ') : '—';
  }

  create(): void {
    const name = this.newName?.trim();
    if (!name) { this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Enter a name.' }); return; }
    if (this.newTeacherId == null) { this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Select a teacher.' }); return; }
    this.creating = true;
    if (this.newFromDate && this.newToDate && this.newToDate < this.newFromDate) {
      this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'To date cannot be before from date.' });
      return;
    }
    const request: AdminCreateGroupClassRequest = {
      name,
      teacherId: this.newTeacherId,
      daysOfWeek: this.getNewDaysOfWeek(),
      startTime: AdminGroupClassesComponent.timeToStr(this.newStartTime) || null,
      endTime: AdminGroupClassesComponent.timeToStr(this.newEndTime) || null,
      startDate: AdminGroupClassesComponent.formatDateForApi(this.newFromDate) ?? null,
      endDate: AdminGroupClassesComponent.formatDateForApi(this.newToDate) ?? null
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
    this.editStartTime = AdminGroupClassesComponent.strToTime(gc.startTime ?? '');
    this.editEndTime = AdminGroupClassesComponent.strToTime(gc.endTime ?? '');
    this.editFromDate = AdminGroupClassesComponent.parseDateFromApi(gc.startDate);
    this.editToDate = AdminGroupClassesComponent.parseDateFromApi(gc.endDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    this.editFromDateMin = today;
    this.editToDateMin = this.editFromDate ? new Date(this.editFromDate) : today;
    if (this.editFromDate) this.editToDateMin.setHours(0, 0, 0, 0);
    const daySet = new Set((gc.daysOfWeek ?? '').split(',').map(s => +s).filter(n => n >= 1 && n <= 7));
    this.editDayOptions = this.DAY_LABELS.map(d => ({ ...d, checked: daySet.has(d.value) }));
    this.showEdit = true;
  }

  update(): void {
    if (!this.selectedClass || this.editTeacherId == null) return;
    if (this.editFromDate && this.editToDate && this.editToDate < this.editFromDate) {
      this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'To date cannot be before from date.' });
      return;
    }
    this.updating = true;
    const request: AdminUpdateGroupClassRequest = {
      name: this.editName?.trim() || this.selectedClass.name,
      teacherId: this.editTeacherId,
      isActive: this.editIsActive,
      daysOfWeek: this.getEditDaysOfWeek(),
      startTime: AdminGroupClassesComponent.timeToStr(this.editStartTime) || null,
      endTime: AdminGroupClassesComponent.timeToStr(this.editEndTime) || null,
      startDate: AdminGroupClassesComponent.formatDateForApi(this.editFromDate) ?? null,
      endDate: AdminGroupClassesComponent.formatDateForApi(this.editToDate) ?? null
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
    this.enrollContractId = null;
    this.enrollSubscriptionId = null;
    this.showEnrollments = true;
    this.adminService.getGroupClassEnrollments(gc.id).subscribe({
      next: (data) => this.enrollments = data,
      error: () => {}
    });
    this.adminService.getContracts(gc.teacherId, undefined, 1).subscribe({
      next: (data) => this.contracts = data,
      error: () => {}
    });
    this.adminService.getSubscriptions(undefined, SubscriptionType.Group, 1).subscribe({
      next: (data) => {
        this.groupSubscriptions = data.filter(
          s => s.status === 1 && new Date(s.subscriptionPeriodEnd) >= new Date()
        );
      },
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

  get assignableGroupSubscriptions(): SubscriptionDto[] {
    if (!this.selectedClass) return [];
    const enrolledStudentIds = new Set(this.enrollments.map(e => e.studentId));
    return this.groupSubscriptions.filter(s => !enrolledStudentIds.has(s.studentId));
  }

  openEnrollModal(): void {
    this.enrollContractId = null;
    this.enrollSubscriptionId = null;
    this.showEnrollModal = true;
  }

  subscriptionOptionLabel(sub: SubscriptionDto): string {
    const end = new Date(sub.subscriptionPeriodEnd).toLocaleDateString();
    return `${sub.studentName} – ${sub.subscriptionId} (until ${end})`;
  }

  enroll(): void {
    if (!this.selectedClass) return;
    if (this.enrollContractId != null) {
      const contract = this.contracts.find(c => c.id === this.enrollContractId);
      if (!contract) return;
      this.enrolling = true;
      this.adminService.enrollInGroupClass(this.selectedClass.id, { studentId: contract.studentId, contractId: contract.id }).subscribe({
        next: () => {
          this.enrolling = false;
          this.enrollContractId = null;
          this.openEnrollments(this.selectedClass!);
          this.load();
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Student enrolled (by contract).' });
        },
        error: (err) => {
          this.error = err.error?.error || err.message || 'Enroll failed';
          this.enrolling = false;
          this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
        }
      });
      return;
    }
    if (this.enrollSubscriptionId != null) {
      const sub = this.groupSubscriptions.find(s => s.id === this.enrollSubscriptionId);
      if (!sub) return;
      this.enrolling = true;
      this.adminService.enrollInGroupClass(this.selectedClass.id, { studentId: sub.studentId, subscriptionId: sub.id }).subscribe({
        next: () => {
          this.enrolling = false;
          this.enrollSubscriptionId = null;
          this.openEnrollments(this.selectedClass!);
          this.load();
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Student enrolled (by group subscription).' });
        },
        error: (err) => {
          this.error = err.error?.error || err.message || 'Enroll failed';
          this.enrolling = false;
          this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
        }
      });
    }
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
