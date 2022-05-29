using Microsoft.Extensions.Options;
using Simple.File.Api.Interfaces;

namespace Simple.File.Api;

public class UserService : IUserService
{
    private readonly Dictionary<string, SimpleUser> _source;

    public UserService(
        IOptions<UserServiceOptions> options
    )
    {
        _source = options.Value.Users.ToDictionary(u => u.Username);
    }


    public bool ValidateCredentials(string username, string password)
    {
        var exists = _source.TryGetValue(username, out var user);
        if (exists && user is not null)
        {
            return user.Password == password;
        }
        else
        {
            return false;
        }
    }
}