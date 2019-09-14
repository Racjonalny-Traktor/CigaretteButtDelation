using microserv.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Linq;

namespace microserv.Controllers
{
    [Route("api/[controller]")]
    public class LitterController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        public LitterController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private CloudBlobContainer GetCloudBlobContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(
                _configuration.GetConnectionString("storage"));
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("pics");
            return container;
        }

        [HttpGet("")]
        public IActionResult GetAllLitter()
        {
            return Ok(_context.Litters.ToArray());
        }
    }
}
