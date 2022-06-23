namespace ChessApi.Controllers.Requests;

public class RegisterRequestBodyType
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}