using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using Simple.File.Api.Controllers;
using Simple.File.Api.Interfaces;

namespace Simple.File.Api.Tests;

public class MockableMultiPartReader: MultipartReader
{
    
    public MockableMultiPartReader(string boundary, Stream stream) : base (boundary, stream) { }
    public new virtual Task<MultipartSection?> ReadNextSectionAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult<MultipartSection?>(new MultipartSection());
    }        
}



public class SimpleFileControllerTests
{
    private Mock<IUserFileStorage> _mockUserFileStorage;
    private SimpleFileStorageOptions _mockStorageOptions;
    private SimpleFileController _testObject;
    private Mock<HttpContext> _mockHttpContext;

    [SetUp]
    public void Setup()
    {
        _mockUserFileStorage = new Mock<IUserFileStorage>();
        _mockStorageOptions = new SimpleFileStorageOptions(Directory.GetCurrentDirectory(), 5);
        
        _mockHttpContext = new Mock<HttpContext>();
        var cts = new CancellationTokenSource();
        _mockHttpContext.Setup(_ => _.RequestAborted).Returns(cts.Token);
        var controllerContext = new ControllerContext() {
            HttpContext = _mockHttpContext.Object,
        };
        
        _testObject = new SimpleFileController(
            _mockUserFileStorage.Object,
            _mockStorageOptions
        )
        {
            ControllerContext = controllerContext
        };
    }

    [Test]
    public void Test_CalculateSha256_Endpoint_Returns_BadRequest_IfFilename_IsEmpty()
    {
        // arrange

        // act
        var result = _testObject.CalculateSha256(string.Empty);
        
        // assert
        Assert.That(result.Result, Is.InstanceOf(typeof(BadRequestObjectResult)));
    }
    
    [Test]
    public void Test_CalculateSha256_Endpoint_Returns_NotFound_IfFile_DoesNotExist()
    {
        // arrange
        _mockUserFileStorage.Setup(_ => _.Exists(It.IsAny<string>())).Returns(false);
        // act
        var result = _testObject.CalculateSha256("test.txt");
        
        // assert
        Assert.That(result.Result, Is.InstanceOf(typeof(NotFoundResult)));
    }
    
    [Test]
    public void Test_CalculateSha256_Endpoint_Returns_Sha256()
    {
        // arrange
        _mockUserFileStorage.Setup(_ => _.Exists(It.IsAny<string>())).Returns(true);
        _mockUserFileStorage.Setup(_ => _.CalculateSha256(It.IsAny<string>()))
            .Returns("688787d8ff144c502c7f5cffaafe2cc588d86079f9de88304c26b0cb99ce91c6");
        // act
        var result = _testObject.CalculateSha256("test.txt");
        
        // assert
        Assert.That(result.Value, Is.EqualTo("688787d8ff144c502c7f5cffaafe2cc588d86079f9de88304c26b0cb99ce91c6"));
    }
    
    [Test]
    public void Test_Delete_Endpoint_Returns_BadRequest_IfFilenameIsEmpty()
    {
        // arrange

        // act
        var result = _testObject.Delete(string.Empty);
        
        // assert
        Assert.That(result, Is.InstanceOf(typeof(BadRequestObjectResult)));
    }
    
    [Test]
    public void Test_Delete_Endpoint_Returns_NotFound_IfFile_DoesNot_Exist()
    {
        // arrange
        _mockUserFileStorage.Setup(_ => _.Exists(It.IsAny<string>())).Returns(false);
        // act
        var result = _testObject.Delete("test.txt");
        
        // assert
        Assert.That(result, Is.InstanceOf(typeof(NotFoundResult)));
    }
    
    [Test]
    public void Test_Delete_Endpoint_Returns_Ok_If_File_IsDeleted()
    {
        // arrange
        _mockUserFileStorage.Setup(_ => _.Exists(It.IsAny<string>())).Returns(true);
        _mockUserFileStorage.Setup(_ => _.Delete(It.IsAny<string>()));
        // act
        var result = _testObject.Delete("test.txt");
        
        // assert
        Assert.That(result, Is.InstanceOf(typeof(OkResult)));
    }
    
    [Test]
    public void Test_Upload_Returns_BadRequest_If_FormHasNoContentType()
    {
        // arrange
        _mockHttpContext.Setup(_ => _.Request.HasFormContentType).Returns(false);
        var mockFormFile = new Mock<IFormFile>();
        // act
        var result = _testObject.Upload(mockFormFile.Object);
        
        // assert
        Assert.That(result.Result, Is.InstanceOf(typeof(BadRequestObjectResult)));
    }
    
    [Test]
    public void Test_Upload_Returns_UnsupportedMediaType_If_BoundaryIsMissing()
    {
        // arrange
        _mockHttpContext.Setup(_ => _.Request.HasFormContentType).Returns(true);
        _mockHttpContext.Setup(_ => _.Request.ContentType).Returns(string.Empty);
        var mockFormFile = new Mock<IFormFile>();
        // act
        var result = _testObject.Upload(mockFormFile.Object);
        
        // assert
        Assert.That(result.Result, Is.InstanceOf(typeof(UnsupportedMediaTypeResult)));
    }
    
    
    
    [Test]
    public async Task Test_Upload_Returns_BadRequest_If_File_IsBiggerThan_FileSizeLimit()
    {
        // arrange
        _mockHttpContext.Setup(_ => _.Request.HasFormContentType).Returns(true);
        _mockHttpContext.Setup(_ => _.Request.ContentType).Returns("multipart/form-data; charset=utf-8; boundary=---WebKitFormBoundary7MA4YWxkTrZu0gW");

        var mockFormFile = new Mock<IFormFile>();
        mockFormFile.Setup(_ => _.Length).Returns(Int32.MaxValue);

        // act
        var result = _testObject.Upload(mockFormFile.Object);
        
        // assert
        Assert.That(result.Result, Is.InstanceOf(typeof(BadRequestObjectResult)));
    }
    
    
    [Test]
    public async Task Test_Uploads_File()
    {
        // arrange
        var filepath = Path.Combine(Directory.GetCurrentDirectory(), "small.txt");
        using (var fileStream = System.IO.File.OpenRead(filepath))
        {
            
            _mockHttpContext.Setup(_ => _.Request.HasFormContentType).Returns(true);
            _mockHttpContext.Setup(_ => _.Request.ContentType).Returns("multipart/form-data; charset=utf-8; boundary=---WebKitFormBoundary7MA4YWxkTrZu0gW");
            _mockHttpContext.Setup(_ => _.Request.Body).Returns(fileStream);
            
            var mockFormFile = new Mock<IFormFile>();
            mockFormFile.Setup(_ => _.Length).Returns(4);
            mockFormFile.Setup(_ => _.OpenReadStream()).Returns(fileStream);
            mockFormFile.Setup(_ => _.ContentDisposition).Returns("form-data; name=\"small\"; filename=\"small.txt\"");
            // act
            var result = _testObject.Upload(mockFormFile.Object);
            
            // assert
            Assert.That(result.Result, Is.InstanceOf(typeof(OkResult)));
        }
    }
    
    
}