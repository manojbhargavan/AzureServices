using Azure;
using AzureServiceLibrary.StorageAccount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace AzureServicesAPI.Controllers
{
    /// <summary>
    /// Storage Account Container Client API
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class StorageAccountContainerController : Controller
    {
        /// <summary>
        /// List all the container in the storage account, no exception handling since this is a sample
        /// </summary>
        /// <param name="logger">DI Logger</param>
        /// <param name="storageAccountName">Storage Account Name</param>
        /// <param name="sharedAccessSignatureToken">Shared access signature Token which can be generated from Azure under Storage Account -> Container -> Shared access signature</param>
        /// <returns>Json Object with some basic information about the container</returns>
        //[Authorize]
        [HttpGet]
        [Route("listBlobs")]
        public IActionResult ListContainers(
            [FromServices] ILogger<StorageAccountContainerController> logger,
            [FromQuery] string storageAccountName,
            [FromQuery] string sharedAccessSignatureToken
            )
        {
            ContainerWrapper storageAccountContainerWrapper = new ContainerWrapper(storageAccountName, sharedAccessSignatureToken);
            Pageable<Azure.Storage.Blobs.Models.BlobContainerItem> blobs = storageAccountContainerWrapper.BlobContainerItems;
            List<dynamic> container = new List<dynamic>();

            foreach (var item in blobs)
            {
                container.Add(new { item.Name, item.Properties });
                logger.LogInformation($"{item.Name} found");
            }

            return new JsonResult(container);
        }


        /// <summary>
        /// List all the container and its contents in the storage account, no exception handling since this is a sample
        /// </summary>
        /// <param name="logger">DI Logger</param>
        /// <param name="storageAccountName">Storage Account Name</param>
        /// <param name="sharedAccessSignatureToken">Shared access signature Token which can be generated from Azure under Storage Account -> Container -> Shared access signature</param>
        /// <returns>Json Object with some basic information about the container and its contents</returns>
        [HttpGet]
        [Route("listBlobsAndContents")]
        public IActionResult ListContainersAndContents(
            [FromServices] ILogger<StorageAccountContainerController> logger,
            [FromQuery] string storageAccountName,
            [FromQuery] string sharedAccessSignatureToken
            )
        {
            ContainerWrapper storageAccountContainerWrapper = new ContainerWrapper(storageAccountName, sharedAccessSignatureToken);
            Pageable<Azure.Storage.Blobs.Models.BlobContainerItem> blobs = storageAccountContainerWrapper.BlobContainerItems;
            List<dynamic> container = new List<dynamic>();

            foreach (var item in blobs)
            {
                var containerItems = storageAccountContainerWrapper.GetContainerContents(item.Name);
                List<dynamic> containerItemsList = new List<dynamic>();

                foreach (var containerItem in containerItems)
                {
                    containerItemsList.Add(new { containerItem.Name, containerItem.Properties.LastModified, containerItem.Properties.ContentLength });
                }

                container.Add(new { item.Name, item.Properties, Blobs = containerItemsList });
                logger.LogInformation($"{item.Name} found");
            }

            return new JsonResult(container);
        }

        /// <summary>
        /// Create container
        /// </summary>
        /// <param name="logger">DI Logger</param>
        /// <param name="storageAccountName">Storage Account Name</param>
        /// <param name="sharedAccessSignatureToken">Shared access signature Token which can be generated from Azure under Storage Account -> Container -> Shared access signature</param>
        /// <param name="containerName">Name of the container to create</param>
        /// <returns>Json Object with some basic information about the container and its contents</returns>
        [HttpPost]
        [Route("createContainer")]
        public IActionResult CreateContainer(
            [FromServices] ILogger<StorageAccountContainerController> logger,
            [FromQuery] string storageAccountName,
            [FromQuery] string sharedAccessSignatureToken,
            [FromQuery] string containerName
            )
        {
            try
            {
                ContainerWrapper storageAccountContainerWrapper = new ContainerWrapper(storageAccountName, sharedAccessSignatureToken);
                var result = storageAccountContainerWrapper.CreateContainer(containerName);
                return new CreatedResult(containerName, result);
            }
            catch(Exception ex)
            {
                logger.LogError($"Error creating container {containerName}, {ex.Message}");
                throw ex;
            }
        }

    }
}
