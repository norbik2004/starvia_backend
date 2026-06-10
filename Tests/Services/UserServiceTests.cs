using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using tr_core.Entities;
using tr_core.Repositories;
using tr_core.Services;
using tr_service.Mapping;

namespace tr_tests.Services
{
    public class UserServiceTests
    {
        protected readonly IMapper mapper;
        protected readonly Mock<IUserService> _userServiceMock;
        protected readonly Mock<UserManager<User>> _userManagerMock;
        protected readonly Mock<IUserRepository> _userRepositoryMock;

        public UserServiceTests() 
        {
            var configuration = new MapperConfiguration(configure: cfg => { cfg.AddProfile<AutoMapperProfile>(); }, null);
            mapper = configuration.CreateMapper();

            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(),
                null, null, null, null, null, null, null, null);

            _userServiceMock = new Mock<IUserService>(_userManagerMock.Object,
                );

        }
    }
}
