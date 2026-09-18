using DentalClinic.Appointments.Api.Contracts.Appointments;
using DentalClinic.Appointments.Domain.Appointments;
using DentalClinic.Appointments.Infrastructure.Persistence;
using DentalClinic.Appointments.ReadModel.AppointmentProjectors;
using DentalClinic.Appointments.ReadModel.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DentalClinic.Appointments.IntegrationTests.Endpoints;

public sealed class AppointmentsEndpointsTests
    : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public AppointmentsEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateThenGetAppointment_ReturnsCreatedAppointment()
    {
        var patientId = Guid.NewGuid();
        var dentistId = Guid.NewGuid();

        var request = new ScheduleAppointmentRequest(
            patientId,
            dentistId,
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero));

        var createResponse = await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        await ApplyPendingProjectionsAsync(_factory);

        var getResponse = await _client.GetAsync(
            createResponse.Headers.Location!);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var responseContent = await getResponse.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseContent);

        var appointment = document.RootElement;

        Assert.Equal(
            patientId,
            appointment.GetProperty("patientId").GetGuid());

        Assert.Equal(
            dentistId,
            appointment.GetProperty("dentistId").GetGuid());
    }

    [Fact]
    public async Task GetAppointment_WhenAppointmentDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            $"/api/appointments/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RescheduleAppointment_ThenGet_ReturnsUpdatedSchedule()
    {
        var createRequest = new ScheduleAppointmentRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero));

        var createResponse = await _client.PostAsJsonAsync(
            "/api/appointments",
            createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        await ApplyPendingProjectionsAsync(_factory);

        var newStartsAt = new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero);
        var newEndsAt = new DateTimeOffset(2026, 10, 2, 10, 30, 0, TimeSpan.Zero);

        var rescheduleRequest = new RescheduleAppointmentRequest(
            newStartsAt,
            newEndsAt);

        var rescheduleResponse = await _client.PutAsJsonAsync(
            $"{createResponse.Headers.Location}/reschedule",
            rescheduleRequest);

        Assert.Equal(HttpStatusCode.NoContent, rescheduleResponse.StatusCode);

        await ApplyPendingProjectionsAsync(_factory);

        var getResponse = await _client.GetAsync(
            createResponse.Headers.Location!);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var responseContent = await getResponse.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseContent);

        var appointment = document.RootElement;

        Assert.Equal(
            newStartsAt,
            appointment.GetProperty("startsAt").GetDateTimeOffset());

        Assert.Equal(
            newEndsAt,
            appointment.GetProperty("endsAt").GetDateTimeOffset());
    }

    [Fact]
    public async Task CancelAppointment_ThenGet_ReturnsCancelledStatus()
    {
        var createRequest = new ScheduleAppointmentRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 1, 9, 30, 0, TimeSpan.Zero));

        var createResponse = await _client.PostAsJsonAsync(
            "/api/appointments",
            createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        await ApplyPendingProjectionsAsync(_factory);

        var cancelResponse = await _client.DeleteAsync(
            createResponse.Headers.Location!);

        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        await ApplyPendingProjectionsAsync(_factory);

        var getResponse = await _client.GetAsync(
            createResponse.Headers.Location!);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var responseContent = await getResponse.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseContent);

        var status = document.RootElement
            .GetProperty("status")
            .GetInt32();

        Assert.Equal((int)AppointmentStatus.Cancelled, status);
    }

    private static async Task ApplyPendingProjectionsAsync(ApiFactory factory)
    {
        using var scope = factory.Services.CreateScope();

        var writeDbContext = scope.ServiceProvider
            .GetRequiredService<AppointmentsDbContext>();

        var dispatcher = new IntegrationEventDispatcher();

        var projector = scope.ServiceProvider
            .GetRequiredService<IAppointmentProjector>();

        var pendingMessages = await writeDbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null)
            .ToListAsync();

        foreach (var message in pendingMessages)
        {
            var integrationEvent = dispatcher.Deserialize(
                message.Type,
                message.Content);

            await projector.ProjectAsync(integrationEvent);

            message.MarkAsProcessed(DateTimeOffset.UtcNow);
        }

        await writeDbContext.SaveChangesAsync();
    }
}