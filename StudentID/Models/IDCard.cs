using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentID.Models
{
	public class IDCard
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public string StudentNo { get; set; }

		[Required]
		public string IndexNo { get; set; }

		[ForeignKey("Student")]
		public Guid StudentId { get; set; }
	}
}