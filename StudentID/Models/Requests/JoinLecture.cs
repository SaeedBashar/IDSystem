namespace StudentID.Models.Requests
{
	public class JoinLectureRequest
	{
        public Guid CourseId { get; set; }
        public string StudentNo { get; set; }
        public string IndexNo { get; set; }
    }
}
