using System.ComponentModel.DataAnnotations;

namespace Echo.Server.Entities;

public class Message
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 发送者 UserId
    /// </summary>
    [Required]
    [MaxLength(36)]
    public string SenderUserId { get; set; } = string.Empty;

    /// <summary>
    /// 接收者 UserId
    /// </summary>
    [Required]
    [MaxLength(36)]
    public string TargetUserId { get; set; } = string.Empty;

    /// <summary>
    /// 消息内容
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime SendTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 消息状态：0-未发送，1-已发送，2-已送达，3-已读
    /// </summary>
    public int Status { get; set; } = 0;
}