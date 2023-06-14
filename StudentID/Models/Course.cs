using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentID.Models
{
	public class Course
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public string CourseCode { get; set; }

		[Required]
		public string CourseName { get; set; }

		[ForeignKey("ProgramOffered")]
		[Required]
		public string ProgramId { get; set; }
	}
}