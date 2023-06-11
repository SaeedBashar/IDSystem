namespace StudentID.Models.Requests
{
	public class NameModificationRequest
	{
		public string LastName { get; set; }
		public string OtherNames { get; set; }

        public IFormFile Image { get; set; }
    }
}
