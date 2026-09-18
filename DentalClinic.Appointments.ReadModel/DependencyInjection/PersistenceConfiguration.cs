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

        services.AddScoped<IAppointmentReadRepository, ReadModelAppointmentRepository>();
        services.AddScoped<IAppointmentProjector, ReadModelAppointmentProjector>();

        return services;
    }
}