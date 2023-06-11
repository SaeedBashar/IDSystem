using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentID.Models
{
	public class AccessLevel
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public string AccessLevelId { get; set; }

		[Required]
		public string AccessName { get; set; }
	}
}