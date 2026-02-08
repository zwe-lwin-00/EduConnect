using EduConnect.API.Middleware;
using EduConnect.Application.Features.Admin.Interfaces;
using EduConnect.Application.Features.Attendance.Interfaces;
using EduConnect.Application.Features.Auth.Interfaces;
using EduConnect.Application.Features.Parents.Interfaces;
using EduConnect.Application.Features.Teachers.Interfaces;
using EduConnect.Application.Features.Homework.Interfaces;
using EduConnect.Application.Features.Notifications.Interfaces;

namespace EduConnect.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<EduConnect.Application.Features.Admin.Interfaces.IAdminService, EduConnect.Infrastructure.Services.AdminService>();
        services.AddScoped<EduConnect.Application.Features.Auth.Interfaces.IAuthService, EduConnect.Infrastructure.Services.AuthService>();
        services.AddScoped<ITeacherService, EduConnect.Infrastructure.Services.TeacherService>();
        services.AddScoped<IAttendanceService, EduConnect.Infrastructure.Services.AttendanceService>();
        services.AddScoped<IParentService, EduConnect.Infrastructure.Services.ParentService>();
        services.AddScoped<IHomeworkService, EduConnect.Infrastructure.Services.HomeworkService>();
        services.AddScoped<INotificationService, EduConnect.Infrastructure.Services.NotificationService>();
        return services;
    }

    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}
