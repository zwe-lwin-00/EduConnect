# EduConnect Freelance School

A premium, agency-based tutoring service platform that bridges the gap between high-quality educators and parents seeking personalized primary education for P1-P4 students in Yangon, Myanmar.

## 🏗️ Architecture

This project follows **Clean Architecture** principles with a feature-based, scalable structure for both backend and frontend.

### Backend (.NET 8.0)
- **EduConnect.API** - Web API layer (Controllers, Middleware, Extensions)
- **EduConnect.Application** - Business logic layer (Feature-based Services, DTOs, Common)
- **EduConnect.Domain** - Domain entities and core business models
- **EduConnect.Infrastructure** - Data access layer (EF Core, Dapper, External services)
- **EduConnect.Shared** - Shared enums, common types, and logging extensions (LoggerExtensions for consistent tracing)

### Frontend (Angular 20)
- **EduConnect.Web** - Angular SPA with PrimeNG components
  - Feature-based modules (lazy-loaded)
  - Core services, guards, and interceptors
  - Shared components and layout

## 🚀 Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0 Web API
- **ORM**: Entity Framework Core 8.0 (for writes/Identity)
- **Micro-ORM**: Dapper (for reads/reporting)
- **Database**: SQL Server
- **Authentication**: JWT with Refresh Tokens
- **Logging**: Serilog (console + file); LoggerExtensions in Shared for consistent Method/LineNumber and credential redaction
- **Security**: AES-256 encryption for sensitive data
- **Architecture**: Clean Architecture with Feature-based organization

### Frontend
- **Framework**: Angular 20
- **UI Components**: PrimeNG 19 (Card, Table, Dialog, Button, Tag, Toolbar, Skeleton, Message, Toast, InputText, etc.) with Aura theme
- **Theme**: PrimeNG design tokens (Aura); consistent spacing and typography
- **State Management**: Angular Signals
- **Architecture**: Feature modules with lazy loading

## 📋 Prerequisites

- .NET 8.0 SDK
- Node.js 18+ and npm
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 / VS Code / Rider (optional)

## 🔌 Port reference

| Service   | Port | URL |
|-----------|------|-----|
| **Backend API** | **5049** | `http://localhost:5049` (API base; frontend calls `http://localhost:5049/api`) |
| **Frontend (Angular)** | **5480** | `http://localhost:5480` |
| **Health (liveness)** | 5049 | `http://localhost:5049/health/live` |
| **Health (readiness)** | 5049 | `http://localhost:5049/health/ready` |

The frontend (`EduConnect.Web/src/environments/environment.ts`) must have `apiUrl: 'http://localhost:5049/api'`. The backend CORS (`appsettings.json` / `appsettings.Development.json`) must allow `http://localhost:5480`. If you see "Cannot connect to server", ensure (1) the API is running (`dotnet run` in `EduConnect.API`) and (2) these ports match.

## 🛠️ Quick Start

### Step 1: Start the Backend API

```bash
cd EduConnect.API
dotnet run
```

**Look for this output:**
```
Now listening on: http://localhost:5049
Default admin account created:
Email: admin@educonnect.com
Password: 1qaz!QAZ
```

**Note the port number** – you'll need it for frontend configuration. The API does not open a browser by default.

### Step 2: Configure Frontend API URL

Edit `EduConnect.Web/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5049/api'  // Match the port from Step 1
};
```

### Step 3: Start the Frontend

```bash
cd EduConnect.Web
npm install
npm start
```

The app will open at `http://localhost:5480`

**Note:** The frontend uses PrimeNG with Angular 20. `EduConnect.Web/.npmrc` sets `legacy-peer-deps=true` so `npm install` works without conflicts (PrimeNG 19 lists Angular 19 as peer).

### Step 4: Login

1. Navigate to `http://localhost:5480` (redirects to `/auth/login`)
2. Enter credentials:
   - **Email:** `admin@educonnect.com`
   - **Password:** `1qaz!QAZ`
3. Click "Login"
4. You'll be redirected to `/admin` dashboard

⚠️ **IMPORTANT:** Change the default admin password immediately after first login!

## 🔐 Default Admin Account

When you first run the application, a default admin account is automatically created:

- **Email:** `admin@educonnect.com`
- **Password:** `1qaz!QAZ`

The account is created automatically when the API starts for the first time. Check the API console output for confirmation.

## 📚 Class types

The platform has **two class types**:

| Type | Description | In the system |
|------|-------------|---------------|
| **One-To-One** | One teacher, one student | Stored as a **Contract** (ContractSession). Admin creates under **One-To-One**; teacher delivers sessions and sets Zoom. |
| **Group** | One teacher, multiple students | Stored as **GroupClass** plus enrollments. Admin creates under **Group**; teacher adds Zoom and manages enrollments. |

In the UI, the admin sidebar uses **One-To-One** and **Group** so both types are clear. The technical term "Contract" means an One-To-One class.

