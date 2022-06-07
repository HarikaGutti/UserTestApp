using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserTestApp.Models
{
    public class PostObjectRequest
    {
        public string BucketName { get; set; }
        public string Key { get; set; }
        public string ContentBody { get; set; }
    }
}
