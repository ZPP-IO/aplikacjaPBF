using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminPanelController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminPanelController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async System.Threading.Tasks.Task<IActionResult> Index()
        {
            ViewBag.NewReportsCount = await _db.Reports.CountAsync(r => r.Status == ReportStatus.New);
            return View();
        }
    }
}
