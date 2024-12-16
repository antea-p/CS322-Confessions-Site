namespace CS322_PZ_AnteaPrimorac5157.ViewModels
{
    public class ConfessionListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public int Likes { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
    }
}
