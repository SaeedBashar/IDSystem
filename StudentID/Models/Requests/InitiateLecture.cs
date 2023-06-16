namespace StudentID.Models.Requests
{
	public class InitiateLecture
	{
        public string WeekNo { get; set; }
        public string LectureDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
