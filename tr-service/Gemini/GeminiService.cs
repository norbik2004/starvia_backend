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
using tr_core.Repositories;
using tr_core.Services;
using tr_core.Services.Gemini;
using tr_service.Exceptions;

namespace tr_service.Gemini
{
    public class GeminiService(ILogger<IGeminiService> logger, IUserService userService, IPostRepository postRepository,
        Client geminiClient, GeminiLLMConfig config) : IGeminiService
    {
        public async Task<GeminiResponse> SendRequestToGemini(string userId, GeminiRequest request)
        {
            bool canUserAccessAi = await userService.CanUserAccessAi(userId);

            if (!canUserAccessAi)
                throw new BadRequestException("User has used hit limit, can't access Ai");

            //validate post and check if user is the owner of the post

            var post = await postRepository.GetByIdAsync(request.UserPrompt.PostId.ToString())
                ?? throw new NotFoundException("Post was not found");

            if (post.UserId != userId)
            {
                throw new UnauthorizedException("User cant access this post");
            }
    
            try
            {
                logger.LogInformation("Sending request to Gemini");

                var response = await geminiClient.Models.GenerateContentAsync(
                    model: request.Model.ToModelString(),
                    contents: request.UserPrompt.Prompt,
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

                post.Status = PostStatus.Generated;
                post.PromptText = request.UserPrompt.Prompt;
                post.Body += $"\n\n Generated text: \n\n{text}";
                postRepository.Update(post);
                await postRepository.SaveChangesAsync();

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
