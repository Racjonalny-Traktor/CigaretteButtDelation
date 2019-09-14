using Accord.MachineLearning;
using microserv.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                return Ok();

            var container = GetCloudBlobContainer();
            var blockBlob = container.GetBlockBlobReference($"{data.Id}.jpg");

            using (var stream = pic.OpenReadStream())
                Task.WaitAll(blockBlob.UploadFromStreamAsync(stream));

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

        [HttpGet("geojson")]
        public IActionResult GetAsGeojson()
        {
            var data = _context.Litters.ToArray();

            return Ok(new
            {
                type = "FeatureCollection",
                crs = new
                {
                    type = "name",
                    properties = new
                    {
                        name = "urn:ogc:def:crs:OGC:1.3:CRS84"
                    }
                },
                features = data.Select((x, i) => new
                {
                    type = "Feature",
                    properties = new
                    {
                        id = i,
                        mag = x.CigarettesNum,
                        time = DateTime.UtcNow.ToFileTime(),
                    },
                    geometry = new
                    {
                        type = "Point",
                        coordinates = new[] { x.Lat, x.Long, x.CigarettesNum }
                    }
                })
            });
        }

        [HttpGet("clustered/{count}")]
        public IActionResult GetClustered(int count)
        {
            var data = _context.Litters.ToArray();

            var kMeans = new KMeans(count);
            var preparedData = data.Select(x => new double[] {
                x.Lat.MapCoordToInt(),
                x.Long.MapCoordToInt(),
                x.CigarettesNum }
            ).ToArray();
           
            var clusters = kMeans.Learn(preparedData);

            return Ok(clusters.Centroids);
        }
    }

    public static class CoordsExtensions
    {
        public static int MapCoordToInt(this double x)
        {
            return (int)Math.Round(x * 1000);
        }

        public static double MapIntToCoord(this int x)
        {
            return x / 1000.0;
        }
    }
}
