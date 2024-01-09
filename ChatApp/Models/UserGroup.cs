namespace ChatApp.Models
{
    public class UserGroup
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public int GroupId {  get; set; }
        public GroupChat GroupChat { get; set; }
    }
}
