using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using StudentID.Data;
using StudentID.Models;
using StudentID.Models.Requests;
using Newtonsoft.Json;
using StudentID.Services;

namespace StudentID.Controllers
{
	public class StudentController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly IWebHostEnvironment _hostEnv;
		private StudentServices _service;

		public StudentController(ApplicationDbContext db, IWebHostEnvironment hostEnv)
		{
			_db = db;
			_hostEnv = hostEnv;
			_service = new StudentServices(_db);
		}

		public IActionResult Index()
		{

			if (HttpContext.Session.GetInt32("IsAuth") == 0)
			{
				return RedirectToAction("SignIn", "Auth");
			}
			string sid = HttpContext.Session.GetString("Id");
			if(sid != null)
			{
				ViewData["LastName"] = HttpContext.Session.GetString("LastName");
				ViewData["OtherNames"] = HttpContext.Session.GetString("OtherNames");

				var viewState = new
				{
					info = _service.getStudentInfo(sid),
					courses = _service.getCourses(sid)
				};

				return View(viewState);
			}

			return RedirectToAction("SignIn", "Auth");
		}

		[HttpPost]
		//[ValidateAntiForgeryToken]
		public async Task<IActionResult> ModifyName(NameModificationRequest req)
		{
			try
			{
				if (HttpContext.Session.GetInt32("IsAuth") == 0)
				{
					return Json(new { status = false, msg = "Authentication Failed!!" });
				}

				var obj = _db.NameModificationDocuments.SingleOrDefault(n=>n.StudentId == Guid.Parse(HttpContext.Session.GetString("Id")));
				if ( obj != null)
				{
					return Json(new {status = false, msg = "[Failed] There Is A Pending Name Modification Request"});
				}

				if(
					string.IsNullOrEmpty(req.LastName) ||
					string.IsNullOrEmpty(req.OtherNames) ||
					req.Image == null
				)
				{
					return Json(new {status = false, msg = "[Failed] All Fields Are Required"});
				}

				if (ModelState.IsValid)
				{
					string wwwRootPath = _hostEnv.WebRootPath;
					string fileName = Path.GetFileNameWithoutExtension(req.Image.FileName);
					string ext = Path.GetExtension(req.Image.FileName);

					Guid imgId = Guid.NewGuid();
					string fname = String.Format("{0}-{1}{2}", imgId.ToString(), fileName, ext);

					var nModify = new NameModificationDocument()
						{
							Id = imgId,
							FileName = fname,
							LastName = req.LastName,
							OtherNames = req.OtherNames,
							StudentId = Guid.Parse(HttpContext.Session.GetString("Id"))
						};
						await _db.NameModificationDocuments.AddAsync(nModify);

					string path = Path.Combine(wwwRootPath + "/image/ProofOfNames", fname);

					using (var fileStream = new FileStream(path, FileMode.Create))
					{
						await req.Image.CopyToAsync(fileStream);
					}

					_db.NmNotifies.Add(new NmNotify()
					{
						StudentId = HttpContext.Session.GetString("Id"),
						Status = "Pending",
						RequestId = imgId.ToString()
					});
					await _db.SaveChangesAsync();
				}

				return Json(new { status = true, msg = "Name Modification Request Sent Successfully!!" });
			}
			catch (Exception)
			{
				return Json(new { status = true, msg = "[ERROR] An Error Occured While Making Request!!" });
			}
		}

		[HttpPost]
		//[ValidateAntiForgeryToken]
		public async Task<IActionResult> ImageUpdate(ImageUpdateRequest req)
		{
			if (HttpContext.Session.GetInt32("IsAuth") == 0)
			{
				return Json(new { status = false, msg = "Authentication Failed!!" });
			}

			var obj = _db.ImageUpdates.SingleOrDefault(n=>n.StudentId == Guid.Parse(HttpContext.Session.GetString("Id")));
			if ( obj != null)
			{
				return Json(new {status = false, msg = "[Failed] There Is A Pending Image Update Request"});
			}

			if(
				req.Image == null
			)
			{
				return Json(new {status = false, msg = "[Failed] All Fields Are Required"});
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
					_db.IuNotifies.Add(new IuNotify()
					{
						StudentId = HttpContext.Session.GetString("Id"),
						Status = "Pending",
						RequestId = imgId.ToString()
					});
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
						// -======
						string wwwRootPath = _hostEnv.WebRootPath;
						string path = Path.Combine(wwwRootPath + "/data/lectures.json");

						var lectures = System.IO.File.ReadAllText(path);
						var data = JsonConvert.DeserializeObject<LectureFile>(lectures);

						var index = -1;
						for (int i = 0; i < data.Lectures.Count; i++)
						{
							if (data.Lectures[i].LecturerId == lec.Id.ToString())
							{
								index = i;
								break;
							}
						}
						int lecIndex = -1;
						for (int i = 0; i < data.Lectures[index].Data.Count; i++)
						{
							if (data.Lectures[index].Data[i].Id == latestObject.Id.ToString())
							{
								lecIndex = i;
								break;
							}
						}

						bool isJoined = false;
						for(int i=0; i<data.Lectures[index].Data[lecIndex].Students.Count(); i++){
							if(data.Lectures[index].Data[lecIndex].Students[i] == req.IndexNo){
								isJoined = true;
							}
						}

						// =======
						if(!isJoined){
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
								return Json(new { status = true, requestHash = obj.RequestHash });
							}
						
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
		}

		public IActionResult StudentInfoApi()
		{
			string sid = HttpContext.Session.GetString("Id");
			var obj = _service.getStudentInfo(sid);

			return Json(obj);
		}

		public IActionResult CoursesApi()
		{
			string sid = HttpContext.Session.GetString("Id");
			var obj = _service.getCourses(sid);

			return Json(obj);
		}

		public async Task<IActionResult> NotificationsApi()
		{
			var sid = HttpContext.Session.GetString("Id");

			var response = _service.getNotificationsNm(sid);

			var res = _db.IuNotifies.Where(i => i.StudentId == sid);

			var result = _db.NmNotifyConfirms.Where(n => n.StudentId == sid).ToList();
			var result1 = _db.IuNotifyConfirms.Where(n => n.StudentId == sid).ToList();

			foreach (var i in result) _db.NmNotifyConfirms.Remove(i);
			
			foreach (var i in result1) _db.IuNotifyConfirms.Remove(i);

			return Json(new { nm = response, iu = res, cnm=result, ciu=result1 });
		}

		public async Task<IActionResult> NotificationConfirmsApi()
		{
			var sid = HttpContext.Session.GetString("Id");
			
			var result = _db.NmNotifyConfirms.Where(n => n.StudentId == sid).ToList();
			var result1 = _db.IuNotifyConfirms.Where(n => n.StudentId == sid).ToList();

			return Json(new { cnm = result, ciu = result1 });
		}


	}
}
