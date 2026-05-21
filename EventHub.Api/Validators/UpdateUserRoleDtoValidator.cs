using EventHub.Api.DTOs.User;
using FluentValidation;

namespace EventHub.Api.Validators;

public class UpdateUserRoleDtoValidator : AbstractValidator<UpdateUserRoleDto>
{
    public UpdateUserRoleDtoValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(x => x == "Admin" || x == "Member").WithMessage("Role must be either 'Admin' or 'Member'.");
    }
}

