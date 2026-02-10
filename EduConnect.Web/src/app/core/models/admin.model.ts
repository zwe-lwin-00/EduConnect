export interface Teacher {
  id: number;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  educationLevel: string;
  hourlyRate: number;
  verificationStatus: number;
  verificationStatusName: string;
  bio?: string;
  specializations?: string;
  createdAt: string;
  verifiedAt?: string;
  isActive: boolean;
}

export interface Parent {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  studentCount: number;
  createdAt: string;
  isActive: boolean;
}

export interface Student {
  id: number;
  parentId: string;
  parentName: string;
  firstName: string;
  lastName: string;
  gradeLevel: number;
  gradeLevelName: string;
  dateOfBirth: string;
  specialNeeds?: string;
  walletBalance: number;
  createdAt: string;
  isActive: boolean;
}

// Dashboard — Master Doc B1
export interface DashboardDto {
  alerts: DashboardAlertDto[];
  todaySessions: TodaySessionDto[];
  pendingActionsCount: number;
  revenueSnapshot: RevenueSnapshotDto;
}

export interface DashboardAlertDto {
  type: string;
  message: string;
  entityId?: string;
  entityName?: string;
}

export interface TodaySessionDto {
  id: number;
  contractId: string;
  teacherName: string;
  studentName: string;
  status: string;
  scheduledTime?: string;
  checkInTime?: string;
  checkOutTime?: string;
}

export interface RevenueSnapshotDto {
  revenueThisMonth: number;
  sessionsDeliveredThisMonth: number;
  hoursConsumedThisMonth: number;
}

// Contracts — Master Doc B4 (monthly subscription only)
export interface ContractDto {
  id: number;
  contractId: string;
  teacherId: number;
  teacherName: string;
  studentId: number;
  studentName: string;
  /** Subscription valid until (end of month). */
  subscriptionPeriodEnd?: string | null;
  status: number;
  statusName: string;
  startDate: string;
  endDate?: string;
  createdAt: string;
}

// Reports — Master Doc B8
export interface DailyReportDto {
  date: string;
  sessionsDelivered: number;
  hoursConsumed: number;
}

export interface MonthlyReportDto {
  year: number;
  month: number;
  revenue: number;
  sessionsDelivered: number;
  hoursConsumed: number;
  teacherUtilization: TeacherUtilizationDto[];
}

export interface TeacherUtilizationDto {
  teacherId: number;
  teacherName: string;
  hoursDelivered: number;
  sessionsCount: number;
}

export interface OnboardTeacherRequest {
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  nrcNumber: string;
  educationLevel: string;
  hourlyRate: number;
  bio?: string;
  specializations?: string;
}

export interface UpdateTeacherRequest {
  firstName: string;
  lastName: string;
  phoneNumber: string;
  educationLevel: string;
  hourlyRate: number;
  bio?: string;
  specializations?: string;
}

export interface CreateParentRequest {
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
}

/** Returned when admin creates a parent. Share the credentials with the parent so they can log in. */
export interface CreateParentResponse {
  userId: string;
  email: string;
  temporaryPassword: string;
}

export interface CreateStudentRequest {
  parentId: string;
  firstName: string;
  lastName: string;
  gradeLevel: number;
  dateOfBirth: string;
  specialNeeds?: string;
}

export interface CreateContractRequest {
  teacherId: number;
  studentId: number;
  startDate: string;
  endDate?: string;
}

export interface WalletAdjustRequest {
  studentId: number;
  contractId: number;
  hours: number;
  reason: string;
}

export interface AdjustHoursRequest {
  hours: number;
  reason: string;
}

export interface PagedRequest {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Admin: Group classes (admin prepares; assigns teacher)
export interface AdminGroupClassDto {
  id: number;
  teacherId: number;
  teacherName?: string;
  name: string;
  isActive: boolean;
  zoomJoinUrl?: string | null;
  createdAt: string;
  enrolledCount: number;
}

export interface AdminGroupClassEnrollmentDto {
  id: number;
  groupClassId: number;
  studentId: number;
  studentName: string;
  contractId: number;
  contractIdDisplay: string;
}

export interface AdminCreateGroupClassRequest {
  name: string;
  teacherId: number;
  zoomJoinUrl?: string | null;
}

export interface AdminUpdateGroupClassRequest {
  name?: string;
  teacherId: number;
  isActive: boolean;
  zoomJoinUrl?: string | null;
}

export interface EnrollInGroupClassRequest {
  studentId: number;
  contractId: number;
}
