using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace itu_minitwit.Pages;

public class AddMessage : PageModel
{
    [Required (ErrorMessage = "Message cannot be empty")]
    [StringLength(160, ErrorMessage = "Maximum length is 160 characters")]
    [Display(Name = "Message Text")]
    public string Message { get; set; }
    
    public void OnGet()
    {
        
    }
}