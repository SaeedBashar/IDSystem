﻿using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using StudentID.Models.Requests;
using StudentID.Data;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

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
		public async Task<IActionResult> SignIn(SignInRequest credentials)
		{
			
			var email = credentials.Email;
			var pword = credentials.Password;

			string page = string.Empty;

			List<Claim> claims = new List<Claim>
			{
				new Claim(ClaimTypes.Email, email)
			};

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
					claims.Add(new Claim("LastName", admin.LastName));
					claims.Add(new Claim("OtherNames", admin.OtherNames));
					claims.Add(new Claim("UserId", admin.Id.ToString()));
					claims.Add(new Claim("UserRole", "admin"));
					page = "Admin";
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

					claims.Add(new Claim("LastName", lecturer.LastName));
					claims.Add(new Claim("OtherNames", lecturer.OtherNames));
					claims.Add(new Claim("UserId", lecturer.Id.ToString()));
					claims.Add(new Claim("UserRole", "lecturer"));

					page = "Lecturer";
					//return RedirectToAction("Index", "Lecturer");

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
					claims.Add(new Claim("LastName", query.Student.LastName));
					claims.Add(new Claim("OtherNames", query.Student.OtherNames));
					claims.Add(new Claim("UserId", query.Student.Id.ToString()));
					claims.Add(new Claim("UserRole", "student"));

					page = "Student";
					//return RedirectToAction("Index", "Student");
				}

			}

			if (!string.IsNullOrEmpty(page))
			{
				var identity = new ClaimsIdentity(claims, "MyCookieAuth");
				ClaimsPrincipal principal = new ClaimsPrincipal(identity);

				await HttpContext.SignInAsync("MyCookieAuth", principal);

				return RedirectToAction("Index", page);
			}

			ViewData["Authentication"] = "[FAILED] User Authentication Failed";
			return View();
		}
	
		public async Task<IActionResult> LogOut()
		{
			
			await HttpContext.SignOutAsync("MyCookieAuth");
			return RedirectToAction("SignIn");
		}
	}
}
