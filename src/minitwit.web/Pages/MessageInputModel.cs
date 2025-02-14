using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using minitwit.core;

namespace itu_minitwit.Pages;


public class MessageInputModel : PageModel
{
    [Required (ErrorMessage = "Message cannot be empty")]
    [StringLength(160, ErrorMessage = "Maximum length is 160 characters")]
    [Display(Name = "Message Text")]
    public string Message { get; set; }
    IMessageRepository _messageRepository; 

    public MessageInputModel(IMessageRepository messageRepository){
        _messageRepository = messageRepository;
    }
}