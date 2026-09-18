using DentalClinic.Appointments.Application.Abstractions.Messaging;

namespace DentalClinic.Appointments.Infrastructure.Persistence;

public sealed class EfOutbox : IOutbox
{
    private readonly AppointmentsDbContext _dbContext;

    public EfOutbox(AppointmentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(IIntegrationEvent integrationEvent)
    {
        var outboxMessage = OutboxMessage.Create(integrationEvent);

        _dbContext.OutboxMessages.Add(outboxMessage);
    }
}