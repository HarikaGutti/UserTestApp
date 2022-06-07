using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using UserTestApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace UserTestApp.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class UserController : Controller
    {
        readonly IHttpClientFactory _clientFactory;
        readonly IAppConfiguration _configuration;
        private readonly ILogger<UserController> _logger;
        private readonly DataContext _dbContext;
        readonly ICloudService _cloudService;
        public UserController(ILogger<UserController> logger, IAppConfiguration configuration,
                                IHttpClientFactory clientFactory, ICloudService cloudService,
                                DataContext dataContext)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
            _logger = logger;
            _cloudService = cloudService;
            _dbContext = dataContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetUserDetails()
        {
            try
            {
                var client = _clientFactory.CreateClient("windowsAuthClient");
                client.BaseAddress = new Uri(_configuration.UserApiUrl);

                var response = await client.GetAsync("api/MyData");
                if (response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<UserDetails>(resp);
                    return Ok(data);
                }
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError(error);
                return BadRequest(error);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }

        [Consumes("application/json")]
        [Produces("application/json")]
        [HttpPut]
        public async Task<IActionResult> ProcessPageContent(PageContent pagecontent)
        {
            try
            {
                _dbContext.Entry(pagecontent).State = pagecontent.Id == 0 ?
                                                EntityState.Added :
                                                EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                return Ok(pagecontent);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> UploadThePageFiles(List<PageFile> PageFiles)
        {
            try
            {
                foreach (var pageFile in PageFiles)
                {
                    var fileToUpload = _configuration.PageFileDirectory + "\\" + pageFile.FileName;

                    using (var sr = new StreamReader(fileToUpload))
                    {
                        pageFile.Content = sr.ReadToEnd();
                    }
                    var referenceKey = await _cloudService.UploadFileAsync(_configuration.BucketName, fileToUpload, pageFile.Content);

                    if (!string.IsNullOrEmpty(referenceKey))
                    {
                        pageFile.ReferenceKey = referenceKey;
                        _dbContext.Entry(pageFile).State = EntityState.Modified;
                        await _dbContext.SaveChangesAsync();                        
                    }
                }
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest();
            }
        }
    }
}
