using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Echo.SignalR
{
    public class SignalRClient : IAsyncDisposable
    {
        private readonly string _hubUrl;
        private readonly Func<Task<string>> _accessTokenProvider;
        private HubConnection? _connection;

        public event Action<string, string, DateTime, Guid>? ReceivePrivateMessage;
        public event Action<Guid, string>? MessageSent;
        public event Action<Guid, string>? MessageDelivered;
        public event Action<Guid, string>? MessageRead;
        public event Action<Exception?>? ConnectionClosed;
        public event Action<string>? Reconnecting;
        public event Action<string>? Reconnected;

        public SignalRClient(string hubUrl, Func<Task<string>> accessTokenProvider)
        {
            _hubUrl = hubUrl ?? throw new ArgumentNullException(nameof(hubUrl));
            _accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_connection != null && _connection.State == HubConnectionState.Connected)
                return;

            _connection = new HubConnectionBuilder()
                .WithUrl(_hubUrl, options =>
                {
                    options.AccessTokenProvider = _accessTokenProvider;

					options.HttpMessageHandlerFactory = handler =>
					{
						if (handler is HttpClientHandler clientHandler)
							clientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
						return handler;
					};
				})
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string, string, DateTime, Guid>("ReceivePrivateMessage", (senderUserId, content, sendTime, messageId) =>
            {
                ReceivePrivateMessage?.Invoke(senderUserId, content, sendTime, messageId);
                // 默认自动标记送达（可按需关闭）
#pragma warning disable CS4014
                MarkAsDeliveredAsync(messageId);
#pragma warning restore CS4014
            });

            _connection.On<Guid, string>("MessageSent", (messageId, info) => MessageSent?.Invoke(messageId, info));
            _connection.On<Guid, string>("MessageDelivered", (messageId, info) => MessageDelivered?.Invoke(messageId, info));
            _connection.On<Guid, string>("MessageRead", (messageId, info) => MessageRead?.Invoke(messageId, info));

            _connection.Reconnecting += ex =>
            {
                Reconnecting?.Invoke(ex?.Message ?? "Reconnecting");
                return Task.CompletedTask;
            };
            _connection.Reconnected += connectionId =>
            {
                Reconnected?.Invoke(connectionId);
                return Task.CompletedTask;
            };
            _connection.Closed += ex =>
            {
                ConnectionClosed?.Invoke(ex);
                return Task.CompletedTask;
            };

            await _connection.StartAsync(cancellationToken);
        }

        public Task StopAsync() => _connection?.StopAsync() ?? Task.CompletedTask;

        public Task SendPrivateMessageAsync(string targetUserId, string content)
        {
            if (_connection == null) throw new InvalidOperationException("SignalR connection is not started.");
            return _connection.SendAsync("SendPrivateMessage", targetUserId, content);
        }

        public Task MarkAsDeliveredAsync(Guid messageId)
        {
            if (_connection == null) throw new InvalidOperationException("SignalR connection is not started.");
            return _connection.SendAsync("MarkAsDelivered", messageId);
        }

        public Task MarkAsReadAsync(Guid messageId)
        {
            if (_connection == null) throw new InvalidOperationException("SignalR connection is not started.");
            return _connection.SendAsync("MarkAsRead", messageId);
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
    }
}
