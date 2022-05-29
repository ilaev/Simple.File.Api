namespace Simple.File.Api.Interfaces;

/// <summary>
/// Apis user repository 
/// </summary>
public interface IUserService
{
    bool ValidateCredentials(string username, string password);
}