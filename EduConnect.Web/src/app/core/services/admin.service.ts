import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { API_ENDPOINTS } from '../constants/api-endpoints.const';
import {
  Teacher,
  Parent,
  Student,
  OnboardTeacherRequest,
  CreateParentRequest,
  CreateStudentRequest,
  CreateContractRequest,
  WalletAdjustRequest,
  AdjustHoursRequest,
  PagedRequest,
  PagedResult,
  DashboardDto,
  ContractDto,
  DailyReportDto,
  MonthlyReportDto,
  TodaySessionDto
} from '../models/admin.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  constructor(private apiService: ApiService) {}

  getDashboard(): Observable<DashboardDto> {
    return this.apiService.get<DashboardDto>(API_ENDPOINTS.ADMIN.DASHBOARD);
  }

  onboardTeacher(request: OnboardTeacherRequest): Observable<{ userId: string; message: string }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.ONBOARD_TEACHER, request);
  }

  getTeachers(request?: PagedRequest): Observable<Teacher[] | PagedResult<Teacher>> {
    if (request) {
      return this.apiService.get<PagedResult<Teacher>>(
        `${API_ENDPOINTS.ADMIN.TEACHERS}?pageNumber=${request.pageNumber}&pageSize=${request.pageSize}${request.searchTerm ? `&searchTerm=${request.searchTerm}` : ''}${request.sortBy ? `&sortBy=${request.sortBy}&sortDescending=${request.sortDescending}` : ''}`
      );
    }
    return this.apiService.get<Teacher[]>(API_ENDPOINTS.ADMIN.TEACHERS);
  }

  getTeacherById(id: number): Observable<Teacher> {
    return this.apiService.get<Teacher>(`${API_ENDPOINTS.ADMIN.TEACHERS}/${id}`);
  }

  verifyTeacher(id: number): Observable<{ success: boolean; message: string }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.VERIFY_TEACHER(id), {});
  }

  rejectTeacher(id: number, reason: string): Observable<{ success: boolean; message: string }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.REJECT_TEACHER(id), { reason });
  }

  setTeacherActive(id: number, isActive: boolean): Observable<{ success: boolean }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.TEACHER_ACTIVATE(id), { isActive });
  }

  createParent(request: CreateParentRequest): Observable<{ userId: string; message: string }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.CREATE_PARENT, request);
  }

  getParents(request?: PagedRequest): Observable<Parent[] | PagedResult<Parent>> {
    if (request) {
      return this.apiService.get<PagedResult<Parent>>(
        `${API_ENDPOINTS.ADMIN.PARENTS}?pageNumber=${request.pageNumber}&pageSize=${request.pageSize}${request.searchTerm ? `&searchTerm=${request.searchTerm}` : ''}${request.sortBy ? `&sortBy=${request.sortBy}&sortDescending=${request.sortDescending}` : ''}`
      );
    }
    return this.apiService.get<Parent[]>(API_ENDPOINTS.ADMIN.PARENTS);
  }

  getParentById(id: string): Observable<Parent> {
    return this.apiService.get<Parent>(`${API_ENDPOINTS.ADMIN.PARENTS}/${id}`);
  }

  createStudent(request: CreateStudentRequest): Observable<{ studentId: number; message: string }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.CREATE_STUDENT, request);
  }

  getStudents(parentId?: string): Observable<Student[]> {
    const url = parentId
      ? `${API_ENDPOINTS.ADMIN.STUDENTS}?parentId=${parentId}`
      : API_ENDPOINTS.ADMIN.STUDENTS;
    return this.apiService.get<Student[]>(url);
  }

  setStudentActive(id: number, isActive: boolean): Observable<{ success: boolean }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.STUDENT_SET_ACTIVE(id), { isActive });
  }

  getContracts(teacherId?: number, studentId?: number, status?: number): Observable<ContractDto[]> {
    let url = API_ENDPOINTS.ADMIN.CONTRACTS;
    const params: string[] = [];
    if (teacherId != null) params.push(`teacherId=${teacherId}`);
    if (studentId != null) params.push(`studentId=${studentId}`);
    if (status != null) params.push(`status=${status}`);
    if (params.length) url += '?' + params.join('&');
    return this.apiService.get<ContractDto[]>(url);
  }

  getContractById(id: number): Observable<ContractDto> {
    return this.apiService.get<ContractDto>(API_ENDPOINTS.ADMIN.CONTRACT_BY_ID(id));
  }

  createContract(request: CreateContractRequest): Observable<ContractDto> {
    return this.apiService.post<ContractDto>(API_ENDPOINTS.ADMIN.CREATE_CONTRACT, request);
  }

  activateContract(id: number): Observable<{ success: boolean }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.CONTRACT_ACTIVATE(id), {});
  }

  cancelContract(id: number): Observable<{ success: boolean }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.CONTRACT_CANCEL(id), {});
  }

  getTodaySessions(): Observable<TodaySessionDto[]> {
    return this.apiService.get<TodaySessionDto[]>(API_ENDPOINTS.ADMIN.ATTENDANCE_TODAY);
  }

  overrideCheckIn(attendanceLogId: number): Observable<{ success: boolean }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.ATTENDANCE_OVERRIDE_CHECKIN(attendanceLogId), {});
  }

  overrideCheckOut(attendanceLogId: number): Observable<{ success: boolean }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.ATTENDANCE_OVERRIDE_CHECKOUT(attendanceLogId), {});
  }

  adjustSessionHours(attendanceLogId: number, request: AdjustHoursRequest): Observable<{ success: boolean }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.ATTENDANCE_ADJUST_HOURS(attendanceLogId), request);
  }

  creditHours(request: WalletAdjustRequest): Observable<{ success: boolean }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.WALLET_CREDIT, request);
  }

  deductHours(request: WalletAdjustRequest): Observable<{ success: boolean }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.WALLET_DEDUCT, request);
  }

  getDailyReport(date?: string): Observable<DailyReportDto> {
    const d = date ? `?date=${date}` : '';
    return this.apiService.get<DailyReportDto>(API_ENDPOINTS.ADMIN.REPORTS_DAILY + d);
  }

  getMonthlyReport(year: number, month: number): Observable<MonthlyReportDto> {
    return this.apiService.get<MonthlyReportDto>(
      `${API_ENDPOINTS.ADMIN.REPORTS_MONTHLY}?year=${year}&month=${month}`
    );
  }
}
