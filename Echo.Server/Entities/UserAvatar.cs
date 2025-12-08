using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace Echo.Server.Entities
{
	public class UserAvatar
	{
		/// <summary>
		/// 主键
		/// </summary>
		[Key]
		public Guid Id { get; set; } = Guid.NewGuid();

		/// <summary>
		/// 关联用户的 UserId（非主键 Id）
		/// </summary>
		[Required]
		[MaxLength(36)]
		public string UserId { get; set; } = string.Empty;

		/// <summary>
		/// 图片二进制数据（BLOB）
		/// </summary>
		public byte[]? Data { get; set; }

		/// <summary>
		/// 外部图片地址（优先于 Data 返回，如果设置则 GET 将重定向到该 URL）
		/// </summary>
		[MaxLength(500)]
		public string? Url { get; set; }

		/// <summary>
		/// MIME 类型，例如 image/png
		/// </summary>
		[MaxLength(100)]
		public string? ContentType { get; set; }

		/// <summary>
		/// 原始文件名
		/// </summary>
		[MaxLength(260)]
		public string? FileName { get; set; }

		/// <summary>
		/// 文件大小（字节）
		/// </summary>
		public long Size { get; set; }

		/// <summary>
		/// 上传时间
		/// </summary>
		public DateTime UploadedAt { get; set; } = DateTime.Now;

		/// <summary>
		/// 可选的缩略图 URL（如果使用外部存储或 CDN）
		/// </summary>
		[MaxLength(500)]
		public string? ThumbnailUrl { get; set; }
	}
}
