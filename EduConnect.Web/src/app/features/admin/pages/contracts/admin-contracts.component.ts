import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule, Table } from 'primeng/table';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { InputTextModule } from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { AdminService } from '../../../../core/services/admin.service';
import { ContractDto, CreateContractRequest, Teacher, Student, SubscriptionDto, SubscriptionType } from '../../../../core/models/admin.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { formatTime12h } from '../../../../shared/utils/time.utils';

@Component({
  selector: 'app-admin-contracts',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    ToolbarModule,
    ButtonModule,
    DialogModule,
    TagModule,
    InputTextModule,
    CardModule,
    CalendarModule,
    DropdownModule,
    CheckboxModule
  ],
  templateUrl: './admin-contracts.component.html',
  styleUrl: './admin-contracts.component.css'
})
export class AdminContractsComponent implements OnInit {
  contracts: ContractDto[] = [];
  teachers: Teacher[] = [];
  students: Student[] = [];
  /** Active One-to-one subscriptions for the create-class dropdown */
  oneToOneSubscriptions: SubscriptionDto[] = [];
  loading = false;
  showCreatePopup = false;
  createForm: FormGroup;

  filterTeacherId: number | null = null;
  filterStudentId: number | null = null;
  filterStatus: number | null = null;

  statusOptions = [
    { label: 'Active', value: 1 },
    { label: 'Completed', value: 2 },
    { label: 'Cancelled', value: 3 },
    { label: 'Expired', value: 4 }
  ];

  statusFilterOptions: { label: string; value: number | null }[] = [
    { label: 'All statuses', value: null },
    { label: 'Active', value: 1 },
    { label: 'Completed', value: 2 },
    { label: 'Cancelled', value: 3 },
    { label: 'Expired', value: 4 }
  ];

  readonly DAY_LABELS: { value: number; label: string }[] = [
    { value: 1, label: 'Mon' }, { value: 2, label: 'Tue' }, { value: 3, label: 'Wed' },
    { value: 4, label: 'Thu' }, { value: 5, label: 'Fri' }, { value: 6, label: 'Sat' }, { value: 7, label: 'Sun' }
  ];
  createDayOptions: { value: number; label: string; checked: boolean }[] = [];

  /** Cached min/max for date pickers (updated on subscription/start change) to avoid PrimeNG overlay lifecycle issues. */
  startDateMinDate: Date | null = null;
  subscriptionMaxDate: Date | null = null;
  endDateMinDate: Date | null = null;

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {
    this.createForm = this.fb.group({
      subscriptionId: [null, Validators.required],
      teacherId: [null, Validators.required],
      startDate: [null as Date | null],
      endDate: [null as Date | null],
      daysOfWeek: [''],
      startTime: [null as Date | null],
      endTime: [null as Date | null]
    });
  }

  get teacherFilterOptions(): { label: string; value: number | null }[] {
    return [{ label: 'All teachers', value: null }, ...this.teachers.map(t => ({ label: t.fullName, value: t.id }))];
  }

  get studentFilterOptions(): { label: string; value: number | null }[] {
    return [{ label: 'All students', value: null }, ...this.students.map(s => ({ label: s.fullName, value: s.id }))];
  }

  get subscriptionDropdownOptions(): { label: string; value: number }[] {
    return this.oneToOneSubscriptions.map(sub => ({ label: this.subscriptionOptionLabel(sub), value: sub.id }));
  }

  get teacherDropdownOptions(): { label: string; value: number }[] {
    return this.teachers.map(t => ({ label: t.fullName, value: t.id }));
  }

  /** Selected subscription when creating a class; used to constrain Start/End date to subscription period. */
  get selectedSubscription(): SubscriptionDto | null {
    const subId = this.createForm.get('subscriptionId')?.value;
    if (subId == null) return null;
    return this.oneToOneSubscriptions.find(s => s.id === subId) ?? null;
  }

  /** Update cached date bounds for the date pickers (avoids getters triggering during overlay open). */
  private updateDateBounds(): void {
    const sub = this.selectedSubscription;
    if (!sub?.startDate || !sub?.subscriptionPeriodEnd) {
      this.startDateMinDate = null;
      this.subscriptionMaxDate = null;
      this.endDateMinDate = null;
      return;
    }
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const subStart = this.parseDateOnly(sub.startDate);
    const subEnd = this.parseDateOnly(sub.subscriptionPeriodEnd);
    this.subscriptionMaxDate = subEnd;
    this.startDateMinDate = subStart && subStart.getTime() > today.getTime() ? subStart : today;
    const start = this.createForm.get('startDate')?.value as Date | null;
    this.endDateMinDate = start && start instanceof Date ? start : this.startDateMinDate;
  }

  private parseDateOnly(isoOrYmd: string): Date | null {
    if (!isoOrYmd?.trim()) return null;
    const s = isoOrYmd.trim();
    const part = s.split('T')[0] || s;
    const [y, m, d] = part.split('-').map(Number);
    if (isNaN(y)) return null;
    return new Date(y, (m || 1) - 1, d || 1);
  }

