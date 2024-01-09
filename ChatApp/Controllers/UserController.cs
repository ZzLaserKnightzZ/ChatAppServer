using ChatApp.Data;
using ChatApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly AppSqliteDbContext _db;
        private readonly IHubContext<ChatHub> _chat;
        public UserController(ILogger<UserController> logger, AppSqliteDbContext db, IHubContext<ChatHub> chat)
        {
            _logger = logger;
            _db = db;
            _chat = chat;
        }

        [HttpPost]
        public async Task<ActionResult> Login(string userName, string pass)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Name == userName && u.Password == pass);
            if (user != null)
            {
                return Ok(new { id = user.Id, name = user.Name });
            }
            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> Online(string? userId,string? connectionId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _db.Users.Include(friend => friend.Friends).FirstOrDefaultAsync(user => user.Id.Equals(Guid.Parse(userId))); //get all friend
                if (user != null)
                {
                    user.IsOnline = true;
                    user.ConnectionId = connectionId;//save context id
                    await _db.SaveChangesAsync();
                    //notify to friend
                    if (user.Friends != null)
                    {
                        foreach (var friend in user.Friends)
                        {
                            var friendUser = await _db.Users.FirstOrDefaultAsync(friendUser => friendUser.Id.Equals(friend.FriendId));
                            if (friendUser != null && !string.IsNullOrEmpty(friendUser.ConnectionId))
                            {
                                await _chat.Clients.Client(friendUser.ConnectionId!).SendAsync("online", user.Name,friendUser.Id.ToString(), connectionId);
                            }

                        }
                    }
                    //return friends
                    return Ok(user.Friends);
                }
            }
             return Ok(new List<string>());
        }

        [HttpPost]
        public async Task<IActionResult> Register(string userName, string pass)
        {
            await _db.Users.AddAsync(new Models.User { Name = userName, Password = pass });
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddFriend(string userId, string friendId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id.Equals(Guid.Parse(userId)))!;
            var friend = await _db.Users.FirstOrDefaultAsync(u => u.Id.Equals(Guid.Parse(friendId)))!;
            var userFriend = new UserFriend { User = user, FriendId = Guid.Parse(friendId) };
            var FriendUser = new UserFriend { User = friend, FriendId = Guid.Parse(userId) };
            await _db.UserFriends.AddAsync(userFriend)!;
            await _db.UserFriends.AddAsync(FriendUser)!;
            await _db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(friend.ConnectionId))
            {
                await _chat.Clients.Client(friend.ConnectionId).SendAsync("message", user.Name, "invite friend");//all noti
            }
            return Ok(true);
        }

        [HttpGet]
        public async Task<IActionResult> AllUser(string userId)
        {
            var users = await _db.Users.AsNoTracking().Where(x => !x.Id.Equals(Guid.Parse(userId))).Select(user => new { name = user.Name, id = user.Id, connectionId = user.ConnectionId }).ToListAsync();
            return Ok(users);
        }

        [HttpGet]
        public async Task<IActionResult> AllFriend(string userId)
        {
            var friend = await _db.UserFriends.Where(friend => friend.UserId.Equals(Guid.Parse(userId)) ).Select((f) => _db.Users.FirstOrDefault(u => u.Id.Equals(f.FriendId))).ToListAsync();
            return Ok(friend);
        }

        [HttpPost]
        public async Task<IActionResult> AddGroup(string userId, string RoomName)
        {
            var newGroup = new GroupChat { RoomName = RoomName };
            var user = await _db.Users.FirstOrDefaultAsync(_ => _.Id.Equals(Guid.Parse(userId)));
            await _db.GroupChats.AddAsync(newGroup);
            await _db.UserGroups.AddAsync(new UserGroup { GroupChat = newGroup, User = user });
            await _db.SaveChangesAsync();
            return Ok(newGroup.Id);
        }

        [HttpGet]
        public async Task<IActionResult> UserAllGroup(string userId)
        {
            var group = await _db.UserGroups.Include(_ => _.GroupChat).Where(_ => _.UserId.Equals(Guid.Parse(userId))).Select(g => g.GroupChat).Select(g => new { room = g.RoomName, id = g.Id }).ToListAsync();
            return Ok(group);
        }

        [HttpGet]
        public async Task<IActionResult> AllGroup(string userId)
        {
            var group = await _db.UserGroups.Include(_=>_.GroupChat).Where(_=> !_.UserId.Equals(Guid.Parse(userId))).Select(g=> g.GroupChat).Select(g => new { room = g.RoomName, id = g.Id }).ToListAsync();
            return Ok(group);
        }

        [HttpPost]
        public async Task<IActionResult> EnterChatGroup(string connectionId,string groupName)
        {
            await _chat.Groups.AddToGroupAsync(connectionId, groupName);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> JoinGroup(string connectionId, int groupId, string userId)
        {
            var group = await _db.GroupChats.FirstOrDefaultAsync(g => g.Id == groupId);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id.Equals(Guid.Parse(userId)));

            if (group != null && user != null)
            {
                var userGroup = new UserGroup { GroupChat = group, User = user };
                await _db.UserGroups.AddAsync(userGroup);
                await _db.SaveChangesAsync();
                await _chat.Groups.AddToGroupAsync(connectionId, group.RoomName);
                //noti
                await _chat.Clients.Groups(group.RoomName).SendAsync(connectionId, groupId, user.Name + " join this group");
                return Ok(userGroup);
            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> LeaveGroup(string userId, int groupId, string connectionId)
        {
            var userGroup = await _db.UserGroups.FirstOrDefaultAsync(ug => ug.Id == groupId && ug.UserId.Equals(Guid.Parse(userId)));
            if (userGroup != null)
            {
                _db.UserGroups.Remove(userGroup);
                await _db.SaveChangesAsync();
                var group = await _db.GroupChats.FirstOrDefaultAsync(_ => _.Id == groupId);
                var user = await _db.Users.FirstOrDefaultAsync(_ => _.Id.Equals(userId));
                await _chat.Groups.RemoveFromGroupAsync(connectionId, group.RoomName);
                //noti
                await _chat.Clients.Groups(group.RoomName).SendAsync(connectionId, groupId, user.Name + " leave this group.");
                return Ok(userGroup);
            }

            return BadRequest();
        }
    }
}
