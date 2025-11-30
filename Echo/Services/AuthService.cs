using Echo.IServices;
using Echo.Shared;
using Echo.Shared.Dtos;
using System;
using System.Threading.Tasks;

namespace Echo.Services
{
	public class AuthService : IAuthService
	{
		private readonly HttpRestClient client;

		public AuthService(HttpRestClient client)
		{
			this.client = client;
		}
		public async Task<ApiResponse<LoginResponseDto>> Login(string username, string password)
		{
			BaseRequest request = new BaseRequest();
			request.Method = RestSharp.Method.Post;
			request.Route = $"api/auth/login";
			request.Parameter = new LoginDto
			{
				Username = username,
				Password = password
			};

			var response = await client.ExecuteAsync<LoginResponseDto>(request);
			return response;
		}

		public async Task Register(string username, string email, string password)
		{
			throw new NotImplementedException();
		}
	}
}
