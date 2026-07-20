using AutoMapper;
using Core.Application.DTO.ImageGeneration.Conversation;
using Core.Application.DTO.ImageGeneration.File;
using Core.Application.DTO.ImageGeneration.Prompt;
using Core.Application.DTO.Platform.Response;
using Core.Application.DTO.Post.Request;
using Core.Application.DTO.Post.Response;
using Core.Application.DTO.PostAttachment.Request;
using Core.Application.DTO.PostAttachment.Response;
using Core.Application.DTO.PostPublication.Request;
using Core.Application.DTO.PostPublication.Response;
using Core.Application.DTO.User.Response;
using Core.Application.DTO.UserPlatform.Request;
using Core.Application.DTO.UserPlatform.Response;
using Core.Application.DTO.UserPrompt.Response;
using Core.Application.DTO.UserSetting.Request;
using Core.Application.DTO.UserSetting.Response;
using Core.Application.DTO.UserUploadedFile.Request;
using Core.Application.DTO.UserUploadedFile.Response;
using Core.Domain.Entities;

namespace Service.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserResponse>();

            CreateMap<Post, PostResponse>();
            CreateMap<Post, PostResponseLong>()
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));

            CreateMap<FileAttachmentRequest, PostAttachment>()
                .ForMember(d => d.UserUploadedFileId, o => o.MapFrom(s => s.UserUploadedFileId))
                .ForMember(d => d.Order, o => o.MapFrom(s => s.Order))
                .ForMember(d => d.PostId, o => o.Ignore());

            CreateMap<PostRequest, Post>();

            CreateMap<Platform, PlatformResponse>();
            CreateMap<PostAttachment, PostAttachmentResponse>();

            CreateMap<UserPlatform, UserPlatformResponse>();
            CreateMap<UserPlatform, UserPlatformResponseLong>();
            CreateMap<UserPlatformUpdateRequest, UserPlatform>();
            CreateMap<UserPlatformRequest, UserPlatform>();

            CreateMap<UserSetting, UserSettingResponse>();
            CreateMap<UserSettingRequest, UserSetting>();

            CreateMap<UserPrompt, UserPromptResponse>();

            CreateMap<PostPublication, PostPublicationResponse>()
                .ForMember(d => d.AccountUsername, o => o.MapFrom(s => s.UserPlatform.AccountUsername))
                .ForMember(d => d.PostBody, o => o.MapFrom(s => s.Post.Body));

            CreateMap<PublishPostRequest, PostPublication>();

            CreateMap<UserUploadedFile, UserUploadedFileResponse>()
                .ForMember(dest => dest.PreviewUrl,
                    opt => opt.MapFrom<UserUploadedFilePreviewUrlResolver>());

            CreateMap<UserUploadedFileRequest, UserUploadedFile>();


            /* image generation */
            CreateMap<ImageConversation, ImageConversationResponse>();

            CreateMap<ImageConversation, ImageConversationLongResponse>()
                .ForMember(dest => dest.ImagePromptResponses, opt => opt.MapFrom(src => src.ImagePrompts));

            CreateMap<ImagePrompt, ImagePromptResponse>()
                .ForMember(dest => dest.ImagePromptFile, opt => opt.MapFrom(src => src.ImagePromptFile));

            CreateMap<ImagePromptFile, ImagePromptFileResponse>()
                .ForMember(dest => dest.PreviewUrl,
                    opt => opt.MapFrom<ConversationImageFilePReviewUrlResolver>());
            /* image generation */
        }
    }
}
