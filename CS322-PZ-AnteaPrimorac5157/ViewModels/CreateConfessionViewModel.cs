using System.ComponentModel.DataAnnotations;

namespace CS322_PZ_AnteaPrimorac5157.ViewModels
{
    public class CreateConfessionViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(50, ErrorMessage = "Title cannot be longer than 50 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        [StringLength(2000, ErrorMessage = "Content cannot be longer than 2000 characters")]
        public string Content { get; set; } = string.Empty;
    }
}
