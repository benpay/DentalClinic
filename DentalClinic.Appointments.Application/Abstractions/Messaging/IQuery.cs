using MediatR;

namespace DentalClinic.Appointments.Application.Abstractions.Messaging;

public interface IQuery<out TResponse> : IRequest<TResponse> { }