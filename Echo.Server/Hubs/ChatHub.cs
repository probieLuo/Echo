using Echo.Server.DataBaseContext;
using Echo.Server.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Echo.Server.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly MyDbContext _dbContext;

        public ChatHub(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 发送私聊消息
        /// </summary>
        public async Task SendPrivateMessage(string targetUserId, string content)
        {
            // 1. 获取当前用户身份（从 JWT 解析）
            var senderUserId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(senderUserId))
                throw new HubException("用户身份验证失败");

            // 2. 验证接收方是否存在
            var targetUserExists = await _dbContext.Users
                .AnyAsync(u => u.UserId == targetUserId);
            if (!targetUserExists)
                throw new HubException($"接收方用户（{targetUserId}）不存在");

            // 3. 持久化消息
            var message = new Message
            {
                SenderUserId = senderUserId,
                TargetUserId = targetUserId,
                Content = content,
                SendTime = DateTime.Now,
                Status = 1 // 已发送
            };
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            // 4. 推送消息给接收方
            await Clients.User(targetUserId).SendAsync(
                "ReceivePrivateMessage",
                senderUserId,
                content,
                message.SendTime,
                message.Id
            );

            // 5. 向发送方返回“已发送”回执
            await Clients.Caller.SendAsync("MessageSent", message.Id, "消息已发送");
        }

        /// <summary>
        /// 标记消息为已送达
        /// </summary>
        public async Task MarkAsDelivered(Guid messageId)
        {
            var message = await _dbContext.Messages.FindAsync(messageId);
            if (message == null || message.Status >= 2)
                return;

            message.Status = 2;
            await _dbContext.SaveChangesAsync();

            // 通知发送方
            await Clients.User(message.SenderUserId).SendAsync(
                "MessageDelivered", message.Id, "消息已送达");
        }

        /// <summary>
        /// 标记消息为已读
        /// </summary>
        public async Task MarkAsRead(Guid messageId)
        {
            var message = await _dbContext.Messages.FindAsync(messageId);
            if (message == null || message.Status >= 3)
                return;

            message.Status = 3;
            await _dbContext.SaveChangesAsync();

            // 通知发送方
            await Clients.User(message.SenderUserId).SendAsync(
                "MessageRead", message.Id, "消息已读");
        }

        /// <summary>
        /// 连接成功回调
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var username = Context.User?.Identity?.Name;
            Console.WriteLine($"用户 {userId}（{username}）已连接，ConnectionId：{Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 断开连接回调
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            Console.WriteLine($"用户 {userId} 断开连接，原因：{exception?.Message ?? "正常断开"}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}