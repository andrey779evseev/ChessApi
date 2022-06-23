namespace ChessApi.Controllers.Responses;

public class RateResponseBodyType
{
    public RateResponseBodyType(string name, string id, int winsCount)
    {
        Name = name;
        Id = id;
        WinsCount = winsCount;
    }

    public string Name { get; set; }
    public string Id { get; set; }
    public int WinsCount { get; set; }
}