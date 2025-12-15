using Echo.Shared;
using Echo.Shared.Dtos.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Echo.IServices
{
	public interface IUserService
	{
		Task<ApiResponse<List<UserDto>>> GetUsers();
		Task<ApiResponse<UserDto>> GetUserById(string userId);
		Task<ApiResponse<UserDto>> UpdateUser(string userId, UpdateUserDto dto);
	}
}
