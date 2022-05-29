using System.Text;
using Moq;
using Simple.File.Api.Interfaces;

namespace Simple.File.Api.Tests;

public class UserFileStorageTests
{
    private Mock<ISimpleFileStorage> _mockSimpleFileStorage;
    private SimpleFileStorageOptions _mockStorageOptions;
    private Mock<ICurrentUserAccessor> _mockCurrentUserAccessor;
    private UserFileStorage _testObject;
    
    [SetUp]
    public void Setup()
    {
        _mockStorageOptions = new SimpleFileStorageOptions(Directory.GetCurrentDirectory(), Int32.MaxValue);
        _mockSimpleFileStorage = new Mock<ISimpleFileStorage>();
        _mockSimpleFileStorage.Setup(_ => _.Store(It.IsAny<Stream>(), It.IsAny<string>())).Returns(Task.CompletedTask).Verifiable();
        _mockSimpleFileStorage.Setup(_ => _.Delete(It.IsAny<string>())).Verifiable();
        _mockSimpleFileStorage.Setup(_ => _.CalculateSha256(It.IsAny<string>())).Verifiable();;
        _mockCurrentUserAccessor = new Mock<ICurrentUserAccessor>();
        _mockCurrentUserAccessor.Setup(_ => _.GetUsername()).Returns("testuser");
        _testObject = new UserFileStorage(
            _mockSimpleFileStorage.Object,
            _mockStorageOptions,
            _mockCurrentUserAccessor.Object
        );
    }

    [Test]
    public async Task Test_Store_Should_Store_Stream_At_UserPath()
    {
        // arrange
        var filename = "justatest.txt";
        var memoryStream = new MemoryStream();
        var test = Encoding.UTF8.GetBytes("test string");
        await memoryStream.WriteAsync(test, 0, test.Length);
        memoryStream.Seek(0, SeekOrigin.Begin);
        
        // act
        await _testObject.Store(memoryStream, filename);
        
        // assert
        var expectedFilename = Path.Combine(Directory.GetCurrentDirectory(), "testuser", filename);
        _mockSimpleFileStorage.Verify(_ => _.Store(It.Is<Stream>(s => s.Length == memoryStream.Length), It.Is<string>(f => f.Equals(expectedFilename))), Times.Once);
        
        await memoryStream.DisposeAsync();
    }
    
    
    [Test]
    public void Test_Delete_Deletes_File_From_UserPath()
    {
        // arrange
        var filename = "justatest.txt";

        // act
         _testObject.Delete(filename);
        
        // assert
        var expectedFilename = Path.Combine(Directory.GetCurrentDirectory(), "testuser", filename);
        _mockSimpleFileStorage.Verify(_ => _.Delete( It.Is<string>(f => f.Equals(expectedFilename))), Times.Once);

    }
    
    [Test]
    public void Test_CalculateSha256_CalculatesSha256_OfFile_From_UserPath()
    {
        // arrange
        var filename = "justatest.txt";

        // act
        _testObject.CalculateSha256(filename);
        
        // assert
        var expectedFilename = Path.Combine(Directory.GetCurrentDirectory(), "testuser", filename);
        _mockSimpleFileStorage.Verify(_ => _.CalculateSha256( It.Is<string>(f => f.Equals(expectedFilename))), Times.Once);

    }
    
    [Test]
    public async Task Test_Exists_Returns_True_If_FileExists_At_Userpath()
    {
        // arrange
        var filename = "justatest.txt";
        var expectedFilename = Path.Combine(Directory.GetCurrentDirectory(), "testuser", "justatest.txt");
        var memoryStream = new MemoryStream();
        var test = Encoding.UTF8.GetBytes("test string");
        await memoryStream.WriteAsync(test, 0, test.Length);
        memoryStream.Seek(0, SeekOrigin.Begin);

        using (var targetFilestream = System.IO.File.Create(expectedFilename))
        {
            await memoryStream.CopyToAsync(targetFilestream);
        }
        await memoryStream.DisposeAsync();
        
        // act
        var result = _testObject.Exists(filename);
        
        // assert
        Assert.That(result, Is.True);

        // clean up
        System.IO.File.Delete(expectedFilename);
        
    }
    
    [Test]
    public void Test_Exists_Returns_False_If_File_DoesNot_Exists_At_Userpath()
    {
        // arrange
        var filename = "justatest.txt";
      
        // act
        var result = _testObject.Exists(filename);
        
        // assert
        Assert.That(result, Is.False);
    }
}