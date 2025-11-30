using Echo.Shared;
using Echo.Shared.Dtos;
using System.Threading.Tasks;

namespace Echo.IServices
{
	public interface IAuthService
	{
		Task<ApiResponse<LoginResponseDto>> Login(string username, string password);
		Task Register(string username, string email, string password);
	}
}
