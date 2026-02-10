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
import { AdminService } from '../../../../core/services/admin.service';
import { ContractDto, CreateContractRequest, Teacher, Student, SubscriptionDto, SubscriptionType } from '../../../../core/models/admin.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';

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
    CardModule
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

  readonly DAY_LABELS: { value: number; label: string }[] = [
    { value: 1, label: 'Mon' }, { value: 2, label: 'Tue' }, { value: 3, label: 'Wed' },
    { value: 4, label: 'Thu' }, { value: 5, label: 'Fri' }, { value: 6, label: 'Sat' }, { value: 7, label: 'Sun' }
  ];
  createDayOptions: { value: number; label: string; checked: boolean }[] = [];

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {
    this.createForm = this.fb.group({
      subscriptionId: [null, Validators.required],
      teacherId: [null, Validators.required],
      startDate: [''],
      endDate: [''],
      daysOfWeek: [''],
      startTime: [''],
      endTime: ['']
    });
  }

  ngOnInit(): void {
    this.loadContracts();
    this.loadTeachers();
    this.loadStudents();
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
      startDate: '',
      endDate: '',
      daysOfWeek: '',
      startTime: '',
      endTime: ''
    });
    this.createDayOptions = this.DAY_LABELS.map(d => ({ ...d, checked: false }));
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
    const time = (c.startTime || c.endTime) ? `${c.startTime || '?'}–${c.endTime || '?'}` : '';
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
        startDate: v.startDate?.trim() || undefined,
        endDate: v.endDate?.trim() || undefined,
        daysOfWeek: v.daysOfWeek?.trim() || undefined,
        startTime: v.startTime?.trim() || undefined,
        endTime: v.endTime?.trim() || undefined
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
