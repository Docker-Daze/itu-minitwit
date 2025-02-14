using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace minitwit.web.Pages;

public class MessageInputModel
{
    [Required (ErrorMessage = "Message cannot be empty")]
    [StringLength(160, ErrorMessage = "Maximum length is 160 characters")]
    [Display(Name = "Message Text")]
    public string Message { get; set; }
}