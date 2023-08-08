using StudentID.Data;
using StudentID.Models;
using StudentID.Models.Requests;

namespace StudentID.Services
{
	public class AdminServices
	{
		private readonly ApplicationDbContext _db;

		public AdminServices(ApplicationDbContext db)
		{
			_db = db;
		}

		public List<NM> NameModifications()
		{
			var query = from s in _db.Students
						join c in _db.IDCards on s.Id equals c.StudentId
						join n in _db.NameModificationDocuments on s.Id equals n.StudentId into stud_mod
						from st in stud_mod.DefaultIfEmpty()
						where st != null
						select new NM(){ Student = s, IDCard = c, MStud = st };
						//select new { Student = s, IDCard = c, MStud = st };

			return query.ToList();
		}

		public List<IU> ImageUpdates()
		{
			var query = from s in _db.Students
						 join i in _db.ImageUpdates on s.Id equals i.StudentId into stud_mod
						 from st in stud_mod.DefaultIfEmpty()
						 where st != null
						 //select new
						 //{
							// currentImg = s.ImageUrl,
							// newImg = st.FileName,
							// requestId = st.Id,
							// studentId = st.StudentId
						 //};
			select new IU()
			{
				currentImg = s.ImageUrl,
				newImg = st.FileName,
				requestId = st.Id,
				studentId = st.StudentId
			};

			return query.ToList();
		}

		public List<ST> Students()
		{
			var query = from s in _db.Students
						 join i in _db.IDCards on s.Id equals i.StudentId
						 //select new
						 //{
							// id = s.Id,
							// name = s.LastName.ToUpper() + " " + s.OtherNames,
							// indexNo = i.IndexNo,
							// studentNo = i.StudentNo
						 //};
			select new ST()
			{
				id = s.Id,
				name = s.LastName.ToUpper() + " " + s.OtherNames,
				indexNo = i.IndexNo,
				studentNo = i.StudentNo
			};

			return query.ToList();
		}
		
		public GC GetCounts()
		{
			var obj = new GC()
			{
				issuedCardCount = Students().Count(),
				nameModifyCount = NameModifications().Count(),
				imageUpdateCount = ImageUpdates().Count()
			};

			return obj;
		}
	}

	public class NM
	{
		public Models.Student Student { get; set; }
		public Models.IDCard IDCard { get; set; }
		public Models.NameModificationDocument MStud { get; set; }
	}

	public class IU
	{
		public string currentImg { get; set; }
		public string newImg { get; set; }
		public Guid requestId { get; set; }
		public Guid studentId { get; set; }
	}

	public class ST
	{
		public Guid id { get; set; }
		public string name { get; set; }
		public string studentNo { get; set; }
		public string indexNo { get; set; }
	}
	public class GC
	{
		public int issuedCardCount { get; set; }
		public int nameModifyCount { get; set; }
		public int imageUpdateCount { get; set; }
	}
}
