namespace StudentID.Models.Requests
{
	public class CardStatusRequest
	{
        public Guid StudentId { get; set; }

		public bool cardStatus { get; set; }
	}
}
