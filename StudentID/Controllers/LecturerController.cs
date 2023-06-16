using Microsoft.AspNetCore.Mvc;
using StudentID.Data;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using StudentID.Models;
using StudentID.Models.Requests;

namespace StudentID.Controllers
{
	public class LecturerController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly IWebHostEnvironment _hostEnv;

		public LecturerController(ApplicationDbContext db, IWebHostEnvironment hostEnv)
		{
			this._db = db;
			this._hostEnv = hostEnv;
		}

		public IActionResult Index()
		{
			var query = _db.Lecturers.ToList();
			var query1 = _db.Students.ToList();
			var query2 = _db.Lectures.Where(l => l.LecturerId == "baeabf3a-6429-4f32-a624-4bc6b616e7cc").ToList();
			
			string wwwRootPath = _hostEnv.WebRootPath;
			string path = Path.Combine(wwwRootPath + "/data/lectures.json");

			var lectures = System.IO.File.ReadAllText(path);
			var data = JsonConvert.DeserializeObject<LectureData>(lectures);

			var viewState = new
			{
				lecturer = query[0],
				students = query1,
				lectures = query2,
				attendance = data
			};
			return View(viewState);
		}

		[HttpPost]
		public IActionResult InitLecture([FromBody] InitiateLecture req)
		{
			try
			{
				Lectures lec = new Lectures()
				{
					Id = Guid.NewGuid(),
					LectureDate = req.LectureDate,
					WeekNo = req.WeekNo,
					StartTime = req.StartTime,
					EndTime = req.EndTime,
					LecturerId = "baeabf3a-6429-4f32-a624-4bc6b616e7cc"
				};
				_db.Lectures.Add(lec);
				_db.SaveChanges();
				return Json(new { status = true, msg = "Lecture Added Successfully!!" });
			}
			catch(Exception e)
			{
				return Json(new { status = false, msg = "An Error Occured" });

			}

		}
	}
}
