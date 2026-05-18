using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectPBF.Data;
using ProjectPBF.Models;

namespace ProjectPBF.Controllers
{
    public class ThreadsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ThreadsController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index(int forumId)
        {
            var forum = await _db.Forums.Include(f => f.Category).FirstOrDefaultAsync(f => f.Id == forumId);
            if (forum == null) return NotFound();
            ViewBag.Forum = forum;
            var threads = await _db.ForumThreads.Where(t => t.ForumId == forumId).OrderByDescending(t => t.CreatedAt).ToListAsync();
            return View(threads);
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
        public async Task<IActionResult> Create(ForumThreadModel model, string initialContent)
        {
            if (!ModelState.IsValid) return View(model);

            model.CreatedAt = DateTime.UtcNow;
            _db.ForumThreads.Add(model);
            await _db.SaveChangesAsync();

            // create initial post
            var post = new ForumPostModel
            {
                ThreadId = model.Id,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                AuthorDisplayName = (await _db.Users.FindAsync(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))))?.Nick ?? User.Identity?.Name ?? "Anonim",
                Content = initialContent,
                CreatedAt = DateTime.UtcNow
            };
            _db.ForumPosts.Add(post);
            await _db.SaveChangesAsync();

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
            var isAdmin = User.IsInRole("Administrator") || User.IsInRole("MistrzGry");

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

        public async Task<IActionResult> Details(int id)
        {
            var thread = await _db.ForumThreads
                .Include(t => t.Posts)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (thread == null) return NotFound();
            // prepare current user's characters for post form
            if (User.Identity?.IsAuthenticated == true)
            {
                var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(uid, out var intUid))
                {
                    var chars = await _db.CharacterModels.Where(c => c.UserId == intUid).AsNoTracking().ToListAsync();
                    ViewBag.Characters = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(chars, "Id", "Name");
                }
            }
            return View(thread);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPost(int threadId, string content, int? characterId)
        {
            if (string.IsNullOrWhiteSpace(content)) return BadRequest("Brak treœci");
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var authorNick = (await _db.Users.FindAsync(int.Parse(uid)))?.Nick ?? User.Identity?.Name ?? "Anonim";
            var post = new ForumPostModel
            {
                ThreadId = threadId,
                UserId = uid,
                CharacterId = characterId,
                AuthorDisplayName = authorNick,
                CharacterDisplayName = null,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };
            if (characterId.HasValue)
            {
                var ch = await _db.CharacterModels.FindAsync(characterId.Value);
                if (ch != null) post.CharacterDisplayName = ch.Name;
            }
            _db.ForumPosts.Add(post);
            await _db.SaveChangesAsync();
            return RedirectToAction("Details", new { id = threadId });
        }

        // EDYCJA W¥TKU (Tytu³)
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var thread = await _db.ForumThreads.Include(t => t.Posts).FirstOrDefaultAsync(t => t.Id == id);
            if (thread == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var firstPost = thread.Posts.OrderBy(p => p.CreatedAt).FirstOrDefault();

            if (User.IsInRole("Administrator") || User.IsInRole("MistrzGry") || firstPost?.UserId == currentUserId)
            {
                return View(thread);
            }
            return Forbid();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string title)
        {
            var thread = await _db.ForumThreads.Include(t => t.Posts).FirstOrDefaultAsync(t => t.Id == id);
            if (thread == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var firstPost = thread.Posts.OrderBy(p => p.CreatedAt).FirstOrDefault();

            if (User.IsInRole("Administrator") || User.IsInRole("MistrzGry") || firstPost?.UserId == currentUserId)
            {
                thread.Title = title;
                await _db.SaveChangesAsync();
                return RedirectToAction("Details", new { id = thread.Id });
            }
            return Forbid();
        }

        [Authorize]
        public async Task<IActionResult> EditPost(int id)
        {
            var post = await _db.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();

            if (User.IsInRole("Administrator") || User.IsInRole("MistrzGry") || post.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return View(post);
            }
            return Forbid();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, string content)
        {
            var post = await _db.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();

            if (User.IsInRole("Administrator") || User.IsInRole("MistrzGry") || post.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                post.Content = content;
                await _db.SaveChangesAsync();
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
            var isAdmin = User.IsInRole("Administrator") || User.IsInRole("MistrzGry");

            var isAuthor = post.UserId == currentUserId;

            if (isAdmin || isAuthor)
            {
                int threadId = post.ThreadId;
                int forumId = post.Thread.ForumId;

                var postCount = await _db.ForumPosts.CountAsync(p => p.ThreadId == threadId);

                if (postCount <= 1)
                {
                    _db.ForumThreads.Remove(post.Thread);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("Index", new { forumId = forumId });
                }

                _db.ForumPosts.Remove(post);
                await _db.SaveChangesAsync();
                return RedirectToAction("Details", new { id = threadId });
            }

            return Forbid();
        }
    }
}