## 💳 Billing Model

**Parent-paid subscriptions** drive class access. A parent pays for a **subscription** for a student: they choose **type** (One-To-One or Group) and **duration** (e.g. 1 or 2 months). Admin creates the subscription (student, type, duration). Admin then **creates classes** and **assigns** who teaches and who attends:

- **One-To-One class**: Admin creates a One-To-One class by assigning a teacher and a student, and linking the student’s active **One-To-One** subscription. One Contract (ContractSession) per teacher–student pair; access follows that subscription’s period.
- **Group class**: Admin creates a Group class (teacher, name) and **assigns students** by selecting each student’s active **Group** subscription. Enrollments can be subscription-backed (no One-To-One contract required) or legacy contract-backed.

Subscriptions have a **subscription period** (start to end date). Admin can **renew** in two ways: (1) from **Subscriptions/Payments**: pick a One-To-One class (contract) and renew—if it is subscription-backed, the linked Subscription is extended by one month; otherwise the legacy period is extended. (2) By **subscription id** (`POST /admin/subscriptions/{id}/renew?additionalMonths=1`) to extend any subscription by N months. No hour credit/deduct; session duration (`HoursUsed`) is recorded for reporting only.

### Configuration (appsettings)

Business rules and thresholds are **driven by config** (no hardcoded magic numbers). In **appsettings.json** (or environment):

- **App** – Dashboard and reporting: `SubscriptionExpiringAlertDays` (default 7), `ContractExpiringAlertDays` (default 14), `ContractEndingSoonNotificationDays` (default 14), `DefaultHourlyRateForRevenue` (default 0, used when teacher rate is not set), `GroupSessionMinHoursPerStudent` (default 0.25), `PasswordGenerationLength` (default 12).
- **SeedData** – Default admin and roles.
- **JwtSettings**, **Cors**, **TimeZone**, **RateLimiting**, **Encryption**, **HealthChecks** – as used by the API.

Teacher **hourly rate** is not used in the subscription flow; it is optional (default 0) and only used for **revenue reporting** when set, or `App:DefaultHourlyRateForRevenue` when not.

## 📁 Project Structure

### Backend Structure

```
EduConnect/
├── EduConnect.API/                    # Web API Layer
│   ├── Controllers/                   # Feature-based controllers
│   ├── Middleware/                    # Custom middleware
│   ├── Extensions/                    # Extension methods
│   └── Program.cs                     # Application startup
│
├── EduConnect.Application/            # Business Logic Layer
│   ├── Common/                        # Shared concerns
│   ├── Features/                      # Feature-based organization
│   │   ├── Auth/
│   │   ├── Admin/
│   │   ├── Teachers/
│   │   ├── Students/
│   │   ├── Contracts/
│   │   ├── GroupClass/
│   │   └── Attendance/
│   └── DTOs/                          # Data Transfer Objects
│
├── EduConnect.Domain/                 # Domain Layer
│   └── Entities/                      # Domain entities
│
├── EduConnect.Infrastructure/         # Infrastructure Layer
│   ├── Data/                          # DbContext, Dapper
│   ├── Repositories/                  # UnitOfWork pattern
│   └── Services/                      # External services
│
└── EduConnect.Shared/                 # Shared Layer
    ├── Enums/                         # Shared enums
    └── Extensions/                    # LoggerExtensions (tracing, no credentials)
```

### Frontend Structure

```
EduConnect.Web/
├── src/app/
│   ├── core/                          # Core functionality
│   │   ├── constants/                 # API endpoints, app config
│   │   ├── guards/                    # Auth & role guards
│   │   ├── interceptors/               # HTTP interceptors
│   │   ├── models/                    # TypeScript models
│   │   └── services/                  # Auth, admin, teacher, parent services
│   ├── shared/                        # Shared components & layout
│   ├── features/                      # Feature modules (lazy-loaded)
│   │   ├── auth/                      # Login, change password
│   │   ├── admin/                     # Sidebar layout; dashboard, teachers, parents, students, One-To-One, Group, attendance, payments, reports
│   │   ├── teacher/                   # Sidebar layout; dashboard, availability, students, sessions, group-classes, homework-grades, calendar, profile
│   │   └── parent/                    # Sidebar layout; my students, student learning
│   └── services/                     # Shared API service
```

## 👤 Roles & Access

| Role   | Access |
|--------|--------|
| **Admin**  | Full control: dashboard, teachers (onboard/edit/verify/reject/activate), parents & students, **One-To-One** classes, **Group** classes, attendance, payments, reports, **Settings** (holidays, other key-value settings). All users created by Admin. |
| **Teacher**| Has account (created by Admin). Dashboard, weekly availability, assigned students, **sessions** (One-To-One and **Group** check-in/check-out with lesson notes), **Group** (view/edit name, **Zoom link**, active; schedule set by admin; manage enrollments), **Profile** (read-only core data; Zoom join URL for One-To-One). Each teacher uses their own Zoom account. No pricing or parent contact. |
| **Parent** | **Has account** (created by Admin via "Create Parent"). Logs in with email/password; sees My Students list and student learning overview (assigned teacher, sessions, progress). Read-only; no self-registration. |

