using microserv.Models;
using Microsoft.AspNetCore.Http;
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

        [HttpPost("")]
        public IActionResult Delate(
            [FromForm] IFormFile pic,
            [FromForm] Litter data)
        {

            _context.Litters.Add(data);
            _context.SaveChanges();

            
            if (pic.Length <= 0)
                return BadRequest("File error");

            var container = GetCloudBlobContainer();
            var blockBlob = container.GetBlockBlobReference($"{data.Id}.jpg");

            using (var stream = pic.OpenReadStream()) 
                blockBlob.UploadFromStreamAsync(stream);

            data.ImageUrl = blockBlob.SnapshotQualifiedStorageUri.PrimaryUri.ToString();
            _context.Litters.Update(data);
            _context.SaveChanges();
  

            return Ok();
        }

        [HttpGet("")]
        public IActionResult GetAllLitter()
        {
            return Ok(_context.Litters.ToArray());
        }
    }
}
