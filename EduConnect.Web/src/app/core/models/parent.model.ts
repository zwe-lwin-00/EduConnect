// Parent's view of their child (student) — Master Doc 11C Phase 1. No student login.
export interface ParentStudentDto {
  id: number;
  firstName: string;
  lastName: string;
  gradeLevel: string;
  totalRemainingHours: number;
  assignedTeacherName?: string;
  activeContractsCount: number;
}

// Student learning overview (read-only) — Master Doc 11C C1
export interface StudentLearningOverviewDto {
  studentId: number;
  studentName: string;
  gradeLevel: string;
  assignedTeachers: AssignedTeacherDto[];
  subjects: string;
  totalRemainingHours: number;
  upcomingSessions: UpcomingSessionDto[];
  completedSessions: CompletedSessionDto[];
}

export interface AssignedTeacherDto {
  teacherId: number;
  teacherName: string;
  contractIdDisplay: string;
  remainingHours: number;
}

export interface UpcomingSessionDto {
  contractIdDisplay: string;
  teacherName: string;
  remainingHours: number;
}

export interface CompletedSessionDto {
  sessionId: number;
  checkInTime: string;
  checkOutTime?: string;
  hoursUsed: number;
  lessonNotes?: string;
  progressReport?: string;
  teacherName: string;
}