**Students (P1–P4)** do not have login accounts in Phase 1. They are linked to a parent; the parent views all student data (One-To-One classes, Group enrollments, sessions, progress) under their own account. Optionally, student accounts could be added later (e.g. so students can log in to view their own schedule).

### Parent + student accounts (how it works)

Because students are young children (P1–P4), **only the parent has a login**. Think of it as a **parent account that includes access to their child(ren)**:

1. **One parent account** = one login (email + password). That account can have **multiple students** (children) linked to it.
2. **Admin creates the parent account**: Go to **Admin → Parents** (sidebar), then click the green **"Create Parent"** button. Enter the parent’s email, first name, last name, and phone. After saving, a **credentials popup** appears with the new **email** and **temporary password**. Share these with the parent (e.g. copy to clipboard and send securely). The parent must change the password on first login.
3. **Admin then links students to that parent**: Go to **Admin → Students** → **"Add Student"**. For each child, select the **parent** (from the dropdown), enter the student’s name, grade (P1–P4), DOB, etc. Each student is linked to exactly one parent.
4. **Parent logs in** at `/auth/login` with the credentials you shared. They see **My Students** (all their linked children) and can open each one to see the **student learning overview** (teachers, sessions, homework, grades). No separate “student account”—everything is under the parent’s account.

So: **parent account = parent + their students**. Admin creates the parent first, shares the login details, then adds students and assigns them to that parent.

## 🔄 Project Flow

### Entry & Authentication

1. **Login** (`/auth/login`)  
   User enters email and password. If already authenticated, user is redirected by role (no login form shown). Otherwise, API returns JWT and user role. If `mustChangePassword` is true, user is sent to Change Password; otherwise redirected by role. If the user had been sent to login with a `returnUrl` (e.g. after session expiry), a short **“Redirecting – Taking you back to where you were…”** toast is shown, then they are sent back to that URL when it belongs to their role (e.g. `/admin/teachers` for Admin).
   - **Admin** → `/admin` (dashboard home)
   - **Teacher** → `/teacher` (dashboard)
   - **Parent** → `/parent` (my students list)

2. **Guards**  
   All `/admin`, `/teacher`, and `/parent` routes are protected by `authGuard` (valid JWT) and `roleGuard` (correct role). Unauthorized or wrong-role access redirects to login (with `returnUrl` or `unauthorized=1` for access-denied).

### Admin Flow

1. **Dashboard** (`/admin`)  
   Overview: alerts, today’s sessions, pending teacher verifications, revenue. Quick links to Teachers, Parents, Students, One-To-One, Group, Attendance, Payments, Reports.

2. **Teachers** (`/admin/teachers`)  
   - **Onboard** new teacher (email, name, phone, NRC, education, bio, specializations). System creates account and can show temporary credentials.  
   - **Verify / Reject** pending teachers.  
   - **Edit** profile, **Reset password**, **Activate / Suspend**.

3. **Parents & Students** (`/admin/parents`, `/admin/students`)  
   - **Create parent**: In **Admin → Parents**, click **"Create Parent"**. Enter email, first name, last name, phone. After create, a popup shows **email** and **temporary password**—share these with the parent so they can log in (they must change password on first login).  
   - **Create students**: In **Admin → Students**, click **"Add Student"**. Select the **parent** from the dropdown and enter the child’s details (name, grade P1–P4, DOB, etc.). Each student is linked to one parent; the parent sees all their linked students under "My Students" when they log in.

4. **Subscriptions** (parent-paid)  
   - Create **subscription** for a student: type (One-To-One or Group), duration (e.g. 1 or 2 months).  
   - List/filter by student, type, status. **Renew** a subscription to extend by one or more months.

5. **One-To-One** (`/admin/contracts`) — One-To-One classes  
   - Create **One-To-One class** (teacher + student). Either link an active **One-To-One subscription** for that student, or use legacy (start/end date; period = calendar month of start date). Optional **schedule** (days of week, start/end time). Schedule is validated (days 1–7, end time after start).  
   - Class moves to Active; access follows the linked subscription or legacy period. **Cancel** when needed.

6. **Group** (`/admin/group-classes`)  
   - **Create** group classes: name, assign teacher, **schedule** (days of week, start/end time). **Zoom link is not set here**—the assigned teacher adds it later. Teacher must exist. Schedule is validated (days 1–7, end time after start).  
   - **Edit** name, assigned teacher, schedule (days, start/end time), and active status. **Cannot change assigned teacher** when the class has enrollments (remove enrollments first or create a new group class). Zoom is left unchanged (teacher-only). **Unexpected schedule changes** (e.g. days or times) are validated; the assigned teacher is **notified** when the schedule is updated so they are aware of changes.  
   - **Enroll** students: by that student’s active **Group subscription** (admin) or by One-To-One class (contract) with that teacher (teacher/legacy). One enrollment per student per group. Remove enrollments as needed.

