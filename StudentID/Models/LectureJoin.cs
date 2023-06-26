using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentID.Models
{
	public class LectureJoin
	{
		public Guid LectureId { get; set; }

		public string RequestHash { get; set; }

		[Key]
		public string StudentNo { get; set; }

		public string IndexNo { get; set; }
	}
}