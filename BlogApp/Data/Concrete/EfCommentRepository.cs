using BlogApp.Data.Abstract;
using BlogApp.Entity;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Data.Concrete
{
    public class EfCommentRepository : ICommentRepository
    {
        private readonly BlogContext _context;

        public EfCommentRepository(BlogContext context)
        {
            _context = context;
        }

        public IQueryable<Comment> Comments => _context.Comments;

        public void CreateComment(Comment comment)
        {
            _context.Comments.Add(comment);
        }
        public async Task<Comment> GetCommentByIdAsync(int commentId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CommentId == commentId);
        }

        public void UpdateComment(Comment comment)
        {
            _context.Comments.Update(comment);
        }

        public void DeleteComment(Comment comment)
        {
            _context.Comments.Remove(comment);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
