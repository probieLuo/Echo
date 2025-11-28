using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Echo.Server.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Echo.Server.DataBaseContext;

namespace Echo.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MyDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public AuthController(MyDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // 1. 验证参数
            if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
                return BadRequest("用户名或密码不能为空");
            if (dto.Username.Length < 3 || dto.Username.Length > 20)
                return BadRequest("用户名长度需 3-20 字符");
            if (dto.Password.Length < 6)
                return BadRequest("密码长度不能少于 6 字符");

            // 2. 检查用户名是否已存在
            var existingUser = await _dbContext.Users
                .AnyAsync(u => u.Username == dto.Username);
            if (existingUser)
                return Conflict("用户名已被占用");

            // 3. 密码加密（BCrypt 算法，自动生成盐值）
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 4. 创建用户
            var user = new User
            {
                Username = dto.Username,
                PasswordHash = passwordHash,
                UserId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.Now
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // 5. 返回结果
            return Ok(new
            {
                Message = "注册成功",
                UserId = user.UserId,
                Username = user.Username
            });
        }

        /// <summary>
        /// 用户登录（返回 JWT Token）
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // 1. 验证参数
            if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
                return BadRequest("用户名或密码不能为空");

            // 2. 查询用户
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null)
                return Unauthorized("用户名或密码错误");

            // 3. 验证密码（BCrypt 解密校验）
            var passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!passwordValid)
                return Unauthorized("用户名或密码错误");

            // 4. 生成 JWT Token
            var token = GenerateJwtToken(user);

            // 5. 返回结果
            return Ok(new
            {
                Token = token,
                UserId = user.UserId,
                Username = user.Username,
                ExpiresIn = 3600 // Token 有效期（秒）：1 小时
            });
        }

		private string GenerateJwtToken(User user)
		{
			var jwtSection = _configuration.GetSection("Jwt");
			var issuer = jwtSection["Issuer"];
			var audience = jwtSection["Audience"];
			var secret = jwtSection["Key"];

			if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
				throw new InvalidOperationException("JWT 配置缺失，请检查 appsettings.json 的 Jwt 节点");

			var claims = new List<Claim>
	            {
		            new Claim(ClaimTypes.NameIdentifier, user.UserId),
		            new Claim(ClaimTypes.Name, user.Username),
		            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
	            };

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: issuer,
				audience: audience,
				claims: claims,
				expires: DateTime.UtcNow.AddHours(1),
				signingCredentials: creds);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}

    /// <summary>
    /// 注册请求 DTO
    /// </summary>
    public class RegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// 登录请求 DTO
    /// </summary>
    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}