using Microsoft.Extensions.Options;
using Simple.File.Api.Interfaces;

namespace Simple.File.Api;

public class UserFileStorage : IUserFileStorage
{
    private readonly ISimpleFileStorage _fileStorage;
    private readonly SimpleFileStorageOptions _storageOptions;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    
    public UserFileStorage(
        ISimpleFileStorage storage,
        SimpleFileStorageOptions options,
        ICurrentUserAccessor currentUserAccessor)
    {
        _fileStorage = storage;
        _storageOptions = options;
        _currentUserAccessor = currentUserAccessor;
    }

    private string GetUserDirectoryPath()
    {
        var basePathPart = _storageOptions.BasePath;
        var userPathPart = _currentUserAccessor.GetUsername();
        return Path.Combine(basePathPart, userPathPart);
    }

    private string GetFullPath(string filename)
    {
        var userDirectoryPath = GetUserDirectoryPath();
        return Path.Combine(userDirectoryPath, filename);
    }
    
    public Task Store(Stream stream, string filename)
    {
        var userDirectory = GetUserDirectoryPath();
        if (!Directory.Exists(userDirectory))
            Directory.CreateDirectory(userDirectory);
        return _fileStorage.Store(stream, GetFullPath(filename));
    }

    public void Delete(string filename)
    {
        _fileStorage.Delete(GetFullPath(filename));
    }

    public string CalculateSha256(string filename)
    {
        return _fileStorage.CalculateSha256(GetFullPath(filename));
    }

    public bool Exists(string filename)
    {
        return System.IO.File.Exists(GetFullPath(filename));
    }
}