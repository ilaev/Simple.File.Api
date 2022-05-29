using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Simple.File.Api.Tests;

public class CurrentUserAccessorTests
{
    private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private CurrentUserAccessor _testObject;
    [SetUp]
    public void Setup()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.Name, "testuser") };
        var identity = new ClaimsIdentity(claims, "Basic");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        context.User = claimsPrincipal;
        _mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);
        _testObject = new CurrentUserAccessor(_mockHttpContextAccessor.Object);
    }

    [Test]
    public void Should_Return_Username()
    {
        var username = _testObject.GetUsername();
        Assert.That(username, Is.EqualTo("testuser"));
    }
}