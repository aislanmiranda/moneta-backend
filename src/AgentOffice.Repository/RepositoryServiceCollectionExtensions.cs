using AgentOffice.Application.Auth;
using AgentOffice.Application.DateControl;
using AgentOffice.Application.DateTimeControl;
using AgentOffice.Application.Modules;
using AgentOffice.Application.Notifications;
using AgentOffice.Application.Routines;
using AgentOffice.Application.TimeControl;
using AgentOffice.Repository.Auth;
using AgentOffice.Repository.DateControl;
using AgentOffice.Repository.DateTimeControl;
using AgentOffice.Repository.Modules;
using AgentOffice.Repository.Notifications;
using AgentOffice.Repository.Routines;
using AgentOffice.Repository.TimeControl;
using Microsoft.Extensions.DependencyInjection;

namespace AgentOffice.Repository;

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
