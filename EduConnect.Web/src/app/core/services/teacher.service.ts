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
  GroupClassDto,
  GroupClassEnrollmentDto,
  GroupSessionDto,
  GroupCheckInRequest,
  GroupCheckOutRequest,
  CreateGroupClassRequest,
  UpdateGroupClassRequest,
  EnrollInGroupClassRequest,
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

  checkInGroup(request: GroupCheckInRequest): Observable<{ sessionId: number; message: string }> {
    return this.apiService.post(API_ENDPOINTS.TEACHER.CHECK_IN_GROUP, request);
  }

  checkOutGroup(request: GroupCheckOutRequest): Observable<{ success: boolean; message: string }> {
    return this.apiService.post(API_ENDPOINTS.TEACHER.CHECK_OUT_GROUP, request);
  }

  getGroupSessions(from?: string, to?: string): Observable<GroupSessionDto[]> {
    let url = API_ENDPOINTS.TEACHER.GROUP_SESSIONS;
    const params = new URLSearchParams();
    if (from) params.set('from', from);
    if (to) params.set('to', to);
    if (params.toString()) url += '?' + params.toString();
    return this.apiService.get<GroupSessionDto[]>(url);
  }

  getGroupClasses(): Observable<GroupClassDto[]> {
    return this.apiService.get<GroupClassDto[]>(API_ENDPOINTS.TEACHER.GROUP_CLASSES);
  }

  createGroupClass(request: CreateGroupClassRequest): Observable<GroupClassDto> {
    return this.apiService.post<GroupClassDto>(API_ENDPOINTS.TEACHER.GROUP_CLASSES, request);
  }

  getGroupClassById(id: number): Observable<GroupClassDto> {
    return this.apiService.get<GroupClassDto>(API_ENDPOINTS.TEACHER.GROUP_CLASS_BY_ID(id));
  }

  updateGroupClass(id: number, request: UpdateGroupClassRequest): Observable<{ success: boolean }> {
    return this.apiService.put(API_ENDPOINTS.TEACHER.GROUP_CLASS_BY_ID(id), request);
  }

  getGroupClassEnrollments(id: number): Observable<GroupClassEnrollmentDto[]> {
    return this.apiService.get<GroupClassEnrollmentDto[]>(API_ENDPOINTS.TEACHER.GROUP_CLASS_ENROLLMENTS(id));
  }

  enrollInGroupClass(groupClassId: number, request: EnrollInGroupClassRequest): Observable<{ success: boolean }> {
    return this.apiService.post(API_ENDPOINTS.TEACHER.GROUP_CLASS_ENROLL(groupClassId), request);
  }

  unenrollFromGroupClass(enrollmentId: number): Observable<{ success: boolean }> {
    return this.apiService.delete(API_ENDPOINTS.TEACHER.GROUP_CLASS_UNENROLL(enrollmentId));
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
