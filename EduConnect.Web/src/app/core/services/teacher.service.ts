import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { API_ENDPOINTS } from '../constants/api-endpoints.const';
import {
  TeacherDashboardDto,
  TeacherProfileDto,
  TeacherAssignedStudentDto,
  TeacherSessionItemDto,
  WeekSessionDto,
  TeacherAvailabilityDto,
  CheckInRequest,
  CheckOutRequest,
  HomeworkDto,
  CreateHomeworkRequest,
  UpdateHomeworkStatusRequest,
  StudentGradeDto,
  CreateGradeRequest
} from '../models/teacher.model';

@Injectable({
  providedIn: 'root'
})
export class TeacherService {
  constructor(private apiService: ApiService) {}

  getDashboard(): Observable<TeacherDashboardDto> {
    return this.apiService.get<TeacherDashboardDto>(API_ENDPOINTS.TEACHER.DASHBOARD);
  }

  getProfile(): Observable<TeacherProfileDto> {
    return this.apiService.get<TeacherProfileDto>(API_ENDPOINTS.TEACHER.PROFILE);
  }

  getAssignedStudents(): Observable<TeacherAssignedStudentDto[]> {
    return this.apiService.get<TeacherAssignedStudentDto[]>(API_ENDPOINTS.TEACHER.STUDENTS);
  }

  getTodaySessions(): Observable<TeacherSessionItemDto[]> {
    return this.apiService.get<TeacherSessionItemDto[]>(API_ENDPOINTS.TEACHER.SESSIONS_TODAY);
  }

  getUpcomingSessions(): Observable<TeacherSessionItemDto[]> {
    return this.apiService.get<TeacherSessionItemDto[]>(API_ENDPOINTS.TEACHER.SESSIONS_UPCOMING);
  }

  getCalendarWeek(weekStart?: string): Observable<WeekSessionDto[]> {
    const q = weekStart ? `?weekStart=${encodeURIComponent(weekStart)}` : '';
    return this.apiService.get<WeekSessionDto[]>(API_ENDPOINTS.TEACHER.CALENDAR_WEEK + q);
  }

  getAvailability(): Observable<TeacherAvailabilityDto[]> {
    return this.apiService.get<TeacherAvailabilityDto[]>(API_ENDPOINTS.TEACHER.AVAILABILITY);
  }

  updateAvailability(availabilities: TeacherAvailabilityDto[]): Observable<{ success: boolean }> {
    return this.apiService.post(API_ENDPOINTS.TEACHER.AVAILABILITY, availabilities);
  }

  checkIn(request: CheckInRequest): Observable<{ sessionId: number; message: string }> {
    return this.apiService.post(API_ENDPOINTS.TEACHER.CHECK_IN, request);
  }

  checkOut(request: CheckOutRequest): Observable<{ success: boolean; message: string }> {
    return this.apiService.post(API_ENDPOINTS.TEACHER.CHECK_OUT, request);
  }

  getHomeworks(studentId?: number, dueDateFrom?: string, dueDateTo?: string): Observable<HomeworkDto[]> {
    const params = new URLSearchParams();
    if (studentId != null) params.set('studentId', String(studentId));
    if (dueDateFrom) params.set('dueDateFrom', dueDateFrom);
    if (dueDateTo) params.set('dueDateTo', dueDateTo);
    const q = params.toString();
    const endpoint = q ? `${API_ENDPOINTS.TEACHER.HOMEWORK}?${q}` : API_ENDPOINTS.TEACHER.HOMEWORK;
    return this.apiService.get<HomeworkDto[]>(endpoint);
  }

  createHomework(request: CreateHomeworkRequest): Observable<HomeworkDto> {
    return this.apiService.post<HomeworkDto>(API_ENDPOINTS.TEACHER.HOMEWORK, request);
  }

  updateHomeworkStatus(homeworkId: number, request: UpdateHomeworkStatusRequest): Observable<HomeworkDto> {
    return this.apiService.put<HomeworkDto>(API_ENDPOINTS.TEACHER.HOMEWORK_STATUS(homeworkId), request);
  }

  getGrades(studentId?: number, gradeDateFrom?: string, gradeDateTo?: string): Observable<StudentGradeDto[]> {
    const params = new URLSearchParams();
    if (studentId != null) params.set('studentId', String(studentId));
    if (gradeDateFrom) params.set('gradeDateFrom', gradeDateFrom);
    if (gradeDateTo) params.set('gradeDateTo', gradeDateTo);
    const q = params.toString();
    const endpoint = q ? `${API_ENDPOINTS.TEACHER.GRADES}?${q}` : API_ENDPOINTS.TEACHER.GRADES;
    return this.apiService.get<StudentGradeDto[]>(endpoint);
  }

  createGrade(request: CreateGradeRequest): Observable<StudentGradeDto> {
    return this.apiService.post<StudentGradeDto>(API_ENDPOINTS.TEACHER.GRADES, request);
  }
}
