namespace Chirp.Core;

/// <summary>
/// The CheepDTO class is used to transfer data from the CheepRepository class to the UI.
/// </summary>
public class CheepDTO()
{
    public required string Id { get; set; }
    public required string Author { get; set; }
    public required string Text { get; set; }
    public required string TimeStamp { get; set; }
    public required string LikeCount { get; set; }
    
}