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
- **EduConnect.Web** - Angular SPA with DevExtreme components
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
- **UI Components**: DevExtreme (Scheduler, DataGrid, etc.)
- **Theme**: Material Design
- **State Management**: Angular Signals
- **Architecture**: Feature modules with lazy loading

## 📋 Prerequisites

- .NET 8.0 SDK
- Node.js 18+ and npm
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 / VS Code / Rider (optional)

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

The app will open at `http://localhost:4200`

### Step 4: Login

1. Navigate to `http://localhost:4200` (redirects to `/auth/login`)
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
│   │   ├── constants/                 # API endpoints
│   │   ├── guards/                   # Route guards
│   │   ├── interceptors/              # HTTP interceptors
│   │   ├── models/                   # TypeScript models
│   │   └── services/                 # Core services
│   ├── shared/                       # Shared components
│   ├── features/                     # Feature modules
│   │   ├── auth/                     # Authentication
│   │   ├── admin/                    # Admin dashboard
│   │   ├── teacher/                  # Teacher dashboard
│   │   └── parent/                   # Parent dashboard
│   └── services/                     # Shared services
```

## 👤 Roles & Access

| Role   | Access |
|--------|--------|
| **Admin**  | Full control: dashboard, teachers (onboard/edit/verify/reject/activate), parents & students, contracts, attendance, payments, reports. All users created by Admin. |
| **Teacher**| Dashboard, weekly availability, assigned students, sessions (check-in/check-out, lesson notes), read-only profile. No pricing or parent contact. |
| **Parent** | My Students list and student learning overview (assigned teacher, sessions, progress). Read-only; no self-registration. |

Students (P1–P4) have no login in Phase 1; data is viewed via the Parent account.

## 🎯 Domain Entities

- **ApplicationUser** - Extended Identity user with role management
- **TeacherProfile** - Teacher information, NRC (encrypted), verification status
- **Student** - Student information linked to parent
- **ContractSession** - Rental agreement tracking (PackageHours, RemainingHours)
- **AttendanceLog** - Session tracking with check-in/out times and lesson notes
- **TeacherAvailability** - Weekly availability schedule
- **StudentWallet** - Hours balance management
- **TransactionHistory** - Transaction records

## ✨ Key Features

### ✅ Completed
- [x] Clean Architecture project structure
- [x] Feature-based organization (backend & frontend)
- [x] JWT Authentication with login/logout and role-based redirect (Admin / Teacher / Parent)
- [x] Admin account auto-creation on startup
- [x] **Admin**: Dashboard (alerts, today’s sessions, pending actions, revenue), Teachers (onboard, edit, verify, reject, activate/suspend), Parents & Students (create, list), Contracts (create, activate, cancel), Attendance (today, override check-in/out, adjust hours), Payments (wallet credit/deduct), Reports (daily/monthly)
- [x] **Teacher**: Dashboard, availability (weekly), assigned students, sessions (check-in/check-out with lesson notes), read-only profile
- [x] **Parent**: My Students list, student learning overview (assigned teacher, sessions, progress)
- [x] Teacher management: onboard, **edit** (name, phone, education, hourly rate, bio, specializations), verify, reject, activate/suspend
- [x] Check-in/check-out system (teacher + admin override)
- [x] Wallet logic (credit/deduct hours, student active/freeze)
- [x] Daily and monthly reports (Dapper-powered)
- [x] Exception handling middleware, Angular guards and interceptors
- [x] Database schema and relationships
- [x] API URL normalization (no double-slash); Swagger and browser auto-launch disabled by default

### 🚧 To Be Implemented
- [ ] Refresh token mechanism
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
| **Cors** | AllowedOrigins | Semicolon-separated origins (e.g. `http://localhost:4200;https://localhost:4200`) |
| **JwtSettings** | SecretKey, Issuer, Audience, ExpirationInMinutes | JWT auth |
| **Encryption** | Key | AES key for sensitive data |
| **SeedData** | **Roles** | Array of role names to seed (default: Admin, Teacher, Parent) |
| **SeedData:DefaultAdmin** | Email, Password, FirstName, LastName, PhoneNumber | Default admin account (created on first run) |
| **Gemini** | ApiKey | Optional Gemini API key |

Example override in `appsettings.Development.json`:

```json
{
  "Cors": { "AllowedOrigins": "http://localhost:4200;https://localhost:4200" },
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

The database is automatically created on first run. For manual setup:

```bash
cd EduConnect.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../EduConnect.API
dotnet ef database update --startup-project ../EduConnect.API
```

## 🔧 Troubleshooting

### Login Issues

**Problem: "Login failed. Please try again."**

1. **Check Browser Console (F12)**
   - Open DevTools → Console tab
   - Look for detailed error messages
   - Check Network tab for failed requests

2. **Common Issues:**

   - **"Cannot connect to server"**
     - API is not running
     - Wrong port in `environment.ts`
     - Solution: Check API console for actual port, update `environment.ts`

   - **"Invalid email or password"**
     - Wrong credentials (case-sensitive)
     - Admin account not created
     - Solution: Check API console for "Default admin account created" message

   - **CORS Error**
     - Frontend URL not allowed
     - Solution: Check `Program.cs` CORS settings allow `http://localhost:4200`

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
- **Authentication**: JWT tokens with 60-minute expiration
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
**Version**: 1.0.0
