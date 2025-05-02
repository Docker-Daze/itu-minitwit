using System.ComponentModel.DataAnnotations;

namespace minitwit.web.Pages;


public class MessageInputModel
{
    [Required(ErrorMessage = "Message cannot be empty")]
    [StringLength(160, ErrorMessage = "Maximum length is 160 characters")]
    [Display(Name = "Message Text")]
    public required string Text { get; set; }
}