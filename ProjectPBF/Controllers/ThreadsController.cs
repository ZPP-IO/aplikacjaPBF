using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;
using ProjectPBF.Models.Enums;
using ProjectPBF.Services;

namespace ProjectPBF.Controllers
{
    public class ThreadsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ActivityLogService _activityLogService;

        public ThreadsController(ApplicationDbContext db, ActivityLogService activityLogService)
        {
            _db = db;
            _activityLogService = activityLogService;
        }

        private int? GetCurrentUserId()
        {
            var rawUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(rawUserId, out var parsedUserId) ? parsedUserId : null;
        }

        // Pomocnicze: sk³ada pojedyncze nazwy flag (z checkboxów <input name="tags" value="Spoiler">)
        // w jedn¹ wartoœæ enuma ContentTag (Flags).
        private static ContentTag ParseTags(string[]? tags)
        {
            var result = ContentTag.None;
            if (tags == null) return result;
            foreach (var t in tags)
            {
                if (Enum.TryParse<ContentTag>(t, true, out var parsed))
                {
                    result |= parsed;
                }
            }
            return result;
        }

        // Lista watkow w danym forum + wyszukiwanie + filtr archiwum
        public async Task<IActionResult> Index(int forumId, string? q, bool showArchived = false)
        {
            var forum = await _db.Forums.Include(f => f.Category).FirstOrDefaultAsync(f => f.Id == forumId);
            if (forum == null) return NotFound();

            ViewBag.Forum = forum;
            ViewBag.Query = q;
            ViewBag.ShowArchived = showArchived;

            var query = _db.ForumThreads
                .Include(t => t.Posts)
                .Where(t => t.ForumId == forumId);

            if (!showArchived)
            {
                query = query.Where(t => !t.IsArchived);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(t =>
                    t.Title.Contains(term) ||
                    t.Posts.Any(p => !p.IsDeleted && p.Content.Contains(term)));
            }

            var threads = await query
                .OrderByDescending(t => t.IsPinned)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(threads);
        }

        // Globalne wyszukiwanie watkow po calym forum
        public async Task<IActionResult> Search(string? q)
        {
            ViewBag.Query = q;

            if (string.IsNullOrWhiteSpace(q))
            {
                return View(new System.Collections.Generic.List<ForumThreadModel>());
            }

            var term = q.Trim();

            var results = await _db.ForumThreads
                .Include(t => t.Forum).ThenInclude(f => f!.Category)
                .Include(t => t.Posts)
                .Where(t => !t.IsArchived && (
                    t.Title.Contains(term) ||
                    t.Posts.Any(p => !p.IsDeleted && p.Content.Contains(term))))
                .OrderByDescending(t => t.CreatedAt)
                .Take(50)
                .ToListAsync();

            return View(results);
        }

