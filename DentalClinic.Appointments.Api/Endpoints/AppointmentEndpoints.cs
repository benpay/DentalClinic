using DentalClinic.Appointments.Api.Contracts.Appointments;
using DentalClinic.Appointments.Application.Features.Appointments;
using DentalClinic.Appointments.Application.Features.Appointments.Commands.CancelAppointment;
using DentalClinic.Appointments.Application.Features.Appointments.Commands.RescheduleAppointment;
using DentalClinic.Appointments.Application.Features.Appointments.Commands.ScheduleAppointment;
using DentalClinic.Appointments.Application.Features.Appointments.Queries.GetAppointmentById;
using DentalClinic.Appointments.Application.Features.Appointments.Queries.GetAppointments;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DentalClinic.Appointments.Api.Endpoints;

public static class AppointmentEndpoints
{
    private const string LoggerCategory =
        "DentalClinic.Appointments.Api.Endpoints.AppointmentEndpoints";

    public static IEndpointRouteBuilder MapAppointmentEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/appointments");

        group.MapPost("", CreateAppointmentAsync);
        group.MapPut("/{appointmentId:guid}/reschedule", RescheduleAppointmentAsync);
        group.MapDelete("/{appointmentId:guid}", CancelAppointmentAsync);
        group.MapGet("", GetAppointmentsAsync);
        group.MapGet("/{appointmentId:guid}", GetAppointmentByIdAsync);

        return endpoints;
    }

    private static async Task<IResult> CreateAppointmentAsync(
        ScheduleAppointmentRequest request,
        ISender sender,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerCategory);

        var command = new ScheduleAppointmentCommand(
            request.PatientId,
            request.DentistId,
            request.StartsAt,
            request.EndsAt);

        logger.LogInformation(
            "POST /api/appointments received: dentist {DentistId}, {StartsAt} - {EndsAt}.",
            request.DentistId,
            request.StartsAt,
            request.EndsAt);

        try
        {
            var appointmentId = await sender.Send(
                command,
                cancellationToken);

            logger.LogInformation(
                "Appointment {AppointmentId} created for dentist {DentistId}.",
                appointmentId,
                request.DentistId);

            return Results.Created(
                $"/api/appointments/{appointmentId}",
                new { Id = appointmentId });
        }
        catch (AppointmentOverlapException)
        {
            logger.LogWarning(
                "Conflict: dentist {DentistId} already has an appointment overlapping " +
                "{StartsAt} - {EndsAt}.",
                request.DentistId,
                request.StartsAt,
                request.EndsAt);

            return Results.Conflict();
        }
        catch (ValidationException exception)
        {
            logger.LogWarning(
                "Validation failed for POST /api/appointments: {Errors}.",
                string.Join("; ", exception.Errors.Select(error => $"{error.PropertyName}: {error.ErrorMessage}")));

            return CreateValidationProblem(exception);
        }
    }

    private static async Task<IResult> RescheduleAppointmentAsync(
        Guid appointmentId,
        RescheduleAppointmentRequest request,
        ISender sender,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerCategory);

        logger.LogInformation(
            "PUT /api/appointments/{AppointmentId}/reschedule received: {StartsAt} - {EndsAt}.",
            appointmentId,
            request.StartsAt,
            request.EndsAt);

        var command = new RescheduleAppointmentCommand(
            appointmentId,
            request.StartsAt,
            request.EndsAt);

        try
        {
            await sender.Send(command, cancellationToken);

            logger.LogInformation(
                "Appointment {AppointmentId} rescheduled to {StartsAt} - {EndsAt}.",
                appointmentId,
                request.StartsAt,
                request.EndsAt);

            return Results.NoContent();
        }
        catch (AppointmentNotFoundException)
        {
            logger.LogWarning(
                "Appointment {AppointmentId} not found for reschedule.",
                appointmentId);

            return Results.NotFound();
        }
        catch (AppointmentOverlapException)
        {
            logger.LogWarning(
                "Conflict: reschedule of appointment {AppointmentId} to " +
                "{StartsAt} - {EndsAt} overlaps another appointment.",
                appointmentId,
                request.StartsAt,
                request.EndsAt);

            return Results.Conflict();
        }
        catch (ValidationException exception)
        {
            logger.LogWarning(
                "Validation failed for reschedule of appointment {AppointmentId}: {Errors}.",
                appointmentId,
                string.Join("; ", exception.Errors.Select(error => $"{error.PropertyName}: {error.ErrorMessage}")));

            return CreateValidationProblem(exception);
        }
    }

    private static async Task<IResult> CancelAppointmentAsync(
        Guid appointmentId,
        ISender sender,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerCategory);

        logger.LogInformation(
            "DELETE /api/appointments/{AppointmentId} received.",
            appointmentId);

        try
        {
            await sender.Send(
                new CancelAppointmentCommand(appointmentId),
                cancellationToken);

            logger.LogInformation(
                "Appointment {AppointmentId} cancelled.",
                appointmentId);

            return Results.NoContent();
        }
        catch (AppointmentNotFoundException)
        {
            logger.LogWarning(
                "Appointment {AppointmentId} not found for cancellation.",
                appointmentId);

            return Results.NotFound();
        }
        catch (ValidationException exception)
        {
            logger.LogWarning(
                "Validation failed for cancellation of appointment {AppointmentId}: {Errors}.",
                appointmentId,
                string.Join("; ", exception.Errors.Select(error => $"{error.PropertyName}: {error.ErrorMessage}")));

            return CreateValidationProblem(exception);
        }
    }

    private static async Task<IResult> GetAppointmentByIdAsync(
        Guid appointmentId,
        ISender sender,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerCategory);

        var appointment = await sender.Send(
            new GetAppointmentByIdQuery(appointmentId),
            cancellationToken);

        if (appointment is null)
        {
            logger.LogWarning(
                "Appointment {AppointmentId} was not found in the read model.",
                appointmentId);

            return Results.NotFound();
        }

        logger.LogInformation(
            "Appointment {AppointmentId} retrieved from the read model " +
            "with status {Status}.",
            appointmentId,
            appointment.Status);

        return Results.Ok(appointment);
    }

    private static async Task<IResult> GetAppointmentsAsync(
        Guid? dentistId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        ISender sender,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(LoggerCategory);

        logger.LogInformation(
            "GET /api/appointments received: dentist {DentistId}, {From} - {To}.",
            dentistId,
            from,
            to);

        var appointments = await sender.Send(
            new GetAppointmentsQuery(dentistId, from, to),
            cancellationToken);

        logger.LogInformation(
            "GET /api/appointments returned {Count} appointment(s) from the read model.",
            appointments.Count);

        return Results.Ok(appointments);
    }

    private static IResult CreateValidationProblem(
        ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.ErrorMessage)
                    .ToArray());

        return Results.ValidationProblem(errors);
    }
}