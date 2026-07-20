using AutoMapper;
using AutoMapper.QueryableExtensions;
using Core.Application.DTO.ImageGeneration.Conversation;
using Core.Application.DTO.Post.Response;
using Core.Application.Services;
using Core.Domain.Entities;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class ImageConversationService(IMapper mapper, IImageConversationRepository imageConversationRepository) : IImageConversationService
    {
        public async Task<ImageConversationResponse> AddImageConversationAsync(string userId, string? title)
        {
            var entity = new ImageConversation
            {
                UserId = userId,
                Title = title,
                CreatedAt = DateTime.UtcNow
            };

            await imageConversationRepository.AddAsync(entity);

            await imageConversationRepository.SaveChangesAsync();

            return new ImageConversationResponse { Title = entity.Title , CreatedAt = entity.CreatedAt};
        }

        public async Task<List<ImageConversationResponse>> GetAllConversationPerUserAsync(string userId)
        {
            var conversations = imageConversationRepository.GetAllAsQueryPerUserIdAsync(userId);

            return await conversations
                .ProjectTo<ImageConversationResponse>(mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<ImageConversationLongResponse> GetImageConversationLongPerId(string userId, int conversationId)
        {
            var conversation = await imageConversationRepository.GetByIdAsync(conversationId.ToString());

            if (conversation == null)
                throw new NotFoundException("Conversation was not found");

            return mapper.Map<ImageConversationLongResponse>(conversation);
        }
    }
}
