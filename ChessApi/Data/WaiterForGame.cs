namespace ChessApi.Data;

public class WaiterForGame
{
    public WaiterForGame(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; set; }
    public string Name { get; set; }
}