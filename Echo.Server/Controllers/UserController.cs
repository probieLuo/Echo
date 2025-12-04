using Echo.Server.DataBaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Echo.Shared;
using Echo.Shared.Dtos.User;

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
    }
}