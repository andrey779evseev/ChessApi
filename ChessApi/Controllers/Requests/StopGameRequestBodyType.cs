namespace ChessApi.Controllers.Requests;

public class StopGameRequestBodyType
{
    public string FirstOpponentId { get; set; }
    public string SecondOpponentId { get; set; }
    public string WinnerId { get; set; }
}