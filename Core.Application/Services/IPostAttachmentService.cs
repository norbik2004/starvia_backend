using Core.Application.DTO.PostAttachment.Request;
using Core.Application.DTO.PostAttachment.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Services
{
    public interface IPostAttachmentService
    {
        public Task<List<PostAttachmentResponse>> AddPostAttachmentAsync(PostAttachmentRequest request, string userId);
        public Task<PostAttachmentResponse> UpdatePostAttachmentAsync(PostAttachmentRequest request, string userId);
        public Task RemovePostAttachment(int postAttachmentId, string userId);
        public Task<PostAttachmentResponse> GetPostAttachmentById(int postAttachmentId, string userId);
        public Task<List<PostAttachmentResponse>> GetPostAttachemntsPerUserAndPost(int postId, string userId);
    }
}
