using StudentID.Data;

namespace StudentID.Services
{
	public class StudentServices
	{
		private readonly ApplicationDbContext _db;

		public StudentServices(ApplicationDbContext db)
		{
			_db = db;
		}

		public SI getStudentInfo(string sid) {
			var query = (from s in _db.Students
						 join c in _db.IDCards on s.Id equals c.StudentId
						 join p in _db.Programs on s.ProgramId equals p.ProgramId
						 where sid == s.Id.ToString()
						 select new SI() { Student = s, IDCard = c, Program = p })
							.SingleOrDefault();
			return query;
		}

		public List<Models.Course> getCourses(string sid)
		{
			var query = from s in _db.Students
						join c in _db.Courses on s.ProgramId equals c.ProgramId
						join l in _db.Lecturers on c.Id equals l.CourseId
						where s.Id.ToString() == sid
						select c;
			return query.ToList();
		}

		public List<NotifyElementNM> getNotificationsNm(string sid)
		{
			var result = _db.NmNotifies.Where(n => n.StudentId == sid).ToList();
			List<NotifyElementNM> response = new List<NotifyElementNM>();
			foreach (var i in result)
			{
				var nm = _db.NameModificationDocuments.FirstOrDefault(x => x.Id.ToString() == i.RequestId);
				response.Add(new NotifyElementNM()
				{
					Id = i.Id,
					Status = i.Status,
					Name = $"{nm.LastName} {nm.OtherNames}"
				});
			}
			return response;
		}

	}

	public class SI
	{
		public Models.Student Student { get; set; }
		public Models.IDCard IDCard { get; set; }
		public Models.ProgramOffered Program { get; set; }
	}

	public class NotifyElementNM
	{
		public int Id {get; set;}
		public string Status { get; set; }
		public string Name { get; set; }
	}
}
