using Echo.Shared;
using Echo.SignalR;
using Newtonsoft.Json;
using RestSharp;
using System.Threading.Tasks;

namespace Echo.Services
{
	public class HttpRestClient
	{
		private readonly string apiUrl;
		protected readonly RestClient client;
		public HttpRestClient(string apiUrl)
		{
			this.apiUrl = apiUrl;
			var options = new RestClientOptions(apiUrl)
			{
				Timeout = null,
			};
			client = new RestClient(options);
		}
		public async Task<ApiResponse> ExecuteAsync(BaseRequest baseRequest)
		{
			var request = new RestRequest(baseRequest.Route, baseRequest.Method);
			request.AddHeader("Authorization", $"Bearer {TokenStore.CurrentToken}");
			request.AddHeader("Content-Type", baseRequest.ContentType);
			if (baseRequest.Parameter != null)
				request.AddJsonBody(JsonConvert.SerializeObject(baseRequest.Parameter));
			var response = await client.ExecuteAsync(request);
			if (!response.IsSuccessful)
				return ApiResponse.Fail($"请求失败，StatusCode：{response.StatusCode}，ErrorMessage：{response.ErrorMessage}");
			return JsonConvert.DeserializeObject<ApiResponse>(response.Content);
		}
		public async Task<ApiResponse<T>> ExecuteAsync<T>(BaseRequest baseRequest)
		{
			var request = new RestRequest(baseRequest.Route, baseRequest.Method);
			request.AddHeader("Authorization", $"Bearer {TokenStore.CurrentToken}");
			request.AddHeader("Content-Type", baseRequest.ContentType);
			if (baseRequest.Parameter != null)
				request.AddJsonBody(JsonConvert.SerializeObject(baseRequest.Parameter));
			var response = await client.ExecuteAsync(request);
			if(!response.IsSuccessful)
				return ApiResponse<T>.Fail($"请求失败，StatusCode：{response.StatusCode}，ErrorMessage：{response.ErrorMessage}");
			return JsonConvert.DeserializeObject<ApiResponse<T>>(response.Content);
		}

		public async Task<ApiResponse<T>> ExecuteAsyncV1<T>(BaseRequest baseRequest)
		{
			var options = new RestClientOptions("https://localhost:7099/")
			{
				Timeout = null,
			};
			var client = new RestClient(options);
			var request = new RestRequest("api/User", baseRequest.Method); 
			request.AddHeader("Authorization", $"Bearer {TokenStore.CurrentToken}");
			var response = await client.ExecuteAsync(request);
			if (!response.IsSuccessful)
				return ApiResponse<T>.Fail($"请求失败，StatusCode：{response.StatusCode}，ErrorMessage：{response.ErrorMessage}");
			return JsonConvert.DeserializeObject<ApiResponse<T>>(response.Content);
		}
	}
}
