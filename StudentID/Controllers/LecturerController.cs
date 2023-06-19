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
			if (HttpContext.Session.GetString("IsAuth") == "false")
			{
				return RedirectToAction("SignIn", "Auth");
			}

			var query = _db.Lecturers.ToList();
			var query1 = _db.Students.ToList();
			var query2 = _db.Lectures.Where(l => l.LecturerId == HttpContext.Session.GetString("Id")).ToList();
			
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
			if (HttpContext.Session.GetString("IsAuth") == "false")
			{
				return Json(new { status = false, msg = "Authenticated Failed!!" });
			}

			try
			{
				var lecId = Guid.NewGuid();
				Lectures lec = new Lectures()
				{
					Id = lecId,
					LectureDate = req.LectureDate,
					WeekNo = req.WeekNo,
					StartTime = req.StartTime,
					EndTime = req.EndTime,
					LecturerId = HttpContext.Session.GetString("IsAuth")
				};
				_db.Lectures.Add(lec);
				_db.SaveChanges();

				string wwwRootPath = _hostEnv.WebRootPath;
				string path = Path.Combine(wwwRootPath + "/data/lectures.json");

				var lectures = System.IO.File.ReadAllText(path);
				var data = JsonConvert.DeserializeObject<LectureFile>(lectures);
				
				var index = -1;
				for(int i=0; i<data.Lectures.Count; i++)
				{
					if(data.Lectures[i].LecturerId == HttpContext.Session.GetString("IsAuth"))
					{
						index = i;
						break;
					}
				}

				LectureData lect = null;
				IList<LectureDataModel> lectureDataModels = new List<LectureDataModel> { };
				if(index == -1)
				{
					
					lectureDataModels.Add(new LectureDataModel() { Id = lecId.ToString(), Students = new List<string> { } });
					lect = new LectureData()
					{
						LecturerId = "baeabf3a-6429-4f32-a624-4bc6b616e7cc",
						Data = lectureDataModels
					};
					data.Lectures.Add(lect);
				}
				else
				{
					data.Lectures[index].Data.Add(new LectureDataModel()
					{
						Id = lecId.ToString(),
						Students = new List<string> { }
					});
				}
				System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(data));
				return Json(new { status = true, msg = "Lecture Added Successfully!!" });
				//return Json(data);
			}
			catch(Exception e)
			{
				return Json(new { status = false, msg = "An Error Occured" });

			}

		}
	}
}
