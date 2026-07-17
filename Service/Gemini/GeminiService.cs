using AutoMapper;
using Core.Application.DTO.ImageGeneration.Prompt;
using Core.Application.DTO.UserPrompt.Request;
using Core.Application.Services;
using Core.Application.Services.Gemini;
using Core.Domain.Consts;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Infrastructure.Repositories;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Abstractions;
using Minio;
using Service.Exceptions;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Gemini
{
    public class GeminiService(ILogger<IGeminiService> logger, IUserService userService, IPostRepository postRepository,
        IUserPromptRepository userPromptRepository, Client geminiClient, GeminiLlMConfig config,
        IGeminiModelHealthService geminiModelHealthService, IImagePromptRepository imagePromptRepository,
        IMinioService minioService, IMapper mapper) : IGeminiService
    {

        public async Task<string> AskAiPostScope(string userId, UserPromptRequest request)
        {
            await UserAccesibilityValidation(userId);

            var post = await postRepository.GetByIdAsync(request.PostId.ToString())
                    ?? throw new NotFoundException("Post was not found");

            if (post.UserId != userId)
            {
                throw new UnauthorizedException("User cant access this post");
            }

            var models = await GetAvailableModelsAsync<LlmGeminiModelType>(
                LlmGeminiModelType.Gemini3FlashPreview);

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

        public async Task<ImagePromptResponse> GenerateImage(string userId, ImagePromptRequest request)
        {
            await UserAccesibilityValidation(userId);

            var pager = await geminiClient.Models.ListAsync();

            await foreach (var model in pager)
            {
                Console.WriteLine(model.Name);
            }

            var models = await GetAvailableModelsAsync<ImageGeminiModelType>(
                 ImageGeminiModelType.Imagen4FastGenerate);

            if (models.Count == 0)
            {
                throw new BadRequestException(
                    "Our Ai models are currently unavalible, please try again in 5 minutes");
            }

            var configuration = config.GetGenerateImagesConfig();

            Exception? lastException = null;

            var aiPrompt = $"""
                Jesteś ekspertem w generowaniu obrazów przez AI.
                
                Zawsze generuj obrazy które są:
                - ultra ostre
                - wysokiej jakości
                - fotorealistyczne tam gdzie to ma sens
                - z czystą, czytelną kompozycją
                - z poprawną anatomią (jeśli występują postacie)
                - z realistycznym oświetleniem
                - w jakości HDR
                - w jakości 8K
                - bez rozmycia
                - bez artefaktów
                
                Jeśli obraz zawiera tekst:
                - zapisz DOKŁADNIE podany tekst
                - bez błędów ortograficznych
                - bez brakujących liter
                - z pełną czytelnością
                - wyśrodkowany
                - z wysokim kontrastem
                
                Zapytanie użytkownika:
                {request.Prompt}
             """;

            foreach (var model in models)
            {
                try
                {
                    logger.LogInformation(
                        "Trying Gemini model {Model}",
                        model);

                    var response = await geminiClient.Models.GenerateImagesAsync(
                        model: model.ToModelString(),
                        config: configuration,
                        prompt: aiPrompt
                    );

                    if(response.GeneratedImages == null || response.GeneratedImages.Count == 0)
                    {
                        throw new BadRequestException(
                            $"Empty response from Ai");
                    }

                    var image = response.GeneratedImages.First();

                    logger.LogInformation(
                        "Model {Model} succeeded",
                        model);
 
                    var formFile = ToFormFile(image.Image!, $"{Guid.NewGuid()}.png");

                    var fileList = new List<(string ObjectPath, IFormFile File)>
                    {
                        ($"{DateTime.UtcNow:yyyy-MM-dd}/{Guid.NewGuid()}.png", formFile)
                    };

                    await minioService.SaveFilesAsync(
                        fileList,
                        BucketNames.GeminiGeneratedImagesBucketName);

                    ImagePrompt imagePrompt = new ImagePrompt
                    {
                        Prompt = request.Prompt,
                        ConversationId = request.ConversationId,
                        ImagePromptFile = new ImagePromptFile
                        {
                            FileName = "StarviaAiGeneratedImage.png",
                            FilePath = fileList.First().ObjectPath
                        }
                    };
                    
                    await imagePromptRepository.AddAsync(imagePrompt);
                    await imagePromptRepository.SaveChangesAsync();

                    var imagePromptResponse = mapper.Map<ImagePromptResponse>(imagePrompt);

                    return imagePromptResponse;
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


        public async Task<string> GenerateUserMimicConfig(string userId, string userTexts)
        {
            await UserAccesibilityValidation(userId);

            var models = await GetAvailableModelsAsync<LlmGeminiModelType>(
                LlmGeminiModelType.Gemini3FlashPreview);

            if (models.Count == 0)
            {
                throw new BadRequestException(
                    "Our Ai models are currently unavalible, please try again in 5 minutes");
            }

            var configuration = config.GetUserMimicGenerationConfig();
            Exception? lastException = null;

            var content = new List<Content>
            {
                new() {
                    Role = "user",
                    Parts =
                    [
                        new Part { Text = userTexts }
                    ]
                }
            };

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

        private async Task UserAccesibilityValidation(string userId)
        {
            bool canUserAccessAi = await userService.CanUserAccessAi(userId);

            if (!canUserAccessAi)
                throw new BadRequestException("User has used hit limit, can't access Ai");
        }

        private async Task<List<TModel>> GetAvailableModelsAsync<TModel>(TModel preferredModel)
            where TModel : Enum
        {
            var models = Enum.GetValues(typeof(TModel))
                .Cast<TModel>()
                .OrderBy(x => EqualityComparer<TModel>.Default.Equals(x, preferredModel) ? 0 : 1);

            var result = new List<TModel>();

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

        private static FormFile ToFormFile(Image image, string fileName)
        {
            if (image.ImageBytes == null)
                throw new ArgumentException("Image does not contain any bytes.");

            var stream = new MemoryStream(image.ImageBytes);

            return new FormFile(
                stream,
                0,
                image.ImageBytes.Length,
                "file",
                fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = image.MimeType ?? "image/png"
            };
        }
    }
}
