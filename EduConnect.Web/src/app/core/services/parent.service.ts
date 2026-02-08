import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { API_ENDPOINTS } from '../constants/api-endpoints.const';
import {
  ParentStudentDto,
  StudentLearningOverviewDto
} from '../models/parent.model';
import { WeekSessionDto } from '../models/teacher.model';

@Injectable({
  providedIn: 'root'
})
export class ParentService {
  constructor(private apiService: ApiService) {}

  getMyStudents(): Observable<ParentStudentDto[]> {
    return this.apiService.get<ParentStudentDto[]>(API_ENDPOINTS.PARENT.MY_STUDENTS);
  }

  getStudentLearningOverview(studentId: number): Observable<StudentLearningOverviewDto> {
    return this.apiService.get<StudentLearningOverviewDto>(
      API_ENDPOINTS.PARENT.STUDENT_LEARNING_OVERVIEW(studentId)
    );
  }

  getStudentCalendarWeek(studentId: number, weekStart?: string): Observable<WeekSessionDto[]> {
    const q = weekStart ? `?weekStart=${encodeURIComponent(weekStart)}` : '';
    return this.apiService.get<WeekSessionDto[]>(API_ENDPOINTS.PARENT.STUDENT_CALENDAR_WEEK(studentId) + q);
  }
}
