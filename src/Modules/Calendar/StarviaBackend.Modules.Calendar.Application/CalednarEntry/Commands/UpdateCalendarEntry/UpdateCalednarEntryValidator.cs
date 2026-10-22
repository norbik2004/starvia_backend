using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace StarviaBackend.Modules.Calendar.Application.CalednarEntry.Commands.UpdateCalendarEntry;

internal sealed class UpdateCalednarEntryValidator : AbstractValidator<UpdateCalendarEntryCommand>
{
    public UpdateCalednarEntryValidator()
    {
        RuleFor(x => x.request.ScheduledAt).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
