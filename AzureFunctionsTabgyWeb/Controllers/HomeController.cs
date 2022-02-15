using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureFunctionsTabgyWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;


namespace AzureFunctionsTabgyWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        static readonly HttpClient client = new HttpClient();
        private readonly BlobServiceClient _blobServiceClient;

        public HomeController(ILogger<HomeController> logger, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        //http://localhost:7071/api/OnSalesUploadWriteToQueue

        [HttpPost]
        public async Task<IActionResult> Index(SalesRequest salesRequest, IFormFile file)
        {
            salesRequest.Id = Guid.NewGuid().ToString();

            using (var content = new StringContent(JsonConvert.SerializeObject(salesRequest),System.Text.Encoding.UTF8,"application/json"))
            {
                //call here our function and pass the content
                HttpResponseMessage response = await client.PostAsync("http://localhost:7071/api/OnSalesUploadWriteToQueue", content);
                string returnValue = response.Content.ReadAsStringAsync().Result;
            }

            if(file != null)
            {
                var filename = salesRequest.Id + Path.GetExtension(file.FileName);
                BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("functionsalesrep");
                var blobClient = blobContainerClient.GetBlobClient(filename);

                var httpHeaders = new BlobHttpHeaders
                {
                    ContentType = file.ContentType
                };
                await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders);
                return View();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}