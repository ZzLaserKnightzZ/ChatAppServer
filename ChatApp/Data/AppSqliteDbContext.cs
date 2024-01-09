using ChatApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChatApp.Data
{
    public class AppSqliteDbContext:DbContext
    {
        public AppSqliteDbContext(DbContextOptions<AppSqliteDbContext> options) : base(options)
        {
            //Database.EnsureCreated();
        }
        public DbSet<User> Users { get; set; }
        public DbSet<GroupChat> GroupChats { get; set; }
        public DbSet<UserFriend> UserFriends { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
    }
}
