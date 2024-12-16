using System.ComponentModel.DataAnnotations;

namespace CS322_PZ_AnteaPrimorac5157.Models
{
	public class Comment
	{
		public int Id { get; set; }

		[Required]
		public string Content { get; set; }

		[Required]
		public string AuthorNickname { get; set; }

		public DateTime DateCreated { get; set; }

		public int ConfessionId { get; set; }

		public virtual Confession Confession { get; set; }
	}
}
