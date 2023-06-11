using Microsoft.AspNetCore.Mvc;

namespace StudentID.Controllers
{
	public class AdminController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Students()
		{
			return View();
		}

		public IActionResult Lecturers()
		{
			return View();
		}
	}
}
