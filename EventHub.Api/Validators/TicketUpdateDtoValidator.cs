using EventHub.Api.DTOs.Ticket;
using FluentValidation;

namespace EventHub.Api.Validators;

public class TicketUpdateDtoValidator : AbstractValidator<TicketUpdateDto>
{
    public TicketUpdateDtoValidator()
    {
        RuleFor(x => x.EventId).GreaterThan(0).WithMessage("Event ID must be greater than 0.");
        RuleFor(x => x.Type).IsInEnum().WithMessage("Ticket type must be a valid enum value (VIP, Regular, Standard, Premium, Basic).");
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.QuantityAvailable).GreaterThanOrEqualTo(0);
    }
}

