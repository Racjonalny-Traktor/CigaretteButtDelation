using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Web;
using Google.Apis.Auth;
using Google.Cloud.Dialogflow.V2;
using microserv.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

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

        private object GetBlobBlockForFile(string filename) => GetCloudBlobContainer().GetBlockBlobReference($"{filename}");
        

        [HttpPost("try")]
        public IActionResult GooglesEndpoint()
        {
            var json = new StreamReader(Request.Body).ReadToEnd();
            _logger.LogWarning(json);

            var data1 = JsonConvert.DeserializeObject<GoogleRequest>(json);
            var data2 = JsonConvert.DeserializeObject<WebhookRequest>(json);

            var attachments = data1.OriginalDetectIntentRequest.Payload.Data.Message.Attachments;
            //image is sent
            if (attachments.Any() &&
                attachments.First().Type
                    .Equals("image", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogInformation("pic is sent, creating");

                var picUrl = attachments.First().Payload.Url;

                var entity = new Litter
                {
                    UserId = data1.OriginalDetectIntentRequest.Payload.Data.Sender.Id,
                    ImageUrl = picUrl
                };

                _context.Litters.Add(entity);
                _context.SaveChanges();
            }
            else //location is sent
            {
                _logger.LogInformation("Location sent, updating ");

                var coords = data2.QueryResult.OutputContexts
                    .FirstOrDefault(x => x.Name.Contains("location", StringComparison.InvariantCultureIgnoreCase));
                if (coords == null) return BadRequest();

                var lat = coords.Parameters.Fields["lat"].NumberValue;
                var lon = coords.Parameters.Fields["long"].NumberValue;

                var userId = data1.OriginalDetectIntentRequest.Payload.Data.Sender.Id;

                //litters without lat/long/num saved
                var litters = _context.Litters
                    .Where(x => x.UserId == userId && x.CigarettesNum <= 0)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToArray();
                var litter = litters.FirstOrDefault();
                var toDelete = litters.Skip(1);

                _context.Litters.RemoveRange(litters);

                litter.CigarettesNum = 1;
                litter.Lat = lat;
                litter.Long = lon;

                _context.Litters.Update(litter);
                _context.SaveChanges();
            }

                //var raw = new JsonSerializer().Deserialize<WebhookRequest>(new BsonReader(Request.Body));

            //    //process the file
            //    //ask for location?
            //    //save 
            //    //display map

            return Ok();
        }

        public class GoogleRequest
        {
            public string ResponseId { get; set; }
            public GoogleQueryResult QueryResult { get; set; }
            public GoogleOriginalDetectIntentRequest OriginalDetectIntentRequest { get; set; }
            public string Session { get; set; }
        }

        public class GoogleQueryResult
        {
            public string QueryText { get; set; }
            public string FullfillmentText { get; set; }
        }

        public class GoogleOriginalDetectIntentRequest
        {
            public string Source { get; set; }
            public GooglePayload Payload { get; set; }
        }

        public class GooglePayload
        {
            public GooglePayloadData Data { get; set; }
            public string Source { get; set; }
        }

        public class GooglePayloadData
        {
            public GoogleRecipent Recipent { get; set; }
            public GoogleMessage Message { get; set; }
            public double Timestamp { get; set; }
            public GoogleSender Sender { get; set; }
        }

        public class GoogleRecipent
        {
            public string Id { get; set; }
        }

        public class GoogleMessage
        {
            public string Mid { get; set; }
            public GoogleAttachment[] Attachments { get; set; }
        }

        public class GoogleAttachment
        {
            public GoogleAttachmentPayload Payload { get; set; }
            public string Type { get; set; }
        }

        public class GoogleAttachmentPayload
        {
            public string Url { get; set; }
        }

        public class GoogleSender
        {
            public string Id { get; set; }
        }


    }
}
