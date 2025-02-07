#nullable disable //Remove null warnings
using System.ComponentModel.DataAnnotations;

namespace Chirp.Web.Pages;

public class CheepInputModel
{
    [Required (ErrorMessage = "Cheep cannot be empty")]
    [StringLength(160, ErrorMessage = "Maximum length is 160 characters")]
    [Display(Name = "Message Text")]
    public string Message { get; set; }
}