using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;
using RRPSPU.Tests.MockData;
using RSPP.Controllers;
using RSPP.Helpers;
using RSPP.Models.DB;
using RSPP.Models.DTOs;
using RSPP.UnitOfWorks.Interfaces;
using System.Linq.Expressions;

namespace RRPSPU.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<RSPPdbContext> _rsppdbContext;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly AccountController _accountController;

        public AccountControllerTests()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _configuration = new Mock<IConfiguration>();
            _rsppdbContext = new Mock<RSPPdbContext>();
            _accountController = new AccountController(
                _rsppdbContext.Object,
                _httpContextAccessor.Object,
                _configuration.Object,
                _unitOfWork.Object);
        }

        [Fact]
        public void Login_WhenCalled_ReturnsLoginView()
        {
            var result = _accountController.Login() as ViewResult;
            Assert.NotNull(result);
            Assert.Equal("Login", result.ViewName);
        }

        [Fact]
        public void Login_WithInvalidAuthenticationRequest_ReturnsFailedResponse()
        {
            var authenticationRequest = new AuthenticationRequest()
            {
                UserEmail = "",
                Password = ""
            };
            _accountController.ModelState.AddModelError("error", "some error");

            var result = _accountController.Login(authenticationRequest);

            Assert.NotNull(result);
            var authenticationResponse = Assert.IsType<AuthenticationResponse>(result.Value);
            Assert.False(authenticationResponse.Success);
            Assert.False(authenticationResponse.IsEmailConfirmed);
            Assert.Null(authenticationResponse.UserType);
            Assert.Contains(AppMessages.LOGIN_FAILED, authenticationResponse.ResultMessage);
        }

        [Fact]
        public void Login_WithValidAuthenticationRequest_ReturnsSuccessResponse()
        {
            var authenticationRequest = new AuthenticationRequest()
            {
                UserEmail = "me@gmail.com",
                Password = ""
            };

            //_rsppdbContext.Object.UserMaster.Setup(u => (DbSet<UserMaster>)u.UserMaster.Where(user => user.UserEmail == It.IsAny<string>()))
            //    .ReturnsDbSet(AuthenticationMock.GetFakeUserMasterList());

            //_rsppdbContext.Setup(u => (DbSet<UserMaster>)u.UserMaster.Where(user => user.UserEmail == It.IsAny<string>()))
            //    .ReturnsDbSet(AuthenticationMock.GetFakeUserMasterList());
            _unitOfWork.Setup(unit => unit.UserMasterRepository.Get(
                It.IsAny<Expression<Func<UserMaster, bool>>?>(),
                        It.IsAny<Func<IQueryable<UserMaster>, IOrderedQueryable<UserMaster>>?>(),
                        It.IsAny<string>(),
                        It.IsAny<int?>(),
                        It.IsAny<int?>())
            ).Returns(() => AuthenticationMock.GetFakeUserMasterList());

            var result = _accountController.Login(authenticationRequest);

            Assert.NotNull(result);
            var authenticationResponse = Assert.IsType<AuthenticationResponse>(result.Value);
            Assert.True(authenticationResponse.Success);
            Assert.True(authenticationResponse.IsEmailConfirmed);
            Assert.NotNull(authenticationResponse.UserType);
            Assert.Contains(AppMessages.LOGIN_SUCCESSFUL, authenticationResponse.ResultMessage);
        }

        [Fact(Skip = "not implemented yet")]
        public void Login_WhenUserEmailDoesNotExist_ReturnsFailedResponse()
        {

        }

        [Fact(Skip = "not implemented yet")]
        public void Login_WhenUserPasswordDoesNotExist_ReturnsFailedResponse()
        {

        }

        [Fact(Skip = "not implemented yet")]
        public void Login_WhenUserIsNotActive_ReturnsFailedResponse()
        {

        }
    }
}
