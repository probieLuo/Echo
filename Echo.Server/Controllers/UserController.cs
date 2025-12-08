using Echo.Server.DataBaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Echo.Shared;
using Echo.Shared.Dtos.User;
using Echo.Server.Entities;
using System.Security.Claims;
using System.IO;

namespace Echo.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly MyDbContext _dbContext;

        public UserController(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ApiResponse<List<UserDto>>> GetUsers()
        {
            var users = await _dbContext.Users.ToListAsync();

            var result = users.Select(u => new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                CreatedAt = u.CreatedAt
            }).ToList();

            return ApiResponse<List<UserDto>>.Success(result);
        }

        [HttpGet("{userId}")]
        public async Task<ApiResponse<UserDto>> GetUserById(string userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return ApiResponse<UserDto>.Fail("用户不存在");

            var result = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return ApiResponse<UserDto>.Success(result);
        }

        [HttpPut("{userId}")]
        public async Task<ApiResponse<UserDto>> UpdateUser(string userId, UpdateUserDto dto)
        {
            if (dto == null)
                return ApiResponse<UserDto>.Fail("请求体不能为空");

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return ApiResponse<UserDto>.Fail("用户不存在");

            if (!string.IsNullOrWhiteSpace(dto.Username))
            {
                if (dto.Username.Length < 3 || dto.Username.Length > 20)
                    return ApiResponse<UserDto>.Fail("用户名长度需 3-20 字符");

                // check if username taken by other user
                var exists = await _dbContext.Users.AnyAsync(u => u.Username == dto.Username && u.UserId != userId);
                if (exists)
                    return ApiResponse<UserDto>.Fail("用户名已被占用");

                user.Username = dto.Username;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                user.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password.Length < 6)
                    return ApiResponse<UserDto>.Fail("密码长度不能少于 6 字符");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            var updated = new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return ApiResponse<UserDto>.Success(updated, "更新成功");
        }

        /// <summary>
        /// 获取用户头像（公开可访问）
        /// - 如果存有外部 Url，则重定向到该 Url（优先使用缩略图，如果请求 query 包含 size=thumb 且缩略图存在）
        /// - 否则返回二进制数据
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{userId}/avatar")]
        public async Task<IActionResult> GetAvatar(string userId, [FromQuery] string? size)
        {
            var avatar = await _dbContext.UserAvatars.FirstOrDefaultAsync(a => a.UserId == userId);
            if (avatar == null)
                return NotFound();

            // 如果有外部 URL，优先重定向（可根据 size=thumb 使用 ThumbnailUrl）
            if (!string.IsNullOrWhiteSpace(avatar.Url))
            {
                if (!string.IsNullOrWhiteSpace(size) && size.Equals("thumb", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(avatar.ThumbnailUrl))
                {
                    return Redirect(avatar.ThumbnailUrl);
                }

                return Redirect(avatar.Url);
            }

            // 返回二进制数据
            if (avatar.Data == null || string.IsNullOrWhiteSpace(avatar.ContentType))
                return NotFound();

            // 添加简单的缓存头
            Response.Headers["Cache-Control"] = "public,max-age=86400"; // 缓存 1 天

            return File(avatar.Data, avatar.ContentType);
        }

        /// <summary>
        /// 上传或更新用户头像（multipart/form-data 上传图片文件）
        /// 仅允许当前登录用户操作自己的头像
        /// </summary>
        [HttpPost("{userId}/avatar/file")]
        public async Task<ApiResponse> UploadAvatarFile(string userId, IFormFile file)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || currentUserId != userId)
                return ApiResponse.Fail("无权限操作此用户的头像");

            if (file == null || file.Length == 0)
                return ApiResponse.Fail("文件为空");

            // 限制大小 2MB
            const long maxSize = 2 * 1024 * 1024;
            if (file.Length > maxSize)
                return ApiResponse.Fail("文件大小不能超过 2MB");

            if (!file.ContentType.StartsWith("image/"))
                return ApiResponse.Fail("只允许上传图片文件");

            byte[] data;
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                data = ms.ToArray();
            }

            var avatar = await _dbContext.UserAvatars.FirstOrDefaultAsync(a => a.UserId == userId);
            if (avatar == null)
            {
                avatar = new UserAvatar
                {
                    UserId = userId,
                    Data = data,
                    ContentType = file.ContentType,
                    FileName = file.FileName,
                    Size = file.Length,
                    UploadedAt = DateTime.Now,
                    Url = null,
                    ThumbnailUrl = null
                };
                _dbContext.UserAvatars.Add(avatar);
            }
            else
            {
                avatar.Data = data;
                avatar.ContentType = file.ContentType;
                avatar.FileName = file.FileName;
                avatar.Size = file.Length;
                avatar.UploadedAt = DateTime.Now;
                // If previously had Url, clear it because now we store binary
                avatar.Url = null;
                avatar.ThumbnailUrl = null;
                _dbContext.UserAvatars.Update(avatar);
            }

            await _dbContext.SaveChangesAsync();

            var resDto = new AvatarDto { Url = $"/api/user/{userId}/avatar" };
            return ApiResponse.Success(resDto, "上传成功");
        }

        /// <summary>
        /// 保存头像 URL（application/json 提交 { url, thumbnailUrl }）
        /// 仅允许当前登录用户操作自己的头像
        /// </summary>
        [HttpPost("{userId}/avatar/url")]
        public async Task<ApiResponse> UploadAvatarUrl(string userId, [FromBody] AvatarDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || currentUserId != userId)
                return ApiResponse.Fail("无权限操作此用户的头像");

            if (dto == null || string.IsNullOrWhiteSpace(dto.Url))
                return ApiResponse.Fail("Url 不能为空");

            if (!Uri.IsWellFormedUriString(dto.Url, UriKind.Absolute))
                return ApiResponse.Fail("Url 格式不正确");

            var avatar = await _dbContext.UserAvatars.FirstOrDefaultAsync(a => a.UserId == userId);
            if (avatar == null)
            {
                avatar = new UserAvatar
                {
                    UserId = userId,
                    Url = dto.Url,
                    ThumbnailUrl = dto.ThumbnailUrl,
                    Data = null,
                    ContentType = null,
                    FileName = null,
                    Size = 0,
                    UploadedAt = DateTime.Now
                };
                _dbContext.UserAvatars.Add(avatar);
            }
            else
            {
                avatar.Url = dto.Url;
                avatar.ThumbnailUrl = dto.ThumbnailUrl;
                // clear binary to save space if URL is used
                avatar.Data = null;
                avatar.ContentType = null;
                avatar.FileName = null;
                avatar.Size = 0;
                avatar.UploadedAt = DateTime.Now;
                _dbContext.UserAvatars.Update(avatar);
            }

            await _dbContext.SaveChangesAsync();

            var resultDto = new AvatarDto { Url = avatar.Url ?? string.Empty, ThumbnailUrl = avatar.ThumbnailUrl };
            return ApiResponse.Success(resultDto, "头像 URL 保存成功");
        }

        /// <summary>
        /// 删除用户头像，仅允许本人操作
        /// </summary>
        [HttpDelete("{userId}/avatar")]
        public async Task<ApiResponse> DeleteAvatar(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || currentUserId != userId)
                return ApiResponse.Fail("无权限操作此用户的头像");

            var avatar = await _dbContext.UserAvatars.FirstOrDefaultAsync(a => a.UserId == userId);
            if (avatar == null)
                return ApiResponse.Fail("未找到头像");

            _dbContext.UserAvatars.Remove(avatar);
            await _dbContext.SaveChangesAsync();

            return ApiResponse.Success(null, "删除成功");
        }
    }
}