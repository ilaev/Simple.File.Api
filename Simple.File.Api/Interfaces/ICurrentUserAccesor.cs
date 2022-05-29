namespace Simple.File.Api.Interfaces;

/// <summary>
/// Provides all necessary information about the current user.
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>
    /// Returns the user name.
    /// </summary>
    /// <returns></returns>
    public string GetUsername();
}