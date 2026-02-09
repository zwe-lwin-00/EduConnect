import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Table, TableModule } from 'primeng/table';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { CardModule } from 'primeng/card';
import { AdminService } from '../../../../core/services/admin.service';
import { Teacher, OnboardTeacherRequest, UpdateTeacherRequest, PagedResult } from '../../../../core/models/admin.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-admin-teachers',
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
    IconFieldModule,
    InputIconModule,
    CardModule
  ],
  templateUrl: './admin-teachers.component.html',
  styleUrl: './admin-teachers.component.css'
})
export class AdminTeachersComponent implements OnInit {
  teachers: Teacher[] = [];
  loading = false;
  showOnboardPopup = false;
  onboardForm: FormGroup;
  showEditPopup = false;
  editForm: FormGroup;
  editingTeacherId: number | null = null;
  selectedTeacher: Teacher | null = null;
  showTeacherDetails = false;
  showCredentialsPopup = false;
  credentialsEmail = '';
  credentialsPassword = '';
  credentialsNote = 'The teacher must change the password on first login.';
  copyFeedback = '';
  showRejectDialog = false;
  rejectTeacherTarget: Teacher | null = null;
  rejectReason = '';

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {
    this.onboardForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      phoneNumber: ['', Validators.required],
      nrcNumber: ['', Validators.required],
      educationLevel: ['', Validators.required],
      hourlyRate: [0, [Validators.required, Validators.min(0)]],
      bio: [''],
      specializations: ['']
    });
    this.editForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      phoneNumber: ['', Validators.required],
      educationLevel: ['', Validators.required],
      hourlyRate: [0, [Validators.required, Validators.min(0)]],
      bio: [''],
      specializations: ['']
    });
  }

  ngOnInit(): void {
    this.loadTeachers();
  }

  loadTeachers(): void {
    this.loading = true;
    this.adminService.getTeachers().subscribe({
      next: (data) => {
        this.teachers = Array.isArray(data) ? data : (data as PagedResult<Teacher>).items;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading teachers:', err);
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message || 'Failed to load teachers' });
      }
    });
  }

  onGlobalFilter(table: Table, event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    table.filterGlobal(value, 'contains');
  }

  openOnboardPopup(): void {
    this.showOnboardPopup = true;
    this.onboardForm.reset();
  }

  closeOnboardPopup(): void {
    this.showOnboardPopup = false;
    this.onboardForm.reset();
  }

  openEditPopup(teacher: Teacher): void {
    this.editingTeacherId = teacher.id;
    this.editForm.patchValue({
      firstName: teacher.firstName,
      lastName: teacher.lastName,
      phoneNumber: teacher.phoneNumber,
      educationLevel: teacher.educationLevel,
      hourlyRate: teacher.hourlyRate,
      bio: teacher.bio ?? '',
      specializations: teacher.specializations ?? ''
    });
    this.showEditPopup = true;
  }

  closeEditPopup(): void {
    this.showEditPopup = false;
    this.editingTeacherId = null;
    this.editForm.reset();
  }

  onSubmitEdit(): void {
    if (this.editForm.valid && this.editingTeacherId != null) {
      const request: UpdateTeacherRequest = this.editForm.value;
      this.adminService.updateTeacher(this.editingTeacherId, request).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Teacher updated successfully' });
          this.closeEditPopup();
          this.loadTeachers();
          this.closeTeacherDetails();
        },
        error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message })
      });
    }
  }

  onSubmitOnboard(): void {
    if (this.onboardForm.valid) {
      const request: OnboardTeacherRequest = this.onboardForm.value;
      this.adminService.onboardTeacher(request).subscribe({
        next: (response) => {
          this.closeOnboardPopup();
          this.loadTeachers();
          this.credentialsEmail = request.email;
          this.credentialsPassword = response.temporaryPassword;
          this.credentialsNote = 'The teacher must change the password on first login.';
          this.copyFeedback = '';
          this.showCredentialsPopup = true;
        },
        error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message })
      });
    }
  }

  closeCredentialsPopup(): void {
    this.showCredentialsPopup = false;
    this.credentialsEmail = '';
    this.credentialsPassword = '';
    this.credentialsNote = 'The teacher must change the password on first login.';
    this.copyFeedback = '';
  }

  copyCredentialsToClipboard(): void {
    const text = `Email: ${this.credentialsEmail}\nTemporary password: ${this.credentialsPassword}\n\n${this.credentialsNote}`;
    if (navigator.clipboard?.writeText) {
      navigator.clipboard.writeText(text).then(() => {
        this.copyFeedback = 'Copied to clipboard!';
        setTimeout(() => (this.copyFeedback = ''), 2000);
      }).catch(() => {
        this.fallbackCopyToClipboard(text);
      });
    } else {
      this.fallbackCopyToClipboard(text);
    }
  }

  private fallbackCopyToClipboard(text: string): void {
    const ta = document.createElement('textarea');
    ta.value = text;
    ta.style.position = 'fixed';
    ta.style.left = '-9999px';
    document.body.appendChild(ta);
    ta.select();
    try {
      document.execCommand('copy');
      this.copyFeedback = 'Copied to clipboard!';
      setTimeout(() => (this.copyFeedback = ''), 2000);
    } catch {
      this.copyFeedback = 'Select and copy manually';
    }
    document.body.removeChild(ta);
  }

  onVerifyTeacher(teacher: Teacher): void {
    this.confirmationService.confirm({
      message: `Verify teacher ${teacher.firstName} ${teacher.lastName}?`,
      header: 'Verify teacher',
      icon: 'pi pi-check-circle',
      accept: () => {
        this.adminService.verifyTeacher(teacher.id).subscribe({
          next: () => { this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Teacher verified' }); this.loadTeachers(); },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message })
        });
      }
    });
  }

  onResetTeacherPassword(teacher: Teacher): void {
    this.confirmationService.confirm({
      message: `Reset password for ${teacher.firstName} ${teacher.lastName}? A new temporary password will be generated. Share it with the teacher.`,
      header: 'Reset password',
      icon: 'pi pi-key',
      accept: () => {
        this.adminService.resetTeacherPassword(teacher.id).subscribe({
          next: (response) => {
            this.credentialsEmail = response.email;
            this.credentialsPassword = response.temporaryPassword;
            this.credentialsNote = 'The teacher must change the password on next login.';
            this.copyFeedback = '';
            this.showCredentialsPopup = true;
            this.closeTeacherDetails();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message })
        });
      }
    });
  }

  openRejectDialog(teacher: Teacher): void {
    this.rejectTeacherTarget = teacher;
    this.rejectReason = '';
    this.showRejectDialog = true;
  }

  closeRejectDialog(): void {
    this.showRejectDialog = false;
    this.rejectTeacherTarget = null;
    this.rejectReason = '';
  }

  onConfirmReject(): void {
    const teacher = this.rejectTeacherTarget;
    if (!teacher || !this.rejectReason.trim()) return;
    this.adminService.rejectTeacher(teacher.id, this.rejectReason.trim()).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Teacher rejected' });
        this.loadTeachers();
        this.closeRejectDialog();
      },
      error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message })
    });
  }

  onSetTeacherActive(teacher: Teacher, isActive: boolean): void {
    const action = isActive ? 'activate' : 'suspend';
    this.confirmationService.confirm({
      message: `${action} teacher ${teacher.firstName} ${teacher.lastName}?`,
      header: isActive ? 'Activate teacher' : 'Suspend teacher',
      icon: isActive ? 'pi pi-check' : 'pi pi-ban',
      accept: () => {
        this.adminService.setTeacherActive(teacher.id, isActive).subscribe({
          next: () => { this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Done' }); this.loadTeachers(); this.closeTeacherDetails(); },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || err.message })
        });
      }
    });
  }

  onViewTeacherDetails(teacher: Teacher): void {
    this.selectedTeacher = teacher;
    this.showTeacherDetails = true;
  }

  closeTeacherDetails(): void {
    this.showTeacherDetails = false;
    this.selectedTeacher = null;
  }

  getVerificationStatusClass(status: number): string {
    switch (status) {
      case 1: return 'status-pending';
      case 2: return 'status-verified';
      case 3: return 'status-rejected';
      default: return '';
    }
  }

  getVerificationStatusText(status: number): string {
    switch (status) {
      case 1: return 'Pending';
      case 2: return 'Verified';
      case 3: return 'Rejected';
      default: return 'Unknown';
    }
  }

  getVerificationSeverity(status: number): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    switch (status) {
      case 1: return 'warn';
      case 2: return 'success';
      case 3: return 'danger';
      default: return 'secondary';
    }
  }
}
