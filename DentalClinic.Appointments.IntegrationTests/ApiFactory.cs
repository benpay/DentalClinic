using DentalClinic.Appointments.Application.Abstractions.Persistence;
using DentalClinic.Appointments.ReadModel.AppointmentProjectors;
using DentalClinic.Appointments.ReadModel.Appointments;
using DentalClinic.Appointments.ReadModel.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DentalClinic.Appointments.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();

            services.RemoveAll<ReadAppointmentsDbContext>();
            services.RemoveAll<IAppointmentReadRepository>();
            services.RemoveAll<IAppointmentProjector>();

            services.AddDbContext<ReadAppointmentsDbContext>(options =>
                options.UseInMemoryDatabase("DentalClinicAppointmentsReadTest"));

            services.AddScoped<IAppointmentReadRepository, ReadModelAppointmentRepository>();
            services.AddScoped<IAppointmentProjector, ReadModelAppointmentProjector>();
        });
    }
}