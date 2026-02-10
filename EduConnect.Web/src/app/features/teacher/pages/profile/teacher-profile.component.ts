import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TeacherService } from '../../../../core/services/teacher.service';
import { TeacherProfileDto } from '../../../../core/models/teacher.model';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-teacher-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, MessageModule, ButtonModule, InputTextModule],
  templateUrl: './teacher-profile.component.html',
  styleUrl: './teacher-profile.component.css'
})
export class TeacherProfileComponent implements OnInit {
  profile: TeacherProfileDto | null = null;
  loading = true;
  error = '';
  zoomJoinUrl = '';
  savingZoom = false;

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
    this.teacherService.getProfile().subscribe({
      next: (data) => {
        this.profile = data;
        this.zoomJoinUrl = data.zoomJoinUrl ?? '';
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load profile';
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
  }

  saveZoomUrl(): void {
    this.savingZoom = true;
    this.teacherService.updateZoomJoinUrl({ zoomJoinUrl: this.zoomJoinUrl?.trim() || null }).subscribe({
      next: () => {
        this.savingZoom = false;
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Zoom link saved.' });
        this.load();
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to save';
        this.savingZoom = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    });
  }
}
