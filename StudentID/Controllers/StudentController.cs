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

			if(HttpContext.Session.GetString("IsAuth") == "false")
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


			return View(query);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ModifyName(NameModificationRequest req)
		{
			if (HttpContext.Session.GetString("IsAuth") == "false")
			{
				return RedirectToAction("SignIn", "Auth");
			}

			if (ModelState.IsValid)
            {
				string wwwRootPath = _hostEnv.WebRootPath;
				string fileName = Path.GetFileNameWithoutExtension(req.Image.FileName);
				string ext = Path.GetExtension(req.Image.FileName);

				Guid imgId = Guid.NewGuid();
				string fname = String.Format("{0}-{1}{2}", imgId.ToString(), fileName, ext);

				string path = Path.Combine(wwwRootPath + "/Image/ProofOfNames", fname);

				using(var  fileStream = new FileStream(path, FileMode.Create))
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
			return RedirectToAction("Index");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ImageUpdate(ImageUpdateRequest req)
		{
			if (HttpContext.Session.GetString("IsAuth") == "false")
			{
				return RedirectToAction("SignIn", "Auth");
			}

			if (ModelState.IsValid)
			{
				string wwwRootPath = _hostEnv.WebRootPath;
				string fileName = Path.GetFileNameWithoutExtension(req.Image.FileName);
				string ext = Path.GetExtension(req.Image.FileName);

				Guid imgId = Guid.NewGuid();
				string fname = String.Format("{0}-{1}{2}", imgId.ToString(), fileName, ext);

				string path = Path.Combine(wwwRootPath + "/Image/updates/", fname);

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
			
			return RedirectToAction("Index");
		}
	}
}
