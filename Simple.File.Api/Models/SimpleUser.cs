namespace Simple.File.Api;

/// <summary>
/// User for basic auth.
/// </summary>
public class SimpleUser
{
    public string Username { get; set; }
    public string Password { get; set; }

    public SimpleUser()
    {}
    public SimpleUser(
        string username,
        string password)
    {
        Username = username;
        Password = password;
    }
}
