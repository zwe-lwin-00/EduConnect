import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { AdminService } from '../../../../core/services/admin.service';
import { Student, ContractDto, WalletAdjustRequest } from '../../../../core/models/admin.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-admin-payments',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TableModule, ButtonModule, DialogModule, InputTextModule],
  templateUrl: './admin-payments.component.html',
  styleUrl: './admin-payments.component.css'
})
export class AdminPaymentsComponent implements OnInit {
  students: Student[] = [];
  contracts: ContractDto[] = [];
  showCreditPopup = false;
  showDeductPopup = false;
  creditForm: FormGroup;
  deductForm: FormGroup;
  selectedStudent: Student | null = null;

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder
  ) {
    this.creditForm = this.fb.group({
      studentId: [null, Validators.required],
      contractId: [null, Validators.required],
      hours: [0, [Validators.required, Validators.min(0.5)]],
      reason: ['', Validators.required]
    });
    this.deductForm = this.fb.group({
      studentId: [null, Validators.required],
      contractId: [null, Validators.required],
      hours: [0, [Validators.required, Validators.min(0.5)]],
      reason: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadStudents();
    this.loadContracts();
  }

  loadStudents(): void {
    this.adminService.getStudents().subscribe({
      next: (data) => this.students = data,
      error: () => {}
    });
  }

  loadContracts(): void {
    this.adminService.getContracts().subscribe({
      next: (data) => this.contracts = data.filter(c => c.status === 1),
      error: () => {}
    });
  }

  openCreditPopup(): void {
    this.showCreditPopup = true;
    this.creditForm.reset();
  }

  closeCreditPopup(): void {
    this.showCreditPopup = false;
  }

  openDeductPopup(): void {
    this.showDeductPopup = true;
    this.deductForm.reset();
  }

  closeDeductPopup(): void {
    this.showDeductPopup = false;
  }

  onSubmitCredit(): void {
    if (this.creditForm.valid) {
      const v = this.creditForm.value;
      const request: WalletAdjustRequest = { studentId: +v.studentId, contractId: +v.contractId, hours: +v.hours, reason: v.reason };
      this.adminService.creditHours(request).subscribe({
        next: () => { alert('Hours credited.'); this.closeCreditPopup(); this.loadStudents(); this.loadContracts(); },
        error: (err) => alert('Error: ' + (err.error?.error || err.message))
      });
    }
  }

  onSubmitDeduct(): void {
    if (this.deductForm.valid) {
      const v = this.deductForm.value;
      const request: WalletAdjustRequest = { studentId: +v.studentId, contractId: +v.contractId, hours: +v.hours, reason: v.reason };
      this.adminService.deductHours(request).subscribe({
        next: () => { alert('Hours deducted.'); this.closeDeductPopup(); this.loadStudents(); this.loadContracts(); },
        error: (err) => alert('Error: ' + (err.error?.error || err.message))
      });
    }
  }

  contractsForStudent(studentId: number): ContractDto[] {
    return this.contracts.filter(c => c.studentId === studentId);
  }
}
