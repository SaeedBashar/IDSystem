namespace StudentID.Models.Requests
{
	public class SignInRequest
	{
		public string UserRole { get; set; }
		public string StudentNumber { get; set; }
		public string LecturerNumber { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
	}
}
