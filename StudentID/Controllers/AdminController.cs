using Microsoft.AspNetCore.Mvc;
using StudentID.Data;
using StudentID.Models;

namespace StudentID.Controllers
{
	public class AdminController : Controller
	{
		private readonly ApplicationDbContext _db;

		public AdminController(ApplicationDbContext db)
		{
			this._db = db;
		}
		public IActionResult Index()
		{

			var query = from s in _db.Students
						join c in _db.IDCards on s.Id equals c.StudentId
						join n in _db.NameModificationDocuments on s.Id equals n.StudentId into stud_mod
						from st in stud_mod.DefaultIfEmpty()
						select new { Student = s, IDCard = c, MStud = st };

			return View(query.ToList());
		}

		public IActionResult getNameModificationDetail(Guid Id)
		{
			var query = (from s in _db.Students
						join c in _db.IDCards on s.Id equals c.StudentId
						join p in _db.Programs on s.ProgramId equals p.ProgramId
						join n in _db.NameModificationDocuments on s.Id equals n.StudentId
						where Id.ToString() == s.Id.ToString()
						select new { Student = s, IDCard = c, Program = p, NModify = n })
						.SingleOrDefault();

			return Json(query);
		}
	}
}
