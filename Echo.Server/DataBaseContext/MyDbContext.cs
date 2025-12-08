using Echo.Server.Entities;
using Microsoft.EntityFrameworkCore;

namespace Echo.Server.DataBaseContext;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    // 用户表
    public DbSet<User> Users { get; set; }

    // 消息表
    public DbSet<Message> Messages { get; set; }

    // 用户头像表
    public DbSet<UserAvatar> UserAvatars { get; set; }

    /// <summary>
    /// 配置表名和索引（可选）
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 用户名唯一索引
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // 消息表索引（优化查询）
        modelBuilder.Entity<Message>()
            .HasIndex(m => new { m.TargetUserId, m.Status });

        // 头像表：UserId 唯一（每个用户一张头像）
        modelBuilder.Entity<UserAvatar>()
            .HasIndex(a => a.UserId)
            .IsUnique();
    }
}