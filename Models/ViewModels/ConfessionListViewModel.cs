namespace CS322_PZ_AnteaPrimorac5157.Models.ViewModels
{
	public class ConfessionListViewModel
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public DateTime DateCreated { get; set; }
		public int Likes { get; set; }
		public int CommentCount { get; set; }
	}
}
