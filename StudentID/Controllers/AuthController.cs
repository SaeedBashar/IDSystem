using Microsoft.AspNetCore.Mvc;
using StudentID.Models.Requests;
using StudentID.Data;

namespace StudentID.Controllers
{
	public class AuthController : Controller
	{
		private readonly ApplicationDbContext _db;
		public AuthController(ApplicationDbContext db)
		{
			this._db = db;
		}

		//[Route("/sign-in")]
		public IActionResult SignIn()
		{
			return View();
		}

		[HttpPost]
		//[Route("/sign-in")]
		[ValidateAntiForgeryToken]
		public IActionResult SignIn(SignInRequest credentials)
		{
			var query = (from s in _db.Students
						 join c in _db.IDCards on s.Id equals c.StudentId
						 where s.Password == credentials.Password && c.StudentNo == credentials.StudentNumber
						 select new { Student = s})
						.SingleOrDefault();

			if (query == null)
			{
				HttpContext.Session.SetString("isAuth", "false");
				return NotFound();
			}
			HttpContext.Session.SetString("LastName", query.Student.LastName);
			HttpContext.Session.SetString("OtherNames", query.Student.OtherNames);
			HttpContext.Session.SetString("IsAuth", "true");
			HttpContext.Session.SetString("Id", query.Student.Id.ToString());

			return RedirectToAction("Index", "Student");
		}
	}
}
