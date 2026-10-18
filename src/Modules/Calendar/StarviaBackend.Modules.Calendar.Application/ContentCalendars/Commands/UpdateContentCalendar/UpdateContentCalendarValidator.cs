using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.UpdateContentCalendar;

namespace StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.AddContentCalendar;

internal sealed class UpdateContentCalendarValidator : AbstractValidator<UpdateContentCalendarCommand>
{
    public UpdateContentCalendarValidator()
    {
        RuleFor(x => x.request.name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.request.contentCalendarId).NotEmpty().WithMessage("Content Calendar ID is required.");
    }
}
