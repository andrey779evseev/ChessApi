namespace ChessApi.Controllers.Requests;

public class StartGameRequestBodyType
{
    public string FirstOpponentId { get; set; }
    public string SecondOpponentId { get; set; }
    public bool IsWhite { get; set; }
    public string Board { get; set; }
}