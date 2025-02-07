namespace Chirp.Core;

/// <summary>
/// This class represents a like on a specific cheep by an author.
/// A cheep can have many likes. An author can only like a specific cheep once.
/// </summary>
public class Like
{
    public string LikeId { get; set; } = Guid.NewGuid().ToString();
    public string? AuthorId { get; set; }
    public string? CheepId { get; set; }
}