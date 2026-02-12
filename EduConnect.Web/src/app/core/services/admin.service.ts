import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { API_ENDPOINTS } from '../constants/api-endpoints.const';
import {
  Teacher,
  Parent,
  Student,
  OnboardTeacherRequest,
  UpdateTeacherRequest,
  CreateParentRequest,
  CreateParentResponse,
  CreateStudentRequest,
  CreateContractRequest,
  AdjustHoursRequest,
  PagedRequest,
  PagedResult,
  DashboardDto,
  ContractDto,
  DailyReportDto,
  MonthlyReportDto,
  TodaySessionDto,
  AdminGroupClassDto,
  AdminGroupClassEnrollmentDto,
  AdminCreateGroupClassRequest,
  AdminUpdateGroupClassRequest,
  EnrollInGroupClassRequest,
  HolidayDto,
  CreateHolidayRequest,
  UpdateHolidayRequest,
  SystemSettingDto,
  UpsertSystemSettingRequest,
  ClassPriceDto,
  UpsertClassPriceRequest,
  SubscriptionDto,
  CreateSubscriptionRequest
} from '../models/admin.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  constructor(private apiService: ApiService) {}

  getDashboard(): Observable<DashboardDto> {
    return this.apiService.get<DashboardDto>(API_ENDPOINTS.ADMIN.DASHBOARD);
  }

  onboardTeacher(request: OnboardTeacherRequest): Observable<{ userId: string; temporaryPassword: string; message: string }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.ONBOARD_TEACHER, request);
  }

  getTeachers(request?: PagedRequest, filters?: { searchTerm?: string; verificationStatus?: number; specializations?: string }): Observable<Teacher[] | PagedResult<Teacher>> {
    const params = new URLSearchParams();
    if (request) {
      params.set('pageNumber', String(request.pageNumber));
      params.set('pageSize', String(request.pageSize));
      if (request.searchTerm) params.set('searchTerm', request.searchTerm);
      if (request.sortBy) {
        params.set('sortBy', request.sortBy);
        params.set('sortDescending', String(request.sortDescending));
      }
    }
    if (filters?.searchTerm) params.set('searchTerm', filters.searchTerm);
    if (filters?.verificationStatus != null) params.set('verificationStatus', String(filters.verificationStatus));
    if (filters?.specializations) params.set('specializations', filters.specializations);
    const q = params.toString();
    const url = q ? `${API_ENDPOINTS.ADMIN.TEACHERS}?${q}` : API_ENDPOINTS.ADMIN.TEACHERS;
    return this.apiService.get<Teacher[] | PagedResult<Teacher>>(url);
  }

  getTeacherById(id: number): Observable<Teacher> {
    return this.apiService.get<Teacher>(`${API_ENDPOINTS.ADMIN.TEACHERS}/${id}`);
  }

  updateTeacher(id: number, request: UpdateTeacherRequest): Observable<{ success: boolean; message: string }> {
    return this.apiService.put(API_ENDPOINTS.ADMIN.TEACHER_UPDATE(id), request);
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

  resetTeacherPassword(teacherId: number): Observable<{ email: string; temporaryPassword: string; message: string }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.RESET_TEACHER_PASSWORD(teacherId), {});
  }

  createParent(request: CreateParentRequest): Observable<CreateParentResponse> {
    return this.apiService.post<CreateParentResponse>(API_ENDPOINTS.ADMIN.CREATE_PARENT, request);
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

  getStudents(parentId?: string, gradeLevel?: number): Observable<Student[]> {
    const params = new URLSearchParams();
    if (parentId) params.set('parentId', parentId);
    if (gradeLevel != null && gradeLevel >= 1 && gradeLevel <= 4) params.set('gradeLevel', String(gradeLevel));
    const q = params.toString();
    const url = q ? `${API_ENDPOINTS.ADMIN.STUDENTS}?${q}` : API_ENDPOINTS.ADMIN.STUDENTS;
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

  renewSubscription(contractId: number): Observable<{ success: boolean; message?: string }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.CONTRACT_RENEW_SUBSCRIPTION(contractId), {});
  }

  getSubscriptions(studentId?: number, type?: number, status?: number): Observable<SubscriptionDto[]> {
    const params = new URLSearchParams();
    if (studentId != null) params.set('studentId', String(studentId));
    if (type != null) params.set('type', String(type));
    if (status != null) params.set('status', String(status));
    const q = params.toString();
    const url = q ? `${API_ENDPOINTS.ADMIN.SUBSCRIPTIONS}?${q}` : API_ENDPOINTS.ADMIN.SUBSCRIPTIONS;
    return this.apiService.get<SubscriptionDto[]>(url);
  }

  createSubscription(request: CreateSubscriptionRequest): Observable<SubscriptionDto> {
    return this.apiService.post<SubscriptionDto>(API_ENDPOINTS.ADMIN.SUBSCRIPTIONS, request);
  }

  renewSubscriptionById(subscriptionId: number, additionalMonths = 1): Observable<{ success: boolean; message?: string }> {
    return this.apiService.post(
      `${API_ENDPOINTS.ADMIN.SUBSCRIPTION_RENEW(subscriptionId)}?additionalMonths=${additionalMonths}`,
      {}
    );
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

  getGroupClasses(): Observable<AdminGroupClassDto[]> {
    return this.apiService.get<AdminGroupClassDto[]>(API_ENDPOINTS.ADMIN.GROUP_CLASSES);
  }

  createGroupClass(request: AdminCreateGroupClassRequest): Observable<AdminGroupClassDto> {
    return this.apiService.post<AdminGroupClassDto>(API_ENDPOINTS.ADMIN.GROUP_CLASSES, request);
  }

  getGroupClassById(id: number): Observable<AdminGroupClassDto> {
    return this.apiService.get<AdminGroupClassDto>(API_ENDPOINTS.ADMIN.GROUP_CLASS_BY_ID(id));
  }

  updateGroupClass(id: number, request: AdminUpdateGroupClassRequest): Observable<{ success: boolean }> {
    return this.apiService.put(API_ENDPOINTS.ADMIN.GROUP_CLASS_BY_ID(id), request);
  }

  getGroupClassEnrollments(id: number): Observable<AdminGroupClassEnrollmentDto[]> {
    return this.apiService.get<AdminGroupClassEnrollmentDto[]>(API_ENDPOINTS.ADMIN.GROUP_CLASS_ENROLLMENTS(id));
  }

  enrollInGroupClass(groupClassId: number, request: EnrollInGroupClassRequest): Observable<{ success: boolean }> {
    return this.apiService.post(API_ENDPOINTS.ADMIN.GROUP_CLASS_ENROLL(groupClassId), request);
  }

  unenrollFromGroupClass(enrollmentId: number): Observable<{ success: boolean }> {
    return this.apiService.delete(API_ENDPOINTS.ADMIN.GROUP_CLASS_UNENROLL(enrollmentId));
  }

  // Settings (holidays, system settings)
  getHolidays(year?: number): Observable<HolidayDto[]> {
    const params = year != null ? `?year=${year}` : '';
    return this.apiService.get<HolidayDto[]>(API_ENDPOINTS.ADMIN.SETTINGS_HOLIDAYS + params);
  }

  createHoliday(request: CreateHolidayRequest): Observable<HolidayDto> {
    return this.apiService.post<HolidayDto>(API_ENDPOINTS.ADMIN.SETTINGS_HOLIDAYS, request);
  }

  updateHoliday(id: number, request: UpdateHolidayRequest): Observable<{ success: boolean }> {
    return this.apiService.put(API_ENDPOINTS.ADMIN.SETTINGS_HOLIDAY_BY_ID(id), request);
  }

  deleteHoliday(id: number): Observable<{ success: boolean }> {
    return this.apiService.delete(API_ENDPOINTS.ADMIN.SETTINGS_HOLIDAY_BY_ID(id));
  }

  getSystemSettings(): Observable<SystemSettingDto[]> {
    return this.apiService.get<SystemSettingDto[]>(API_ENDPOINTS.ADMIN.SETTINGS_SYSTEM);
  }

  upsertSystemSetting(request: UpsertSystemSettingRequest): Observable<SystemSettingDto> {
    return this.apiService.post<SystemSettingDto>(API_ENDPOINTS.ADMIN.SETTINGS_SYSTEM, request);
  }

  getClassPrices(): Observable<ClassPriceDto[]> {
    return this.apiService.get<ClassPriceDto[]>(API_ENDPOINTS.ADMIN.SETTINGS_CLASS_PRICES);
  }

  upsertClassPrice(request: UpsertClassPriceRequest): Observable<ClassPriceDto> {
    return this.apiService.post<ClassPriceDto>(API_ENDPOINTS.ADMIN.SETTINGS_CLASS_PRICES, request);
  }

  deleteClassPrice(id: number): Observable<void> {
    return this.apiService.delete<void>(API_ENDPOINTS.ADMIN.SETTINGS_CLASS_PRICE_DELETE(id));
  }
}
