using Moneta.Application.Auth;
using Moneta.Application.DateControl;
using Moneta.Application.DateTimeControl;
using Moneta.Application.Modules;
using Moneta.Application.Notifications;
using Moneta.Application.Routines;
using Moneta.Application.TimeControl;
using Moneta.Repository.Auth;
using Moneta.Repository.DateControl;
using Moneta.Repository.DateTimeControl;
using Moneta.Repository.Modules;
using Moneta.Repository.Notifications;
using Moneta.Repository.Routines;
using Moneta.Repository.TimeControl;
using Microsoft.Extensions.DependencyInjection;

namespace Moneta.Repository;

public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IDateControlPartyRepository, DateControlPartyRepository>();
        services.AddScoped<IDateControlCategoryRepository, DateControlCategoryRepository>();
        services.AddScoped<ITimeControlTypeRepository, TimeControlTypeRepository>();
        services.AddScoped<IDateControlSourceRepository, DateControlSourceRepository>();
        services.AddScoped<IDateControlEntryRepository, DateControlEntryRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IRoutineRepository, RoutineRepository>();
        services.AddScoped<ITimeControlPartyRepository, TimeControlPartyRepository>();
        services.AddScoped<ITimeControlItemRepository, TimeControlItemRepository>();
        services.AddScoped<ITimeControlOccurrenceRepository, TimeControlOccurrenceRepository>();
        services.AddScoped<IDateTimeControlEntryRepository, DateTimeControlEntryRepository>();

        return services;
    }
}
