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
