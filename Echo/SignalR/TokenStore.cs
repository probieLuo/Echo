namespace Echo.SignalR
{
    /// <summary>
    /// 进程级 Token 与当前用户信息持有者
    /// </summary>
    public static class TokenStore
    {
        public static string? CurrentToken { get; set; }
        public static string? CurrentUserId { get; set; }
    }
}
