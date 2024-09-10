using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace ImageScanning.Services
{
    public interface IBlobStorageService
    {
        Task<string> UploadImageAsync(IFormFile image);
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            var blobServiceEndpoint = configuration["AzureBlobStorage:BlobServiceEndpoint"];
            var storageAccountName = configuration["AzureBlobStorage:StorageAccountName"];
            var storageAccountKey = configuration["AzureBlobStorage:StorageAccountKey"];
            var blobContainerName = configuration["AzureBlobStorage:BlobContainerName"];

            var blobServiceClient = new BlobServiceClient(new Uri(blobServiceEndpoint),
                new Azure.Storage.StorageSharedKeyCredential(storageAccountName, storageAccountKey));
            _containerClient = blobServiceClient.GetBlobContainerClient(blobContainerName);
        }

        public async Task<string> UploadImageAsync(IFormFile image)
        {
            // Create the container if it doesn't exist, without specifying PublicAccessType
            await _containerClient.CreateIfNotExistsAsync();

            var blobClient = _containerClient.GetBlobClient(image.FileName);

            using (var stream = image.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = image.ContentType });
            }

            return blobClient.Uri.ToString();
        }
    }
}
