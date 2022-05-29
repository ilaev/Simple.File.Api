using Microsoft.Extensions.Options;
using Moq;

namespace Simple.File.Api.Tests;

public class UserRepositoryTests
{
    private Mock<IOptions<UserServiceOptions>> _mockUserServiceOptions;
    private UserService _testObject;
    
    [SetUp]
    public void Setup()
    {
        _mockUserServiceOptions = new Mock<IOptions<UserServiceOptions>>();
        var options = new UserServiceOptions(new SimpleUser[1] { new SimpleUser("testuser", "1234") });
        _mockUserServiceOptions.Setup(_ => _.Value).Returns(options);
        _testObject = new UserService(_mockUserServiceOptions.Object);
    }

    [Test]
    public void Test_Validate_Credentials_Should_ReturnTrueWhenMatchingUserExists()
    {
        var areCredentialsValid = this._testObject.ValidateCredentials("testuser", "1234");
        Assert.That(areCredentialsValid, Is.True);
    }
    
    [Test]
    public void Test_Validate_Credentials_Should_ReturnFalseWhenMatchingUserExistsButPasswordIsWrong()
    {
        var areCredentialsValid = this._testObject.ValidateCredentials("testuser", "123455");
        Assert.That(areCredentialsValid, Is.False);
    }
    
    [Test]
    public void Test_Validate_Credentials_Should_ReturnFalseWhenMatchingUserDoesNotExist()
    {
        var areCredentialsValid = this._testObject.ValidateCredentials("wrongusername", "1234");
        Assert.That(areCredentialsValid, Is.False);
    }
    
}