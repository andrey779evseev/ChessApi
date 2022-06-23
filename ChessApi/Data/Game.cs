namespace ChessApi.Data;

public class Game
{
    public Game(string firstOpponentId, string secondOpponentId, GameStatus status, string? winnerId, string? winnerName)
    {
        Id = Guid.NewGuid().ToString();
        FirstOpponentId = firstOpponentId;
        SecondOpponentId = secondOpponentId;
        Status = status;
        WinnerId = winnerId;
        WinnerName = winnerName;
    }

    public string Id { get; set; }
    public string FirstOpponentId { get; set; }
    public string SecondOpponentId { get; set; }
    public GameStatus Status { get; set; }
    public string? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    
    
    public enum GameStatus
    {
        InProgress = 0,
        Finished = 1
    }
}