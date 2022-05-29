namespace Simple.File.Api;

public class SimpleFileStorageOptions
{
    public string BasePath { get; private set; }
    public int FileSizeLimitInBytes { get; private set; }

    public SimpleFileStorageOptions(
        string basePath,
        int fileSizeLimit)
    {
        BasePath = basePath;
        FileSizeLimitInBytes = fileSizeLimit;
    }
}