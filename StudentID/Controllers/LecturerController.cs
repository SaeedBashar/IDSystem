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
			//if (HttpContext.Session.GetInt32("IsAuth") == 0)
			//{
			//	return RedirectToAction("SignIn", "Auth");
			//}
			//string lid = HttpContext.Session.GetString("Id");
			string lid = User.FindFirst("UserId")?.Value;
			if(lid != null)
			{

				var query = _db.Lecturers.SingleOrDefault(lec => lec.Id.ToString() == lid);
				var query1 = _db.Students.ToList();
				var query2 = _db.Lectures.Where(l => l.LecturerId == lid).ToList();
				var query3 = _db.Courses.Where(p=> p.Id == query.CourseId).ToList()[0];

				string wwwRootPath = _hostEnv.WebRootPath;
				string path = Path.Combine(wwwRootPath + "/data/lectures.json");

				var lectures = System.IO.File.ReadAllText(path);
				var data = JsonConvert.DeserializeObject<LectureFile>(lectures);

				LectureData obj = null;
				for(int i=0; i<data.Lectures.Count; i++)
				{
					if(data.Lectures[i].LecturerId == lid)
					{
						obj = data.Lectures[i];
						break;
					}
				}

				IList<LectureDataModel> trObj = obj != null ? obj.Data : new List<LectureDataModel>();
				if (obj != null)
				{
					for(var x=0; x<trObj.Count; x++)
					{
						for(var i=0; i<trObj[x].Students.Count; i++)
						{
							var que = (from s in _db.Students
									  join c in _db.IDCards on s.Id equals c.StudentId
									  where trObj[x].Students[i] == c.IndexNo
									  select new { FullName = $"{s.LastName} {s.OtherNames}" })
									  .SingleOrDefault();
							trObj[x].Students[i] = que.FullName;
						}
					}
				}
				

				//ViewData["LastName"] = HttpContext.Session.GetString("LastName");
				//ViewData["OtherNames"] = HttpContext.Session.GetString("OtherNames");

				var viewState = new
				{
					lecturer = query,
					students = query1,
					lectures = query2,
					attendance = trObj,
					course = query3
				};
				return View(viewState);
			}

			return RedirectToAction("SignIn", "Auth");
		}

		[HttpPost]
		public IActionResult InitLecture([FromBody] InitiateLecture req)
		{
			
			string lid = User.FindFirst("UserId")?.Value;
			try
			{
				DateTimeOffset date1 = DateTimeOffset.Parse(DateTime.Now.Date.ToString());
				DateTimeOffset date2 = DateTimeOffset.Parse(req.LectureDate);
				int result = date1.CompareTo(date2);
				if (result <= 0)
				{
					var lecId = Guid.NewGuid();
					Lectures lec = new Lectures()
					{
						Id = lecId,
						LectureDate = req.LectureDate,
						WeekNo = req.WeekNo,
						StartTime = req.StartTime,
						EndTime = req.EndTime,
						LecturerId = lid
					};
					_db.Lectures.Add(lec);
					_db.SaveChanges();

					string wwwRootPath = _hostEnv.WebRootPath;
					string path = Path.Combine(wwwRootPath + "/data/lectures.json");

					var lectures = System.IO.File.ReadAllText(path);
					var data = JsonConvert.DeserializeObject<LectureFile>(lectures);

					var index = -1;
					for (int i = 0; i < data.Lectures.Count; i++)
					{
						if (data.Lectures[i].LecturerId == lid)
						{
							index = i;
							break;
						}
					}

					LectureData lect;
					IList<LectureDataModel> lectureDataModels = new List<LectureDataModel> { };
					if (index == -1)
					{

						lectureDataModels.Add(new LectureDataModel() { Id = lecId.ToString(), WeekNo = req.WeekNo, Students = new List<string> { } });
						lect = new LectureData()
						{
							LecturerId = lid,
							Data = lectureDataModels
						};
						data.Lectures.Add(lect);
					}
					else
					{
						data.Lectures[index].Data.Add(new LectureDataModel()
						{
							Id = lecId.ToString(),
							WeekNo = req.WeekNo,
							Students = new List<string> { }
						});
					}
					System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(data));
					return Json(new { status = true, msg = "Lecture Added Successfully!!" });
				}
				else
				{
					return Json(new { status = false, msg = "Cannot Set Lecture Back In Time" });
				}
			}
			catch(Exception)
			{
				return Json(new { status = false, msg = "[ERROR] An Unexpected Error Occured!!" });
			}
		}

		[HttpGet]
		public IActionResult AddStudent([FromQuery] string reqHash)
		{
			string lid = User.FindFirst("UserId")?.Value;
			var obj = _db.LectureJoins.SingleOrDefault(l => l.RequestHash == reqHash);

			if(obj == null)
			{
				return Json(new { status = false, msg = "Student Request Can Not Be Found" });
			}

			string wwwRootPath = _hostEnv.WebRootPath;
			string path = Path.Combine(wwwRootPath + "/data/lectures.json");

			var lectures = System.IO.File.ReadAllText(path);
			var data = JsonConvert.DeserializeObject<LectureFile>(lectures);

			var index = -1;
			for (int i = 0; i < data.Lectures.Count; i++)
			{
				if (data.Lectures[i].LecturerId == lid)
				{
					index = i;
					break;
				}
			}
			int lecIndex = -1;
			for (int i = 0; i < data.Lectures[index].Data.Count; i++)
			{
				if (data.Lectures[index].Data[i].Id == obj.LectureId.ToString())
				{
					lecIndex = i;
					break;
				}
			}

			if (lecIndex != -1)
			{
				data.Lectures[index].Data[lecIndex].Students.Add(obj.IndexNo);
				System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(data));

				var pendingJoin = _db.PendingJoinRequests.FirstOrDefault(x=>x.RequestHash == reqHash);
				if(pendingJoin != null)
                {
					pendingJoin.IsApproved = true;
					_db.PendingJoinRequests.Update(pendingJoin);
                }
                _db.LectureJoins.Remove(obj);

				_db.SaveChanges();
				return Json(new
				{
					status = true,
					msg = "Lecture Joined Successfully!!"
				});
			}
			else
			{
				return Json(new { status = false, msg = "Lecture Not Found!!" });
			}
		}
	}
}
