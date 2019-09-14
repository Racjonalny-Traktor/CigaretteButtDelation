using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
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
        public IActionResult GooglesEndpoint([FromBody] GoogleRequest data)
        {
            _logger.LogError(new StreamReader(Request.Body).ReadToEnd());
            _logger.LogError("#####");

            //try
            //{
            //    using (var wc = new WebClient())
            //    {
            //        var link = HttpUtility.HtmlDecode(data.OriginalDetectIntentRequest.Payload.Data.Message.Attachments
            //                       .FirstOrDefault()?.Payload.Url) ?? data.OriginalDetectIntentRequest.Payload.Data.Message.Attachments.FirstOrDefault()?.Payload.Url;
            //        wc.DownloadFile(new Uri(link), $"./pic_{data.ResponseId}");
            //    }
            //    //process the file
            //    //ask for location?
            //    //save 
            //    //display map

                var testAnswer = $"Dialogflow Request for intent {data.QueryResult.FullfillmentText}'";
                var dialogflowResponse = new WebhookResponse
                {
                    FulfillmentText = testAnswer,
                    FulfillmentMessages =
                    { new Intent.Types.Message
                        { SimpleResponses = new Intent.Types.Message.Types.SimpleResponses
                            { SimpleResponses_ =
                                { new Intent.Types.Message.Types.SimpleResponse
                                    {
                                        DisplayText = testAnswer,
                                        TextToSpeech = testAnswer,
                                        //Ssml = $"<speak>{testAnswer}</speak>"
                                    }
                                }
                            }
                        }
                    }
                };
                var jsonResponse = dialogflowResponse.ToString();
                return new ContentResult { Content = jsonResponse, ContentType = "application/json" };

            //}
            //catch (Exception e)
            //{
            //    return BadRequest(new
            //    {
            //        e.Message
            //    });
            //}


            //return Ok();
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
