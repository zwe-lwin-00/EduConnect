import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { AdminService } from '../../../../core/services/admin.service';
import { Student, ContractDto } from '../../../../core/models/admin.model';
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
  showRenewPopup = false;
  renewForm: FormGroup;

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder
  ) {
    this.renewForm = this.fb.group({
      contractId: [null, Validators.required]
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

  openRenewPopup(): void {
    this.showRenewPopup = true;
    this.renewForm.reset();
  }

  closeRenewPopup(): void {
    this.showRenewPopup = false;
  }

  onSubmitRenew(): void {
    if (this.renewForm.valid) {
      const contractId = +this.renewForm.value.contractId;
      this.adminService.renewSubscription(contractId).subscribe({
        next: () => { alert('Subscription renewed for one month.'); this.closeRenewPopup(); this.loadContracts(); },
        error: (err) => alert('Error: ' + (err.error?.error || err.message))
      });
    }
  }

  contractLabel(c: ContractDto): string {
    if (c.subscriptionPeriodEnd) {
      const d = new Date(c.subscriptionPeriodEnd);
      return `${c.contractId} until ${d.toLocaleDateString()}`;
    }
    return c.contractId;
  }
}
