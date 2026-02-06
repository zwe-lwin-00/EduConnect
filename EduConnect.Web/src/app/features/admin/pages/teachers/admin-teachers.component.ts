import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxDataGridModule, DxButtonModule, DxPopupModule, DxFormModule } from 'devextreme-angular';
import { AdminService } from '../../../../core/services/admin.service';
import { Teacher, OnboardTeacherRequest, UpdateTeacherRequest, PagedResult } from '../../../../core/models/admin.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-admin-teachers',
  standalone: true,
  imports: [
    CommonModule,
    DxDataGridModule,
    DxButtonModule,
    DxPopupModule,
    DxFormModule,
    ReactiveFormsModule
  ],
  templateUrl: './admin-teachers.component.html',
  styleUrl: './admin-teachers.component.css'
})
export class AdminTeachersComponent implements OnInit {
  teachers: Teacher[] = [];
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

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder
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
    this.adminService.getTeachers().subscribe({
      next: (data) => {
        this.teachers = Array.isArray(data) ? data : (data as PagedResult<Teacher>).items;
      },
      error: (err) => console.error('Error loading teachers:', err)
    });
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
          alert('Teacher updated successfully!');
          this.closeEditPopup();
          this.loadTeachers();
          this.closeTeacherDetails();
        },
        error: (err) => alert('Error: ' + (err.error?.error || err.message))
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
        error: (err) => alert('Error: ' + (err.error?.error || err.message))
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
    if (confirm(`Verify teacher ${teacher.firstName} ${teacher.lastName}?`)) {
      this.adminService.verifyTeacher(teacher.id).subscribe({
        next: () => { alert('Teacher verified.'); this.loadTeachers(); },
        error: (err) => alert('Error: ' + (err.error?.error || err.message))
      });
    }
  }

  onResetTeacherPassword(teacher: Teacher): void {
    if (confirm(`Reset password for ${teacher.firstName} ${teacher.lastName}? A new temporary password will be generated. Share it with the teacher.`)) {
      this.adminService.resetTeacherPassword(teacher.id).subscribe({
        next: (response) => {
          this.credentialsEmail = response.email;
          this.credentialsPassword = response.temporaryPassword;
          this.credentialsNote = 'The teacher must change the password on next login.';
          this.copyFeedback = '';
          this.showCredentialsPopup = true;
          this.closeTeacherDetails();
        },
        error: (err) => alert('Error: ' + (err.error?.error || err.message))
      });
    }
  }

  onRejectTeacher(teacher: Teacher): void {
    const reason = prompt('Enter rejection reason:');
    if (reason) {
      this.adminService.rejectTeacher(teacher.id, reason).subscribe({
        next: () => { alert('Teacher rejected.'); this.loadTeachers(); },
        error: (err) => alert('Error: ' + (err.error?.error || err.message))
      });
    }
  }

  onSetTeacherActive(teacher: Teacher, isActive: boolean): void {
    const action = isActive ? 'activate' : 'suspend';
    if (confirm(`${action} teacher ${teacher.firstName} ${teacher.lastName}?`)) {
      this.adminService.setTeacherActive(teacher.id, isActive).subscribe({
        next: () => { alert('Done.'); this.loadTeachers(); this.closeTeacherDetails(); },
        error: (err) => alert('Error: ' + (err.error?.error || err.message))
      });
    }
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
}
