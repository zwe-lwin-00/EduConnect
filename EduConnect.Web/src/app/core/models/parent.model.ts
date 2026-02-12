// Parent's view of their child (student) — Master Doc 11C Phase 1. No student login.
export interface ParentStudentDto {
  id: number;
  fullName: string;
  gradeLevel: string;
  subscriptionValidUntil?: string | null;
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
  subscriptionValidUntil?: string | null;
  upcomingSessions: UpcomingSessionDto[];
  completedSessions: CompletedSessionDto[];
  homeworks: HomeworkItemDto[];
  grades: GradeItemDto[];
}

export interface HomeworkItemDto {
  id: number;
  title: string;
  description?: string;
  dueDate: string;
  status: number;
  statusText: string;
  teacherFeedback?: string;
  teacherName: string;
}

export interface GradeItemDto {
  id: number;
  title: string;
  gradeValue: string;
  maxValue?: number;
  gradeDate: string;
  notes?: string;
  teacherName: string;
}

export interface AssignedTeacherDto {
  teacherId: number;
  teacherName: string;
  contractIdDisplay: string;
  subscriptionPeriodEnd?: string | null;
}

export interface UpcomingSessionDto {
  contractIdDisplay: string;
  teacherName: string;
  subscriptionPeriodEnd?: string | null;
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
