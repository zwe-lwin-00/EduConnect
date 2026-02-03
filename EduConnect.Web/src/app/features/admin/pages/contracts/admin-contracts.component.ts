import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxDataGridModule, DxButtonModule, DxPopupModule } from 'devextreme-angular';
import { AdminService } from '../../../../core/services/admin.service';
import { ContractDto, CreateContractRequest, Teacher, Student } from '../../../../core/models/admin.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-admin-contracts',
  standalone: true,
  imports: [CommonModule, DxDataGridModule, DxButtonModule, DxPopupModule, ReactiveFormsModule],
  templateUrl: './admin-contracts.component.html',
  styleUrl: './admin-contracts.component.css'
})
export class AdminContractsComponent implements OnInit {
  contracts: ContractDto[] = [];
  teachers: Teacher[] = [];
  students: Student[] = [];
  showCreatePopup = false;
  createForm: FormGroup;

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder
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
    this.adminService.getContracts().subscribe({
      next: (data) => this.contracts = data,
      error: (err) => console.error('Error loading contracts:', err)
    });
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
          alert('Contract created successfully!');
          this.closeCreatePopup();
          this.loadContracts();
        },
        error: (err) => alert('Error: ' + (err.error?.error || err.message))
      });
    }
  }

  cancelContract(c: ContractDto): void {
    if (!confirm(`Cancel contract ${c.contractId}?`)) return;
    this.adminService.cancelContract(c.id).subscribe({
      next: () => this.loadContracts(),
      error: (err) => alert('Error: ' + (err.error?.error || err.message))
    });
  }

  statusName(status: number): string {
    const map: Record<number, string> = { 1: 'Active', 2: 'Completed', 3: 'Cancelled', 4: 'Expired' };
    return map[status] ?? 'Unknown';
  }
}
