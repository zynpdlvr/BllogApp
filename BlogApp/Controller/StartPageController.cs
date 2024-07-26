using System.Security.Claims;
using BlogApp.Data;
using BlogApp.Data.Abstract;
using BlogApp.Entity;
using BlogApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Controllers
{
    public class StartPageController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IUserRepository _userRepository;
        private ITagRepository _tagRepository;

        public StartPageController(IPostRepository postRepository, ICommentRepository commentRepository, IUserRepository userRepository, ITagRepository tagRepository)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _userRepository = userRepository;
            _tagRepository = tagRepository;
        }

        public async Task<IActionResult> Index(string tag)
        {
            var posts = _postRepository.Posts;

            if (!string.IsNullOrEmpty(tag))
            {
                posts = posts.Where(x => x.Tags.Any(t => t.Url == tag));
            }
            if (User.IsInRole("admin"))
            {
                var adminData = new
                {
                    UserCount = await _userRepository.Users.CountAsync(),
                    TagStatistics = await _tagRepository.Tags
                        .Select(tag => new
                        {
                            Text = tag.Text,
                            PostCount = tag.Posts.Count
                        })
                        .ToListAsync(),
                    MostActiveUsers = await _userRepository.Users
                        .OrderByDescending(user => user.Posts.Count)
                        .Take(10)
                        .Select(user => new
                        {
                            UserName = user.UserName,
                            PostCount = user.Posts.Count
                        })
                        .ToListAsync()
                };

                ViewData["AdminData"] = adminData;
            }

            return View(new PostViewModel { Posts = await posts.ToListAsync() });
        }

        public async Task<IActionResult> Details(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return NotFound();
            }

            var post = await _postRepository.Posts
                .Include(x => x.User)
                .Include(x => x.Tags)
                .Include(x => x.Comments)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(p => p.Url == url);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int postId, string commentText)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if not authenticated
            }

            var post = await _postRepository.Posts
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post == null)
            {
                return NotFound();
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid user ID.");
            }

            var user = _userRepository.GetUserById(userId); // Use the new method
            if (user == null)
            {
                return NotFound();
            }

            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Text = commentText,
                PublishedOn = DateTime.UtcNow
            };

            _commentRepository.CreateComment(comment);
            await _commentRepository.SaveChangesAsync();

            return RedirectToAction("Details", new { url = post.Url });
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(PostCreateViewModel model)
        {

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                _postRepository.CreatePost(
                    new Post
                    {
                        Title = model.Title,
                        Content = model.Content,
                        Description = model.Description,
                        Url = model.Url,
                        UserId = int.Parse(userId ?? ""),
                        Image = "images.jpg",
                        IsActive = false
                    }
                );
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<IActionResult> List()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "");
            var role = User.FindFirstValue(ClaimTypes.Role);

            var posts = _postRepository.Posts;

            if (string.IsNullOrEmpty(role))
            {
                posts = posts.Where(i => i.UserId == userId);
            }

            return View(await posts.ToListAsync());
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var post = _postRepository.Posts.Include(x => x.Tags).FirstOrDefault(i => i.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            ViewBag.Tags = _tagRepository.Tags.ToList();
            return View(
                new PostCreateViewModel
                {
                    PostId = post.PostId,
                    Title = post.Title,
                    Description = post.Description,
                    Content = post.Content,
                    Url = post.Url,
                    IsActive = post.IsActive,
                    Tags = post.Tags
                }

                );
        }

        [HttpPost]

        public IActionResult Edit(PostCreateViewModel model, int[] tagIds)
        {
            if (ModelState.IsValid)
            {
                var entityUpdate = new Post
                {
                    PostId = model.PostId,
                    Title = model.Title,
                    Description = model.Description,
                    Content = model.Content,
                    Url = model.Url,

                };
                if (User.FindFirstValue(ClaimTypes.Role) == "admin")
                {
                    entityUpdate.IsActive = model.IsActive;
                }
                _postRepository.EditPost(entityUpdate, tagIds);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _postRepository.Posts.FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _postRepository.Posts.FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            _postRepository.DeletePost(post);
            await _postRepository.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> EditComment(int commentId, string commentText)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            // Allow admins to edit any comment
            bool isAdmin = User.IsInRole("admin");

            if (comment.UserId != userId && !isAdmin)
            {
                return Unauthorized();
            }

            comment.Text = commentText;
            _commentRepository.UpdateComment(comment);
            await _commentRepository.SaveChangesAsync();

            var post = await _postRepository.Posts
                .FirstOrDefaultAsync(p => p.PostId == comment.PostId);

            if (post == null || string.IsNullOrEmpty(post.Url))
            {
                return NotFound();
            }

            return RedirectToAction("Details", "StartPage", new { url = post.Url });
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                return Json(new { success = false });
            }

            int currentUserId;
            bool isValidUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out currentUserId);

            if (!isValidUserId)
            {
                return Json(new { success = false });
            }

            if (User.IsInRole("admin") || comment.UserId == currentUserId)
            {
                _commentRepository.DeleteComment(comment);
                await _commentRepository.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }





    }}