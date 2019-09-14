﻿using microserv.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace microserv.Controllers
{

    [Route("api/[controller]")]
    public class GoogleController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public GoogleController(DataContext context, IConfiguration configuration)
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

        [HttpPost("try")]
        public IActionResult GooglesEndpoint(object o)
        {

            LogHelper.LogWarning(o.ToString());

            return Ok();
        }

    }
}