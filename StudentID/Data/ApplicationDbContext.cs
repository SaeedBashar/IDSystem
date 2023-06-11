using Microsoft.EntityFrameworkCore;
using StudentID.Models;

namespace StudentID.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{

		}

		public DbSet<Student> Students { get; set; }

		public DbSet<IDCard> IDCards { get; set; }

		public DbSet<ProgramOffered> Programs { get; set; }

		public DbSet<Admin> Admins  { get; set; }

		public DbSet<AccessLevel> AccessLevels { get; set; }

        public DbSet<NameModificationDocument> NameModificationDocuments { get; set; }

		public DbSet<ImageUpdate> ImageUpdates { get; set; }
	}
}
