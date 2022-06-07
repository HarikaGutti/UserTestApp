using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Json;
using System.Threading.Tasks;
using UserTestApp.Models;

namespace UserTestApp.Services
{
    public class CloudService : ICloudService
    {
        readonly string _cloudApiUrl;
        readonly string _appKey;
        readonly ILogger<CloudService> _logger;

        public CloudService(ILogger<CloudService> logger, IConfiguration configuration)
        {
            if (configuration != null)
            {
                _cloudApiUrl = configuration["cloudApiUrl"];
                _appKey = configuration["AppKey"];
            }
            _logger = logger;
        }

        public async Task<string> GetToken(string appKey)
        {
            using (var client = new HttpClient())
            {
                var form = new Dictionary<string, string>
               {
                   {"appKey", appKey}
               };
                var tokenResponse = await client.PostAsync(_cloudApiUrl + "/oauth/token", new FormUrlEncodedContent(form));
                var token = await tokenResponse.Content.ReadAsAsync<Token>(new[] { new JsonMediaTypeFormatter() });
                if (string.IsNullOrEmpty(token.Error))
                {
                    _logger.LogInformation("Token issued is: {0}", token.AccessToken);
                    return token.AccessToken;
                }
                else
                {
                    _logger.LogError("Error encoutered while fetching the token : {0}", token.Error);

                }
            }
            return string.Empty;

        }
        public async Task<string> UploadFileAsync(string bucketName, string fileToUpload, string contentBody)
        {
            try
            {
                var token = await GetToken(_appKey);
                if (!string.IsNullOrEmpty(token))
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(_cloudApiUrl);
                        client.DefaultRequestHeaders.Add("token", token);
                        var postRequest = new PostObjectRequest
                        {
                            BucketName = bucketName,
                            Key = fileToUpload,
                            ContentBody = contentBody
                        };
                        var response = await client.PostAsync("api/postData", JsonContent.Create(postRequest));

                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation($"Uploaded { fileToUpload}.");
                            var referenceId = await response.Content.ReadAsStringAsync();
                            return referenceId;
                        }
                    }
                }
                return string.Empty;
            }
            catch (Exception e)
            {
                _logger.LogError(
                    "Error encountered when writing an object"
                    , e.Message);
                throw;
            }

        }
    }
}