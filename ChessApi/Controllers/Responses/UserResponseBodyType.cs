namespace ChessApi.Controllers.Responses;

public class UserResponseBodyType
{
    public UserResponseBodyType(string name, string id)
    {
        Name = name;
        Id = id;
    }

    public string Name { get; set; }
    public string Id { get; set; }
}