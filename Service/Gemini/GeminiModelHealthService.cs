using Core.Application.Services.Gemini;
using Core.Domain.Enums;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Gemini
{
    public class GeminiModelHealthService(IConnectionMultiplexer multiplexer) : IGeminiModelHealthService
    {
        private readonly IDatabase database = multiplexer.GetDatabase();

        public async Task<bool> IsAvailableAsync(GeminiModelType model)
        {
            return !await database.KeyExistsAsync(GetKey(model.ToModelString()));
        }

        public async Task MarkAsFailedAsync(GeminiModelType model)
        {
            await database.StringSetAsync(
                GetKey(model.ToModelString()),
                "failed",
                TimeSpan.FromMinutes(5));
        }

        private static string GetKey(string model)
             => $"gemini:model:{model}:failed";
    }
}