7. **Attendance** (`/admin/attendance`)  
   - View **today’s sessions**; override check-in/check-out if needed.  
   - **Adjust session hours** (edit recorded duration for reporting; no deduction).

8. **Payments / Subscriptions** (`/admin/payments`)  
   - **Renew subscription** for an One-To-One class (legacy: extends to end of next calendar month). Subscription-based renew is via Subscriptions (renew by subscription id, extend by N months).  
   - Monthly-only: no hour credit/deduct.

9. **Reports** (`/admin/reports`)  
   - **Daily** and **monthly** reports (e.g. sessions, revenue) powered by Dapper.

10. **Settings** (`/admin/settings`)  
   - **Holidays**: Add, edit, delete holiday dates (school closed / no classes). List filtered by calendar year (or all).  
   - **Other settings**: Key-value store (e.g. SchoolName, AcademicYearStart, Notice). Add or update by key; use for custom data and notices.

### Teacher Flow

1. **Dashboard** (`/teacher`)  
   Summary and quick access to availability, students, and sessions.

2. **Availability** (`/teacher/availability`)  
   Set **weekly time slots** when the teacher is available. Admin uses this for scheduling (Phase 1: manual confirmation).

3. **Students** (`/teacher/students`)  
   View **assigned students** (from active contracts): name, grade, subjects, contract status, contract ID, subscription valid until. No parent contact details.

4. **Sessions** (`/teacher/sessions`)  
   - **One-to-one**: Check-in per contract to start a session; check-out with lesson notes (required). Session is tied to contract; duration is recorded (monthly subscription).  
   - **Group class**: Select a group class (with enrolled students), start a group session; check-out with lesson notes. Duration is recorded per student (monthly subscription; no hour deduction). Enrolled students’ contracts.  
   - **Zoom**: If the teacher has set a Zoom join URL (in Profile for 1:1, or per group class), a **“Join Zoom meeting”** link appears when a session is in progress. Each teacher uses their own Zoom account and sets up meetings for their classes.  
   - Admin can override or adjust in Attendance.

5. **Group classes** (`/teacher/group-classes`)  
   - **Admin creates** group classes (name, teacher, schedule: days and start/end time) and assigns the teacher; teachers only see classes assigned to them. **Schedule is read-only** for the teacher.  
   - **Edit** name, **Zoom link**, and active status (teacher adds the Zoom meeting URL for the class); **enroll** students (by contract—one enrollment per student per group; requires an active 1:1 contract with that teacher). Each teacher uses their own Zoom account: create a meeting and paste the join link per class.  
   - Enrolled students are included when the teacher starts a group session from Sessions; the group’s Zoom link is shown for the in-progress session.

6. **Homework & Grades** (`/teacher/homework-grades`)  
   - **Assign homework** to assigned students (title, description, due date).  
   - **Mark** homework as Submitted or Graded (with optional teacher feedback).  
   - **Add grades** for students (title, value, optional max, date, notes).  
   - Only students with an active contract with the teacher can receive homework/grades. Parents see homework and grades in the student learning overview.

7. **Profile** (`/teacher/profile`)  
   Read-only view of own profile (name, email, education, rate, bio, specializations, verification status). **Zoom meeting (1:1)**: Teachers can set their default Zoom join URL for one-to-one sessions (e.g. Personal Meeting Room); used when they start a 1:1 session.

### Parent Flow

Parents **have their own login accounts**. Admin creates each parent (Create Parent) and can share or reset credentials so the parent can sign in.

1. **My Students** (`/parent`)  
   After login, the parent sees a list of students linked to their account. Each student can be opened for a detailed learning view.

2. **Student Learning** (`/parent/student/:studentId`)  
   For one student: assigned teacher, contract info, **sessions** (check-in/out, lesson notes), **homework** (from teachers, with status and feedback), **grades** (assessments from teachers), and progress. Read-only; no self-registration or editing in Phase 1.

### Data Flow Summary

- **Users & roles**: Admin creates all users (teachers, parents). Teachers are verified by admin.  
- **Subscriptions** (parent-paid): Student + type (One-To-One / Group) + duration. Admin creates and renews (extend by N months). **One-To-One** (Contract): Link Teacher ↔ Student; optionally backed by an active One-To-One subscription or legacy period.  
- **Sessions**: **One-To-One**—attendance (check-in/out, lesson notes) per Contract; **Group**—admin creates Group classes and assigns teacher; teacher edits and enrolls students (by One-To-One contract if needed), starts a Group session and checks out with notes; duration (HoursUsed) is recorded for reporting; no hour deduction.  
- **Zoom**: Each teacher uses their own Zoom account. They set a default Zoom join URL in Profile (for 1:1) and per group class; the app stores and shows “Join Zoom meeting” when a session is in progress. No Zoom API—teachers paste links from their Zoom account.  
- **Reports** aggregate sessions and revenue for admin.

