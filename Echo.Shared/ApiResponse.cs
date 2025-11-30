namespace Echo.Shared
{
	public class ApiResponse
	{
		public string Message { get; set; }

		public bool Status { get; set; }

		public object Result { get; set; }

		public static ApiResponse Fail(string msg)
		{
			return new ApiResponse
			{
				Status = false,
				Message = msg
			};
		}

		public static ApiResponse Success(object result, string msg = "成功")
		{
			return new ApiResponse
			{
				Status = true,
				Message = msg,
				Result = result
			};
		}
	}

	public class ApiResponse<T>
	{
		public string Message { get; set; }

		public bool Status { get; set; }

		public T Result { get; set; }

		public static ApiResponse<T> Fail(string msg)
		{
			return new ApiResponse<T>
			{
				Status = false,
				Message = msg
			};
		}

		public static ApiResponse<T> Success(T result, string msg = "成功")
		{
			return new ApiResponse<T>
			{
				Status = true,
				Message = msg,
				Result = result
			};
		}
	}
}
