using System;
using System.Collections.Generic;
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

        public GoogleController(DataContext context, IConfiguration configuration, ILogger<GoogleController> logger)
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
            //dialogflow api is broken
            //had to do my own classes
            //built-in WebhookRequest/Response are lacking all of the data
            //messenger has limited its functionality with buttons last month
            //google location doesn't even work
            //fml

            var json = new StreamReader(Request.Body).ReadToEnd();
            _logger.LogWarning(json);

            var data1 = JsonConvert.DeserializeObject<GoogleRequest>(json);
            var data2 = JsonConvert.DeserializeObject<WebhookRequest>(json);

            if (data1.OriginalDetectIntentRequest.Source.Equals("facebook", StringComparison.InvariantCultureIgnoreCase)
            )
            {

                var attachments = data1?.OriginalDetectIntentRequest?.Payload?.Data?.Message?.Attachments;
                //image is sent
                if (attachments != null
                    && attachments.Any()
                    && attachments.First().Type.Equals("image", StringComparison.InvariantCultureIgnoreCase))
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

                    var reponseText = "Please share your location with us so we know where those cigarette butts are";
                    var dialogflowResponse = new WebhookResponse
                    {
                        FulfillmentText = reponseText,
                        FulfillmentMessages =
                        {
                            new Intent.Types.Message
                            {
                                SimpleResponses = new Intent.Types.Message.Types.SimpleResponses
                                {
                                    SimpleResponses_ =
                                    {
                                        new Intent.Types.Message.Types.SimpleResponse
                                        {
                                            DisplayText = reponseText,
                                            TextToSpeech = reponseText,
                                        }
                                    }
                                }
                            }
                        }
                    };
                    var jsonResponse = dialogflowResponse.ToString();
                    return new ContentResult {Content = jsonResponse, ContentType = "application/json"};
                    ;
                }
                else if (data1?.OriginalDetectIntentRequest?.Payload?.Data?.Postback?.Payload != null
                         && data1.OriginalDetectIntentRequest.Payload.Data.Postback.Payload.Contains("LOCATION",
                             StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogInformation("Location sent, updating ");

                    var coords = data1?.OriginalDetectIntentRequest?.Payload?.Data?.Postback?.Data;
                    if (coords == null) return BadRequest();

                    var userId = data1.OriginalDetectIntentRequest.Payload.Data.Sender.Id;

                    //litters without lat/long/num saved
                    var litters = _context.Litters
                        .Where(x => x.UserId == userId && x.CigarettesNum <= 0)
                        .OrderByDescending(x => x.CreatedAt)
                        .ToArray();
                    var litter = litters.FirstOrDefault();
                    if (litter != null)
                    {

                        var toDelete = litters.Skip(1);

                        _context.Litters.RemoveRange(litters);

                        litter.CigarettesNum = 1;
                        litter.Lat = coords.Long;
                        litter.Long = coords.Lat;

                        _context.Litters.Update(litter);
                        _context.SaveChanges();



                        var reportsnum =
                            _context.Litters.Count(x => x.UserId == userId && x.CreatedAt.Month == DateTime.Now.Month);

                        var nth =
                            reportsnum % 10 == 1 ? "st" :
                            reportsnum % 10 == 2 ? "nd" :
                            reportsnum % 10 == 3 ? "rd" : "th";
                        var reponseText =
                            $"Thank you for taking care of the Earth! We'll use this data to take care of cigarette butt littering problem! This was your {reportsnum}{nth} delation :)' \n You can see them now on the map http://fajeczky.azurewebsites.net/";
                        var dialogflowResponse = new WebhookResponse
                        {
                            FulfillmentText = reponseText
                        };


                        var jsonResponse = dialogflowResponse.ToString();
                        return new ContentResult {Content = jsonResponse, ContentType = "application/json"};

                    }
                }

                {
                    var reponseText = "Please take a photo first";
                    var dialogflowResponse = new WebhookResponse
                    {
                        FulfillmentText = reponseText,
                        FulfillmentMessages =
                        {
                            new Intent.Types.Message
                            {
                                SimpleResponses = new Intent.Types.Message.Types.SimpleResponses
                                {
                                    SimpleResponses_ =
                                    {
                                        new Intent.Types.Message.Types.SimpleResponse
                                        {
                                            DisplayText = reponseText,
                                            TextToSpeech = reponseText,
                                        }
                                    }
                                }
                            }
                        }
                    };
                    var jsonResponse = dialogflowResponse.ToString();
                    return new ContentResult {Content = jsonResponse, ContentType = "application/json"};

                }
            }

            else if (data1.OriginalDetectIntentRequest.Source.Equals("google",
                StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogInformation("google");
                var res = new
                {
                    expectedInputs = new[]
                    {
                        new
                        {
                            possibleIntents = new []
                            {
                                new
                                {
                                    intent = "actions.intent.PERMISSION",
                                    inputValueData = new
                                    {
                                        @type = "type.googleapis.com/google.actions.v2.PermissionValueSpec",
                                        permissions = new [] {"NAME", "DEVICE_PRECISE_LOCATION" }
                                    },
                                    optContext = "to locate you"
                                }
                            }
                        }
                    }
                };
                var x = new WebhookResponse()
                {
                    FollowupEventInput = new EventInput() { Name = "DEVICE_PRECISE_LOCATION" }
                };
                var jsonResponse = res.ToString();
                return new ContentResult { Content = jsonResponse, ContentType = "application/json" };
            }

            return Ok();

            //    //process the file
            //    //ask for location?
            //    //save 
            //    //display map

        }
        #region Model

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
            public GooglePostback Postback { get; set; }
        }

        public class GooglePostback
        {
            public string Payload { get; set; }
            public GoogleCoords Data { get; set; }
        }

        public class GoogleCoords
        {
            public double Lat { get; set; }
            public double Long { get; set; }
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

        #endregion model
    }
}
