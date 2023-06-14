using Microsoft.AspNetCore.Mvc;
using StudentID.Data;

namespace StudentID.Controllers
{
	public class LecturerController : Controller
	{
		private readonly ApplicationDbContext _db;

		public LecturerController(ApplicationDbContext db)
		{
			this._db = db;
		}

		public IActionResult Index()
		{
			var query = _db.Lecturers.ToList();
			var query1 = _db.Students.ToList();
			var viewState = new
			{
				lecturer = query[0],
				students = query1
			};
			return View(viewState);
		}
	}
}
