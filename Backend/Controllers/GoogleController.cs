using System;
using System.IO;
using System.Linq;
using Google.Cloud.Dialogflow.V2;
using microserv.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace microserv.Controllers
{

    [Route("api/[controller]")]
    public class GoogleController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger _logger;
        private const StringComparison _stringcompare = StringComparison.InvariantCultureIgnoreCase;

        public GoogleController(DataContext context, ILogger<GoogleController> logger)
        {
            _context = context;
            _logger = logger;
        }

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
            _logger.LogWarning(json);   //display json for debug purposes

            var data1 = JsonConvert.DeserializeObject<GoogleRequest>(json); // my own type
            var data2 = JsonConvert.DeserializeObject<WebhookRequest>(json); // type from google lib that doesnt work

            try
            {
                return HandleChatbotRequest(data1);
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e);
            }
        }

        private IActionResult HandleChatbotRequest(GoogleRequest data1)
        {
            var source = data1.OriginalDetectIntentRequest.Source;

            if (source.Equals("facebook", _stringcompare))
                return ResponseForFacebook(data1);
            else if (source.Equals("google", _stringcompare))
                return ResponseForGoogleAssistant();
            else
                throw new NotImplementedException("that source is not implemented");
        }

        private IActionResult ResponseForGoogleAssistant()
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

        private IActionResult ResponseForFacebook(GoogleRequest data1)
        {
            _logger.LogInformation("facebook");

            var attachments = data1?.OriginalDetectIntentRequest?.Payload?.Data?.Message?.Attachments;
            if (attachments != null && attachments.Any(x => x.Type.Equals("image", _stringcompare)))
            {
                _logger.LogInformation("pic is sent, creating");

                var entity = new Litter
                {
                    UserId = data1.OriginalDetectIntentRequest.Payload.Data.Sender.Id,
                    ImageUrl = attachments.First().Payload.Url
                };

                _context.Litters.Add(entity);
                _context.SaveChanges();

                return ResponseShareLocation();
            }
            else if (data1?.OriginalDetectIntentRequest?.Payload?.Data?.Postback?.Payload != null
                     && data1.OriginalDetectIntentRequest.Payload.Data.Postback.Payload.Contains("LOCATION", _stringcompare))
            {
                _logger.LogInformation("Location is sent, updating ");

                var coords = data1?.OriginalDetectIntentRequest?.Payload?.Data?.Postback?.Data;
                if (coords == null)
                    return BadRequest();

                var userId = data1.OriginalDetectIntentRequest.Payload.Data.Sender.Id;

                var litters = _context.Litters   //litters without lat/long/num saved
                    .Where(x => x.UserId == userId && x.CigarettesNum <= 0)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToArray();

                var litter = litters.FirstOrDefault();  //newest one
                if (litter != null)
                {
                    _context.Litters.RemoveRange(litters.Skip(1));

                    litter.CigarettesNum = 1;
                    litter.Lat = coords.Long;
                    litter.Long = coords.Lat;

                    _context.Litters.Update(litter);
                    _context.SaveChanges();

                    return ResponseThankYou(userId);
                }
            }

            return ResponseUploadPhoto();
        }

        private static IActionResult ResponseShareLocation()
        {
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
            return new ContentResult { Content = jsonResponse, ContentType = "application/json" };
        }

        private IActionResult ResponseThankYou(string userId)
        {
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
            return new ContentResult { Content = jsonResponse, ContentType = "application/json" };
        }

        private static IActionResult ResponseUploadPhoto()
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
            return new ContentResult { Content = jsonResponse, ContentType = "application/json" };
        }
    }
}
