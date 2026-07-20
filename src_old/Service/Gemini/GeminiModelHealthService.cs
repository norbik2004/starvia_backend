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

        public async Task<bool> IsAvailableAsync<TModel>(TModel model)
            where TModel : Enum
        {
            return !await database.KeyExistsAsync(GetKey(model.ToModelString()));
        }

        public async Task MarkAsFailedAsync<TModel>(TModel model)
            where TModel : Enum
        {
            await database.StringSetAsync(
                GetKey(model.ToModelString()),
                "failed",
                TimeSpan.FromMinutes(1));
        }

        private static string GetKey(string model)
            => $"gemini:model:{model}:failed";
    }
}
