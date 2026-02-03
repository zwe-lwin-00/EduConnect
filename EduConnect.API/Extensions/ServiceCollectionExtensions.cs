using EduConnect.API.Middleware;
using EduConnect.Application.Features.Admin.Interfaces;
using EduConnect.Application.Features.Attendance.Interfaces;
using EduConnect.Application.Features.Auth.Interfaces;
using EduConnect.Application.Features.Contracts.Interfaces;
using EduConnect.Application.Features.Students.Interfaces;
using EduConnect.Application.Features.Teachers.Interfaces;

namespace EduConnect.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<EduConnect.Application.Features.Admin.Interfaces.IAdminService, EduConnect.Infrastructure.Services.AdminService>();
        services.AddScoped<EduConnect.Application.Features.Auth.Interfaces.IAuthService, EduConnect.Infrastructure.Services.AuthService>();
        
        // TODO: Register other services when implemented
        // services.AddScoped<ITeacherService, TeacherService>();
        // services.AddScoped<IStudentService, StudentService>();
        // services.AddScoped<IContractService, ContractService>();
        // services.AddScoped<IAttendanceService, AttendanceService>();
        
        return services;
    }

    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}
