using Core.Application.DTO.ImageGeneration.Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Services
{
    public interface IImageConversationService
    {
        public Task<ImageConversationResponse> AddImageConversationAsync(string userId, string? title);
        public Task<List<ImageConversationResponse>> GetAllConversationPerUserAsync(string userId);
        public Task<ImageConversationLongResponse> GetImageConversationLongPerId(string userId, int conversationId);
    }
}
