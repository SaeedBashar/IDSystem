using Microsoft.AspNetCore.Mvc;
using StudentID.Data;
using StudentID.Models;
using StudentID.Models.Requests;

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
		public async Task<IActionResult> JoinLecture()
		{
			return View();
		}
	}
}
