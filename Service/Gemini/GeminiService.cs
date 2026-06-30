using Core.Application.DTO.UserPrompt.Request;
using Core.Application.Services;
using Core.Application.Services.Gemini;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Infrastructure.Repositories;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Abstractions;
using Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Gemini
{
    public class GeminiService(ILogger<IGeminiService> logger, IUserService userService, IPostRepository postRepository,
        IUserPromptRepository userPromptRepository, Client geminiClient, GeminiLlMConfig config,
        IGeminiModelHealthService geminiModelHealthService) : IGeminiService
    {

        public async Task<string> AskAiPostScope(
            string userId,
            UserPromptRequest request)
        {
            var post = await UserAccesibilityValidation(userId, request);

            var models = await GetAvailableModelsAsync(
                GeminiModelType.Gemini3FlashPreview);

            if (models.Count == 0)
            {
                throw new BadRequestException(
                    "Our Ai models are currently unavalible, please try again in 5 minutes");
            }

            GenerateContentConfig configuration = request.ConversationType switch
            {
                GeminiConversationType.GeneratePost => config.GetPostGenerationConfig(),
                GeminiConversationType.AskGemini => config.GetAskGenerationConfig(),
                _ => throw new BadRequestException("Invalid conversation type"),
            };

            var content = new List<Content>();

            if (request.ConversationType == GeminiConversationType.AskGemini)
            {
                var previousPrompts = await userPromptRepository
                    .GetAllPerPostIdAndUserIdConversationVise(request.PostId, userId);

                content.AddRange(BuildContents(previousPrompts));
            }

            var prompt = request.Prompt;

            if(request.IncludePostText == true)
            {
                prompt += "\n\n Użytkownik dodatkowo aktualną załączył treść posta: \n" + post.Body;
            }

            content.Add(new Content
            {
                Role = "user",
                Parts =
                [
                    new Part { Text = prompt }
                ]
            });

            Exception? lastException = null;

            foreach (var model in models)
            {
                try
                {
                    logger.LogInformation(
                        "Trying Gemini model {Model}",
                        model);

                    var response = await geminiClient.Models.GenerateContentAsync(
                        model: model.ToModelString(),
                        contents: content,
                        config: configuration
                    );

                    var text = response?.Candidates?
                        .FirstOrDefault()?
                        .Content?
                        .Parts?
                        .FirstOrDefault()?
                        .Text;

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        throw new BadRequestException(
                            $"Empty response from Ai");
                    }

                    var userPrompt = new UserPrompt
                    {
                        Prompt = request.Prompt,
                        Response = text,
                        UserId = userId,
                        PostId = post.Id,
                        ConversationType = request.ConversationType
                    };

                    await userPromptRepository.AddAsync(userPrompt);
                    await userPromptRepository.SaveChangesAsync();

                    logger.LogInformation(
                        "Model {Model} succeeded",
                        model);

                    return text;
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    logger.LogWarning(
                        ex,
                        "Model {Model} failed. Marking as unhealthy.",
                        model);

                    await geminiModelHealthService.MarkAsFailedAsync(model);

                }
            }

            logger.LogError(
                lastException,
                "All Gemini models failed.");

            throw new BadRequestException(
                "Our Ai models are currently unavalible, please try again in 5 minutes");
        }

        private async Task<Post> UserAccesibilityValidation(string userId, UserPromptRequest request)
        {
            bool canUserAccessAi = await userService.CanUserAccessAi(userId);

            if (!canUserAccessAi)
                throw new BadRequestException("User has used hit limit, can't access Ai");

            var post = await postRepository.GetByIdAsync(request.PostId.ToString())
                    ?? throw new NotFoundException("Post was not found");

            if (post.UserId != userId)
            {
                throw new UnauthorizedException("User cant access this post");
            }

            return post;
        }

        private async Task<List<GeminiModelType>> GetAvailableModelsAsync(
            GeminiModelType preferredModel)
        {
            var models = Enum.GetValues<GeminiModelType>()
                .OrderBy(x => x == preferredModel ? 0 : 1);

            var result = new List<GeminiModelType>();

            foreach (var model in models)
            {
                if (await geminiModelHealthService.IsAvailableAsync(model))
                {
                    result.Add(model);
                }
            }

            return result;
        }


        private static List<Content> BuildContents(
            IEnumerable<UserPrompt> history)
        {
            return history
                .OrderBy(x => x.CreatedAt)
                .Take(10)
                .SelectMany(x => new[]
                {
            new Content
            {
                Role = "user",
                Parts =
                [
                    new Part { Text = x.Prompt }
                ]
            },
            new Content
            {
                Role = "model",
                Parts =
                [
                    new Part { Text = x.Response }
                ]
            }
                })
                .ToList();
        }
    }
}
