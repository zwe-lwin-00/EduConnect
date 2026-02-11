# EduConnect – Flow logic verification

This document records a **detailed trace** of the project flow to ensure nothing is out of logic. Use it when changing flows or adding features.

---

## 1. Intended flow (order of operations)

| Step | Who | Action |
|------|-----|--------|
| 1 | Admin | Create **Teachers**, **Parents**, **Students** (linked to parent). |
| 2 | Admin | **Subscriptions** (Add paid months): Student + Type (One-to-one or Group) + Duration. |
| 3 | Admin | **One-To-One**: New class → Student (active One-to-one subscription only), Teacher, optional schedule. Creates ContractSession. |
| 4 | Admin | **Group**: Create group (name, teacher, schedule) → Enroll by Group subscription or by 1:1 contract (same teacher). |
| 5 | Teacher | Sessions: check-in/check-out 1:1 and group; lesson notes. |
| 6 | Calendars | Teacher & parent month view: 1:1 + group, DateYmd, Completed/Upcoming/Holiday. |

---

## 2. Access and validation (backend)

### ContractSession (1:1)

- **HasActiveAccess()** (Domain):
  - If `Status != Active` → false.
  - If `SubscriptionId` set and `Subscription` loaded → `Subscription.HasActiveAccess()`.
  - Else (legacy) → `SubscriptionPeriodEnd.HasValue && SubscriptionPeriodEnd >= UtcNow`.
- **Subscription.HasActiveAccess()**: `Status == Active && SubscriptionPeriodEnd >= UtcNow`.
- **ScheduleValidation.Validate(daysOfWeek, startTime, endTime)** (Application):
  - Days: comma-separated 1–7 (1=Mon … 7=Sun), no duplicates.
  - If both start and end time set → end must be **after** start.

**Used in:**

- `AdminService.CreateContractAsync`: subscription must be same student, type OneToOne, active; then `ScheduleValidation.Validate`.
- `AttendanceService.CheckInAsync`: contract must exist, same teacher, active, and `HasActiveAccess()`.
- `GroupClassService.EnrollStudentAsync`: contract must exist, same teacher/student, active, `HasActiveAccess()`.
- Teacher/Parent calendars: only include contracts with schedule when `!string.IsNullOrWhiteSpace(DaysOfWeek) && StartTime.HasValue`; access via `HasActiveAccess()` or equivalent (subscription/period).

### GroupClassEnrollment

- **By Group subscription**: `GroupClassService.EnrollStudentBySubscriptionAsync` — subscription must be same student, type Group, `HasActiveAccess()`, not already enrolled.
- **By 1:1 contract**: `GroupClassService.EnrollStudentAsync` — contract same teacher/student, `HasActiveAccess()`, not already enrolled.
- **HasGroupEnrollmentAccess** (ParentService): enrollment has access if (SubscriptionId set and subscription active and period valid) or (ContractId set and contract has active access).

**Used in:**

- Parent month calendar: only group sessions for enrollments where `HasGroupEnrollmentAccess(e, now)` and group is active.

### Group class (admin)

- **Create**: `GroupClassService.CreateByAdminAsync` — name required, teacher exists, `ScheduleValidation.Validate(DaysOfWeek, StartTime, EndTime)`.
- **Update**: `GroupClassService.UpdateByAdminAsync` — if `TeacherId` changes and `Enrollments.Count > 0` → throw `HAS_ENROLLMENTS`; else `ScheduleValidation.Validate`; on schedule change, teacher is notified.

---

## 3. Parent access (backend)

All parent-facing APIs that take a student id use the **current user id** (parent) and enforce **student belongs to that parent**:

- `GetMyStudentsAsync(parentUserId)`: `Where(s => s.ParentId == parentUserId && s.IsActive)`.
- `GetStudentLearningOverviewAsync(parentUserId, studentId)`: `FirstOrDefaultAsync(s => s.Id == studentId && s.ParentId == parentUserId)` → null if not found (controller returns 404).
- `GetSessionsForStudentWeekAsync(parentUserId, studentId, …)`: same filter → empty list if not found.
- `GetSessionsForStudentMonthAsync(parentUserId, studentId, …)`: same filter → empty list if not found.

So a parent **cannot** see another parent’s student data.

---

## 4. Frontend consistency

### Auth and guards

- **authGuard**: no JWT → redirect to `/auth/login` with `returnUrl=state.url`.
- **roleGuard**: wrong role → redirect to login with `returnUrl`.
- **Login**: after success, redirect to `returnUrl` **only if** `returnUrl.startsWith(roleHome)` (e.g. Admin → `/admin`, Teacher → `/teacher`, Parent → `/parent`). So a parent never gets sent to `/admin` via returnUrl.

### One-To-One (admin) create

- Dropdown loads **active One-to-one subscriptions** only: `status === 1 && subscriptionPeriodEnd >= today`.
- On submit: `studentId: sub.studentId`, `subscriptionId: sub.id` (from selected option). Backend validates subscription same student and type.

### Group class (admin) enrollments

- **Assignable contracts**: from `getContracts(gc.teacherId, undefined, 1)` (same teacher, status Active); exclude already enrolled students.
- **Assignable group subscriptions**: from `getSubscriptions(..., Group, 1)` then filter `status === 1 && subscriptionPeriodEnd >= today`; exclude already enrolled.
- Enroll by contract: `{ studentId: contract.studentId, contractId: contract.id }`.
- Enroll by subscription: `{ studentId: sub.studentId, subscriptionId: sub.id }`.
- **Edit group**: Teacher dropdown disabled when `selectedClass.enrolledCount > 0` (backend still rejects change with clear error if bypassed).

### Parent student learning

- **Route** `/parent/student/:studentId`: in `ngOnInit`, `studentId` is parsed from route. If not a valid integer ≥ 1, set `error = 'Invalid student'`, `loading = false`, and **do not** call API. Valid id → call `getStudentLearningOverview`, `loadWeek`, `loadMonth`.

---

## 5. Calendars (scheduled vs completed)

- **Teacher** (TeacherService): Upcoming/scheduled 1:1 and group only when `!string.IsNullOrWhiteSpace(DaysOfWeek) && StartTime.HasValue`. Completed from AttendanceLogs / GroupSessions.
- **Parent** (ParentService): Same idea — contracts and group classes with schedule; month calendar uses `HasActiveAccess()` for 1:1 and `HasGroupEnrollmentAccess` for group enrollments.

---

## 6. Quick regression checklist

When changing flows, re-check:

1. **Contract create**: Subscription dropdown = active One-to-one only; backend checks same student, type, active.
2. **Group enroll**: Assignable = same teacher (contracts) or Group type + active (subscriptions); exclude enrolled; backend validates type and access.
3. **Group edit**: Backend blocks teacher change when enrollments exist; frontend disables teacher dropdown when enrollments > 0.
4. **Check-in (1:1)**: Backend requires `HasActiveAccess()` on contract.
5. **Calendars**: Only show scheduled when DaysOfWeek and StartTime set; use correct access (HasActiveAccess / HasGroupEnrollmentAccess).
6. **Parent**: All student APIs filter by ParentId; invalid studentId in URL handled in frontend (no API call).

---

*Verified: February 2026 (full codebase trace).*
