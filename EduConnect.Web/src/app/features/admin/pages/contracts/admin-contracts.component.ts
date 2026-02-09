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
import { ContractDto, CreateContractRequest, Teacher, Student } from '../../../../core/models/admin.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';

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

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder,
    private messageService: MessageService
  ) {
    this.createForm = this.fb.group({
      teacherId: [null, Validators.required],
      studentId: [null, Validators.required],
      packageHours: [8, [Validators.required, Validators.min(1)]],
      startDate: [new Date().toISOString().slice(0, 10), Validators.required],
      endDate: ['']
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
      error: (err) => { console.error('Error loading contracts:', err); this.loading = false; this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message || 'Failed to load contracts' }); }
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
    this.showCreatePopup = true;
    this.createForm.reset({
      packageHours: 8,
      startDate: new Date().toISOString().slice(0, 10),
      endDate: ''
    });
  }

  closeCreatePopup(): void {
    this.showCreatePopup = false;
  }

  onSubmitCreate(): void {
    if (this.createForm.valid) {
      const v = this.createForm.value;
      const request: CreateContractRequest = {
        teacherId: +v.teacherId,
        studentId: +v.studentId,
        packageHours: +v.packageHours,
        startDate: v.startDate,
        endDate: v.endDate || undefined
      };
      this.adminService.createContract(request).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Contract created successfully' });
          this.closeCreatePopup();
          this.loadContracts();
        },
        error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message })
      });
    }
  }

  cancelContract(c: ContractDto): void {
    if (!confirm(`Cancel contract ${c.contractId}?`)) return;
    this.adminService.cancelContract(c.id).subscribe({
      next: () => { this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Contract cancelled' }); this.loadContracts(); },
      error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message })
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
