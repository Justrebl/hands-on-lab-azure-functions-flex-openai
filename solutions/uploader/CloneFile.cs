using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;

namespace FuncStd
{
    public class CloneFile
    {
        private readonly ILogger _logger;
        private readonly BlobContainerClient _audioContainerClient;

        public CloneFile(ILoggerFactory loggerFactory, IAzureClientFactory<BlobServiceClient> blobClientFactory)
        {
            _logger = loggerFactory.CreateLogger<CloneFile>();
            var storageAccountContainer = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT_CONTAINER") ?? throw new ArgumentNullException("STORAGE_ACCOUNT_CONTAINER");
            _audioContainerClient = blobClientFactory.CreateClient("audioUploader").GetBlobContainerClient(storageAccountContainer);
            _audioContainerClient.CreateIfNotExists();
        }

        [Function(nameof(CloneFile))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "get")] HttpRequest req,
            string fileName
        )
        {
            _logger.LogInformation("Processing a new audio file upload request");

            // Get the blob in the container named as fileName
            var blobClient = _audioContainerClient.GetBlobClient(fileName);
            var content = await blobClient.OpenReadAsync();

            // Create a new blob and copy the content from the existing blob
            var newBlobName = $"{Guid.NewGuid()}.wav";
            var newBlobClient = _audioContainerClient.GetBlobClient(newBlobName);
            await newBlobClient.UploadAsync(content);

            // Store the file as a blob and return a success response
            return new OkObjectResult("Copied!");
        }

        
    }
}
