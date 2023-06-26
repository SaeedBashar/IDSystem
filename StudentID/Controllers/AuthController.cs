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
		public IActionResult SignIn()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult SignIn(SignInRequest credentials)
		{
			var email = credentials.Email;
			var pword = credentials.Password;
			if (credentials.UserRole == "admin")
			{

				var admin = _db.Admins.SingleOrDefault(a => a.Email == email && a.Password == pword);
				if (admin != null)
				{
					HttpContext.Session.SetString("LastName", admin.LastName);
					HttpContext.Session.SetString("OtherNames", admin.OtherNames);
					HttpContext.Session.SetString("IsAuth", "true");
					HttpContext.Session.SetString("Id", admin.Id.ToString());

					return RedirectToAction("Index", "Admin");
				}
			}
			else if (credentials.UserRole == "lecturer")
			{
				var lecturer = _db.Lecturers.SingleOrDefault(a =>
						a.Email == email &&
						a.Password == pword &&
						a.LecturerNo == credentials.LecturerNumber);
				if (lecturer != null)
				{
					HttpContext.Session.SetString("LastName", lecturer.LastName);
					HttpContext.Session.SetString("OtherNames", lecturer.OtherNames);
					HttpContext.Session.SetString("IsAuth", "true");
					HttpContext.Session.SetString("Id", lecturer.Id.ToString());

					return RedirectToAction("Index", "Lecturer");

				}
			}
			else if (credentials.UserRole == "student")
			{
				var query = (from s in _db.Students
							 join c in _db.IDCards on s.Id equals c.StudentId
							 where s.Password == credentials.Password && c.StudentNo == credentials.StudentNumber
							 select new { Student = s })
						.SingleOrDefault();

				if (query != null)
				{
					HttpContext.Session.SetString("LastName", query.Student.LastName);
					HttpContext.Session.SetString("OtherNames", query.Student.OtherNames);
					HttpContext.Session.SetString("IsAuth", "true");
					HttpContext.Session.SetString("Id", query.Student.Id.ToString());

					return RedirectToAction("Index", "Student");
				}

			}

			HttpContext.Session.SetString("isAuth", "false");
			return NotFound();

		}
	}
}
