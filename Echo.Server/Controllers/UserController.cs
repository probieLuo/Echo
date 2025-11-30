using Echo.Server.DataBaseContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> GetUsers()
        {
            var result = _dbContext.Users.ToList();

            return Ok(new
            {
                Data=result
            });
        }

		[HttpGet("{userId}")]
		public async Task<IActionResult> GetUserById( string userId)
		{
			var result = _dbContext.Users.FirstOrDefaultAsync(u=>u.UserId.Equals(userId)).Result;

			return Ok(new
			{
				Data = result
			});
		}
	}
}