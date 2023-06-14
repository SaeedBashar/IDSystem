using Microsoft.AspNetCore.Mvc;
using StudentID.Data;
using StudentID.Models;
using StudentID.Models.Requests;
using System.IO;

namespace StudentID.Controllers
{
	public class AdminController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly IWebHostEnvironment _hostEnv;

		public AdminController(ApplicationDbContext db, IWebHostEnvironment hostEnv)
		{
			this._db = db;
			this._hostEnv = hostEnv;
		}
		public IActionResult Index()
		{

			var query = from s in _db.Students
						join c in _db.IDCards on s.Id equals c.StudentId
						join n in _db.NameModificationDocuments on s.Id equals n.StudentId into stud_mod
						from st in stud_mod.DefaultIfEmpty()
						where st != null
						select new { Student = s, IDCard = c, MStud = st };

			var query1 = from s in _db.Students
						 join i in _db.ImageUpdates on s.Id equals i.StudentId into stud_mod
						 from st in stud_mod.DefaultIfEmpty()
						 where st != null
						 select new { 
							 currentImg=s.ImageUrl, 
							 newImg=st.FileName, 
							 requestId=st.Id, 
							 studentId=st.StudentId
						 };

			var viewState = new
			{
				nameModifications = query.ToList(),
				imageUpdates = query1.ToList()
			};

			return View(viewState);
		}

		public IActionResult GetNameModificationDetail(Guid Id)
		{
			var query = (from s in _db.Students
						 join c in _db.IDCards on s.Id equals c.StudentId
						 join p in _db.Programs on s.ProgramId equals p.ProgramId
						 join n in _db.NameModificationDocuments on s.Id equals n.StudentId
						 where Id.ToString() == s.Id.ToString()
						 select new { Student = s, IDCard = c, Program = p, NModify = n })
						.SingleOrDefault();

			return Json(query);
		}

		[HttpPost]
		public async Task<IActionResult> ApproveNameModificationRequest([FromBody] NameModificationApproval reqBody)
		{
			try
			{
				var student = _db.Students.FirstOrDefault(x => x.Id == Guid.Parse(reqBody.StudentId));
				var nModify = _db.NameModificationDocuments
					.FirstOrDefault(
						x => x.Id == Guid.Parse(reqBody.RequestId) &&
							 x.StudentId == Guid.Parse(reqBody.StudentId)
					);

				if (student == null || nModify == null)
				{
					throw new Exception("Record Was Not Found");
				}

				student.LastName = nModify.LastName;
				student.OtherNames = nModify.OtherNames;

				string wwwRootPath = _hostEnv.WebRootPath;
				string fname = nModify.FileName;
				string filePath = Path.Combine(wwwRootPath + "/image/ProofOfNames", fname);
				if (System.IO.File.Exists(filePath))
				{
					// Delete the file
					System.IO.File.Delete(filePath);
					Console.WriteLine("File removed successfully");
				}
				else
				{
					Console.WriteLine("File not found");
					return Json(new { status = false, msg = "Error Deleting File!!" });
				}
				_db.NameModificationDocuments.Remove(nModify);

				await _db.SaveChangesAsync();

				return Json(new { status = true, msg = "Name Modification Approved Successfully!!" });
			}
			catch (Exception ex)
			{
				return Json(new { status = false, msg = "Unexpected Error!!" });
			}
			
		}

