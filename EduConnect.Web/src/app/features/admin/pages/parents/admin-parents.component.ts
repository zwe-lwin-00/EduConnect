import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import { AdminService } from '../../../../core/services/admin.service';
import { Parent, CreateParentRequest, CreateParentResponse, PagedResult } from '../../../../core/models/admin.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-admin-parents',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TableModule, ButtonModule, DialogModule, InputTextModule, CardModule],
  templateUrl: './admin-parents.component.html',
  styleUrl: './admin-parents.component.css'
})
export class AdminParentsComponent implements OnInit {
  parents: Parent[] = [];
  loading = true;
  showCreatePopup = false;
  showCredentialsPopup = false;
  credentialsEmail = '';
  credentialsPassword = '';
  copyFeedback = '';
  createForm: FormGroup;

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder,
    private messageService: MessageService
  ) {
    this.createForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      fullName: ['', Validators.required],
      phoneNumber: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadParents();
  }

  loadParents(): void {
    this.loading = true;
    this.adminService.getParents().subscribe({
      next: (data) => {
        this.parents = Array.isArray(data) ? data : (data as PagedResult<Parent>).items;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message || 'Failed to load parents.' });
      }
    });
  }

  openCreatePopup(): void {
    this.showCreatePopup = true;
    this.createForm.reset();
  }

  closeCreatePopup(): void {
    this.showCreatePopup = false;
    this.createForm.reset();
  }

  onSubmitCreate(): void {
    if (this.createForm.valid) {
      const request: CreateParentRequest = this.createForm.value;
      this.adminService.createParent(request).subscribe({
        next: (res: CreateParentResponse) => {
          this.closeCreatePopup();
          this.loadParents();
          this.credentialsEmail = res.email;
          this.credentialsPassword = res.temporaryPassword;
          this.showCredentialsPopup = true;
          this.copyFeedback = '';
        },
        error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message || 'Failed to create parent.' })
      });
    }
  }

  closeCredentialsPopup(): void {
    this.showCredentialsPopup = false;
    this.credentialsEmail = '';
    this.credentialsPassword = '';
    this.copyFeedback = '';
  }

  copyCredentialsToClipboard(): void {
    const text = `Email: ${this.credentialsEmail}\nTemporary password: ${this.credentialsPassword}\n\nThey must change password on first login.`;
    navigator.clipboard.writeText(text).then(() => {
      this.copyFeedback = 'Copied!';
      setTimeout(() => (this.copyFeedback = ''), 2000);
    }).catch(() => {
      this.copyFeedback = 'Copy failed';
    });
  }
}
