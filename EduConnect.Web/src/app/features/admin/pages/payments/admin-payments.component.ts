import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { AdminService } from '../../../../core/services/admin.service';
import { Student, ContractDto } from '../../../../core/models/admin.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';

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
  loading = true;
  showRenewPopup = false;
  renewForm: FormGroup;

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder,
    private messageService: MessageService
  ) {
    this.renewForm = this.fb.group({
      contractId: [null, Validators.required]
    });
  }

  ngOnInit(): void {
    this.loading = true;
    this.loadStudents();
    this.loadContracts();
  }

  loadStudents(): void {
    this.adminService.getStudents().subscribe({
      next: (data) => { this.students = data; this.loading = false; },
      error: (err) => {
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message || 'Failed to load students.' });
      }
    });
  }

  loadContracts(): void {
    this.adminService.getContracts().subscribe({
      next: (data) => this.contracts = data.filter(c => c.status === 1),
      error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message || 'Failed to load contracts.' })
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
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Subscription renewed for one month.' });
          this.closeRenewPopup();
          this.loadContracts();
        },
        error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message || 'Renew failed.' })
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
