using CS322_PZ_AnteaPrimorac5157.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CS322_PZ_AnteaPrimorac5157.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<Confession> Confessions { get; set; }
		public DbSet<Comment> Comments { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Comment>()
				.HasOne(c => c.Confession)
				.WithMany(c => c.Comments)
				.HasForeignKey(c => c.ConfessionId);

			modelBuilder.Entity<Confession>()
				.Property(c => c.RowVersion)
				.IsRowVersion();
		}
	}
}
