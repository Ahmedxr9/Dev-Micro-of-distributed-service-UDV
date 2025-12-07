using FluentValidation;
using Shared.DTOs;

namespace GatewayService.Validators;

public class NotificationRequestValidator : AbstractValidator<NotificationRequestDto>
{
    public NotificationRequestValidator()
    {
        RuleFor(x => x.Channel)
            .NotEmpty().WithMessage("Channel is required")
            .Must(channel => new[] { "email", "sms", "push" }.Contains(channel.ToLower()))
            .WithMessage("Channel must be one of: email, sms, push");

        RuleFor(x => x.Recipient)
            .NotEmpty().WithMessage("Recipient is required")
            .MaximumLength(500).WithMessage("Recipient must not exceed 500 characters");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(5000).WithMessage("Message must not exceed 5000 characters");
    }
}