  static timeToStr(d: Date | null | undefined): string {
    if (!d || !(d instanceof Date)) return '';
    const h = d.getHours();
    const m = d.getMinutes();
    return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`;
  }

  ngOnInit(): void {
    this.loadContracts();
    this.loadTeachers();
    this.loadStudents();
    // When subscription changes, clear start/end dates and update date bounds (next tick to avoid calendar overlay lifecycle issues)
    this.createForm.get('subscriptionId')?.valueChanges.subscribe(() => {
      this.createForm.patchValue({ startDate: null, endDate: null }, { emitEvent: false });
      setTimeout(() => this.updateDateBounds(), 0);
    });
    // When start date changes: clear end if before new start; update endDateMinDate
    this.createForm.get('startDate')?.valueChanges.subscribe((start: Date | null) => {
      const end = this.createForm.get('endDate')?.value as Date | null;
      if (end && start && start instanceof Date && end instanceof Date && end < start) {
        this.createForm.patchValue({ endDate: null }, { emitEvent: false });
      }
      setTimeout(() => this.updateDateBounds(), 0);
    });
  }

  loadContracts(): void {
    this.loading = true;
    const teacherId = this.filterTeacherId ?? undefined;
    const studentId = this.filterStudentId ?? undefined;
    const status = this.filterStatus ?? undefined;
    this.adminService.getContracts(teacherId, studentId, status).subscribe({
      next: (data) => { this.contracts = data; this.loading = false; },
      error: (err) => { console.error('Error loading One-To-One classes:', err); this.loading = false; this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message || 'Failed to load One-To-One classes' }); }
    });
  }

  onGlobalFilter(table: Table, event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    table.filterGlobal(value, 'contains');
  }

  applyContractFilters(): void {
    this.loadContracts();
  }

  clearContractFilters(): void {
    this.filterTeacherId = null;
    this.filterStudentId = null;
    this.filterStatus = null;
    this.loadContracts();
  }

  loadTeachers(): void {
    this.adminService.getTeachers().subscribe({
      next: (data) => this.teachers = Array.isArray(data) ? data : (data as { items: Teacher[] }).items ?? data,
      error: () => {}
    });
  }

  loadStudents(): void {
    this.adminService.getStudents().subscribe({
      next: (data) => this.students = data,
      error: () => {}
    });
  }

  openCreatePopup(): void {
    this.loadOneToOneSubscriptions();
    this.showCreatePopup = true;
    this.createForm.reset({
      subscriptionId: null,
      teacherId: null,
      startDate: null,
      endDate: null,
      daysOfWeek: '',
      startTime: null,
      endTime: null
    });
    this.createDayOptions = this.DAY_LABELS.map(d => ({ ...d, checked: false }));
    this.startDateMinDate = null;
    this.subscriptionMaxDate = null;
    this.endDateMinDate = null;
  }

  onCreateDayChange(): void {
    const checked = this.createDayOptions.filter(d => d.checked).map(d => d.value).sort((a, b) => a - b);
    this.createForm.patchValue({ daysOfWeek: checked.length ? checked.join(',') : '' });
  }

  loadOneToOneSubscriptions(): void {
    this.adminService.getSubscriptions(undefined, SubscriptionType.OneToOne, 1).subscribe({
      next: (data) => {
        this.oneToOneSubscriptions = data.filter(
          s => s.status === 1 && new Date(s.subscriptionPeriodEnd) >= new Date()
        );
      },
      error: () => {}
    });
  }

  subscriptionOptionLabel(sub: SubscriptionDto): string {
    const end = new Date(sub.subscriptionPeriodEnd).toLocaleDateString();
    return `${sub.studentName} – ${sub.subscriptionId} (until ${end})`;
  }

  formatSchedule(c: ContractDto): string {
    if (!c.daysOfWeek && !c.startTime && !c.endTime) return '—';
    const days = c.daysOfWeek ? c.daysOfWeek.split(',').map(n => this.DAY_LABELS[+n - 1]?.label || n).join(', ') : '';
    const time = (c.startTime || c.endTime) ? `${formatTime12h(c.startTime) || '?'} – ${formatTime12h(c.endTime) || '?'}` : '';
    return [days, time].filter(Boolean).join(' · ') || '—';
  }

  closeCreatePopup(): void {
    this.showCreatePopup = false;
  }

  onSubmitCreate(): void {
    if (this.createForm.valid) {
      const v = this.createForm.value;
      const sub = this.oneToOneSubscriptions.find(s => s.id === +v.subscriptionId);
      if (!sub) {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Select a student with an active One-to-one subscription.' });
        return;
      }
      const request: CreateContractRequest = {
        teacherId: +v.teacherId,
        studentId: sub.studentId,
        subscriptionId: sub.id,
        startDate: v.startDate instanceof Date ? v.startDate.toISOString().slice(0, 10) : undefined,
        endDate: v.endDate instanceof Date ? v.endDate.toISOString().slice(0, 10) : undefined,
        daysOfWeek: v.daysOfWeek?.trim() || undefined,
        startTime: AdminContractsComponent.timeToStr(v.startTime) || undefined,
        endTime: AdminContractsComponent.timeToStr(v.endTime) || undefined
      };
      this.adminService.createContract(request).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'One-To-One class created successfully' });
          this.closeCreatePopup();
          this.loadContracts();
        },
        error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message })
      });
    }
  }

  cancelContract(c: ContractDto): void {
    this.confirmationService.confirm({
      message: `Cancel this One-To-One class (${c.contractId})?`,
      header: 'Cancel One-To-One class',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.adminService.cancelContract(c.id).subscribe({
          next: () => { this.messageService.add({ severity: 'success', summary: 'Success', detail: 'One-To-One class cancelled' }); this.loadContracts(); },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message })
        });
      }
    });
  }

  statusName(status: number): string {
    const map: Record<number, string> = { 1: 'Active', 2: 'Completed', 3: 'Cancelled', 4: 'Expired' };
    return map[status] ?? 'Unknown';
  }

  statusSeverity(status: number): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    const map: Record<number, 'success' | 'info' | 'warn' | 'danger' | 'secondary'> = {
      1: 'success',
      2: 'info',
      3: 'danger',
      4: 'warn'
    };
    return map[status] ?? 'secondary';
  }
}
