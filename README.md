# EduConnect Freelance School

A premium, agency-based tutoring service platform that bridges the gap between high-quality educators and parents seeking personalized primary education for P1-P4 students in Yangon, Myanmar.

## 🏗️ Architecture

This project follows **Clean Architecture** principles with a feature-based, scalable structure for both backend and frontend.

### Backend (.NET 8.0)
- **EduConnect.API** - Web API layer (Controllers, Middleware, Extensions)
- **EduConnect.Application** - Business logic layer (Feature-based Services, DTOs, Common)
- **EduConnect.Domain** - Domain entities and core business models
- **EduConnect.Infrastructure** - Data access layer (EF Core, Dapper, External services)
- **EduConnect.Shared** - Shared enums and common types

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
- **Logging**: Serilog
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
    └── Enums/                         # Shared enums
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
│   │   ├── admin/                     # Sidebar layout; dashboard, teachers, parents, students, contracts, attendance, payments, reports
│   │   ├── teacher/                   # Sidebar layout; dashboard, availability, students, sessions, group-classes, homework-grades, calendar, profile
│   │   └── parent/                    # Sidebar layout; my students, student learning
│   └── services/                     # Shared API service
```

## 👤 Roles & Access

| Role   | Access |
|--------|--------|
| **Admin**  | Full control: dashboard, teachers (onboard/edit/verify/reject/activate), parents & students, contracts, attendance, payments, reports. All users created by Admin. |
| **Teacher**| Has account (created by Admin). Dashboard, weekly availability, assigned students, **sessions** (one-to-one and **group class** check-in/check-out with lesson notes), **group classes** (view/edit name, Zoom, active; manage enrollments—**admin creates** group classes and assigns teacher), **Profile** (read-only core data; can set **Zoom join URL** for 1:1 teaching). Each teacher uses their own Zoom account. No pricing or parent contact. |
| **Parent** | **Has account** (created by Admin via "Create Parent"). Logs in with email/password; sees My Students list and student learning overview (assigned teacher, sessions, progress). Read-only; no self-registration. |

**Students (P1–P4)** do not have login accounts in Phase 1. They are linked to a parent; the parent views all student data (contracts, sessions, progress) under their own account. Optionally, student accounts could be added later (e.g. so students can log in to view their own schedule).

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
   Overview: alerts, today’s sessions, pending teacher verifications, revenue. Quick links to Teachers, Parents, Students, Contracts, Attendance, Payments, Reports.

2. **Teachers** (`/admin/teachers`)  
   - **Onboard** new teacher (email, name, phone, NRC, education, hourly rate, bio, specializations). System creates account and can show temporary credentials.  
   - **Verify / Reject** pending teachers.  
   - **Edit** profile, **Reset password**, **Activate / Suspend**.

3. **Parents & Students** (`/admin/parents`, `/admin/students`)  
   - **Create parent**: In **Admin → Parents**, click **"Create Parent"**. Enter email, first name, last name, phone. After create, a popup shows **email** and **temporary password**—share these with the parent so they can log in (they must change password on first login).  
   - **Create students**: In **Admin → Students**, click **"Add Student"**. Select the **parent** from the dropdown and enter the child’s details (name, grade P1–P4, DOB, etc.). Each student is linked to one parent; the parent sees all their linked students under "My Students" when they log in.

4. **Contracts** (`/admin/contracts`)  
   - Create **contract** (teacher + student + package hours + start/end date).  
   - Contract moves to Active; remaining hours drive session usage.  
   - **Cancel** contract when needed.

5. **Group classes** (`/admin/group-classes`)  
   - **Create** group classes (name, assign teacher, optional Zoom join URL). Teacher must exist.  
   - **Edit** name, assigned teacher, Zoom URL, and active status. **Cannot change assigned teacher** when the class has enrollments (remove enrollments first or create a new group class).  
   - **Enroll** students (by contract for that teacher; one enrollment per student per group). Remove enrollments as needed.

6. **Attendance** (`/admin/attendance`)  
   - View **today’s sessions**; override check-in/check-out if needed.  
   - **Adjust hours** (deduct from contract remaining hours).

7. **Payments** (`/admin/payments`)  
   - **Credit** or **Deduct** hours on a contract (with reason).  
   - Student wallet balance reflects available hours.

8. **Reports** (`/admin/reports`)  
   - **Daily** and **monthly** reports (e.g. sessions, revenue) powered by Dapper.

### Teacher Flow

1. **Dashboard** (`/teacher`)  
   Summary and quick access to availability, students, and sessions.

2. **Availability** (`/teacher/availability`)  
   Set **weekly time slots** when the teacher is available. Admin uses this for scheduling (Phase 1: manual confirmation).

3. **Students** (`/teacher/students`)  
   View **assigned students** (from active contracts): name, grade, subjects, contract status, contract ID, remaining hours. No parent contact details.

4. **Sessions** (`/teacher/sessions`)  
   - **One-to-one**: Check-in per contract to start a session; check-out with lesson notes (required). Session is tied to contract; remaining hours are updated.  
   - **Group class**: Select a group class (with enrolled students), start a group session; check-out with lesson notes. Hours are split across enrolled students’ contracts.  
   - **Zoom**: If the teacher has set a Zoom join URL (in Profile for 1:1, or per group class), a **“Join Zoom meeting”** link appears when a session is in progress. Each teacher uses their own Zoom account and sets up meetings for their classes.  
   - Admin can override or adjust in Attendance.

5. **Group classes** (`/teacher/group-classes`)  
   - **Admin creates** group classes and assigns the teacher; teachers only see classes assigned to them.  
   - **Edit** name, Zoom URL, and active status; **enroll** students (by contract—one enrollment per student per group; requires an active 1:1 contract with that teacher). Each teacher uses their own Zoom account: create a meeting and paste the join link per class.  
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
- **Contracts** link Teacher ↔ Student (package hours, remaining hours, status).  
- **Sessions**: **One-to-one**—attendance (check-in/out, lesson notes) per contract; **group**—admin creates group classes and assigns teacher; teacher edits and enrolls students (by contract), starts a group session and checks out with notes; hours are deducted from each enrolled student’s contract.  
- **Zoom**: Each teacher uses their own Zoom account. They set a default Zoom join URL in Profile (for 1:1) and per group class; the app stores and shows “Join Zoom meeting” when a session is in progress. No Zoom API—teachers paste links from their Zoom account.  
- **Wallet** (student) and **transactions** track credit/deduct and balance.  
- **Reports** aggregate sessions and revenue for admin.

## 🎯 Domain Entities

- **ApplicationUser** - Extended Identity user with role management
- **TeacherProfile** - Teacher information, NRC (encrypted), verification status; optional **ZoomJoinUrl** (default for 1:1 teaching)
- **Student** - Student information linked to parent
- **ContractSession** - Rental agreement tracking (PackageHours, RemainingHours)
- **AttendanceLog** - One-to-one session tracking (check-in/out, lesson notes); optional **ZoomJoinUrl** per session
- **TeacherAvailability** - Weekly availability schedule
- **GroupClass** - Group class (teacher, name, active); optional **ZoomJoinUrl** for that class. Created by admin; teacher validated on create; assigned teacher cannot be changed when enrollments exist.
- **GroupClassEnrollment** - Student enrolled in a group class (one per student per class; links to that teacher’s contract for hour deduction)
- **GroupSession** - One delivered group session (check-in/out, lesson notes, duration); optional **ZoomJoinUrl** per session
- **GroupSessionAttendance** - Per-student attendance and hours deducted for a group session
- **StudentWallet** - Hours balance management
- **TransactionHistory** - Transaction records
- **Homework** - Teacher–student assignments (title, description, due date, status: Assigned / Submitted / Graded / Overdue, teacher feedback)
- **StudentGrade** - Grades and assessments (title, grade value, optional max value, date, notes) linked to teacher and student
- **RefreshToken** - Stored hashed refresh tokens per user; used to issue new access tokens without re-login; revoked on logout or after rotation

## ✨ Key Features

### ✅ Completed
- [x] Clean Architecture project structure
- [x] Feature-based organization (backend & frontend)
- [x] JWT Authentication with login/logout and role-based redirect (Admin / Teacher / Parent)
- [x] Admin account auto-creation on startup
- [x] **Admin**: Dashboard (alerts, today’s sessions, pending actions, revenue), Teachers (onboard, edit, verify, reject, activate/suspend), Parents & Students (create, list), Contracts (create, activate, cancel), **Group classes** (create, assign teacher, optional Zoom; edit name/teacher/Zoom/active; enroll students by contract; cannot change teacher when enrollments exist), Attendance (today, override check-in/out, adjust hours), Payments (wallet credit/deduct), Reports (daily/monthly)
- [x] **Teacher**: Dashboard, availability (weekly), assigned students, **sessions** (one-to-one and **group class** check-in/check-out with lesson notes), **Group classes** (edit name/Zoom/active, enroll students by contract—admin creates and assigns; set Zoom link per class), **Homework & Grades** (assign homework, mark submitted/graded with feedback, add grades for assigned students), profile (read-only core data; **Zoom join URL** for 1:1 teaching)
- [x] **Parent**: My Students list, student learning overview (assigned teacher, sessions, progress, **homework and grades** from teachers)
- [x] Teacher management: onboard, **edit** (name, phone, education, hourly rate, bio, specializations), verify, reject, activate/suspend
- [x] Check-in/check-out system (teacher + admin override)
- [x] Wallet logic (credit/deduct hours, student active/freeze)
- [x] Daily and monthly reports (Dapper-powered)
- [x] Exception handling middleware, Angular guards and interceptors
- [x] Database schema and relationships
- [x] API URL normalization (no double-slash); Swagger and browser auto-launch disabled by default
- [x] **UI**: PrimeNG-based professional design: **Login** (Card, InputText, Message, gradient background); **Admin & Teacher dashboards** (Card widgets, Tag for alert/session status, Skeleton loading, Message for errors); **Layouts** (PrimeNG Toolbar and Button in admin/teacher topbars; design tokens for sidebar and surface); **Tables** (Teachers, Contracts: Card wrapper, Toolbar with global search, Table loading state, empty-state message; success/error via Toast/MessageService instead of `alert()`). **Confirmations**: All destructive or important actions use **PrimeNG ConfirmDialog** (ConfirmationService) instead of browser `confirm()`—e.g. verify/reject teacher, reset password, cancel contract, override check-in/out, remove enrollment, check-in/group session. **Reject teacher** uses a **Dialog with required reason input** instead of `prompt()`. **Return URL**: After login with a valid `returnUrl`, a short “Redirecting – Taking you back…” toast is shown before navigating. Sidebar layout (admin, teacher, parent) with on/off toggle; favicon PNG; row number column (#) in tables.
- [x] **Two class types**: One-to-one (contract-based check-in/out) and **group classes** (create group, enroll students by contract, start/check-out group session; hours split across enrollments; parent notifications)
- [x] **Zoom for teaching**: Each teacher uses their own Zoom account. Teachers set default Zoom join URL in Profile (1:1) and per group class; app shows “Join Zoom meeting” when a session is in progress (no Zoom API—links stored and displayed)
- [x] **Refresh tokens**: Access token + refresh token on login; refresh token stored (hashed) in DB; 401 triggers refresh and retry; logout revokes refresh tokens; rotation on refresh

### 🚧 To Be Implemented
- [ ] Teacher NRC/certificate document upload and verification workflow
- [ ] Scheduling approval workflow (admin confirms slots)
- [ ] Payment integration (KBZPay/Wave) – Phase 1 is manual confirmation
- [ ] Gemini API integration for progress reports

## ⚙️ Configuration (all dynamic – no hardcoding)

### Backend (appsettings.json / appsettings.Development.json)

All backend config is driven by `appsettings.json`. Override per environment with `appsettings.Development.json` or `appsettings.Production.json`.

| Section | Key | Description |
|--------|-----|-------------|
| **ConnectionStrings** | DefaultConnection | SQL Server connection string |
| **Cors** | AllowedOrigins | Semicolon-separated origins (e.g. `http://localhost:5480;https://localhost:5480`) |
| **JwtSettings** | SecretKey, Issuer, Audience, ExpirationInMinutes | JWT auth |
| **Encryption** | Key | AES key for sensitive data |
| **SeedData** | **Roles** | Array of role names to seed (default: Admin, Teacher, Parent) |
| **SeedData:DefaultAdmin** | Email, Password, FirstName, LastName, PhoneNumber | Default admin account (created on first run) |
| **Gemini** | ApiKey | Optional Gemini API key |

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

## 📊 Development Guidelines

### Backend
- Use **Dapper** for all read/search queries for optimal performance
- Use **EF Core** for write operations and Identity management
- Follow feature-based organization when adding new features
- Implement services in `Infrastructure/Services/` (implements Application interfaces)
- Use `BaseController` for common controller functionality
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
**Version**: 1.3.0

---

For a step-by-step view of how users move through the app (login → role redirect → admin/teacher/parent flows and data relationships), see [Project Flow](#-project-flow) above.
