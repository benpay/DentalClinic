using DentalClinic.Appointments.Api.Contracts.Appointments;
using DentalClinic.Appointments.Application.Features.Appointments;
using DentalClinic.Appointments.Application.Features.Appointments.Commands.CancelAppointment;
using DentalClinic.Appointments.Application.Features.Appointments.Commands.RescheduleAppointment;
using DentalClinic.Appointments.Application.Features.Appointments.Commands.ScheduleAppointment;
using DentalClinic.Appointments.Application.Features.Appointments.Queries.GetAppointmentById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace DentalClinic.Appointments.Api.Endpoints;

public static class AppointmentEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/appointments");

        group.MapPost("", CreateAppointmentAsync);
        group.MapPut("/{appointmentId:guid}/reschedule", RescheduleAppointmentAsync);
        group.MapDelete("/{appointmentId:guid}", CancelAppointmentAsync);
        group.MapGet("/{appointmentId:guid}", GetAppointmentByIdAsync);

        return endpoints;
    }

    private static async Task<IResult> CreateAppointmentAsync(
        ScheduleAppointmentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ScheduleAppointmentCommand(
            request.PatientId,
            request.DentistId,
            request.StartsAt,
            request.EndsAt);

        try
        {
            var appointmentId = await sender.Send(
                command,
                cancellationToken);

            return Results.Created(
                $"/api/appointments/{appointmentId}",
                new { Id = appointmentId });
        }
        catch (ValidationException exception)
        {
            return CreateValidationProblem(exception);
        }
    }

    private static async Task<IResult> RescheduleAppointmentAsync(
        Guid appointmentId,
        RescheduleAppointmentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RescheduleAppointmentCommand(
            appointmentId,
            request.StartsAt,
            request.EndsAt);

        try
        {
            await sender.Send(command, cancellationToken);

            return Results.NoContent();
        }
        catch (AppointmentNotFoundException)
        {
            return Results.NotFound();
        }
        catch (ValidationException exception)
        {
            return CreateValidationProblem(exception);
        }
    }

    private static async Task<IResult> CancelAppointmentAsync(
        Guid appointmentId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(
                new CancelAppointmentCommand(appointmentId),
                cancellationToken);

            return Results.NoContent();
        }
        catch (AppointmentNotFoundException)
        {
            return Results.NotFound();
        }
        catch (ValidationException exception)
        {
            return CreateValidationProblem(exception);
        }
    }

    private static async Task<IResult> GetAppointmentByIdAsync(
        Guid appointmentId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var appointment = await sender.Send(
            new GetAppointmentByIdQuery(appointmentId),
            cancellationToken);

        return appointment is null
            ? Results.NotFound()
            : Results.Ok(appointment);
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