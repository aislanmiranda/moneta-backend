using Moneta.Application.Auth;
using Moneta.Application.DateControl;
using Moneta.Application.DateTimeControl;
using Moneta.Application.Modules;
using Moneta.Application.Notifications;
using Moneta.Application.Routines;
using Moneta.Application.TimeControl;
using Moneta.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Moneta.Application;

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
