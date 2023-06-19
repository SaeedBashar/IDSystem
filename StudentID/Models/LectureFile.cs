using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentID.Models
{
	public class LectureFile
	{
		public IList<LectureData> Lectures { get; set; }
	}
}