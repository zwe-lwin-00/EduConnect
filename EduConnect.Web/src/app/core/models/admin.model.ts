export interface Teacher {
  id: number;
  userId: string;
  email: string;
  fullName: string;
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
  fullName: string;
  phoneNumber: string;
  studentCount: number;
  createdAt: string;
  isActive: boolean;
}

export interface Student {
  id: number;
  parentId: string;
  parentName: string;
  fullName: string;
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
  subscriptionPeriodEnd?: string | null;
  status: number;
  statusName: string;
  startDate: string;
  endDate?: string;
  /** Comma-separated ISO day numbers (1=Mon .. 7=Sun). */
  daysOfWeek?: string | null;
  startTime?: string | null;
  endTime?: string | null;
  createdAt: string;
}

export interface CreateContractRequest {
  teacherId: number;
  studentId: number;
  /** When set, this 1:1 class uses the student's One-to-one subscription (required for correct flow). */
  subscriptionId?: number | null;
  startDate?: string | null;
  endDate?: string | null;
  daysOfWeek?: string | null;
  startTime?: string | null;
  endTime?: string | null;
}

/** Subscription type: 1 = One-to-one class, 2 = Group class */
export const SubscriptionType = { OneToOne: 1, Group: 2 } as const;

export interface SubscriptionDto {
  id: number;
  subscriptionId: string;
  studentId: number;
  studentName: string;
  type: number;
  typeName: string;
  startDate: string;
  subscriptionPeriodEnd: string;
  status: number;
  statusName: string;
  createdAt: string;
}

export interface CreateSubscriptionRequest {
  studentId: number;
  type: number;
  durationMonths?: number;
  startDate?: string | null;
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
  fullName: string;
  phoneNumber: string;
  nrcNumber: string;
  educationLevel: string;
  hourlyRate: number;
  bio?: string;
  specializations?: string;
}

export interface UpdateTeacherRequest {
  fullName: string;
  phoneNumber: string;
  educationLevel: string;
  hourlyRate: number;
  bio?: string;
  specializations?: string;
}

export interface CreateParentRequest {
  email: string;
  fullName: string;
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
  fullName: string;
  gradeLevel: number;
  dateOfBirth: string;
  specialNeeds?: string;
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
  daysOfWeek?: string | null;
  startTime?: string | null;
  endTime?: string | null;
  /** Optional: class runs from this date (ISO date). */
  startDate?: string | null;
  /** Optional: class runs until this date (ISO date). */
  endDate?: string | null;
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
  contractId?: number | null;
  contractIdDisplay?: string | null;
  subscriptionId?: number | null;
  subscriptionIdDisplay?: string | null;
}

export interface AdminCreateGroupClassRequest {
  name: string;
  teacherId: number;
  /** Comma-separated ISO day numbers (1=Mon .. 7=Sun), e.g. "1,3,5". */
  daysOfWeek?: string | null;
  /** Class start time e.g. "09:00". */
  startTime?: string | null;
  /** Class end time e.g. "10:00". */
  endTime?: string | null;
  /** Optional: class runs from this date (YYYY-MM-DD). When set, enrollment requires subscription to cover this period. */
  startDate?: string | null;
  /** Optional: class runs until this date (YYYY-MM-DD). */
  endDate?: string | null;
}

export interface AdminUpdateGroupClassRequest {
  name?: string;
  teacherId: number;
  isActive: boolean;
  daysOfWeek?: string | null;
  startTime?: string | null;
  endTime?: string | null;
  startDate?: string | null;
  endDate?: string | null;
}

/** Backend expects either contractId or subscriptionId (admin can enroll by contract or by Group subscription). */
export interface EnrollInGroupClassRequest {
  studentId: number;
  contractId?: number | null;
  subscriptionId?: number | null;
}

// Settings (holidays, system settings)
export interface HolidayDto {
  id: number;
  date: string;
  name: string;
  description?: string | null;
  createdAt: string;
}

export interface CreateHolidayRequest {
  date: string;
  name: string;
  description?: string | null;
}

export interface UpdateHolidayRequest {
  date: string;
  name: string;
  description?: string | null;
}

export interface SystemSettingDto {
  id: number;
  key: string;
  value: string;
  description?: string | null;
  updatedAt: string;
}

export interface UpsertSystemSettingRequest {
  key: string;
  value: string;
  description?: string | null;
}

/** Class price per grade and class type (One-to-one or Group). */
export interface ClassPriceDto {
  id: number;
  gradeLevel: number;
  gradeLevelName: string;
  classType: number;
  classTypeName: string;
  pricePerMonth: number;
  currency: string;
  updatedAt: string;
}

export interface UpsertClassPriceRequest {
  gradeLevel: number;
  classType: number;
  pricePerMonth: number;
  currency?: string | null;
}

/** Class type: 1 = One-to-one, 2 = Group */
export const ClassType = { OneToOne: 1, Group: 2 } as const;
