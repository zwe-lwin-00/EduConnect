import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DxDataGridModule, DxButtonModule, DxTabPanelModule, DxPopupModule, DxFormModule } from 'devextreme-angular';
import { AdminService } from '../../../../core/services/admin.service';
import { Teacher, Parent, Student, OnboardTeacherRequest, PagedRequest, PagedResult, DashboardDto, DashboardAlertDto } from '../../../../core/models/admin.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DxDataGridModule,
    DxButtonModule,
    DxTabPanelModule,
    DxPopupModule,
    DxFormModule,
    ReactiveFormsModule
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
  dashboard: DashboardDto | null = null;
  teachers: Teacher[] = [];
  parents: Parent[] = [];
  students: Student[] = [];
  
  showOnboardPopup = false;
  onboardForm: FormGroup;
  
  selectedTeacher: Teacher | null = null;
  showTeacherDetails = false;
  rejectReason = '';

  teacherSearch = '';
  teacherStatus: number | '' = '';
  teacherSubject = '';
  studentParentId = '';
  studentGrade: number | '' = '';

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
  }

  ngOnInit(): void {
    this.loadDashboard();
    this.loadTeachers();
    this.loadParents();
    this.loadStudents();
  }

  loadDashboard(): void {
    this.adminService.getDashboard().subscribe({
      next: (data) => { this.dashboard = data; },
      error: (err) => console.error('Error loading dashboard:', err)
    });
  }

  get contractExpiringAlerts(): DashboardAlertDto[] {
    return (this.dashboard?.alerts ?? []).filter(a => a.type === 'ContractExpiring');
  }

  get lowHoursAlerts(): DashboardAlertDto[] {
    return (this.dashboard?.alerts ?? []).filter(a => a.type === 'LowHours');
  }

  loadTeachers(): void {
    const filters = (this.teacherSearch || this.teacherStatus !== '' || this.teacherSubject)
      ? {
          searchTerm: this.teacherSearch || undefined,
          verificationStatus: this.teacherStatus === '' ? undefined : Number(this.teacherStatus),
          specializations: this.teacherSubject || undefined
        }
      : undefined;
    this.adminService.getTeachers(undefined, filters).subscribe({
      next: (data) => {
        this.teachers = Array.isArray(data) ? data : (data as PagedResult<Teacher>).items;
      },
      error: (error) => console.error('Error loading teachers:', error)
    });
  }

  loadParents(): void {
    this.adminService.getParents().subscribe({
      next: (data) => {
        this.parents = Array.isArray(data) ? data : (data as PagedResult<Parent>).items;
      },
      error: (error) => console.error('Error loading parents:', error)
    });
  }

  loadStudents(): void {
    const parentId = this.studentParentId || undefined;
    const gradeLevel = this.studentGrade === '' ? undefined : Number(this.studentGrade);
    this.adminService.getStudents(parentId, gradeLevel).subscribe({
      next: (data) => {
        this.students = data;
      },
      error: (error) => console.error('Error loading students:', error)
    });
  }

  applyTeacherFilters(): void {
    this.loadTeachers();
  }

  clearTeacherFilters(): void {
    this.teacherSearch = '';
    this.teacherStatus = '';
    this.teacherSubject = '';
    this.loadTeachers();
  }

  applyStudentFilters(): void {
    this.loadStudents();
  }

  clearStudentFilters(): void {
    this.studentParentId = '';
    this.studentGrade = '';
    this.loadStudents();
  }

  openOnboardPopup(): void {
    this.showOnboardPopup = true;
    this.onboardForm.reset();
  }

  closeOnboardPopup(): void {
    this.showOnboardPopup = false;
    this.onboardForm.reset();
  }

  onSubmitOnboard(): void {
    if (this.onboardForm.valid) {
      const request: OnboardTeacherRequest = this.onboardForm.value;
      this.adminService.onboardTeacher(request).subscribe({
        next: () => {
          alert('Teacher onboarded successfully!');
          this.closeOnboardPopup();
          this.loadTeachers();
        },
        error: (error) => {
          alert('Error onboarding teacher: ' + (error.error?.error || error.message));
        }
      });
    }
  }

  onVerifyTeacher(teacher: Teacher): void {
    if (confirm(`Verify teacher ${teacher.firstName} ${teacher.lastName}?`)) {
      this.adminService.verifyTeacher(teacher.id).subscribe({
        next: () => {
          alert('Teacher verified successfully!');
          this.loadTeachers();
        },
        error: (error) => {
          alert('Error verifying teacher: ' + (error.error?.error || error.message));
        }
      });
    }
  }

  onRejectTeacher(teacher: Teacher): void {
    this.selectedTeacher = teacher;
    this.rejectReason = '';
    const reason = prompt('Enter rejection reason:');
    if (reason) {
      this.adminService.rejectTeacher(teacher.id, reason).subscribe({
        next: () => {
          alert('Teacher rejected successfully!');
          this.loadTeachers();
        },
        error: (error) => {
          alert('Error rejecting teacher: ' + (error.error?.error || error.message));
        }
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
