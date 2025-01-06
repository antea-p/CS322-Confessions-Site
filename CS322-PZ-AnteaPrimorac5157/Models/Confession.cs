using System.ComponentModel.DataAnnotations;

namespace CS322_PZ_AnteaPrimorac5157.Models
{
	public class Confession
	{
		public int Id { get; set; }

		[Required]
		public string Title { get; set; } = string.Empty;

		[Required]
		public string Content { get; set; } = string.Empty;

		public DateTime DateCreated { get; set; } = DateTime.UtcNow;

		public int Likes { get; set; } = 0;

		[Timestamp]
		public byte[] RowVersion { get; set; } = Array.Empty<byte>();

		public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
	}
}