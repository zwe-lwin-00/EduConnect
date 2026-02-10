import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { MessageModule } from 'primeng/message';
import { TeacherService } from '../../../../core/services/teacher.service';
import { TeacherAssignedStudentDto } from '../../../../core/models/teacher.model';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-teacher-students',
  standalone: true,
  imports: [CommonModule, TableModule, MessageModule],
  templateUrl: './teacher-students.component.html',
  styleUrl: './teacher-students.component.css'
})
export class TeacherStudentsComponent implements OnInit {
  students: TeacherAssignedStudentDto[] = [];
  loading = true;
  error = '';

  constructor(
    private teacherService: TeacherService,
    private messageService: MessageService
  ) {}

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
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
  }
}
