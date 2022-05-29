using System.Text;

namespace Simple.File.Api.Tests;

public class SimpleFileStorageTests
{
    private SimpleFileStorage _testObject;
    private string _pathForTest;

    [SetUp]
    public void Setup()
    {
        _testObject = new SimpleFileStorage();
        _pathForTest = Path.Combine(Directory.GetCurrentDirectory(), "test.txt");

    }

    [TearDown]
    public void TearDown()
    {
        System.IO.File.Delete(_pathForTest);
    }

    [Test]
    public async Task Test_Store_Saves_Stream_AtProvidedFilepath()
    {
        // arrange
        var memoryStream = new MemoryStream();
        var test = Encoding.UTF8.GetBytes("test string");
        await memoryStream.WriteAsync(test, 0, test.Length);
        memoryStream.Seek(0, SeekOrigin.Begin);
        // act
        await _testObject.Store(memoryStream, _pathForTest);
        memoryStream.Close();
        await memoryStream.DisposeAsync();
        // assert
        Assert.That(System.IO.File.Exists(_pathForTest), Is.True);
    }
    
    [Test]
    public async Task Test_Delete_Deletes_File_AtProvidedFilepath()
    {
        // arrange
        var memoryStream = new MemoryStream();
        var test = Encoding.UTF8.GetBytes("test string");
        await memoryStream.WriteAsync(test, 0, test.Length);
        memoryStream.Seek(0, SeekOrigin.Begin);
        await _testObject.Store(memoryStream, _pathForTest);
        await memoryStream.DisposeAsync();
        
        // act
        _testObject.Delete(_pathForTest);
        // assert
        Assert.That(System.IO.File.Exists(_pathForTest), Is.False);
    }

    [Test]
    public async Task Test_Delete_DoesNothingIfFileIsLockedByAnotherProcess()
    {
        // arrange
        var pathToBigFile = Path.Combine(Directory.GetCurrentDirectory(), "fake.txt");

        using (var fileStream = System.IO.File.OpenRead(pathToBigFile))
        {
            // act
            _testObject.Delete(pathToBigFile);

            // assert
            Assert.That(System.IO.File.Exists(pathToBigFile), Is.True);
        }
    }

    [Test]
    public async Task Test_CalculateSha256_CalculatesSha256_ForProvidedFilepath()
    {
        // arrange
        var memoryStream = new MemoryStream();
        var test = Encoding.UTF8.GetBytes("test string");
        await memoryStream.WriteAsync(test, 0, test.Length);
        memoryStream.Seek(0, SeekOrigin.Begin);
        await _testObject.Store(memoryStream, _pathForTest);
        await memoryStream.DisposeAsync();
        // act
        var resultSha256 = _testObject.CalculateSha256(_pathForTest);
        // assert
        Assert.That(resultSha256, Is.EqualTo("d5579c46dfcc7f18207013e65b44e4cb4e2c2298f4ac457ba8f82743f31e930b"));
    }
    
    [Test]
    public void Test_CalculateSha256_Returns_StringEmpty_IfFilepathDoesNotExist()
    {
        // arrange
        // don't create a file
        // act
        var resultSha256 = _testObject.CalculateSha256(_pathForTest);
        // assert
        Assert.That(resultSha256, Is.EqualTo(string.Empty));
    }
}