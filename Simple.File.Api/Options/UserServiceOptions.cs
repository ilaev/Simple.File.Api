namespace Simple.File.Api;

public class UserServiceOptions
{
    public SimpleUser[] Users { get; set; }

    public UserServiceOptions()
    {
    }
    public UserServiceOptions(
        SimpleUser[] users)
    {
        Users = users;
    }
}