## 🎯 Domain Entities

- **ApplicationUser** - Extended Identity user with role management
- **TeacherProfile** - Teacher information, NRC (encrypted), verification status; optional **ZoomJoinUrl** (default for 1:1 teaching)
- **Student** - Student information linked to parent
- **Subscription** - Parent-paid: student, type (OneToOne/Group), start/end period. Funds One-To-One class or Group enrollment.
- **ContractSession** - One-To-One class: teacher–student; optional SubscriptionId (One-To-One) or legacy SubscriptionPeriodEnd; optional schedule (DaysOfWeek, StartTime, EndTime) set by admin.
- **AttendanceLog** - One-to-one session tracking (check-in/out, lesson notes); optional **ZoomJoinUrl** per session
- **TeacherAvailability** - Weekly availability schedule
- **GroupClass** - Group class (teacher, name, active); **schedule** (DaysOfWeek, StartTime, EndTime) set by admin; **ZoomJoinUrl** set by the assigned teacher (not admin). Created by admin; assigned teacher cannot be changed when enrollments exist.
- **GroupClassEnrollment** - Student enrolled in a group class (one per student per class; links to that teacher’s contract subscription validity checked for access)
- **GroupSession** - One delivered group session (check-in/out, lesson notes, duration); optional **ZoomJoinUrl** per session
- **GroupSessionAttendance** - Per-student attendance and duration (HoursUsed) for a group session
- **StudentWallet** - (Legacy; monthly-only: not used for billing.)
- **TransactionHistory** - (Legacy; monthly-only: not used for billing.)
- **Homework** - Teacher–student assignments (title, description, due date, status: Assigned / Submitted / Graded / Overdue, teacher feedback)
- **StudentGrade** - Grades and assessments (title, grade value, optional max value, date, notes) linked to teacher and student
- **RefreshToken** - Stored hashed refresh tokens per user; used to issue new access tokens without re-login; revoked on logout or after rotation
- **Holiday** - Admin-defined holiday date (name, optional description); used for scheduling and reporting (e.g. no classes).
- **SystemSetting** - Key-value store for admin-configurable data (e.g. SchoolName, Notice); unique key, value, optional description.

## ✨ Key Features

