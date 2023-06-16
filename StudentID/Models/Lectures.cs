using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentID.Models
{
	public class Lectures
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public string LectureDate { get; set; }

		[Required]
		public string StartTime { get; set; }

		[Required]
		public string EndTime { get; set; }

		[Required]
		public string WeekNo { get; set; }


		[ForeignKey("Lecturer")]
		[Required]
		public string LecturerId { get; set; }
	}
}