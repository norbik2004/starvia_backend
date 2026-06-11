using AutoMapper;
using Core.DTO.Platform.Response;
using Core.DTO.Post.Request;
using Core.DTO.Post.Response;
using Core.DTO.PostPublication.Request;
using Core.DTO.PostPublication.Response;
using Core.DTO.User.Response;
using Core.DTO.UserPlatform.Request;
using Core.DTO.UserPlatform.Response;
using Core.DTO.UserPrompt.Response;
using Core.DTO.UserSetting.Request;
using Core.DTO.UserSetting.Response;
using Core.Entities;

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
        }
    }
}
