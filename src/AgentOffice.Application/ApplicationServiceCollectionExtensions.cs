using AgentOffice.Application.Auth;
using AgentOffice.Application.DateControl;
using AgentOffice.Application.DateTimeControl;
using AgentOffice.Application.Modules;
using AgentOffice.Application.Notifications;
using AgentOffice.Application.Routines;
using AgentOffice.Application.TimeControl;
using AgentOffice.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AgentOffice.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<IDateControlPartyService, DateControlPartyService>();
        services.AddScoped<IDateControlCategoryService, DateControlCategoryService>();
        services.AddScoped<ITimeControlTypeService, TimeControlTypeService>();
        services.AddScoped<IDateControlSourceService, DateControlSourceService>();
        services.AddScoped<IDateControlEntryService, DateControlEntryService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRoutineService, RoutineService>();
        services.AddScoped<ITimeControlPartyService, TimeControlPartyService>();
        services.AddScoped<ITimeControlItemService, TimeControlItemService>();
        services.AddScoped<ITimeControlOccurrenceService, TimeControlOccurrenceService>();
        services.AddScoped<IDateTimeControlEntryService, DateTimeControlEntryService>();

        return services;
    }
}
