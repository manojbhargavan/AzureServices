using Azure;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServicesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BlobStorageController : Controller
    {
        private string GetConnectionString(string storageAccountName, string shareAccessSignitureToken)
        {
            return $"BlobEndpoint=https://{storageAccountName}.blob.core.windows.net/;" +
                                        $"SharedAccessSignature={shareAccessSignitureToken}";
        }

        private Pageable<Azure.Storage.Blobs.Models.BlobContainerItem> GetContainers(string storageAccountName, string shareAccessSignitureToken)
        {
            string connectionString = GetConnectionString(storageAccountName, shareAccessSignitureToken);
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            var blobs = blobServiceClient.GetBlobContainers();
            return blobs;
        }

        private dynamic GetContainerItems(string storageAccountName, string shareAccessSignitureToken, string containerName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(GetConnectionString(storageAccountName, shareAccessSignitureToken));
            var blobs = blobServiceClient.GetBlobContainerClient(containerName);
            List<dynamic> items = new List<dynamic>();
            foreach (var item in blobs.GetBlobs())
            {
                items.Add(new { item.Name, item.Properties.ContentType });
            }

            return items;
        }

        //[Authorize]
        [HttpGet]
        [Route("listBlobs")]
        public IActionResult ListContainers(
            [FromServices] ILogger<BlobStorageController> logger,
            [FromQuery] string storageAccountName,
            [FromQuery] string shareAccessSignitureToken
            )
        {
            Pageable<Azure.Storage.Blobs.Models.BlobContainerItem> blobs = GetContainers(storageAccountName, shareAccessSignitureToken);
            List<dynamic> container = new List<dynamic>();

            foreach (var item in blobs)
            {
                container.Add(new { item.Name, item.Properties });
                logger.LogInformation($"{item.Name} found");
            }

            return new JsonResult(container);
        }



        [HttpGet]
        [Route("listBlobsAndContents")]
        public IActionResult ListContainersAndContents(
            [FromServices] ILogger<BlobStorageController> logger,
            [FromQuery] string storageAccountName,
            [FromQuery] string shareAccessSignitureToken
            )
        {
            Pageable<Azure.Storage.Blobs.Models.BlobContainerItem> blobs = GetContainers(storageAccountName, shareAccessSignitureToken);
            List<dynamic> container = new List<dynamic>();

            foreach (var item in blobs)
            {
                container.Add(new { item.Name, item.Properties, Blobs = GetContainerItems(storageAccountName, shareAccessSignitureToken, item.Name) });
                logger.LogInformation($"{item.Name} found");
            }

            return new JsonResult(container);
        }


    }
}
