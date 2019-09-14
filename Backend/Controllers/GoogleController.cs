using System.IO;
using microserv.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace microserv.Controllers
{

    [Route("api/[controller]")]
    public class GoogleController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public GoogleController(DataContext context, IConfiguration configuration, ILogger<GoogleController> logger )
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }


        private CloudBlobContainer GetCloudBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(
                _configuration.GetConnectionString("storage"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("pics");
            return container;
        }

        [HttpPost("try")]
        public IActionResult GooglesEndpoint()
        {
            _logger.LogError(new StreamReader(Request.Body).ReadToEnd());
            _logger.LogError(JsonConvert.SerializeObject(Request.Headers));
            _logger.LogError($"cl:{Request.ContentLength}, ct:{Request.ContentType}, host:{Request.Host}");
            _logger.LogError("#####");
            return Ok();
        }
         
    }
}
