using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserTestApp.Models
{
	public interface IAppConfiguration
	{
		public string BucketName { get; set; }
		public string Region { get; set; }
		public string UserApiUrl { get; set; }
		public string PageFileDirectory { get; set; }
		public string Appkey { get; set; }
	}
}
