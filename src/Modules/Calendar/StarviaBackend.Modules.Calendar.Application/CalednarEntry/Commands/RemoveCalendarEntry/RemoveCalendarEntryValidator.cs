using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.RemoveCalendarEntry;

internal sealed class RemoveCalendarEntryValidator : AbstractValidator<RemoveCalendarEntryCommand>
{
    public RemoveCalendarEntryValidator()
    {
        RuleFor(x => x.request.CalendarEntryId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
