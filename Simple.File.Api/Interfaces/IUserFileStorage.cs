namespace Simple.File.Api.Interfaces;

/// <summary>
/// Provides *user based* physical storage.
/// </summary>
public interface IUserFileStorage
{
    public Task Store(Stream stream, string filename);
    public void Delete(string filename);
    public string CalculateSha256(string filename);
    public bool Exists(string filename);
}