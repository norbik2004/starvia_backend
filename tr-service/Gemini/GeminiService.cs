using dotenv.net;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.Gemini;
using tr_core.DTO.Gemini.Request;
using tr_core.Enums;
using tr_core.Services;
using tr_core.Services.Gemini;
using tr_service.Exceptions;

namespace tr_service.Gemini
{
    public class GeminiService(ILogger<IGeminiService> logger, Client geminiClient, GeminiLLMConfig config, IUserService userService) : IGeminiService
    {
        public async Task<GeminiResponse> SendRequestToGemini(string userId, GeminiRequest request)
        {
            if (!userService.CanGeneratePostAsync(userId).Result)
            {
                logger.LogWarning("User has reached the generation limit");
                throw new BadRequestException("Generation limit reached for user");
            }
            try
            {
                logger.LogInformation("Sending request to Gemini");

                var response = await geminiClient.Models.GenerateContentAsync(
                    model: request.Model.ToModelString(),
                    contents: request.Prompt,
                    config: config.GetConfig()
                );

                var text = response?.Candidates?
                    .FirstOrDefault()?
                    .Content?
                    .Parts?
                    .FirstOrDefault()?
                    .Text;

                if (string.IsNullOrWhiteSpace(text))
                {
                    logger.LogWarning("Empty response from Gemini");
                    throw new BadRequestException("Gemini returned null");
                }

                logger.LogInformation("Response received from Gemini");
                await userService.IncrementPostCounterAsync(userId);

                return new GeminiResponse
                {
                    Response = text
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while calling Gemini");

                throw new BadRequestException("Error while communicating with gemini", ex);
            }
        }
    }
}
