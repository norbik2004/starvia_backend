using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.AddCalendarEntry;

internal sealed class AddCalendarEntryValidator : AbstractValidator<AddCalendarEntryCommand>
{
    public AddCalendarEntryValidator()
    {
        RuleFor(x => x.request.CalendarId).NotEmpty().WithMessage("CalendarId is required");
        RuleFor(x => x.request.PostId).NotEmpty().WithMessage("PostId is required");
        RuleFor(x => x.request.ScheduledAt).NotEmpty().WithMessage("ScheduledAt is required");
    }
}
