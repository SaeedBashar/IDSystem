using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentID.Models
{
	public class ProgramOffered
	{
		[Key]
		public string ProgramId { get; set; }

		[Required]
		public string ProgramName { get; set; }
	}
}