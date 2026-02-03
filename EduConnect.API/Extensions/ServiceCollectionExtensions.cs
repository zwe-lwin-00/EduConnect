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
        services.AddScoped<EduConnect.Application.Features.Admin.Interfaces.IAdminService, EduConnect.Infrastructure.Services.AdminService>();
        services.AddScoped<EduConnect.Application.Features.Auth.Interfaces.IAuthService, EduConnect.Infrastructure.Services.AuthService>();
        services.AddScoped<ITeacherService, EduConnect.Infrastructure.Services.TeacherService>();
        services.AddScoped<IAttendanceService, EduConnect.Infrastructure.Services.AttendanceService>();
        return services;
    }

    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}
