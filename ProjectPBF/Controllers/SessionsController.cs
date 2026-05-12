using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;

namespace ProjectPBF.Controllers
{
    [Authorize]
    public class SessionsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<SessionsController> _logger;

        public SessionsController(ApplicationDbContext db, ILogger<SessionsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Lista sesji dla kampanii
        public async Task<IActionResult> Index(int campaignId)
        {
            var campaign = await _db.CampaignModels.FindAsync(campaignId);
            if (campaign == null) return NotFound();

            var sessions = await _db.SessionModels
                .Include(s => s.GameMaster)
                .Where(s => s.CampaignId == campaignId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            ViewBag.Campaign = campaign;
            return View(sessions);
        }

        // Formularz tworzenia sesji (GET)
        public async Task<IActionResult> Create(int campaignId)
        {
            var campaign = await _db.CampaignModels.FindAsync(campaignId);
            if (campaign == null) return NotFound();

            // tylko MG kampanii mo¿e tworzyæ sesjê
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var currentUserId) || campaign.GameMasterId != currentUserId)
                return Forbid();

            var model = new SessionModel
            {
                CampaignId = campaignId,
                GameMasterId = campaign.GameMasterId,
                DurationMinutes = 120,
                IsPrivate = true
            };

            ViewBag.Campaign = campaign;
            return View(model);
        }

