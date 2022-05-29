namespace Simple.File.Api;

public class SimpleFileApiSettings
{
    public UserServiceOptions UserServiceOptions { get; set; }
    public string FilesLocation { get; set; }
    public int FileSizeLimitInBytes { get; set; }
}

public static class SimpleFileApiSettingsExtensions
{
    public static SimpleFileStorageOptions GetStorageOptions(this SimpleFileApiSettings apiSettings)
    {
        return new SimpleFileStorageOptions(apiSettings.FilesLocation, apiSettings.FileSizeLimitInBytes);
    }
}