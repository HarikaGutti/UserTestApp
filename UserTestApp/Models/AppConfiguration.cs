using Microsoft.Extensions.Configuration;

namespace UserTestApp.Models
{
    public class AppConfiguration : IAppConfiguration
	{
        public AppConfiguration(IConfiguration configuration)
        {
			if (configuration != null)
			{
				UserApiUrl = configuration["UserApiUrl"];
				BucketName = configuration["BucketName"];
				Region = configuration["Region"];
				PageFileDirectory = configuration["PageFileDirectory"];
				Appkey = configuration["AppKey"];
			}
		}
		public string BucketName { get; set; }
		public string Region { get; set; }
		public string UserApiUrl { get; set; }
		public string PageFileDirectory { get; set; }
		public string Appkey { get; set; }
	}
}
