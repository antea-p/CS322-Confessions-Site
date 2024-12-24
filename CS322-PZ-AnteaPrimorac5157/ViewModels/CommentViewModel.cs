namespace CS322_PZ_AnteaPrimorac5157.ViewModels
{
    public class CommentViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string AuthorNickname { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
    }
}
