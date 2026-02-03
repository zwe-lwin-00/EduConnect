import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ParentService } from '../../../../core/services/parent.service';
import { StudentLearningOverviewDto } from '../../../../core/models/parent.model';

@Component({
  selector: 'app-parent-student-learning',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './parent-student-learning.component.html',
  styleUrl: './parent-student-learning.component.css'
})
export class ParentStudentLearningComponent implements OnInit {
  overview: StudentLearningOverviewDto | null = null;
  studentId: number | null = null;
  loading = true;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private parentService: ParentService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('studentId');
    this.studentId = id ? +id : null;
    if (this.studentId != null) {
      this.load();
    } else {
      this.error = 'Invalid student';
      this.loading = false;
    }
  }

  load(): void {
    if (this.studentId == null) return;
    this.loading = true;
    this.error = '';
    this.parentService.getStudentLearningOverview(this.studentId).subscribe({
      next: (data) => {
        this.overview = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.error || err.message || 'Failed to load';
        this.loading = false;
      }
    });
  }
}
