using System.ComponentModel.DataAnnotations;

namespace CS322_PZ_AnteaPrimorac5157.Models
{
	public class Confession
	{
		public int Id { get; set; }

		[Required]
		public string Title { get; set; }

		[Required]
		public string Content { get; set; }

		public DateTime DateCreated { get; set; }

		public int Likes { get; set; }

		public virtual ICollection<Comment> Comments { get; set; }
	}
}
