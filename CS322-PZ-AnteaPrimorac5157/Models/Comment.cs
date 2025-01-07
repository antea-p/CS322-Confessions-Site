using System.ComponentModel.DataAnnotations;

namespace CS322_PZ_AnteaPrimorac5157.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string AuthorNickname { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public int Likes { get; set; } = 0;

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public int ConfessionId { get; set; }

        public virtual Confession Confession { get; set; } = null!;
    }
}
