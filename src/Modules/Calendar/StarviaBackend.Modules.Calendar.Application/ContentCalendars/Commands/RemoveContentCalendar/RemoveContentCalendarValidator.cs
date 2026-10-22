using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace StarviaBackend.Modules.Calendar.Application.ContentCalendars.Commands.RemoveContentCalendar;

internal sealed class RemoveContentCalendarValidator : AbstractValidator<RemoveContentCalendarCommand>
{
    public RemoveContentCalendarValidator()
    {
        RuleFor(x => x.request.contentCalendarId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
