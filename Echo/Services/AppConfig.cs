using Echo.Services.IServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Echo.Services
{
	internal class AppConfig : IAppConfig
	{
		private const string ConfigFileName = "appconfig.yaml";
		public static AppConfig Instance => new AppConfig();
		public bool IsDevelopment { get; set; } = true;
		public DbConfig Database { get; set; } = new DbConfig();

		private AppConfig()
		{
			LoadConfig();
		}

		public void LoadConfig()
		{
			try
			{
				var path = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
				if (!File.Exists(path))
				{
					SaveConfig();
				}

				var yaml = File.ReadAllText(path, Encoding.UTF8);

				var deserializer = new DeserializerBuilder()
					.WithNamingConvention(CamelCaseNamingConvention.Instance)
					.IgnoreUnmatchedProperties()
					.Build();

				var tmp = deserializer.Deserialize<AppConfig>(yaml);
				if (tmp != null)
				{
					// Map deserialized values onto the current instance
					IsDevelopment = tmp.IsDevelopment;
					Database = tmp.Database ?? new DbConfig();
				}
			}
			catch(Exception e)
			{

			}
		}

		public void SaveConfig()
		{
			try
			{
				var path = Path.Combine(AppContext.BaseDirectory, ConfigFileName);

				var serializer = new SerializerBuilder()
					.WithNamingConvention(CamelCaseNamingConvention.Instance)
					.Build();

				var yaml = serializer.Serialize(this);

				File.WriteAllText(path, yaml, Encoding.UTF8);
			}
			catch (Exception e)
			{

			}
		}
	}
}
