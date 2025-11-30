using Echo.Server.DataBaseContext;
using Echo.Server.Entities;
using Echo.Shared;
using Echo.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
			if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
				return BadRequest("用户名或密码不能为空");
			if (dto.Username.Length < 3 || dto.Username.Length > 20)
				return BadRequest("用户名长度需 3-20 字符");
			if (dto.Password.Length < 6)
				return BadRequest("密码长度不能少于 6 字符");

			var existingUser = await _dbContext.Users
				.AnyAsync(u => u.Username == dto.Username);
			if (existingUser)
				return Conflict("用户名已被占用");

			var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

			var user = new User
			{
				Username = dto.Username,
				PasswordHash = passwordHash,
				Email = dto.Email,
				UserId = Guid.NewGuid().ToString(),
				CreatedAt = DateTime.Now
			};
			_dbContext.Users.Add(user);
			await _dbContext.SaveChangesAsync();

			return Ok(new ApiResponse
			{
				Message = "注册成功",
				Status = true,
				Result = new
				{
					UserId = user.UserId,
					Username = user.Username
				}
			});
		}

		/// <summary>
		/// 用户登录（返回 JWT Token）
		/// </summary>
		[HttpPost("login")]
		public async Task<ApiResponse<LoginResponseDto>> Login(LoginDto dto)
		{
			if (string.IsNullOrEmpty(dto.Username) || string.IsNullOrEmpty(dto.Password))
				return ApiResponse<LoginResponseDto>.Fail("密码和用户名必填。");

			var user = await _dbContext.Users
				.FirstOrDefaultAsync(u => u.Username == dto.Username);
			if (user == null)
				return ApiResponse<LoginResponseDto>.Fail("用户名或密码错误。");

			var passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
			if (!passwordValid)
				return ApiResponse<LoginResponseDto>.Fail("用户名或密码错误。"); 

			var token = GenerateJwtToken(user);

			return new ApiResponse<LoginResponseDto>
			{
				Status = true,
				Message = "登录成功",
				Result = new LoginResponseDto
				{
					Token = token,
					UserId = user.UserId,
					Username = user.Username
				}
			};
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
}