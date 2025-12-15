using System;
using System.Collections.Generic;
using System.Text;

namespace Echo.Services.IServices
{
	public interface IAppConfig
	{
		bool IsDevelopment { get; set; }
		DbConfig Database { get; set; }

		void SaveConfig();
		void LoadConfig();
	}

	public class DbConfig
	{
		public string ConnectionString { get; set; } = "Data Source=echo.db;";
	}
}
