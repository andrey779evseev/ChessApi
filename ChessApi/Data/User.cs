namespace ChessApi.Data;

public class User
{
    public User(string name, string email, string password)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        Email = email;
        Password = password;
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}