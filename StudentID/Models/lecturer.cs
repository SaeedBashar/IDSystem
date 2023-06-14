using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentID.Models
{
	public class Lecturer
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public string LecturerNo { get; set; }

		[Required]
		public string LastName { get; set; }

		[Required]
		public string OtherNames { get; set; }

		[Required]
		public string Email { get; set; }

		[Required]
		public string Phone { get; set; }

		[Required]
		public string Password { get; set; }

		[ForeignKey("AccessLevel")]
		[Required]
		public string AccessLevelId { get; set; }

		[ForeignKey("Course")]
		[Required]
		public Guid CourseId { get; set; }
	}
}