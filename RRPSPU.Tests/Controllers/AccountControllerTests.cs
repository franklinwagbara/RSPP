using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Moq;
using RRPSPU.Tests.MockData;
using RSPP.Controllers;
using RSPP.Helpers;
using RSPP.Models.DB;
using RSPP.Models.DTOs;
using RSPP.Services.Interfaces;
using RSPP.UnitOfWorks.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace RRPSPU.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<RSPPdbContext> _rsppdbContext;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IEmailer> _emailer;
        private readonly AccountController _accountController;

        private readonly DefaultHttpContext _httpContext;
        private readonly Mock<IUrlHelper> _urlHelperMock;

        public AccountControllerTests()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _configuration = new Mock<IConfiguration>();
            _rsppdbContext = new Mock<RSPPdbContext>();
            _emailer = new Mock<IEmailer>();

            _accountController = new AccountController(
                _rsppdbContext.Object,
                _httpContextAccessor.Object,
                _configuration.Object,
                _unitOfWork.Object,
                _emailer.Object);

            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Scheme = "http";
            _accountController.ControllerContext.HttpContext = _httpContext;

            _urlHelperMock = new Mock<IUrlHelper>();
            _accountController.Url = _urlHelperMock.Object;
        }

        #region Login

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
            Assert.Contains($"{AppMessages.LOGIN} {AppMessages.FAILED}", authenticationResponse.ResultMessage);
        }

        [Fact(Skip = "pending implementation for httpContextAccessor")]
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
            ).Returns(() => AuthenticationMock.GetFakeUnConfirmedUserMasterList());

            var result = _accountController.Login(authenticationRequest);

            Assert.NotNull(result);
            var authenticationResponse = Assert.IsType<AuthenticationResponse>(result.Value);
            Assert.True(authenticationResponse.Success);
            Assert.True(authenticationResponse.IsEmailConfirmed);
            Assert.NotNull(authenticationResponse.UserType);
            Assert.Contains($"{AppMessages.LOGIN} {AppMessages.SUCCESSFUL}", authenticationResponse.ResultMessage);
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

        #endregion

        #region Registration

        [Fact]
        public void AccountRegister_WhenCalled_ReturnsAccountRegisterView()
        {
            var result = _accountController.AccountRegister() as ViewResult;
            Assert.NotNull(result);
            Assert.Equal("AccountRegister", result.ViewName);
        }

        [Fact]
        public void AccountRegister_WithInvalidRegistrationRequest_ReturnsFailedResponse()
        {
            var registrationRequest = new RegistrationRequest()
            {
                UserEmail = "",
                Password = ""
            };
            _accountController.ModelState.AddModelError("error", "some error");

            var result = _accountController.AccountRegister(registrationRequest);

            Assert.NotNull(result);
            var registrationResponse = Assert.IsType<BasicResponse>(result.Value);
            Assert.False(registrationResponse.Success);
            Assert.Contains($"{AppMessages.REGISTRATION} {AppMessages.FAILED}", registrationResponse.ResultMessage);
        }

        [Fact]
        public void AccountRegister_WhenEmailAlreadyExists_ReturnsFailedResponse()
        {
            var userList = AuthenticationMock.GetFakeUnConfirmedUserMasterList();
            var user = userList.FirstOrDefault();
            if (user != null)
            {

                var registrationRequest = new RegistrationRequest()
                {
                    UserEmail = user.UserEmail,
                    Password = user.Password,
                    ConfirmPassword = user.Password,
                    MobilePhoneNumber = "09087866567",
                    CompanyAddress = "address",
                    CompanyName = "company name"
                };

                _unitOfWork.Setup(unit => unit.UserMasterRepository.Get(
                    It.IsAny<Expression<Func<UserMaster, bool>>?>(),
                            It.IsAny<Func<IQueryable<UserMaster>, IOrderedQueryable<UserMaster>>?>(),
                            It.IsAny<string>(),
                            It.IsAny<int?>(),
                            It.IsAny<int?>())
                ).Returns(() => userList);

                var result = _accountController.AccountRegister(registrationRequest);

                Assert.NotNull(result);
                var registrationResponse = Assert.IsType<BasicResponse>(result.Value);
                Assert.False(registrationResponse.Success);
                Assert.Contains($"{AppMessages.EMAIL} {AppMessages.EXISTS}", registrationResponse.ResultMessage);
            }
        }

        [Fact]
        public void AccountRegister_WhenRequestIsValid_ReturnsSuccessResponse()
        {

            string url = "callbackUrl";
            _urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns(url).Verifiable();

            var userList = AuthenticationMock.GetFakeEmptyUserMasterList();
            var user = AuthenticationMock.GetFakeUnConfirmedUserMaster();
            var registrationRequest = new RegistrationRequest()
            {
                UserEmail = user.UserEmail,
                Password = user.Password,
                ConfirmPassword = user.Password,
                MobilePhoneNumber = user.PhoneNum,
                CompanyAddress = user.CompanyAddress,
                CompanyName = user.CompanyName
            };

            _unitOfWork.Setup(unit => unit.UserMasterRepository.Get(
                It.IsAny<Expression<Func<UserMaster, bool>>?>(),
                        It.IsAny<Func<IQueryable<UserMaster>, IOrderedQueryable<UserMaster>>?>(),
                        It.IsAny<string>(),
                        It.IsAny<int?>(),
                        It.IsAny<int?>())
            ).Returns(() => userList);

            _emailer.Setup(email => email.SendEmail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                )).Returns(BasicResponse.Result(true, $"{AppMessages.EMAIL} {AppMessages.SENDING} {AppMessages.SUCCESSFUL}"));


            var result = _accountController.AccountRegister(registrationRequest);

            Assert.NotNull(result);
            var registrationResponse = Assert.IsType<BasicResponse>(result.Value);
            Assert.True(registrationResponse.Success);
            Assert.Contains($"{AppMessages.REGISTRATION} {AppMessages.SUCCESSFUL} {AppMessages.EMAIL_VERIFICATION_LINK_SENT}", registrationResponse.ResultMessage);
            _unitOfWork.Verify(unit => unit.UserMasterRepository.Add(It.IsAny<UserMaster>()), Times.Once);

            //confirm that password is encrypted
            //confirm created on has value
            //confirm userType has correct value
            //confirm userRole has correct value
            //emailConfirmed is false
            //emailConfirmationToken has correct value
        }

        [Fact]
        public void AccountRegister_WhenRequestIsValidButEmailIsNotSent_ReturnsSuccessResponse()
        {

            string url = "callbackUrl";
            _urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>())).Returns(url).Verifiable();

            var userList = AuthenticationMock.GetFakeEmptyUserMasterList();
            var user = AuthenticationMock.GetFakeUnConfirmedUserMaster();
            var registrationRequest = new RegistrationRequest()
            {
                UserEmail = user.UserEmail,
                Password = user.Password,
                ConfirmPassword = user.Password,
                MobilePhoneNumber = user.PhoneNum,
                CompanyAddress = user.CompanyAddress,
                CompanyName = user.CompanyName
            };

            _unitOfWork.Setup(unit => unit.UserMasterRepository.Get(
                It.IsAny<Expression<Func<UserMaster, bool>>?>(),
                        It.IsAny<Func<IQueryable<UserMaster>, IOrderedQueryable<UserMaster>>?>(),
                        It.IsAny<string>(),
                        It.IsAny<int?>(),
                        It.IsAny<int?>())
            ).Returns(() => userList);

            _emailer.Setup(email => email.SendEmail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
                )).Returns(BasicResponse.Result(false, $"{AppMessages.EMAIL} {AppMessages.SENDING} {AppMessages.FAILED}"));


            var result = _accountController.AccountRegister(registrationRequest);

            Assert.NotNull(result);
            var registrationResponse = Assert.IsType<BasicResponse>(result.Value);
            Assert.True(registrationResponse.Success);
            Assert.Contains($"{AppMessages.REGISTRATION} {AppMessages.SUCCESSFUL} {AppMessages.EMAIL_VERIFICATION_LINK_FAILED}", registrationResponse.ResultMessage);
            _unitOfWork.Verify(unit => unit.UserMasterRepository.Add(It.IsAny<UserMaster>()), Times.Once);
        }
        #endregion
    }
}