        // Obs³uga tworzenia sesji (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SessionModel model)
        {
            var campaign = await _db.CampaignModels.FindAsync(model.CampaignId);
            if (campaign == null) return NotFound();

            // autoryzacja: tylko MG kampanii
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var currentUserId) || campaign.GameMasterId != currentUserId)
                return Forbid();

            // Jeœli formularz przesy³a oddzielnie datê i czas (StartDate, StartTime), po³¹cz je tutaj.
            var form = Request.Form;
            if (form.ContainsKey("StartDate"))
            {
                var startDate = form["StartDate"].ToString();
                var startTime = form.ContainsKey("StartTime") ? form["StartTime"].ToString() : "00:00";
                if (!string.IsNullOrWhiteSpace(startDate))
                {
                    if (DateTime.TryParse($"{startDate} {startTime}", out var parsed))
                    {
                        model.StartAt = DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
                    }
                }
            }

            // Bezpieczeñstwo: przypisujemy MG kampanii jako GameMaster sesji
            model.GameMasterId = campaign.GameMasterId;
            model.CreatedAt = DateTime.UtcNow;

            if (!ModelState.IsValid)
            {
                // Zaloguj szczegó³y b³êdów dla ³atwiejszej diagnostyki
                var errors = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                _logger.LogWarning("ModelState invalid when creating session for campaign {CampaignId}. Errors: {Errors}", model.CampaignId, errors);
                TempData["ErrorMessage"] = "Walidacja formularza nie powiod³a siê: " + (string.IsNullOrEmpty(errors) ? "sprawdŸ pola." : errors);

                ViewBag.Campaign = campaign;
                return View(model);
            }

            try
            {
                // Zapis sesji
                _db.SessionModels.Add(model);
                await _db.SaveChangesAsync();

                // Automatycznie dodaj MG jako cz³onka sesji (admin)
                var member = new SessionMemberModel
                {
                    SessionId = model.Id,
                    UserId = campaign.GameMasterId,
                    IsAdmin = true,
                    Status = SessionMembershipStatus.Accepted,
                    JoinedAt = DateTime.UtcNow
                };
                _db.SessionMembers.Add(member);
                await _db.SaveChangesAsync();

                // Widoczny komunikat potwierdzaj¹cy
                TempData["SuccessMessage"] = "Sesja zosta³a utworzona.";

                // PRZEKIEROWANIE: do szczegó³ów nowo utworzonej sesji
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "B³¹d zapisu sesji dla kampanii {CampaignId}", model.CampaignId);
                TempData["ErrorMessage"] = "Wyst¹pi³ b³¹d przy zapisie sesji. Spróbuj ponownie.";
                ViewBag.Campaign = campaign;
                return View(model);
            }
        }

        // Szczegó³y sesji
        public async Task<IActionResult> Details(int id)
        {
            var session = await _db.SessionModels
                .Include(s => s.GameMaster)
                .Include(s => s.Campaign)
                .Include(s => s.Members).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null) return NotFound();
            return View(session);
        }

        // Z³ó¿ proœbê o do³¹czenie do sesji (lub ponów proœbê)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int id)
        {
            var session = await _db.SessionModels
                .Include(s => s.Members)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null) return NotFound();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var currentUserId)) return Challenge();

            // SprawdŸ czy u¿ytkownik ju¿ ma wpis
            var existing = session.Members.FirstOrDefault(m => m.UserId == currentUserId);
            if (existing != null)
            {
                if (existing.Status == SessionMembershipStatus.Accepted)
                {
                    TempData["ErrorMessage"] = "Jesteœ ju¿ uczestnikiem tej sesji.";
                }
                else if (existing.Status == SessionMembershipStatus.Pending)
                {
                    TempData["ErrorMessage"] = "Masz ju¿ oczekuj¹c¹ proœbê o do³¹czenie.";
                }
                else
                {
                    // Je¿eli wczeœniej odrzucono, utwórz now¹ proœbê (lub zresetuj status)
                    existing.Status = SessionMembershipStatus.Pending;
                    existing.JoinedAt = null;
                    _db.SessionMembers.Update(existing);
                    await _db.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Wys³ano proœbê o ponowne do³¹czenie.";
                }

                return RedirectToAction(nameof(Details), new { id });
            }

            var request = new SessionMemberModel
            {
                SessionId = id,
                UserId = currentUserId,
                IsAdmin = false,
                Status = SessionMembershipStatus.Pending,
                JoinedAt = null
            };

            _db.SessionMembers.Add(request);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Wys³ano proœbê o do³¹czenie do sesji.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Akceptacja proœby o do³¹czenie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveMember(int memberId)
        {
            var member = await _db.SessionMembers
                .Include(m => m.Session)
                .FirstOrDefaultAsync(m => m.Id == memberId);

            if (member == null) return NotFound();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var currentUserId)) return Challenge();

            // Tylko MG sesji lub cz³onek sesji z IsAdmin mo¿e akceptowaæ
            var session = member.Session;
            var isSessionGM = session.GameMasterId == currentUserId;
            var isSessionAdmin = await _db.SessionMembers.AnyAsync(m => m.SessionId == session.Id && m.UserId == currentUserId && m.IsAdmin && m.Status == SessionMembershipStatus.Accepted);

            if (!isSessionGM && !isSessionAdmin) return Forbid();

            member.Status = SessionMembershipStatus.Accepted;
            member.JoinedAt = DateTime.UtcNow;
            _db.SessionMembers.Update(member);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cz³onek zaakceptowany.";
            return RedirectToAction(nameof(Details), new { id = session.Id });
        }

        // Odrzucenie proœby o do³¹czenie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectMember(int memberId)
        {
            var member = await _db.SessionMembers
                .Include(m => m.Session)
                .FirstOrDefaultAsync(m => m.Id == memberId);

            if (member == null) return NotFound();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var currentUserId)) return Challenge();

            var session = member.Session;
            var isSessionGM = session.GameMasterId == currentUserId;
            var isSessionAdmin = await _db.SessionMembers.AnyAsync(m => m.SessionId == session.Id && m.UserId == currentUserId && m.IsAdmin && m.Status == SessionMembershipStatus.Accepted);

            if (!isSessionGM && !isSessionAdmin) return Forbid();

            member.Status = SessionMembershipStatus.Rejected;
            _db.SessionMembers.Update(member);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Proœba odrzucona.";
            return RedirectToAction(nameof(Details), new { id = session.Id });
        }
    }
}