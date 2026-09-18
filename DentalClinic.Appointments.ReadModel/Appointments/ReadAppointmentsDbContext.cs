using DentalClinic.Appointments.ReadModel.Appointments;
using Microsoft.EntityFrameworkCore;

namespace DentalClinic.Appointments.ReadModel.Appointments;

public sealed class ReadAppointmentsDbContext : DbContext
{
    public ReadAppointmentsDbContext(
        DbContextOptions<ReadAppointmentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppointmentProjection> AppointmentProjections
        => Set<AppointmentProjection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppointmentProjection>(builder =>
        {
            builder.ToTable("appointment_projections");

            builder.HasKey(projection => projection.Id);

            builder.Property(projection => projection.PatientId)
                .IsRequired();

            builder.Property(projection => projection.DentistId)
                .IsRequired();

            builder.Property(projection => projection.StartsAt)
                .IsRequired();

            builder.Property(projection => projection.EndsAt)
                .IsRequired();

            builder.Property(projection => projection.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(projection => projection.Version)
                .IsConcurrencyToken();

            builder.Property(projection => projection.UpdatedAtUtc)
                .IsRequired();

            builder.HasIndex(projection => projection.DentistId);

            builder.HasIndex(projection => projection.StartsAt);
        });
    }
}