using BlogApp.Entity;

namespace BlogApp.Data.Abstract
{

    public interface ICommentRepository
    {

        IQueryable<Comment> Comments { get; }
        void CreateComment(Comment comment);
        void UpdateComment(Comment comment);
        void DeleteComment(Comment comment);
        Task<Comment> GetCommentByIdAsync(int id);
        Task SaveChangesAsync();
    }
}