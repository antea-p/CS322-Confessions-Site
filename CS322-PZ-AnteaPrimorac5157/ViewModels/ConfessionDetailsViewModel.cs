namespace CS322_PZ_AnteaPrimorac5157.ViewModels
{
    public class ConfessionDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public int Likes { get; set; }
        public bool UserHasLiked { get; set; }
        public ICollection<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();

        public CreateCommentViewModel NewComment { get; set; } = new();
    }
}
