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

## 🛠️ Getting Started

### Backend Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd EduConnect
   ```

2. **Update Configuration**
   
   Edit `EduConnect.API/appsettings.json`:
   - Update `ConnectionStrings:DefaultConnection` with your SQL Server connection string
   - Update `JwtSettings:SecretKey` with a secure key (minimum 32 characters)
   - Update `Encryption:Key` with a 32-character encryption key
   - Update `Gemini:ApiKey` (when implementing AI features)

3. **Restore and Build**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Run Database Migrations**
   ```bash
   cd EduConnect.Infrastructure
   dotnet ef migrations add InitialCreate --startup-project ../EduConnect.API
   dotnet ef database update --startup-project ../EduConnect.API
   ```

5. **Run the API**
   ```bash
   cd EduConnect.API
   dotnet run
   ```
   
   The API will be available at:
   - HTTPS: `https://localhost:5001`
   - HTTP: `http://localhost:5000`
   - Swagger UI: `https://localhost:5001/swagger`

### Frontend Setup

1. **Navigate to Web Project**
   ```bash
   cd EduConnect.Web
   ```

2. **Install Dependencies**
   ```bash
   npm install
   ```

3. **Update Environment Configuration**
   
   Edit `src/environments/environment.ts`:
   ```typescript
   export const environment = {
     production: false,
     apiUrl: 'https://localhost:5001/api'  // Update with your API URL
   };
   ```

4. **Run Development Server**
   ```bash
   npm start
   ```
   
   The app will be available at `http://localhost:4200`

## 📁 Project Structure

### Backend Structure

```
EduConnect/
├── EduConnect.API/                    # Web API Layer
│   ├── Controllers/                   # Feature-based controllers
│   │   ├── BaseController.cs          # Base controller with common functionality
│   │   ├── AuthController.cs
│   │   ├── AdminController.cs
│   │   └── HealthController.cs
│   ├── Middleware/                    # Custom middleware
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Extensions/                    # Extension methods
│   │   └── ServiceCollectionExtensions.cs
│   └── Program.cs                     # Application startup & configuration
│
├── EduConnect.Application/            # Business Logic Layer
│   ├── Common/                        # Shared application concerns
│   │   ├── Interfaces/IService.cs
│   │   ├── Exceptions/                # Custom exceptions
│   │   └── Models/                    # Common models (PagedResult, etc.)
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
│   └── Services/                      # External services (Encryption, etc.)
│
└── EduConnect.Shared/                 # Shared Layer
    └── Enums/                         # Shared enums
```

### Frontend Structure

```
EduConnect.Web/
├── src/
│   ├── app/
│   │   ├── core/                      # Core functionality (singletons)
│   │   │   ├── constants/             # API endpoints, app constants
│   │   │   ├── guards/               # Route guards (auth, role)
│   │   │   ├── interceptors/         # HTTP interceptors
│   │   │   ├── models/               # TypeScript models/interfaces
│   │   │   └── services/             # Core services (AuthService)
│   │   │
│   │   ├── shared/                   # Shared components, directives, pipes
│   │   │   └── components/
│   │   │       └── layout/           # Layout components
│   │   │
│   │   ├── features/                 # Feature modules (lazy-loaded)
│   │   │   ├── auth/                 # Authentication
│   │   │   ├── admin/                # Admin dashboard
│   │   │   ├── teacher/             # Teacher dashboard
│   │   │   ├── parent/              # Parent dashboard
│   │   │   └── dashboard/           # Main dashboard
│   │   │
│   │   ├── services/                 # Shared services
│   │   ├── app.config.ts            # App configuration
│   │   └── app.routes.ts            # Main routing
│   │
│   ├── assets/                       # Static assets
│   └── environments/                 # Environment configurations
```

For detailed structure documentation, see [PROJECT_STRUCTURE_V2.md](./PROJECT_STRUCTURE_V2.md)

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
- [x] JWT Authentication infrastructure
- [x] Exception handling middleware
- [x] Base controllers and services
- [x] Angular guards and interceptors
- [x] Core services and models
- [x] Database schema and relationships

### 🚧 In Progress / To Be Implemented

#### Sprint 1: Identity & Admin Core
- [ ] JWT Authentication service implementation
- [ ] Login/Logout endpoints
- [ ] Refresh token mechanism
- [ ] Admin Dashboard (CRUD for Teachers and Parents)
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

### Environment Variables

For production, use environment variables or Azure Key Vault for:
- Database connection strings
- JWT secret keys
- Encryption keys
- Gemini API key
- External service credentials

### CORS

The API is configured to allow requests from `http://localhost:4200` (Angular dev server). Update CORS policy in `Program.cs` for production:

```csharp
policy.WithOrigins("https://yourdomain.com")
```

## 🔒 Security

- **Encryption**: NRC numbers and sensitive data encrypted using AES-256
- **Authentication**: JWT tokens with refresh token mechanism
- **Authorization**: Role-based access control (Admin, Teacher, Parent)
- **CORS**: Configured to restrict API access to authorized domains
- **Password Policy**: Enforced password requirements
- **Audit Logging**: All admin actions logged using Serilog

## 📊 Development Guidelines

### Backend
- Use **Dapper** for all read/search queries for optimal performance
- Use **EF Core** for write operations and Identity management
- Follow feature-based organization when adding new features
- Implement services in `Application/Features/{Feature}/Services/`
- Use `BaseController` for common controller functionality
- Throw custom exceptions (`BusinessException`, `NotFoundException`)

### Frontend
- Use feature modules for new features (lazy-loaded)
- Place shared components in `shared/components/`
- Use Angular Signals for reactive state management
- Implement guards for route protection
- Use interceptors for HTTP request/response handling
- Follow TypeScript strict mode

## 🧪 Testing

### Backend Testing
```bash
# Run unit tests (when implemented)
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

## 📝 API Documentation

API documentation is available via Swagger UI when running the API:
- Development: `https://localhost:5001/swagger`
- Includes JWT authentication support

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
