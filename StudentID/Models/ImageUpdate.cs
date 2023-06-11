using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentID.Models
{
	public class ImageUpdate
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public string FileName { get; set; }

		[ForeignKey("Student")]
		public Guid StudentId { get; set; }
	}
}