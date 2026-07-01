using AutoMapper;
using Core.Application.DTO.PostAttachment.Request;
using Core.Application.DTO.PostAttachment.Response;
using Core.Application.Services;
using Core.Domain.Entities;
using Core.Infrastructure.Repositories;
using Service.Exceptions;
namespace Service.Services
{
    public class PostAttachmentService(IPostAttachmentRepository postAttachmentRepository, IPostRepository postRepository,
        IMapper mapper) : IPostAttachmentService
    {
        public async Task<List<PostAttachmentResponse>> AddPostAttachmentAsync(PostAttachmentRequest request, string userId)
        {
            var post = await postRepository.GetByIdAsync(request.PostId.ToString())
                ?? throw new NotFoundException("Post was not found");

            if (post.UserId != userId)
            {
                throw new UnauthorizedException("You are not authorized to add attachment to this post");
            }

            var attachments = mapper.Map<List<PostAttachment>>(request.Attachments);

            attachments.ForEach(x => x.PostId = request.PostId);

            foreach (var attachment in attachments)
            {
                await postAttachmentRepository.AddAsync(attachment);
            }

            await postAttachmentRepository.SaveChangesAsync();

            return mapper.Map<List<PostAttachmentResponse>>(attachments);
        }

        public Task<List<PostAttachmentResponse>> GetPostAttachemntsPerUserAndPost(int postId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<PostAttachmentResponse> GetPostAttachmentById(int postAttachmentId, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task RemovePostAttachment(int postAttachmentId, string userId)
        {
            var postAttachment = await postAttachmentRepository.GetByIdAsync(postAttachmentId.ToString())
                ?? throw new NotFoundException("Post attachment was not found");

            if (postAttachment.Post.UserId != userId)
            {
                throw new UnauthorizedException("You are not authorized to remove this attachment");
            }

            postAttachmentRepository.Remove(postAttachment);
            await postAttachmentRepository.SaveChangesAsync();
        }

        public async Task<List<PostAttachmentResponse>> UpdatePostAttachmentOrdersAsync(
            UpdatePostAttachmentOrdersRequest request,
            string userId)
        {
            var post = await postRepository.GetByIdAsync(request.PostId.ToString())
                ?? throw new NotFoundException("Post was not found");

            if (post.UserId != userId)
            {
                throw new UnauthorizedException("You are not authorized to modify this post");
            }

            var attachments = await postAttachmentRepository.GetAllPerPostAndUserId(request.PostId, userId);

            foreach (var attachment in attachments)
            {
                var updated = request.Attachments.FirstOrDefault(x =>
                    x.UserUploadedFileId == attachment.UserUploadedFileId);

                if (updated != null)
                {
                    attachment.Order = updated.Order;
                }
            }

            await postAttachmentRepository.SaveChangesAsync();

            return mapper.Map<List<PostAttachmentResponse>>(attachments);
        }
    }
}
