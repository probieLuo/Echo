using System.ComponentModel.DataAnnotations;

namespace Echo.Server.Entities;

public class User
{
    /// <summary>
    /// 主键
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string PasswordHash { get; set; } = string.Empty;

	/// <summary>
	/// email
	/// </summary>
	[EmailAddress]
	public string Email { get; set; } = string.Empty;

	/// <summary>
	/// 用户唯一标识（用于 SignalR 身份绑定，可自定义格式）
	/// </summary>
	[Required]
    [MaxLength(36)]
    public string UserId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}