import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxDataGridModule } from 'devextreme-angular';
import { TeacherService } from '../../../../core/services/teacher.service';
import { TeacherAssignedStudentDto } from '../../../../core/models/teacher.model';

@Component({
  selector: 'app-teacher-students',
  standalone: true,
  imports: [CommonModule, DxDataGridModule],
  templateUrl: './teacher-students.component.html',
  styleUrl: './teacher-students.component.css'
})
export class TeacherStudentsComponent implements OnInit {
  students: TeacherAssignedStudentDto[] = [];
  loading = true;
  error = '';

  constructor(private teacherService: TeacherService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.teacherService.getAssignedStudents().subscribe({
      next: (data) => {
        this.students = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load';
        this.loading = false;
      }
    });
  }
}
