using Echo.IServices;
using Echo.Shared;
using Echo.Shared.Dtos.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Echo.Services
{
	public class UserService : IUserService
	{
		private readonly HttpRestClient client;

		public UserService(HttpRestClient client)
		{
			this.client = client;
		}

		public async Task<ApiResponse<List<UserDto>>> GetUsers()
		{
			var request = new BaseRequest
			{
				Method = RestSharp.Method.Get,
				Route = "api/User"
			};

			return await client.ExecuteAsync<List<UserDto>>(request);
		}

		public async Task<ApiResponse<UserDto>> GetUserById(string userId)
		{
			var request = new BaseRequest
			{
				Method = RestSharp.Method.Get,
				Route = $"api/user/{userId}"
			};

			return await client.ExecuteAsync<UserDto>(request);
		}

		public async Task<ApiResponse<UserDto>> UpdateUser(string userId, UpdateUserDto dto)
		{
			var request = new BaseRequest
			{
				Method = RestSharp.Method.Put,
				Route = $"api/user/{userId}",
				Parameter = dto
			};

			return await client.ExecuteAsync<UserDto>(request);
		}
	}
}
