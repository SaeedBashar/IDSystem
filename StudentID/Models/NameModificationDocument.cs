using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentID.Models
{
	public class NameModificationDocument
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public string FileName { get; set; }

		[Required]
		public string LastName { get; set; }

		[Required]
		public string OtherNames { get; set; }

		[ForeignKey("Student")]
		public Guid StudentId { get; set; }
	}
}