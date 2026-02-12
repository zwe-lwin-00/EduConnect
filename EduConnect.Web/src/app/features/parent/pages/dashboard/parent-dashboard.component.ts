import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ParentService } from '../../../../core/services/parent.service';
import { ParentStudentDto } from '../../../../core/models/parent.model';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-parent-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, CardModule, MessageModule, ButtonModule, ProgressSpinnerModule],
  templateUrl: './parent-dashboard.component.html',
  styleUrl: './parent-dashboard.component.css'
})
export class ParentDashboardComponent implements OnInit {
  students: ParentStudentDto[] = [];
  loading = true;
  error = '';

  constructor(
    private parentService: ParentService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.parentService.getMyStudents().subscribe({
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
