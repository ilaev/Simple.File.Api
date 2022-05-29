namespace Simple.File.Api.Interfaces;

/// <summary>
/// ISimpleFileStorage is responsible for the plain file interactions.
/// </summary>
public interface ISimpleFileStorage
{
    /// <summary>
    /// Stores specified stream at provided path.
    /// </summary>
    /// <param name="stream">Stream to store to a file</param>
    /// <param name="filepath">Must be an absolute path</param>
    /// <returns></returns>
    public Task Store(Stream stream, string filepath);
    /// <summary>
    /// Deletes provided absolute file path from storage.
    /// Throws an exception if file is being processed by another process.
    /// </summary>
    /// <param name="filepath">Absolute file path</param>
    /// <returns></returns>
    public void Delete(string filepath);
    /// <summary>
    /// Calculates SHA256 for a stored file.
    /// Throws if file does not exist.
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    public string CalculateSha256(string filepath);
}