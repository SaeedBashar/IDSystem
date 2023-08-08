using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentID.Data;
using StudentID.Models;
using StudentID.Models.Requests;
using StudentID.Services;
using System.IO;
using System.Security.Claims;

namespace StudentID.Controllers
{
	[Authorize]
	public class AdminController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly IWebHostEnvironment _hostEnv;
		private AdminServices _service;

		public AdminController(ApplicationDbContext db, IWebHostEnvironment hostEnv)
		{
			_db = db;
			_hostEnv = hostEnv;
			_service = new AdminServices(_db);
		}
		public IActionResult Index()
		{
			
			string aid = User.FindFirst("UserId")?.Value;
			
			if(aid != null)
			{
				var counts = _service.GetCounts();
				var viewState = new
				{
					nameModifications = _service.NameModifications(),
					imageUpdates = _service.ImageUpdates(),
					students = _service.Students(),
					issuedCardCount = counts.issuedCardCount,
					nameModifyCount = counts.nameModifyCount,
					imageUpdateCount = counts.imageUpdateCount
				};

				return View(viewState);
			}

			return RedirectToAction("SignIn", "Auth");
		}


		public IActionResult getStudentDetails(Guid id)
		{
			
			var query = from s in _db.Students
						join c in _db.IDCards on s.Id equals c.StudentId
						where s.Id == id
						select new { student = s, card = c };

			return Json(query);
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
				var nmNotify = _db.NmNotifies.FirstOrDefault(x =>
					x.StudentId == reqBody.StudentId &&
					x.RequestId == reqBody.RequestId);

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
				_db.NmNotifyConfirms.Add(new NmNotifyConfirm()
				{
					Status = "Approved",
					StudentId = nmNotify.StudentId,
					RequestId = nmNotify.RequestId
				});
				_db.NmNotifies.Remove(nmNotify);


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
				var nmNotify = _db.NmNotifies.FirstOrDefault(x =>
					x.StudentId == reqBody.StudentId &&
					x.RequestId == reqBody.RequestId);

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
				_db.NmNotifyConfirms.Add(new NmNotifyConfirm()
				{
					Status = "Rejected",
					StudentId = nmNotify.StudentId,
					RequestId = nmNotify.RequestId
				});
				_db.NmNotifies.Remove(nmNotify);
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
				var iuNotify = _db.IuNotifies.FirstOrDefault(x =>
					x.StudentId == reqBody.StudentId &&
					x.RequestId == reqBody.RequestId);

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
					_db.IuNotifyConfirms.Add(new IuNotifyConfirm()
					{
						Status = "Approved",
						StudentId = iuNotify.StudentId,
						RequestId = iuNotify.RequestId
					});
					_db.IuNotifies.Remove(iuNotify);
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
				var iuNotify = _db.IuNotifies.FirstOrDefault(x =>
					x.StudentId == reqBody.StudentId &&
					x.RequestId == reqBody.RequestId);

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
					_db.IuNotifyConfirms.Add(new IuNotifyConfirm()
					{
						Status = "Rejected",
						StudentId = iuNotify.StudentId,
						RequestId = iuNotify.RequestId
					});
					_db.IuNotifies.Remove(iuNotify);
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
		
		public IActionResult ChangeCardStatus([FromBody] CardStatusRequest body)
		{

			
			var studentCard = _db.IDCards.SingleOrDefault(s => s.StudentId == body.StudentId);

			if(studentCard != null)
			{

				if(body.cardStatus && studentCard.IsActive)
				{
					return Json(new {status = true, msg = "Student Card Is Already Activated!"});

				}

				if(!body.cardStatus && !studentCard.IsActive)
				{
					return Json(new { status = true, msg = "Student Card Is Already Deactivated!" });

				}

				studentCard.IsActive = body.cardStatus;
				_db.SaveChanges();
				return Json(new { status = true, msg = "Student Card Updated Successfully!" });
			}
			else
			{
				return Json(new { status = false, msg = "Student Card Not Found!" });
			}
		}

		public IActionResult CountsApi()
		{
			var obj = _service.GetCounts();
			return Json(obj);
		}

		public IActionResult NameModificationsApi()
		{
			var obj = _service.NameModifications();
			return Json(obj);
		}

		public IActionResult ImageUpdatesApi()
		{
			var obj = _service.ImageUpdates();
			return Json(obj);
		}

		public IActionResult StudentsApi()
		{
			var obj = _service.Students();
			return Json(obj);
		}
	}
}
