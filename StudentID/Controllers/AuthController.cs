using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using StudentID.Models.Requests;
using StudentID.Data;
using System.Text;

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
			using (SHA256 sha256 = SHA256.Create())
			{
				byte[] inputBytes = Encoding.UTF8.GetBytes(credentials.Password);

				byte[] hashBytes = sha256.ComputeHash(inputBytes);
				string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
				pword = hash;
			}
			if (credentials.UserRole == "admin")
			{

				var admin = _db.Admins.SingleOrDefault(a => a.Email == email && a.Password == pword);
				if (admin != null)
				{
					HttpContext.Session.SetString("LastName", admin.LastName);
					HttpContext.Session.SetString("OtherNames", admin.OtherNames);
					HttpContext.Session.SetInt32("IsAuth", 1);
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
					HttpContext.Session.SetInt32("IsAuth", 1);
					HttpContext.Session.SetString("Id", lecturer.Id.ToString());

					return RedirectToAction("Index", "Lecturer");

				}
			}
			else if (credentials.UserRole == "student")
			{
				var query = (from s in _db.Students
							 join c in _db.IDCards on s.Id equals c.StudentId
							 where s.Password == pword && c.StudentNo == credentials.StudentNumber
							 select new { Student = s, isActive = c.IsActive })
						.SingleOrDefault();

				if (query != null)
				{
					if(!query.isActive)
					{
						ViewData["cardStatus"] = "Your ID Card Has Been Deactivated";
						return View();
					}
					HttpContext.Session.SetString("LastName", query.Student.LastName);
					HttpContext.Session.SetString("OtherNames", query.Student.OtherNames);
					HttpContext.Session.SetInt32("IsAuth", 1);
					HttpContext.Session.SetString("Id", query.Student.Id.ToString());

					return RedirectToAction("Index", "Student");
				}

			}

			HttpContext.Session.SetInt32("IsAuth", 0);
			ViewData["Authentication"] = "[FAILED] User Authentication Failed";
			return View();
		}
	
		public IActionResult LogOut()
		{
			HttpContext.Session.SetString("LastName", string.Empty);
			HttpContext.Session.SetString("OtherNames", string.Empty);
			HttpContext.Session.SetInt32("IsAuth", 0);
			HttpContext.Session.SetString("Id", string.Empty);

			return RedirectToAction("SignIn");
		}
	}
}
