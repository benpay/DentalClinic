using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.ReadModel.AppointmentProjectors;
using DentalClinic.Appointments.ReadModel.Appointments;
using DentalClinic.Appointments.ReadModel.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DentalClinic.Appointments.ReadModel.DependencyInjection;

public static class PersistenceConfiguration
{
    public static IServiceCollection AddReadModel(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ReadAppointmentsDbContext>(options =>
            options.UseNpgsql(connectionString));

        RegisterReadModelServices(services);

        return services;
    }

    public static IServiceCollection AddReadModelInMemory(
        this IServiceCollection services)
    {
        services.AddDbContext<ReadAppointmentsDbContext>(options =>
            options.UseInMemoryDatabase("DentalClinicAppointmentsRead"));

        RegisterReadModelServices(services);

        return services;
    }

    private static void RegisterReadModelServices(
        IServiceCollection services)
    {
        services.AddScoped<IAppointmentReadRepository, ReadModelAppointmentRepository>();
        services.AddScoped<IAppointmentProjector, ReadModelAppointmentProjector>();
        services.AddHostedService<ReadModelInitializer>();
    }
}