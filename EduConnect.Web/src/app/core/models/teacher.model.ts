// Teacher dashboard — Master Doc 11B B1
export interface TeacherDashboardDto {
  todaySessions: TeacherSessionItemDto[];
  upcomingSessions: TeacherSessionItemDto[];
  totalRemainingHours: number;
  alerts: TeacherAlertDto[];
}

export interface TeacherSessionItemDto {
  id: number;
  contractId: number;
  contractIdDisplay: string;
  studentName: string;
  status: string;
  scheduledTime?: string;
  checkInTime?: string;
  checkOutTime?: string;
  lessonNotes?: string;
  canCheckIn: boolean;
  canCheckOut: boolean;
}

/** Week calendar session (teacher "My sessions this week" / parent "Upcoming sessions"). */
export interface WeekSessionDto {
  attendanceLogId: number;
  contractId: number;
  contractIdDisplay: string;
  date: string;
  startTime: string;
  endTime?: string;
  studentName: string;
  teacherName: string;
  status: string;
  hoursUsed: number;
}

export interface TeacherAlertDto {
  type: string;
  message: string;
}

// Assigned students — Master Doc 11B B3. No parent contact.
export interface TeacherAssignedStudentDto {
  studentId: number;
  studentName: string;
  gradeLevel: string;
  subjects: string;
  contractStatus: string;
  contractIdDisplay: string;
  remainingHours: number;
}

// Profile read-only — no prices
export interface TeacherProfileDto {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  educationLevel: string;
  bio?: string;
  specializations?: string;
  verificationStatus: string;
}

export interface TeacherAvailabilityDto {
  id?: number;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  isAvailable: boolean;
}

export interface CheckInRequest {
  contractId: number;
}

export interface CheckOutRequest {
  sessionId: number;
  lessonNotes: string;
}

export const HOMEWORK_STATUS = { Assigned: 1, Submitted: 2, Graded: 3, Overdue: 4 } as const;

export interface HomeworkDto {
  id: number;
  studentId: number;
  studentName: string;
  teacherId: number;
  teacherName: string;
  contractSessionId?: number;
  contractIdDisplay?: string;
  title: string;
  description?: string;
  dueDate: string;
  status: number;
  statusText: string;
  submittedAt?: string;
  gradedAt?: string;
  teacherFeedback?: string;
  createdAt: string;
}

export interface CreateHomeworkRequest {
  studentId: number;
  contractSessionId?: number;
  title: string;
  description?: string;
  dueDate: string;
}

export interface UpdateHomeworkStatusRequest {
  status: number;
  teacherFeedback?: string;
}

export interface StudentGradeDto {
  id: number;
  studentId: number;
  studentName: string;
  teacherId: number;
  teacherName: string;
  contractSessionId?: number;
  contractIdDisplay?: string;
  title: string;
  gradeValue: string;
  maxValue?: number;
  gradeDate: string;
  notes?: string;
  createdAt: string;
}

export interface CreateGradeRequest {
  studentId: number;
  contractSessionId?: number;
  title: string;
  gradeValue: string;
  maxValue?: number;
  gradeDate: string;
  notes?: string;
}
