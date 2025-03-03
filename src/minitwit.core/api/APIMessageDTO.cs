namespace minitwit.core;

public class APIMessageDTO
{
    public required string user  { get; set; }
    public required string content { get; set; }
    public required string pub_date { get; set; }
}