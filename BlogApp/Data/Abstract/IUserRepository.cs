using BlogApp.Entity;

namespace BlogApp.Data.Abstract{

    public interface IUserRepository{

        IQueryable<User> Users {get;}
        void CreateUser(User user);
        User GetUserById(int userId); 
        void UpdateUser(User user);
    Task<int> SaveChangesAsync();
    }
}