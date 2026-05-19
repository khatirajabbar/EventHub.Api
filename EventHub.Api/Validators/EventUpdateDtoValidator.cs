using EventHub.Api.DTOs.Event;
using FluentValidation;

namespace EventHub.Api.Validators;

public class EventUpdateDtoValidator : AbstractValidator<EventUpdateDto>
{
    public EventUpdateDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Date).GreaterThan(DateTime.UtcNow).WithMessage("Date must be in the future.");
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OrganizerId).GreaterThan(0);
    }
}

