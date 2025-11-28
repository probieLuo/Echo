using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly MyDbContext _dbContext;

        public UserController(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 查询
        /// </summary>
        [HttpPost("All")]
        public async Task<IActionResult> GetAll()
        {
            var result = _dbContext.Users.ToList();

            return Ok(new
            {
                Data=result
            });
        }
    }
}