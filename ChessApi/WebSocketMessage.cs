namespace ChessApi;

public class WebSocketMessage
{
    public WebSocketMessage(string toUserId, string fromUserId, string fromUserName, EnumWebSocketMessageType messageType, bool? isWhite = false, string? chessBoard = null, string? chatMessage = null, string? winnerId = null, string? winnerName = null)
    {
        ToUserId = toUserId;
        FromUserId = fromUserId;
        FromUserName = fromUserName;
        MessageType = messageType;
        IsWhite = isWhite;
        ChessBoard = chessBoard;
        ChatMessage = chatMessage;
        WinnerId = winnerId;
        WinnerName = winnerName;
    }

    public string ToUserId { get; set; }
    public string FromUserId { get; set; }
    public string FromUserName { get; set; }
    public EnumWebSocketMessageType MessageType { get; set; }
    public string? ChessBoard { get; set; }
    public string? ChatMessage { get; set; }
    public bool? IsWhite { get; set; }
    public string? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    
    public class Cell
    {
        public EnumFigureType Figure { get; set; }
        public bool IsWhite { get; set; }
        public bool IsWhiteCell { get; set; }
    }
    
    public enum EnumWebSocketMessageType
    {
        ChatMessage = 0,
        ChessStep = 1,
        StartGame = 2,
        EndGame = 3,
    }
    
    public enum EnumFigureType
    {
        None = 0,
        Pawn = 1,
        Rook = 2,
        Elephant = 3,
        Queen = 4,
        King = 5,
        Horse = 6
    }
}