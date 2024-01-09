using ChatApp.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using System;

namespace ChatApp.Data
{
    public class ChatHub:Hub
    {
        private readonly AppSqliteDbContext _db;
        public ChatHub(AppSqliteDbContext db) {
            _db = db;
        }
        #region video
        public async Task Candidate(string userId, string candidate)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id.Equals(Guid.Parse(userId)));
            if (user != null)
            {
                await Clients.Client(user.ConnectionId).SendAsync("Candidate", candidate);
            }
        }

        public async Task Signal(string offer_ans,string toUserId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id.Equals(Guid.Parse(toUserId)));
            if (user != null)
            {
                await Clients.Client(user.ConnectionId).SendAsync("Signal", offer_ans);
            }
        }

        public async Task Call(string userId, string offer, string name,string fromuserId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id.Equals(Guid.Parse(userId)));
            if (user != null)
            {
                await Clients.Client(user.ConnectionId).SendAsync("Call", offer, name, user.Id, fromuserId);
            }
        }
        public async Task AcceptCall(string userId,string Ans,string name,string fromuserId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id.Equals(Guid.Parse(userId)));
            if (user != null) {
                await Clients.Client(user.ConnectionId).SendAsync("acceptCall", Ans,name,user.Id, fromuserId);
            }
        }
        #endregion
        public async Task SendToUser(string connectionId,string userName,string msg) //all noti
        {
            await Clients.Client(connectionId).SendAsync("message", userName, msg);
        }

        public async Task SendToUserChat(string userId, string userName, string msg) //user to user
        {
            var toUser = await _db.Users.FirstOrDefaultAsync(user => user.Id.Equals(Guid.Parse(userId)));
            if (toUser != null)
            {
                await Clients.Client(toUser.ConnectionId).SendAsync("chat", userName, msg, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        public async Task SendToGroup(string groupName,string user,string msg)
        {
            await Clients.Groups(groupName).SendAsync("Groupchat", user,msg,DateTime.Now.ToString("HH:mm:ss dd-MM-YYYY"));
        }

        public override async Task<Task> OnDisconnectedAsync(Exception? exception)
        {
            var user = await _db.Users.FirstOrDefaultAsync(user => user.ConnectionId == Context.ConnectionId);
            if (user != null)
            {
                user.IsOnline = false;
                user.ConnectionId = "";
                await _db.SaveChangesAsync();
            }
            return base.OnDisconnectedAsync(exception);
        }
    }
}
