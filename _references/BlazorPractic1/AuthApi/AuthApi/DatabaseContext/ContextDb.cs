using AuthApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.DatabaseContext
{
    public class ContextDb : DbContext
    {
        public ContextDb(DbContextOptions options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Movies> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Session> Sessions { get; set; }

        public DbSet<Message> Messages { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }
    }
}