		[HttpPost]
		public async Task<IActionResult> RejectNameModificationRequest([FromBody] NameModificationApproval reqBody)
		{
			try
			{
				var nModify = _db.NameModificationDocuments
					.FirstOrDefault(
						x => x.Id == Guid.Parse(reqBody.RequestId) &&
							 x.StudentId == Guid.Parse(reqBody.StudentId)
					);

				if (nModify == null)
				{
					throw new Exception("Record Was Not Found");
				}

				string wwwRootPath = _hostEnv.WebRootPath;
				string fname = nModify.FileName;
				string filePath = Path.Combine(wwwRootPath + "/image/ProofOfNames", fname);
				if (System.IO.File.Exists(filePath))
				{
					// Delete the file
					System.IO.File.Delete(filePath);
					Console.WriteLine("File removed successfully");
				}
				else
				{
					Console.WriteLine("File not found");
					return Json(new { status = false, msg = "Error Deleting File!!" });
				}
				_db.NameModificationDocuments.Remove(nModify);

				await _db.SaveChangesAsync();

				return Json(new { status = true, msg = "Name Modification Rejected Successfully!!" });
			}
			catch (Exception ex)
			{
				return Json(new { status = false, msg = "Unexpected Error!!" });
			}

		}
	
		[HttpPost]
		public async Task<IActionResult> ApproveImageUpdate([FromBody] NameModificationApproval reqBody)
		{
			try
			{
				var student = _db.Students.FirstOrDefault(x => x.Id == Guid.Parse(reqBody.StudentId));
				var nModify = _db.ImageUpdates
					.FirstOrDefault(
						x => x.Id == Guid.Parse(reqBody.RequestId) &&
							 x.StudentId == Guid.Parse(reqBody.StudentId)
					);

				if (student == null || nModify == null)
				{
					throw new Exception("Record Was Not Found");
				}


				string wwwRootPath = _hostEnv.WebRootPath;
				string fname = nModify.FileName;
				string oldPath = Path.Combine(wwwRootPath + "/image/approved", student.ImageUrl);
				string newPath = Path.Combine(wwwRootPath + "/image/updates", nModify.FileName);
				if (System.IO.File.Exists(oldPath))
				{
					// Delete the file
					System.IO.File.Delete(oldPath);
					System.IO.File.Move(newPath, Path.Combine(wwwRootPath + "/image/approved", nModify.FileName));
					student.ImageUrl = nModify.FileName;
					_db.ImageUpdates.Remove(nModify);
					await _db.SaveChangesAsync();
					Console.WriteLine("File removed successfully");
				}
				else
				{
					Console.WriteLine("File not found");
					return Json(new { status = false, msg = "Error Deleting File!!" });
				}
				return Json(new { status = true, msg = "Image Update Approved Successfully!!" });
			}
			catch (Exception ex)
			{
				return Json(new { status = false, msg = "Unexpected Error!!" });
			}

		}

		[HttpPost]
		public async Task<IActionResult> RejectImageUpdate([FromBody] NameModificationApproval reqBody)
		{
			try
			{
				var student = _db.Students.FirstOrDefault(x => x.Id == Guid.Parse(reqBody.StudentId));
				var nModify = _db.ImageUpdates
					.FirstOrDefault(
						x => x.Id == Guid.Parse(reqBody.RequestId) &&
							 x.StudentId == Guid.Parse(reqBody.StudentId)
					);

				if (student == null || nModify == null)
				{
					throw new Exception("Record Was Not Found");
				}


				string wwwRootPath = _hostEnv.WebRootPath;
				string fname = nModify.FileName;
				// string oldPath = Path.Combine(wwwRootPath + "/image/approved", student.ImageUrl);
				string newPath = Path.Combine(wwwRootPath + "/image/updates", nModify.FileName);
				if (System.IO.File.Exists(newPath))
				{
					// Delete the file
					System.IO.File.Delete(newPath);
					// System.IO.File.Move(newPath, Path.Combine(wwwRootPath + "/image/approved", nModify.FileName));
					_db.ImageUpdates.Remove(nModify);
					await _db.SaveChangesAsync();
					Console.WriteLine("File removed successfully");
				}
				else
				{
					Console.WriteLine("File not found");
					return Json(new { status = false, msg = "Error Deleting File!!" });
				}
				return Json(new { status = true, msg = "Image Update Rejected Successfully!!" });
			}
			catch (Exception ex)
			{
				return Json(new { status = false, msg = "Unexpected Error!!" });
			}

		}
	}
}
