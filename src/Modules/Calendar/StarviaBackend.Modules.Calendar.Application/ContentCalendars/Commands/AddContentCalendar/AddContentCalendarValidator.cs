using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.AddContentCalendar;

internal sealed class AddContentCalendarValidator : AbstractValidator<AddContentCalendarCommand>
{
    public AddContentCalendarValidator()
    {
        RuleFor(x => x.request.name).NotEmpty().WithMessage("Name is required");
    }
}
