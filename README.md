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
Password: Admin@123
```

**Note the port number** - you'll need it for frontend configuration.

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
   - **Password:** `Admin@123`
3. Click "Login"
4. You'll be redirected to `/admin` dashboard

⚠️ **IMPORTANT:** Change the default admin password immediately after first login!

## 🔐 Default Admin Account

When you first run the application, a default admin account is automatically created:

- **Email:** `admin@educonnect.com`
- **Password:** `Admin@123`

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
- [x] JWT Authentication with login/logout
- [x] Admin account auto-creation on startup
- [x] Admin Dashboard with DevExtreme components
- [x] Teacher management (onboard, verify, reject, view)
- [x] Parent and Student management
- [x] Exception handling middleware
- [x] Angular guards and interceptors
- [x] Database schema and relationships

### 🚧 To Be Implemented

#### Sprint 1: Identity & Admin Core
- [x] JWT Authentication service
- [x] Login/Logout endpoints
- [x] Admin Dashboard (CRUD for Teachers and Parents)
- [ ] Refresh token mechanism
- [ ] Teacher Management (NRC upload, certificate verification)

#### Sprint 2: Scheduling & Matching
- [ ] Availability Engine (DevExtreme Scheduler)
- [ ] Matching Algorithm (Grade/Subject based)
- [ ] Conflict Detection (Prevent double-booking)
- [ ] Booking approval workflow

#### Sprint 3: Attendance & Billing
- [ ] Check-in/Check-out System
- [ ] Wallet Logic (Automatic hour deduction)
- [ ] Transaction history
- [ ] Payment Integration (KBZPay/Wave)

#### Sprint 4: AI & Reporting
- [ ] Gemini API Integration (Progress reports)
- [ ] Admin Analytics Dashboard (Dapper-powered)
- [ ] Teacher utilization reports
- [ ] Revenue reports

## ⚙️ Configuration

### Backend Configuration

Edit `EduConnect.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EduConnectDb;..."
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGenerationMustBeAtLeast32CharactersLong!",
    "Issuer": "EduConnect",
    "Audience": "EduConnectUsers",
    "ExpirationInMinutes": 60
  },
  "Encryption": {
    "Key": "YourEncryptionKeyMustBe32CharactersLong!!"
  }
}
```

### Frontend Configuration

Edit `EduConnect.Web/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5049/api'  // Match your API port
};
```

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
   - Go to `http://localhost:5049/swagger`
   - Try `/api/auth/login` endpoint
   - See actual error response

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

API documentation is available via Swagger UI when running the API:
- **Swagger UI**: `http://localhost:5049/swagger` (or your API URL)
- Includes JWT authentication support
- Test endpoints directly from Swagger

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
