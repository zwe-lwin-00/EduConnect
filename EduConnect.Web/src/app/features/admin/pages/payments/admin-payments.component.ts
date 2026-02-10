import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { AdminService } from '../../../../core/services/admin.service';
import {
  Student,
  ContractDto,
  SubscriptionDto,
  CreateSubscriptionRequest,
  SubscriptionType,
  PagedResult
} from '../../../../core/models/admin.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-admin-payments',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    CardModule,
    TagModule
  ],
  templateUrl: './admin-payments.component.html',
  styleUrl: './admin-payments.component.css'
})
export class AdminPaymentsComponent implements OnInit {
  subscriptions: SubscriptionDto[] = [];
  students: Student[] = [];
  contracts: ContractDto[] = [];
  loading = true;
  showAddSubscriptionPopup = false;
  showRenewContractPopup = false;
  addSubscriptionForm: FormGroup;
  renewForm: FormGroup;

  filterType: number | null = null;
  filterStatus: number | null = 1; // default Active
  typeOptions = [
    { label: 'All types', value: null },
    { label: 'One-to-one', value: SubscriptionType.OneToOne },
    { label: 'Group', value: SubscriptionType.Group }
  ];
  statusOptions = [
    { label: 'Active', value: 1 },
    { label: 'All statuses', value: null },
    { label: 'Expired', value: 4 }
  ];
  durationOptions = [1, 2, 3, 6, 12].map(m => ({ label: `${m} month(s)`, value: m }));
  creating = false;
  renewingSubscriptionId: number | null = null;

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {
    this.addSubscriptionForm = this.fb.group({
      studentId: [null, Validators.required],
      type: [SubscriptionType.OneToOne, Validators.required],
      durationMonths: [1, [Validators.required, Validators.min(1), Validators.max(24)]]
    });
    this.renewForm = this.fb.group({
      contractId: [null, Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadSubscriptions();
    this.loadStudents();
    this.loadContracts();
  }

  loadSubscriptions(): void {
    this.loading = true;
    const type = this.filterType ?? undefined;
    const status = this.filterStatus ?? undefined;
    this.adminService.getSubscriptions(undefined, type, status).subscribe({
      next: (data) => {
        this.subscriptions = data;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.error || err.message || 'Failed to load subscriptions.'
        });
      }
    });
  }

  loadStudents(): void {
    this.adminService.getStudents().subscribe({
      next: (data) => (this.students = data),
      error: () => {}
    });
  }

  loadContracts(): void {
    this.adminService.getContracts().subscribe({
      next: (data) => (this.contracts = data.filter(c => c.status === 1)),
      error: () => {}
    });
  }

  applyFilters(): void {
    this.loadSubscriptions();
  }

  openAddSubscriptionPopup(): void {
    this.addSubscriptionForm.reset({
      studentId: null,
      type: SubscriptionType.OneToOne,
      durationMonths: 1
    });
    this.showAddSubscriptionPopup = true;
  }

  closeAddSubscriptionPopup(): void {
    this.showAddSubscriptionPopup = false;
  }

  onSubmitAddSubscription(): void {
    if (this.addSubscriptionForm.invalid) return;
    const v = this.addSubscriptionForm.value;
    const request: CreateSubscriptionRequest = {
      studentId: +v.studentId,
      type: +v.type,
      durationMonths: +v.durationMonths
    };
    this.creating = true;
    this.adminService.createSubscription(request).subscribe({
      next: () => {
        this.creating = false;
        this.closeAddSubscriptionPopup();
        this.loadSubscriptions();
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `Subscription added: ${v.durationMonths} month(s) ${v.type === SubscriptionType.OneToOne ? 'One-to-one' : 'Group'}.`
        });
      },
      error: (err) => {
        this.creating = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.error || err.message || 'Failed to add subscription.'
        });
      }
    });
  }

  renewSubscription(sub: SubscriptionDto): void {
    this.confirmationService.confirm({
      message: `Renew "${sub.subscriptionId}" (${sub.studentName}) by 1 month?`,
      header: 'Renew subscription',
      icon: 'pi pi-calendar-plus',
      accept: () => {
        this.renewingSubscriptionId = sub.id;
        this.adminService.renewSubscriptionById(sub.id, 1).subscribe({
          next: () => {
            this.renewingSubscriptionId = null;
            this.loadSubscriptions();
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Subscription renewed for one month.'
            });
          },
          error: (err) => {
            this.renewingSubscriptionId = null;
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: err.error?.error || err.message || 'Renew failed.'
            });
          }
        });
      }
    });
  }

  openRenewContractPopup(): void {
    this.showRenewContractPopup = true;
    this.renewForm.reset();
  }

  closeRenewContractPopup(): void {
    this.showRenewContractPopup = false;
  }

  onSubmitRenewContract(): void {
    if (!this.renewForm.valid) return;
    const contractId = +this.renewForm.value.contractId;
    this.adminService.renewSubscription(contractId).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'One-to-one class subscription renewed for one month.'
        });
        this.closeRenewContractPopup();
        this.loadContracts();
      },
      error: (err) =>
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.error || err.message || 'Renew failed.'
        })
    });
  }

  contractLabel(c: ContractDto): string {
    if (c.subscriptionPeriodEnd) {
      const d = new Date(c.subscriptionPeriodEnd);
      return `${c.contractId} until ${d.toLocaleDateString()}`;
    }
    return c.contractId;
  }

  isSubscriptionActive(sub: SubscriptionDto): boolean {
    return sub.status === 1 && new Date(sub.subscriptionPeriodEnd) >= new Date();
  }

  studentLabel(s: Student): string {
    return `${s.firstName} ${s.lastName} (${s.gradeLevelName})`;
  }
}