### ✅ Completed
- [x] Clean Architecture project structure
- [x] Feature-based organization (backend & frontend)
- [x] JWT Authentication with login/logout and role-based redirect (Admin / Teacher / Parent)
- [x] Admin account auto-creation on startup
- [x] **Admin**: Dashboard (alerts, today’s sessions, pending actions, revenue), Teachers (onboard, edit, verify, reject, activate/suspend), Parents & Students (create, list), **One-To-One** (create with optional schedule—activate, cancel), **Group** (create with schedule, assign teacher—Zoom set by teacher; enroll students by One-To-One or Group subscription), Attendance, Subscriptions (renew monthly), Reports (daily/monthly), **Settings** (holidays, other key-value settings)
- [x] **Teacher**: Dashboard, availability (weekly), assigned students, **sessions** (One-To-One and **Group** check-in/check-out with lesson notes), **Group** (edit name/Zoom/active; enroll students by One-To-One contract; set Zoom per class), **Homework & Grades** (assign homework, mark submitted/graded, add grades), profile (read-only; **Zoom join URL** for One-To-One)
- [x] **Parent**: My Students list, student learning overview (assigned teacher, sessions, progress, **homework and grades** from teachers)
- [x] Teacher management: onboard, **edit** (name, phone, education, bio, specializations), verify, reject, activate/suspend
- [x] Check-in/check-out system (teacher + admin override)
- [x] Subscription billing (monthly only; renew via Admin), student active/freeze
- [x] Daily and monthly reports (Dapper-powered)
- [x] Exception handling middleware, Angular guards and interceptors
- [x] **Time zone**: All times use **Myanmar (Asia/Yangon, UTC+6:30)**—backend “today” and reports use Myanmar date; frontend displays dates/times with `+0630`; API serializes `DateTime` as UTC with `Z` for correct client display
- [x] **Admin reset teacher password**: Admin can reset a teacher’s password from Teachers list or teacher details; a new temporary password is shown and the teacher must change it on next login
- [x] **Logging**: Serilog file sink; LoggerExtensions (Shared) used in API and Infrastructure for traceable logs (Method/LineNumber, credential redaction)
- [x] **API & security**: Server-side validation (FluentValidation), rate limiting on login, health checks (liveness/readiness), consistent error contract (`error`, `code`, `details?`, `requestId?`)
- [x] Database schema and relationships
- [x] API URL normalization (no double-slash); Swagger and browser auto-launch disabled by default
- [x] **UI**: PrimeNG-based professional design: **Login** (Card, InputText, Message, gradient background); **Admin & Teacher dashboards** (Card widgets, Tag for alert/session status, Skeleton loading, Message for errors); **Layouts** (PrimeNG Toolbar and Button in admin/teacher topbars; design tokens for sidebar and surface); **Tables** (Teachers, Contracts: Card wrapper, Toolbar with global search, Table loading state, empty-state message; success/error via Toast/MessageService instead of `alert()`). **Confirmations**: All destructive or important actions use **PrimeNG ConfirmDialog** (ConfirmationService) instead of browser `confirm()`—e.g. verify/reject teacher, reset password, cancel contract, override check-in/out, remove enrollment, check-in/group session. **Reject teacher** uses a **Dialog with required reason input** instead of `prompt()`. **Return URL**: After login with a valid `returnUrl`, a short “Redirecting – Taking you back…” toast is shown before navigating. **Breadcrumbs**: Admin, Teacher, and Parent layouts show a PrimeNG breadcrumb (e.g. Home > Teachers, Home > Learning overview) above the main content so users know where they are and can navigate back. Sidebar layout (admin, teacher, parent) with on/off toggle; favicon PNG; row number column (#) in tables.
- [x] **Two class types**: **One-To-One** (contract-based check-in/out) and **Group** (create Group class, enroll students by One-To-One or Group subscription, start/check-out Group session; duration recorded per student; parent notifications)
- [x] **Schedule validation**: Days of week (1–7, no duplicates) and end time after start time for Group and One-To-One; teacher **notified** when admin changes a Group class schedule; enrollment by contract correctly checks access for subscription-backed contracts
- [x] **Zoom for teaching**: Each teacher uses their own Zoom account. Teachers set default Zoom join URL in Profile (One-To-One) and per Group class; app shows “Join Zoom meeting” when a session is in progress (no Zoom API—links stored and displayed)
- [x] **Refresh tokens**: Access token + refresh token on login; refresh token stored (hashed) in DB; 401 triggers refresh and retry; logout revokes refresh tokens; rotation on refresh
- [x] **Notifications – Mark all as read**: Notification bell (Admin, Teacher, Parent) includes “Mark all as read”; API `POST /notifications/mark-all-read` marks all stored notifications for the current user (better UX when there are many notifications)
- [x] **Sidebar UI/UX**: Professional sidebar (Admin/Teacher/Parent): PrimeIcons for nav items, two-line brand (EduConnect + role), dark gradients, section labels with borders, active state with left accent; parent layout uses same PrimeNG Toolbar as admin/teacher
- [x] **Responsive UI**: Viewport meta and 768px breakpoint; sidebar off-canvas + overlay on mobile; main padding 16px on small screens; PrimeNG tables scroll horizontally; toolbars and filters wrap; dashboard grids single column on mobile; reports, attendance, forms, and page containers full width on mobile; breadcrumb and auth pages avoid overflow
- [x] **UI/UX consistency (Admin, Teacher, Parent)**: All list/detail pages use PrimeNG (p-card, p-table, p-dialog, p-button, p-message). Consistent page headers (`page-header-compact` + short description), table loading states, empty states (icon + message + hint), and error handling (Toast/MessageService; no `alert()`). Group classes (admin & teacher), Attendance, Reports, Homework & Grades, Availability, Calendar, and Profile aligned with the same patterns. Parent dashboard and student learning overview use p-card, p-button, and consistent empty/error states.
- [x] **Project flow validation**: Auth (login, change-password, returnUrl, guards), 401 refresh, and role-based routing verified. API ↔ frontend IDs (contract, student, teacher, parent) and payloads checked; invalid Parent student route param (e.g. `/parent/student/xyz`) now handled without calling the API (show "Invalid student" and do not load).

### 🚧 To Be Implemented
- [ ] Teacher NRC/certificate document upload and verification workflow
- [ ] Scheduling approval workflow (admin confirms slots)
- [ ] Payment integration (KBZPay/Wave) – Phase 1 is manual confirmation
- [ ] Gemini API integration for progress reports

## ⚙️ Configuration (all dynamic – no hardcoding)

Backend and frontend use **dynamic config only**: no fallback literals in code for URLs, timezones, rate limits, or secrets. Backend requires the keys below (throws at startup if missing); frontend uses `environment.ts` / `environment.prod.ts` for API URL and timezone.

### Backend (appsettings.json / appsettings.Development.json)

All backend config is driven by `appsettings.json`. Override per environment with `appsettings.Development.json` or `appsettings.Production.json`.

| Section | Key | Description |
|--------|-----|-------------|
| **ConnectionStrings** | DefaultConnection | SQL Server connection string |
| **Cors** | AllowedOrigins | Semicolon-separated origins (e.g. `http://localhost:5480;https://localhost:5480`) |
| **JwtSettings** | SecretKey, Issuer, Audience, ExpirationInMinutes | JWT auth |
| **Encryption** | Key | AES key for sensitive data |
| **SeedData** | **Roles** | Array of role names to seed (default: Admin, Teacher, Parent) |
| **SeedData:DefaultAdmin** | Email, Password, FirstName, LastName, PhoneNumber, Role (optional; defaults to first in Roles) | Default admin account (created on first run) |
| **Gemini** | ApiKey | Optional Gemini API key |
| **RateLimiting** | GlobalPermitLimit, GlobalWindowMinutes, AuthPermitLimit, AuthWindowMinutes, RejectedMessage, RejectedCode | Rate limit and 429 response text |
| **TimeZone** | Id (e.g. Asia/Yangon), IdFallback (e.g. Myanmar Standard Time) | App timezone for "today" and reports |
| **HealthChecks** | LivePath, ReadyPath | Paths for liveness/readiness (default /health/live, /health/ready) |

Example override in `appsettings.Development.json`:

```json
{
  "Cors": { "AllowedOrigins": "http://localhost:5480;https://localhost:5480" },
  "SeedData": {
    "DefaultAdmin": {
      "Email": "admin@educonnect.com",
      "Password": "1qaz!QAZ",
      "FirstName": "Admin",
      "LastName": "User",
      "PhoneNumber": "+959123456789"
    }
  }
}
```

### Frontend (environment + app-config)

- **`src/environments/environment.ts`** – Development: set `apiUrl` to your API base (e.g. `http://localhost:5049/api`).
- **`src/environments/environment.prod.ts`** – Production: set `apiUrl` per deployment (replaced at build time).
- **`src/app/core/constants/app-config.ts`** – Central config re-export; use `appConfig` in services. Add more keys here as needed.

### Logging (Serilog + file, no credentials)

- **Console and file**: Serilog writes to console and to rolling text files under `Logs/` (path configurable via `Serilog:File:Path` in appsettings; default `Logs/educonnect-.txt` with daily rolling). `Serilog:File:RetainedFileCountDays` limits how many days of files to keep (e.g. 31).
- **Standard format**: Use `LoggerExtensions` from **`EduConnect.Shared/Extensions/LoggerExtensions.cs`** so each log line includes **Method** and **LineNumber** (via Serilog `LogContext`): `ErrorLog(ex, message)`, `ErrorLog(message)`, `InformationLog(message)`, `WarningLog(message)`, `DebugLog(message)`. Caller info is filled automatically via `[CallerMemberName]` and `[CallerLineNumber]`. The same extensions are used in **API** (controllers, middleware) and **Infrastructure** (services) for consistent, traceable logs.
- **No credentials**: All message strings passed to these extensions are redacted for common credential-like substrings (password, token, secret, authorization, etc.) before writing. Never log request bodies or headers that may contain credentials; use the extensions for consistent, safe logging.

### Production / Security

- **Do not commit production secrets.** For production, set sensitive values via environment variables or a secret store (e.g. Azure Key Vault), not in `appsettings.json`:
  - `ConnectionStrings:DefaultConnection`
  - `JwtSettings:SecretKey`
  - `Encryption:Key`
  - `SeedData:DefaultAdmin:Password` (or omit to use the fallback in code; change after first login)
- In development you can use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) so secrets stay out of the repo.

