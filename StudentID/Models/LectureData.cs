using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentID.Models
{
	public class LectureDataModel
	{
		public string Id { get; set; }

		public string WeekNo { get; set; }

		public IList<string> Students { get; set; }
	}

	public class LectureData
	{
		public string LecturerId { get; set; }

		public IList<LectureDataModel> Data { get; set; }
	}
}