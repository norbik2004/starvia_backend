using Core.Application.DTO.PostAttachment.Request;
using Core.Application.DTO.PostAttachment.Response;
using Core.Application.Services;
using Core.Domain.Entities;
using Core.Infrastructure.Repositories;
using Service.Exceptions;
namespace Service.Services
{
    public class PostAttachmentService(IPostAttachmentRepository postAttachmentRepository, IPostRepository postRepository) : IPostAttachmentService
    {
        public async Task<List<PostAttachmentResponse>> AddPostAttachmentAsync(PostAttachmentRequest request, string userId)
        {
            var post = await postRepository.GetByIdAsync(request.PostId.ToString())
                ?? throw new NotFoundException("Post was not found");

            if(post.UserId != userId)
            {
                throw new UnauthorizedException("You are not authorized to add attachment to this post");
            }

            List<PostAttachmentResponse> result = [];

            foreach(var postFile in request.Attachemnts)
            {
                PostAttachment postAttachment = new()
                {
                    PostId = request.PostId,
                    UserUploadedFileId = postFile.UploadedFileId,
                    Order = postFile.Order,
                };

                await postAttachmentRepository.AddAsync(postAttachment);

                result.Add(new PostAttachmentResponse
                {
                    PostId = postAttachment.PostId,
                    UserUploadedFileId = postAttachment.UserUploadedFileId,
                    Order = postAttachment.Order,
                });
            }

            await postAttachmentRepository.SaveChangesAsync();

            return result;
        }

        public Task<List<PostAttachmentResponse>> GetPostAttachemntsPerUserAndPost(int postId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<PostAttachmentResponse> GetPostAttachmentById(int postAttachmentId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task RemovePostAttachment(int postAttachmentId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<PostAttachmentResponse> UpdatePostAttachmentAsync(PostAttachmentRequest request, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
