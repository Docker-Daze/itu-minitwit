namespace Chirp.Core;

/// <summary>
/// The AuthorDTO class is used to transfer data from the AuthorRepository class to the UI.
/// </summary>
public class AuthorDTO
{
    public required string Name { get; set; }
    
    public required string Email { get; set; }
}