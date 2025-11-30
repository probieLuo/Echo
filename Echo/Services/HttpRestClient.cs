using Echo.Shared;
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
			client = new RestClient();
		}
		public async Task<ApiResponse> ExecuteAsync(BaseRequest baseRequest)
		{
			var request = new RestRequest(apiUrl + baseRequest.Route);
			request.Method = baseRequest.Method;
			request.AddHeader("ContentType", baseRequest.ContentType);
			if (baseRequest.Parameter != null)
				request.AddJsonBody(JsonConvert.SerializeObject(baseRequest.Parameter));
			var response = await client.ExecuteAsync(request);
			return JsonConvert.DeserializeObject<ApiResponse>(response.Content);
		}
		public async Task<ApiResponse<T>> ExecuteAsync<T>(BaseRequest baseRequest)
		{
			var request = new RestRequest(apiUrl + baseRequest.Route);
			request.Method = baseRequest.Method;
			request.AddHeader("ContentType", baseRequest.ContentType);
			if (baseRequest.Parameter != null)
				request.AddJsonBody(JsonConvert.SerializeObject(baseRequest.Parameter));
			var response = await client.ExecuteAsync(request);
			return JsonConvert.DeserializeObject<ApiResponse<T>>(response.Content);
		}
	}
}
