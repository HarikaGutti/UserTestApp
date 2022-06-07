using System.Threading.Tasks;

namespace UserTestApp.Services
{
    public interface ICloudService
    {
        Task<string> GetToken(string appkey);
        Task<string> UploadFileAsync(string bucketName, string fileToUpload, string content);
    }
}