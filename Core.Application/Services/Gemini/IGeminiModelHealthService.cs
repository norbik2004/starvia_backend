using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Services.Gemini
{
    public interface IGeminiModelHealthService
    {
        Task<bool> IsAvailableAsync(GeminiModelType model);
        Task MarkAsFailedAsync(GeminiModelType model);
    }
}
