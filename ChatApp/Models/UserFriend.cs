
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp.Models
{
    public class UserFriend
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid FriendId { get; set; }
        //public User Friend { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.UtcNow; 
    }
}
