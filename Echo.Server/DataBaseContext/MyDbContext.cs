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
    }
}