using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentID.Models
{
	public class LectureDataModel
	{
		public string Id { get; set; }

		public List<string> Students { get; set; }
	}

	public class LectureData
	{
		public string LecturerId { get; set; }

		public LectureDataModel Data { get; set; }
	}
}