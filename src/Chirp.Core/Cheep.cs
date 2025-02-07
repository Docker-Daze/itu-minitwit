using System.ComponentModel.DataAnnotations;

namespace Chirp.Core;

/// <summary>
/// This class represents a cheep of the Chirp application.
/// A cheep is an author written text that is published to the Chirp application.
/// Authors can see other authors cheeps and have the option to like the cheep and follow the author. 
/// </summary>
public class Cheep
{
    public string CheepId { get; set; } = Guid.NewGuid().ToString();
    [Required]
    [StringLength(160, ErrorMessage = "Cheeps can't be longer than 160 characters.")]
    public string? Text { get; set; }
    public DateTime TimeStamp { get; set; }
    [Required]
    public Author? Author { get; set; }
    public ICollection<Like>? Likes { get; set; }
}