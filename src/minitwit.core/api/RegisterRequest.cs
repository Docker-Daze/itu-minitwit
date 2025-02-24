namespace minitwit.core;

public class RegisterRequest
{
    public required string username { get; set; }
    public required string email { get; set; }
    public required string pwd { get; set; }
}