using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using StudentID.Data;
using StudentID.Models;
using StudentID.Models.Requests;
using Newtonsoft.Json;

namespace StudentID.Controllers
{
	public class StudentController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly IWebHostEnvironment _hostEnv;

		public StudentController(ApplicationDbContext db, IWebHostEnvironment hostEnv)
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
			string sid = HttpContext.Session.GetString("Id");

			var query = (from s in _db.Students
						 join c in _db.IDCards on s.Id equals c.StudentId
						 join p in _db.Programs on s.ProgramId equals p.ProgramId
						 where sid == s.Id.ToString()
						 select new { Student = s, IDCard = c, Program = p })
						.SingleOrDefault();

			var query1 = from s in _db.Students
						 join c in _db.Courses on s.ProgramId equals c.ProgramId
						 join l in _db.Lecturers on c.Id equals l.CourseId
						 where s.Id.ToString() == sid
						 select c;

			var viewState = new
			{
				info = query,
				courses = query1.ToList()
			};

			return View(viewState);
		}

		[HttpPost]
		//[ValidateAntiForgeryToken]
		public async Task<IActionResult> ModifyName(NameModificationRequest req)
		{
			if (HttpContext.Session.GetString("IsAuth") == "false")
			{
				return Json(new { status = false, msg = "Authentication Failed!!" });
			}

			if (ModelState.IsValid)
			{
				string wwwRootPath = _hostEnv.WebRootPath;
				string fileName = Path.GetFileNameWithoutExtension(req.Image.FileName);
				string ext = Path.GetExtension(req.Image.FileName);

				Guid imgId = Guid.NewGuid();
				string fname = String.Format("{0}-{1}{2}", imgId.ToString(), fileName, ext);

				string path = Path.Combine(wwwRootPath + "/image/ProofOfNames", fname);

				using (var fileStream = new FileStream(path, FileMode.Create))
				{
					await req.Image.CopyToAsync(fileStream);

					var nModify = new NameModificationDocument()
					{
						Id = imgId,
						FileName = fname,
						LastName = req.LastName,
						OtherNames = req.OtherNames,
						StudentId = Guid.Parse(HttpContext.Session.GetString("Id"))
					};
					await _db.NameModificationDocuments.AddAsync(nModify);
					await _db.SaveChangesAsync();
				}
			}

			return Json(new { status = true, msg = "Name Modification Request Sent Successfully!!" });
		}

		[HttpPost]
		//[ValidateAntiForgeryToken]
		public async Task<IActionResult> ImageUpdate(ImageUpdateRequest req)
		{
			if (HttpContext.Session.GetString("IsAuth") == "false")
			{
				return Json(new { status = false, msg = "Authentication Failed!!" });
			}

			if (ModelState.IsValid)
			{
				string wwwRootPath = _hostEnv.WebRootPath;
				string fileName = Path.GetFileNameWithoutExtension(req.Image.FileName);
				string ext = Path.GetExtension(req.Image.FileName);

				Guid imgId = Guid.NewGuid();
				string fname = String.Format("{0}-{1}{2}", imgId.ToString(), fileName, ext);

				string path = Path.Combine(wwwRootPath + "/image/updates/", fname);

				using (var fileStream = new FileStream(path, FileMode.Create))
				{
					await req.Image.CopyToAsync(fileStream);
					
					var imgUdpate = new ImageUpdate()
					{
						Id = imgId,
						FileName = fname,
						StudentId = Guid.Parse(HttpContext.Session.GetString("Id"))
					};
					await _db.ImageUpdates.AddAsync(imgUdpate);
					await _db.SaveChangesAsync();
				}
			}

			return Json(new { status = true, msg = "Image Update Request Sent Successfully!!" });
		}
		
		[HttpPost]
		public async Task<IActionResult> JoinLecture([FromBody] JoinLectureRequest req)
		{
			try
			{

				var lec = _db.Lecturers.SingleOrDefault(l => l.CourseId == req.CourseId);

				if (lec != null)
				{
					List<Lectures> lects = _db.Lectures.Where(l => l.LecturerId == lec.Id.ToString()).ToList();

					if (lects.Count != 0)
					{
						var convertedObjects = lects.Select(obj => new
						{
							LectureDate = DateTime.Parse(obj.LectureDate),
							StartTime = TimeSpan.Parse(obj.StartTime),
							EndTime = TimeSpan.Parse(obj.EndTime),
							WeekNo = obj.WeekNo,
							LecturerId = obj.LecturerId,
							Id = obj.Id
						});
						// Sort the converted objects based on the combined date and time value in descending order
						var sortedObjects = convertedObjects
							.OrderByDescending(obj => obj.LectureDate.Date)
							.ThenByDescending(obj=> obj.StartTime);

						// Get the object with the latest date and time (first item after sorting)
						var latestObject = sortedObjects.FirstOrDefault();

						if (DateTime.Now.TimeOfDay.CompareTo(latestObject.StartTime) < 0)
						{
							return Json(new { status = false, msg = "Lecture Has Not Started Yet!!" });
						}
						
						if (DateTime.Now.TimeOfDay.CompareTo(latestObject.EndTime) > 0)
						{
							return Json(new { status = false, msg = "Lecture Has Ended Already!!" });
						}

						var obj = _db.LectureJoins.SingleOrDefault(l =>
							l.StudentNo == req.StudentNo &&
							l.IndexNo == req.IndexNo
						);

						string reqHash;
						if(obj == null)
						{
							using (SHA256 sha256 = SHA256.Create())
							{
								byte[] inputBytes = Encoding.UTF8.GetBytes(
									req.IndexNo + req.StudentNo + 
									latestObject.Id + DateTime.Now.ToString());

								byte[] hashBytes = sha256.ComputeHash(inputBytes);
								string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
								reqHash = hash;
							}

							_db.LectureJoins.Add(new LectureJoin()
							{
								LectureId = latestObject.Id,
								StudentNo = req.StudentNo,
								IndexNo = req.IndexNo,
								RequestHash = reqHash
							});
							_db.SaveChanges();
							return Json(new { status = true, requestHash = reqHash });
						}
						else
						{
							return Json(new { status = false, msg = "User Has Already Joined Lecture" });
						}

						//if (DateTime.Now.TimeOfDay < latestObject.StartTime)
						//{
						//	return Json(new { status = false, msg = "Lectures Has Not Started Yet!!" });
						//}
						//if (DateTime.Now.TimeOfDay > latestObject.EndTime)
						//{
						//	return Json(new { status = false, msg = "Lectures Has Ended Already!!" });
						//}
						//if (latestObject.StartTime < DateTime.Now.TimeOfDay && latestObject.EndTime > DateTime.Now.TimeOfDay)
						//{
						//	string wwwRootPath = _hostEnv.WebRootPath;
						//	string path = Path.Combine(wwwRootPath + "/data/lectures.json");

						//	var lectures = System.IO.File.ReadAllText(path);
						//	var data = JsonConvert.DeserializeObject<LectureFile>(lectures);

						//	var index = -1;
						//	for (int i = 0; i < data.Lectures.Count; i++)
						//	{
						//		if (data.Lectures[i].LecturerId == lec.Id.ToString())
						//		{
						//			index = i;
						//			break;
						//		}
						//	}
						//	int lecIndex = -1;
						//	for (int i = 0; i < data.Lectures[index].Data.Count; i++)
						//	{
						//		if (data.Lectures[index].Data[i].Id == latestObject.Id.ToString())
						//		{
						//			lecIndex = i;
						//			break;
						//		}
						//	}
						//	if (lecIndex != -1)
						//	{
						//		//TODO:Re-Implement Logic
						//		data.Lectures[index].Data[lecIndex].Students.Add(req.IndexNo);
						//		System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(data));
						//		return Json(new
						//		{
						//			status = true,
						//			msg = "Lectures Joined Successfully!!",
						//			lectureId = latestObject.Id
						//		});
						//	}
						//	else
						//	{
						//		return Json(new { status = false, msg = "Lectures Not Found!!" });
						//	}
						//}

					}
					else
					{
						return Json(new { status = false, msg = "No Lecture Available For This Lecturer" });
					}
				}
				else
				{
					return Json(new { status = false, msg = "Error No Lecturer Available" });
				}
			}
			catch (Exception e)
			{
				return Json(new { status = false, msg = "Unexpected Error Occured!!" });
			}

			return Json(req);
		}
	}
}
