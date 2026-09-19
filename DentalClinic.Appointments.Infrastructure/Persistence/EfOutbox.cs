using DentalClinic.Appointments.Application.Abstractions.Messaging;

namespace DentalClinic.Appointments.Infrastructure.Persistence;

public sealed class EfOutbox : IOutbox
{
    private readonly AppointmentsDbContext _dbContext;

    public EfOutbox(AppointmentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(Guid aggregateId, IIntegrationEvent integrationEvent)
    {
        var outboxMessage = OutboxMessage.Create(aggregateId, integrationEvent);

        _dbContext.OutboxMessages.Add(outboxMessage);
    }
}