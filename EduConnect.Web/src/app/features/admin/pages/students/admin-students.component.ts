import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { AdminService } from '../../../../core/services/admin.service';
import { Student, Parent, CreateStudentRequest, PagedResult } from '../../../../core/models/admin.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-admin-students',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TableModule, ButtonModule, DialogModule, InputTextModule],
  templateUrl: './admin-students.component.html',
  styleUrl: './admin-students.component.css'
})
export class AdminStudentsComponent implements OnInit {
  students: Student[] = [];
  parents: Parent[] = [];
  showCreatePopup = false;
  createForm: FormGroup;
  gradeOptions = [
    { value: 1, label: 'P1' },
    { value: 2, label: 'P2' },
    { value: 3, label: 'P3' },
    { value: 4, label: 'P4' }
  ];

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder
  ) {
    this.createForm = this.fb.group({
      parentId: ['', Validators.required],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      gradeLevel: [1, [Validators.required, Validators.min(1), Validators.max(4)]],
      dateOfBirth: ['', Validators.required],
      specialNeeds: ['']
    });
  }

  ngOnInit(): void {
    this.loadStudents();
    this.loadParents();
  }

  loadStudents(): void {
    this.adminService.getStudents().subscribe({
      next: (data) => this.students = data,
      error: (err) => console.error('Error loading students:', err)
    });
  }

  loadParents(): void {
    this.adminService.getParents().subscribe({
      next: (data) => {
        this.parents = Array.isArray(data) ? data : (data as PagedResult<Parent>).items ?? [];
      },
      error: () => {}
    });
  }

  openCreatePopup(): void {
    this.showCreatePopup = true;
    this.createForm.reset({ gradeLevel: 1 });
  }

  closeCreatePopup(): void {
    this.showCreatePopup = false;
    this.createForm.reset();
  }

  onSubmitCreate(): void {
    if (this.createForm.valid) {
      const v = this.createForm.value;
      const dob = typeof v.dateOfBirth === 'string' ? v.dateOfBirth : (v.dateOfBirth as Date)?.toISOString?.()?.slice(0, 10) ?? '';
      const request: CreateStudentRequest = {
        parentId: v.parentId,
        firstName: v.firstName,
        lastName: v.lastName,
        gradeLevel: +v.gradeLevel,
        dateOfBirth: dob,
        specialNeeds: v.specialNeeds || undefined
      };
      this.adminService.createStudent(request).subscribe({
        next: () => {
          alert('Student created successfully!');
          this.closeCreatePopup();
          this.loadStudents();
        },
        error: (err) => alert('Error: ' + (err.error?.error || err.message))
      });
    }
  }

  setStudentActive(student: Student, isActive: boolean): void {
    if (!confirm(`${isActive ? 'Activate' : 'Freeze'} student ${student.firstName} ${student.lastName}?`)) return;
    this.adminService.setStudentActive(student.id, isActive).subscribe({
      next: () => this.loadStudents(),
      error: (err) => alert('Error: ' + (err.error?.error || err.message))
    });
  }
}
