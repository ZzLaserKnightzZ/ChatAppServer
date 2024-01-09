using System.Text.Json.Serialization;

namespace ChatApp.Models
{
    public class GroupChat
    {
        public int Id { get; set; }
        public string RoomName { get; set; }
        [JsonIgnore]
        public ICollection<UserGroup> UserGroups { get; set; }
    }
}
