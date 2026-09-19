using DentalClinic.Appointments.Domain.Appointments;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.Appointments.Infrastructure.Persistence;

public sealed class AppointmentsDbContext : DbContext
{
    public AppointmentsDbContext(
        DbContextOptions<AppointmentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(builder =>
        {
            builder.HasKey(appointment => appointment.Id);

            builder.Property(appointment => appointment.Status)
                .HasConversion<string>();

            builder.Property(appointment => appointment.Version)
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.HasKey(message => message.Id);

            builder.Property(message => message.Type)
                .IsRequired();

            builder.Property(message => message.Content)
                .IsRequired();
        });
    }
}