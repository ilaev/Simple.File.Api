using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using Simple.File.Api.Interfaces;
using Simple.File.Api.Security;

namespace Simple.File.Api.Tests;

public class BasicAuthenticationHandlerTests
{
    private Mock<IUserService> _mockUserService;
    private BasicAuthenticationHandler _testObject;
    
    [SetUp]
    public void Setup()
    {
        var mockOptionsMonitor = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        // This Setup is required for .NET Core 3.1 onwards.
        mockOptionsMonitor
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns(new AuthenticationSchemeOptions());

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var logger = new Mock<ILogger<BasicAuthenticationHandler>>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(logger.Object);
        var mockUrlEncoder = new Mock<UrlEncoder>();
        var mockSystemClock = new Mock<SystemClock>();
        _mockUserService = new Mock<IUserService>();
        
        _testObject = new BasicAuthenticationHandler(
            mockOptionsMonitor.Object,
            mockLoggerFactory.Object,
            mockUrlEncoder.Object,
            mockSystemClock.Object,
            _mockUserService.Object);
    }

    [Test]
    public async Task Test_AuthenticateAsync_Should_Authenticate_When_Credentials_AreValid()
    {
        // arrange
        var context = new DefaultHttpContext();
        var authorizationHeader = new StringValues("Basic dGVzdHVzZXI6MTIzNA==");
        context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);
        await _testObject.InitializeAsync(new AuthenticationScheme("Basic", null, typeof(BasicAuthenticationHandler)), context);
        _mockUserService.Setup(_ => _.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        // act     
        var authResult = await _testObject.AuthenticateAsync();
       
       Assert.That(authResult.Succeeded, Is.True);
    }
    
    [Test]
    public async Task Test_AuthenticateAsync_Should_AugmentClaimsPrincipal_When_Credentials_AreValid()
    {
        // arrange
        var context = new DefaultHttpContext();
        var authorizationHeader = new StringValues("Basic dGVzdHVzZXI6MTIzNA==");
        context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);
        await _testObject.InitializeAsync(new AuthenticationScheme("Basic", null, typeof(BasicAuthenticationHandler)), context);
        _mockUserService.Setup(_ => _.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        // act     
        var authResult = await _testObject.AuthenticateAsync();
       
        Assert.That(authResult.Principal, Is.Not.Null);
        Assert.That(authResult.Principal.FindFirstValue(ClaimTypes.Name), Is.EqualTo("testuser"));
    }
    
    [Test]
    public async Task Test_AuthenticateAsync_Should_Not_Authenticate_When_Credentials_AreInvalid()
    {
        // arrange
        var context = new DefaultHttpContext();
        var authorizationHeader = new StringValues("Basic d3Jvbmd1c2VyOjEyMzQ=");
        context.Request.Headers.Add(HeaderNames.Authorization, authorizationHeader);
        await _testObject.InitializeAsync(new AuthenticationScheme("Basic", null, typeof(BasicAuthenticationHandler)), context);
        _mockUserService.Setup(_ => _.ValidateCredentials(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        // act     
        var authResult = await _testObject.AuthenticateAsync();
       
        Assert.That(authResult.Succeeded, Is.False);
        Assert.That(authResult.Failure.Message, Is.EqualTo("wrong user credentials"));
    }
    
    [Test]
    public async Task HandleAuthenticateAsync_NoAuthorizationHeader_ReturnsAuthenticateResultFail()
    {
        // arrange
        var context = new DefaultHttpContext();
        await _testObject.InitializeAsync(new AuthenticationScheme("Basic", null, typeof(BasicAuthenticationHandler)), context);
        var authResult = await _testObject.AuthenticateAsync();

        Assert.That(authResult.Succeeded, Is.False);
        Assert.That(authResult.Failure.Message, Is.EqualTo("Invalid Authorization Header"));
    }
}