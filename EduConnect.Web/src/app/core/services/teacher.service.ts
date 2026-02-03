import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { API_ENDPOINTS } from '../constants/api-endpoints.const';
import {
  TeacherDashboardDto,
  TeacherProfileDto,
  TeacherAssignedStudentDto,
  TeacherSessionItemDto,
  TeacherAvailabilityDto,
  CheckInRequest,
  CheckOutRequest
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
}
