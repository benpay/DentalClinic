using DentalClinic.Appointments.Api.Contracts.Appointments;
using DentalClinic.Appointments.Domain.Appointments;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DentalClinic.Appointments.IntegrationTests.Endpoints;

public sealed class AppointmentsEndpointsTests
    : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public AppointmentsEndpointsTests(ApiFactory factory)
    {
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

        var newStartsAt = new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero);
        var newEndsAt = new DateTimeOffset(2026, 10, 2, 10, 30, 0, TimeSpan.Zero);

        var rescheduleRequest = new RescheduleAppointmentRequest(
            newStartsAt,
            newEndsAt);

        var rescheduleResponse = await _client.PutAsJsonAsync(
            $"{createResponse.Headers.Location}/reschedule",
            rescheduleRequest);

        Assert.Equal(HttpStatusCode.NoContent, rescheduleResponse.StatusCode);

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

        var cancelResponse = await _client.DeleteAsync(
            createResponse.Headers.Location!);

        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

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
}