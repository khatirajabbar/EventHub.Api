using EventHub.Api.DTOs.Organizer;
using FluentValidation;

namespace EventHub.Api.Validators;

public class OrganizerCreateDtoValidator : AbstractValidator<OrganizerCreateDto>
{
    public OrganizerCreateDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).MaximumLength(20);
    }
}