using System.ComponentModel.DataAnnotations;

namespace CS322_PZ_AnteaPrimorac5157.ViewModels
{
    public class EditCommentViewModel
    {
        public int Id { get; set; }
        public int ConfessionId { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [StringLength(512, ErrorMessage = "Content cannot be longer than 512 characters")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nickname is required")]
        [StringLength(48, ErrorMessage = "Nickname cannot be longer than 48 characters")]
        public string AuthorNickname { get; set; } = string.Empty;
    }
}
