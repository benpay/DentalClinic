using MediatR;

namespace DentalClinic.Appointments.Application.Abstractions.Messaging;

public interface ICommand<out TResponse> : IRequest<TResponse> { }