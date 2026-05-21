using EventHub.Api.DTOs.Ticket;
using FluentValidation;

namespace EventHub.Api.Validators;

public class TicketUpdateDtoValidator : AbstractValidator<TicketUpdateDto>
{
    public TicketUpdateDtoValidator()
    {
        RuleFor(x => x.EventId).GreaterThan(0).WithMessage("Event ID must be greater than 0.");
        RuleFor(x => x.Type).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.QuantityAvailable).GreaterThanOrEqualTo(0);
    }
}

