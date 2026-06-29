using AutoMapper;
using Core.Application.DTO.Platform.Response;
using Core.Application.DTO.Post.Request;
using Core.Application.DTO.Post.Response;
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
            CreateMap<PostRequest, Post>();

            CreateMap<Platform, PlatformResponse>();

            CreateMap<UserPlatform, UserPlatformResponse>();
            CreateMap<UserPlatform, UserPlatformResponseLong>();
            CreateMap<UserPlatformUpdateRequest, UserPlatform>();
            CreateMap<UserPlatformRequest, UserPlatform>();

            CreateMap<UserSetting, UserSettingResponse>();
            CreateMap<UserSettingRequest, UserSetting>();

            CreateMap<UserPrompt, UserPromptResponse>();

            CreateMap<PostPublication, PostPublicationResponse>();
            CreateMap<PublishPostRequest, PostPublication>();

            CreateMap<UserUploadedFile, UserUploadedFileResponse>()
                .ForMember(dest => dest.PreviewUrl,
                    opt => opt.MapFrom<UserUploadedFilePreviewUrlResolver>());

            CreateMap<UserUploadedFileRequest, UserUploadedFile>();
        }
    }
}
