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
using tr_core.Entities;
using tr_core.Enums;
using tr_core.Repositories;
using tr_core.Services;
using tr_core.Services.Gemini;
using tr_service.Exceptions;

namespace tr_service.Gemini
{
    public class GeminiService(ILogger<IGeminiService> logger, IUserService userService, IPostRepository postRepository,
        IUserPromptRepository userPromptRepository, Client geminiClient, GeminiLlMConfig config) : IGeminiService
    {
        public async Task<GeminiResponse> AskGemini(string userId, GeminiRequest request)
        {
            _ = await UserAccesibilityValidation(userId, request);

            try
            {
                logger.LogInformation("Sending request to Gemini");

                var response = await geminiClient.Models.GenerateContentAsync(
                    model: request.Model.ToModelString(),
                    contents: request.UserPrompt.Prompt,
                    config: config.GetAskGenerationConfig()
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

                var userPrompt = new UserPrompt
                {
                    Prompt = request.UserPrompt.Prompt,
                    Response = text,
                    UserId = userId,
                    PostId = request.UserPrompt.PostId
                };

                await userPromptRepository.AddAsync(userPrompt);
                await userPromptRepository.SaveChangesAsync();

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

        public async Task GeneratePost(string userId, GeminiRequest request)
        {
            var post = await UserAccesibilityValidation(userId, request);

            try
            {
                logger.LogInformation("Sending request to Gemini");

                var response = await geminiClient.Models.GenerateContentAsync(
                    model: request.Model.ToModelString(),
                    contents: request.UserPrompt.Prompt,
                    config: config.GetPostGenerationConfig()
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

                var userPrompt = new UserPrompt
                {
                    Prompt = request.UserPrompt.Prompt,
                    Response = text,
                    UserId = userId,
                    PostId = post.Id
                };

                await userPromptRepository.AddAsync(userPrompt);
                await userPromptRepository.SaveChangesAsync();

                post.Status = PostStatus.Generated;
                post.PromptText = request.UserPrompt.Prompt;
                post.Body += $"\n\n Wygenerowany tekst: \n\n{text}";

                postRepository.Update(post);
                await postRepository.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while calling Gemini");

                throw new BadRequestException("Error while communicating with gemini", ex);
            }
        }

        private async Task<Post> UserAccesibilityValidation(string userId, GeminiRequest request)
        {
            bool canUserAccessAi = await userService.CanUserAccessAi(userId);

            if (!canUserAccessAi)
                throw new BadRequestException("User has used hit limit, can't access Ai");

            var post = await postRepository.GetByIdAsync(request.UserPrompt.PostId.ToString())
                    ?? throw new NotFoundException("Post was not found");

            if (post.UserId != userId)
            {
                throw new UnauthorizedException("User cant access this post");
            }

            return post;
        }
    }
}
