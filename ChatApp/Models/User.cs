using System.Text.Json.Serialization;

namespace ChatApp.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? ConnectionId { get; set; }
        public bool? IsOnline { get; set; }
        [JsonIgnore]
        public ICollection<UserFriend>? Friends { get; set; }
        [JsonIgnore]
        public ICollection<UserGroup>? UserGroups { get; set; }
    }
}