        [Authorize]
        public IActionResult Create(int forumId)
        {
            ViewBag.ForumId = forumId;
            return View(new ForumThreadModel { ForumId = forumId });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumThreadModel model, string initialContent, string[]? tags = null)
        {
            if (!ModelState.IsValid) return View(model);

            model.CreatedAt = DateTime.UtcNow;
            model.Tags = ParseTags(tags);
            _db.ForumThreads.Add(model);
            await _db.SaveChangesAsync();

            var currentUserIdRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserId = GetCurrentUserId();

            var post = new ForumPostModel
            {
                ThreadId = model.Id,
                UserId = currentUserIdRaw,
                AuthorDisplayName = currentUserId.HasValue
                    ? (await _db.Users.FindAsync(currentUserId.Value))?.Nick ?? User.Identity?.Name ?? "Anonim"
                    : User.Identity?.Name ?? "Anonim",
                Content = initialContent,
                CreatedAt = DateTime.UtcNow
            };

            _db.ForumPosts.Add(post);
            await _db.SaveChangesAsync();

            if (currentUserId.HasValue)
            {
                await _activityLogService.LogAsync(
                    currentUserId.Value,
                    ActivityActionType.Created,
                    "ForumPost",
                    post.Id,
                    $"Dodano pierwszy post w nowym w¹tku: {model.Title}"
                );
            }

            return RedirectToAction("Details", new { id = model.Id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var thread = await _db.ForumThreads
                .Include(t => t.Posts)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (thread == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Administrator") || User.IsInRole("GameMaster");

            var firstPost = thread.Posts.OrderBy(p => p.CreatedAt).FirstOrDefault();
            var isAuthor = firstPost != null && firstPost.UserId == currentUserId;

            if (isAdmin || isAuthor)
            {
                int forumId = thread.ForumId;
                _db.ForumThreads.Remove(thread);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index", new { forumId = forumId });
            }

            return Forbid();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var thread = await _db.ForumThreads
                .Include(t => t.Posts)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (thread == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Administrator") || User.IsInRole("GameMaster");
            var firstPost = thread.Posts.OrderBy(p => p.CreatedAt).FirstOrDefault();
            var isAuthor = firstPost != null && firstPost.UserId == currentUserId;

            if (!isAdmin && !isAuthor) return Forbid();

            thread.IsArchived = true;
            thread.ArchivedAt = DateTime.UtcNow;
            thread.ArchivedByUserId = currentUserId;
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = thread.Id });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,GameMaster")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unarchive(int id)
        {
            var thread = await _db.ForumThreads.FindAsync(id);
            if (thread == null) return NotFound();

            thread.IsArchived = false;
            thread.ArchivedAt = null;
            thread.ArchivedByUserId = null;
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = thread.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var thread = await _db.ForumThreads
                .Include(t => t.Posts)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (thread == null) return NotFound();

            if (User.Identity?.IsAuthenticated == true)
            {
                var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(uid, out var intUid))
                {
                    var chars = await _db.CharacterModels
                        .Where(c => c.UserId == intUid)
                        .AsNoTracking()
                        .ToListAsync();

                    ViewBag.Characters = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(chars, "Id", "Name");
                }
            }

            return View(thread);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPost(int threadId, string content, int? characterId, string[]? tags = null)
        {
            if (string.IsNullOrWhiteSpace(content)) return BadRequest("Brak treœci");

            var thread = await _db.ForumThreads.FindAsync(threadId);
            if (thread == null) return NotFound();
            if (thread.IsArchived) return BadRequest("W¹tek jest zarchiwizowany - nie mo¿na dodawaæ nowych postów.");

            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserId = GetCurrentUserId();

            var authorNick = currentUserId.HasValue
                ? (await _db.Users.FindAsync(currentUserId.Value))?.Nick ?? User.Identity?.Name ?? "Anonim"
                : User.Identity?.Name ?? "Anonim";

            var post = new ForumPostModel
            {
                ThreadId = threadId,
                UserId = uid,
                CharacterId = characterId,
                AuthorDisplayName = authorNick,
                CharacterDisplayName = null,
                Content = content,
                Tags = ParseTags(tags),
                CreatedAt = DateTime.UtcNow
            };

            if (characterId.HasValue)
            {
                var ch = await _db.CharacterModels.FindAsync(characterId.Value);
                if (ch != null) post.CharacterDisplayName = ch.Name;
            }

            _db.ForumPosts.Add(post);
            await _db.SaveChangesAsync();

            if (currentUserId.HasValue)
            {
                await _activityLogService.LogAsync(
                    currentUserId.Value,
                    ActivityActionType.Created,
                    "ForumPost",
                    post.Id,
                    $"Dodano post w w¹tku o ID {threadId}"
                );
            }

            return RedirectToAction("Details", new { id = threadId });
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var thread = await _db.ForumThreads.Include(t => t.Posts).FirstOrDefaultAsync(t => t.Id == id);
            if (thread == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var firstPost = thread.Posts.OrderBy(p => p.CreatedAt).FirstOrDefault();

            if (User.IsInRole("Administrator") || User.IsInRole("GameMaster") || firstPost?.UserId == currentUserId)
            {
                return View(thread);
            }

            return Forbid();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string title, string[]? tags = null)
        {
            var thread = await _db.ForumThreads.Include(t => t.Posts).FirstOrDefaultAsync(t => t.Id == id);
            if (thread == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var firstPost = thread.Posts.OrderBy(p => p.CreatedAt).FirstOrDefault();

            if (User.IsInRole("Administrator") || User.IsInRole("GameMaster") || firstPost?.UserId == currentUserId)
            {
                thread.Title = title;
                thread.Tags = ParseTags(tags);
                await _db.SaveChangesAsync();
                return RedirectToAction("Details", new { id = thread.Id });
            }

            return Forbid();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator,GameMaster")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePin(int id)
        {
            var thread = await _db.ForumThreads.FindAsync(id);
            if (thread == null) return NotFound();

            thread.IsPinned = !thread.IsPinned;
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = thread.Id });
        }

        [Authorize]
        public async Task<IActionResult> EditPost(int id)
        {
            var post = await _db.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();

            if (User.IsInRole("Administrator") || User.IsInRole("GameMaster") || post.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return View(post);
            }

            return Forbid();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, string content, string[]? tags = null)
        {
            var post = await _db.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();

            if (User.IsInRole("Administrator") || User.IsInRole("GameMaster") || post.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                post.Content = content;
                post.Tags = ParseTags(tags);
                post.EditedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                var currentUserId = GetCurrentUserId();
                if (currentUserId.HasValue)
                {
                    await _activityLogService.LogAsync(
                        currentUserId.Value,
                        ActivityActionType.Updated,
                        "ForumPost",
                        post.Id,
                        $"Zedytowano post o ID {post.Id} w w¹tku o ID {post.ThreadId}"
                    );
                }

                return RedirectToAction("Details", new { id = post.ThreadId });
            }

            return Forbid();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _db.ForumPosts.Include(p => p.Thread).FirstOrDefaultAsync(p => p.Id == id);
            if (post == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserIdInt = GetCurrentUserId();
            var isAdmin = User.IsInRole("Administrator") || User.IsInRole("GameMaster");
            var isAuthor = post.UserId == currentUserId;

            if (isAdmin || isAuthor)
            {
                int threadId = post.ThreadId;
                int forumId = post.Thread.ForumId;
                int deletedPostId = post.Id;

                var postCount = await _db.ForumPosts.CountAsync(p => p.ThreadId == threadId);

                if (postCount <= 1)
                {
                    _db.ForumThreads.Remove(post.Thread);
                    await _db.SaveChangesAsync();

                    if (currentUserIdInt.HasValue)
                    {
                        await _activityLogService.LogAsync(
                            currentUserIdInt.Value,
                            ActivityActionType.Deleted,
                            "ForumPost",
                            deletedPostId,
                            $"Usuniêto ostatni post i ca³y w¹tek o ID {threadId}"
                        );
                    }

                    return RedirectToAction("Index", new { forumId = forumId });
                }

                _db.ForumPosts.Remove(post);
                await _db.SaveChangesAsync();

                if (currentUserIdInt.HasValue)
                {
                    await _activityLogService.LogAsync(
                        currentUserIdInt.Value,
                        ActivityActionType.Deleted,
                        "ForumPost",
                        deletedPostId,
                        $"Usuniêto post o ID {deletedPostId} z w¹tku o ID {threadId}"
                    );
                }

                return RedirectToAction("Details", new { id = threadId });
            }

            return Forbid();
        }
    }
}