### Database Setup

The database is automatically created on first run. For manual setup or after adding new entities (e.g. Homework, StudentGrade):

```bash
cd EduConnect.API
dotnet ef migrations add YourMigrationName --project ../EduConnect.Infrastructure
dotnet ef database update --project ../EduConnect.Infrastructure
```

If you added **Homework**, **StudentGrade**, or **RefreshToken** entities, run a migration (e.g. `AddHomeworkAndStudentGrade`, `AddRefreshToken`) so the new tables are created.

## 🔧 Troubleshooting

### Login Issues

**Problem: "Login failed. Please try again."**

1. **Check Browser Console (F12)**
   - Open DevTools → Console tab
   - Look for detailed error messages
   - Check Network tab for failed requests

2. **Common Issues:**

   - **"Cannot connect to server"**
     - API is not running, or wrong port / CORS mismatch
     - Solution: See [Port reference](#-port-reference) above. Start the API first (`dotnet run` in `EduConnect.API` → port 5049). Ensure `environment.ts` has `apiUrl: 'http://localhost:5049/api'` and backend CORS allows `http://localhost:5480`.

   - **"Invalid email or password"**
     - Wrong credentials (case-sensitive)
     - Admin account not created
     - Solution: Check API console for "Default admin account created" message

   - **CORS Error**
     - Frontend URL not allowed
     - Solution: Check `Program.cs` CORS settings allow `http://localhost:5480`

   - **SSL Certificate Error or 404 after redirect**
     - API is redirecting HTTP to HTTPS but HTTPS isn't configured
     - Solution: HTTPS redirection is disabled in development mode. If you see 404 errors, ensure you're using HTTP (`http://localhost:5049`) and restart the API

3. **Test API Directly:**
   - Use a REST client (e.g. Postman or the `.http` file in the API project) to call `POST http://localhost:5049/api/auth/login` with JSON body `{ "email": "admin@educonnect.com", "password": "1qaz!QAZ" }`
   - Or re-enable Swagger in `Program.cs` and open `http://localhost:5049/swagger`

### API Connection Issues

- Verify API is running (check console output)
- Match API URL in `environment.ts` with actual API port
- Check CORS configuration in `Program.cs`
- **Important**: The API has HTTPS redirection disabled in development mode. Always use HTTP (`http://localhost:5049`) for local development
- If you see 404 errors after login attempts, restart the API to ensure the latest configuration is loaded
- For production HTTPS, trust dev certificate:
  ```bash
  dotnet dev-certs https --trust
  ```

## 🔒 Security

- **Encryption**: NRC numbers and sensitive data encrypted using AES-256
- **Authentication**: JWT access tokens (e.g. 60-minute expiration) and refresh tokens (e.g. 7-day expiration, stored hashed in DB). On 401 the client refreshes the access token and retries; logout revokes refresh tokens.
- **Authorization**: Role-based access control (Admin, Teacher, Parent)
- **CORS**: Configured to restrict API access to authorized domains
- **Password Policy**: 
  - Minimum 8 characters
  - Must contain uppercase, lowercase, digit, and special character
- **Account Lockout**: After 5 failed attempts, account locked for 5 minutes
- **Audit Logging**: All admin actions logged using Serilog

### Backend – API & Security

- **Request validation**: Server-side validation with **FluentValidation** on DTOs (e.g. `LoginRequest`, `CreateParentRequest`, `OnboardTeacherRequest`, `CreateStudentRequest`, `ChangePasswordRequest`, `RefreshTokenRequest`). Invalid payloads are rejected with a standard error response before hitting business logic.
- **Rate limiting**: Built-in ASP.NET Core rate limiter—global limit per IP and a stricter **auth** policy (e.g. 10 requests/minute) on `POST /api/auth/login` to reduce brute-force and abuse.
- **Health checks**: 
  - **Liveness**: `GET /health/live` – process is running (for containers/orchestration).
  - **Readiness**: `GET /health/ready` – DB connectivity check. Use for load balancers and Kubernetes readiness probes.
  - Legacy `GET /api/health` remains and documents the above endpoints.
- **Consistent error contract**: All API errors use the same JSON shape: `{ "error", "code", "details?", "requestId?" }`. Exception middleware and validation filter use the same format; `requestId` matches `HttpContext.TraceIdentifier` for log correlation.

## 📊 Development Guidelines

### Backend
- Use **Dapper** for all read/search queries for optimal performance
- Use **EF Core** for write operations and Identity management
- Follow feature-based organization when adding new features
- Implement services in `Infrastructure/Services/` (implements Application interfaces)
- Use `BaseController` for common controller functionality (provides `Logger` and `GetUserId()`; inject `ILogger<T>` and pass to base)
- Throw custom exceptions (`BusinessException`, `NotFoundException`)

### Frontend
- Use feature modules for new features (lazy-loaded)
- Prefer PrimeNG components (Card, Table, Toolbar, Button, Tag, Skeleton, Message, Toast, Dialog, InputText) for consistent UI; use design tokens (`var(--p-primary-*)`, `var(--p-surface-*)`, `var(--p-text-color)`) in component styles
- Use `MessageService` (Toast) for success/error feedback instead of `alert()`
- Place shared components in `shared/components/`
- Use Angular Signals for reactive state management
- Implement guards for route protection
- Use interceptors for HTTP request/response handling
- Follow TypeScript strict mode

## 📝 API Documentation

Swagger UI is **disabled by default**. To enable it, uncomment `app.UseSwagger()` and `app.UseSwaggerUI()` in `EduConnect.API/Program.cs` (inside the `if (app.Environment.IsDevelopment())` block). Then open `http://localhost:5049/swagger` in your browser. The API does not auto-open a browser on run; this is controlled in `EduConnect.API/Properties/launchSettings.json` (`launchBrowser: false`).

## 🧪 Testing

### Backend Testing
```bash
dotnet test
```

### Frontend Testing
```bash
cd EduConnect.Web
npm test
```

## 📦 Build & Deployment

### Backend
```bash
dotnet publish -c Release -o ./publish
```

### Frontend
```bash
cd EduConnect.Web
npm run build --configuration production
```

## 🤝 Contributing

1. Create a feature branch from `main`
2. Follow the project structure and naming conventions
3. Write clean, maintainable code
4. Add appropriate error handling
5. Update documentation as needed
6. Submit a pull request

## 📄 License

Proprietary - EduConnect Freelance School

## 📞 Support

For questions or support, contact the development team.

---

**Last Updated**: February 2026  
**Version**: 1.7.0

---

For a step-by-step view of how users move through the app (login → role redirect → admin/teacher/parent flows and data relationships), see [Project Flow](#-project-flow) above.
