using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentID.Models
{
	public class NmNotify
	{
		[Key]
		public int Id { get; set; }
		public string StudentId { get; set; }
		public string RequestId { get; set; }
		public string Status { get; set; }
	}
}
