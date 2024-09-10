using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImageScanning.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IComputerVisionService _computerVisionService;
        private readonly IMemoryCache _cache;

        public HomeController(IBlobStorageService blobStorageService, IComputerVisionService computerVisionService, IMemoryCache cache)
        {
            _blobStorageService = blobStorageService;
            _computerVisionService = computerVisionService;
            _cache = cache;
        }

        // GET: Home/Index
        public IActionResult Index()
        {
            return View();
        }

        // POST: Home/UploadImage
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                // Step 1: Upload Image to Blob Storage
                var imageUrl = await _blobStorageService.UploadImageAsync(image);

                // Step 2: Process Image using Computer Vision API
                var description = await _computerVisionService.GetImageDescriptionAsync(imageUrl);

                // Step 3: Cache the response
                _cache.Set(imageUrl, description, TimeSpan.FromMinutes(10));

                // Step 4: Pass the Image URL and Description to the View
                ViewBag.ImageUrl = imageUrl;
                ViewBag.Description = description;

                return View("Index");
            }

            ViewBag.Message = "Please select an image to upload.";
            return View("Index");
        }
    }

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
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = _containerClient.GetBlobClient(image.FileName);

            using (var stream = image.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = image.ContentType });
            }

            return blobClient.Uri.ToString();
        }
    }

    public interface IComputerVisionService
    {
        Task<string> GetImageDescriptionAsync(string imageUrl);
    }

    public class ComputerVisionService : IComputerVisionService
    {
        private readonly ComputerVisionClient _client;

        public ComputerVisionService(IConfiguration configuration)
        {
            var endpoint = configuration["ComputerVision:Endpoint"];
            var apiKey = configuration["ComputerVision:ApiKey"];

            _client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(apiKey))
            {
                Endpoint = endpoint
            };
        }

        public async Task<string> GetImageDescriptionAsync(string imageUrl)
        {
            var descriptionResult = await _client.DescribeImageAsync(imageUrl);
            return descriptionResult.Captions.Count > 0 ? descriptionResult.Captions[0].Text : "No description available.";
        }
    }
}
