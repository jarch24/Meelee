using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Meelee.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Meelee.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;

        public HomeController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
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

        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            var task = GetImagesAsync();
            task.Wait();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            string storageConnectionString = _config.GetSection("AzureStorage")["Default"];
            CloudStorageAccount conn;
            Response.Redirect("/Home/Upload");

            if (CloudStorageAccount.TryParse(storageConnectionString, out conn))
            {
                CloudStorageAccount acc = CloudStorageAccount.Parse(storageConnectionString);
                CloudBlobClient blob = acc.CreateCloudBlobClient();
                CloudBlobContainer container = blob.GetContainerReference("storage2428-container");
                await container.CreateIfNotExistsAsync();
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob,
                };
                await container.SetPermissionsAsync(permissions);
                foreach (var item in files)
                {
                    byte[] arr = new byte[item.Length];
                    //var t = item.OpenReadStream();
                    string imageName = item.FileName + Path.GetExtension(item.FileName);
                    var blockBlob = container.GetBlockBlobReference(imageName);

                    blockBlob.Properties.ContentType = item.ContentType;
                    var task = blockBlob.UploadFromStreamAsync(item.OpenReadStream());
                    task.Wait();
                    //t.BeginRead(arr, 0, arr.Length, x =>
                    //{


                    //}, "Koo");

                    //blockBlob.UploadFromByteArrayAsync(arr, 0, arr.Length);
                    //blockBlob.DownloadToFileAsync(imageName, FileMode.Create);
                    //break;
                }
            }

            return View();
        }

        public async Task GetImagesAsync()
        {
            ViewBag.list = new List<string>();
            string storageConnectionString = _config.GetSection("AzureStorage")["Default"];

            CloudStorageAccount storage = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blob = storage.CreateCloudBlobClient();
            CloudBlobContainer container = blob.GetContainerReference("storage2428-container");
            await container.CreateIfNotExistsAsync();

            var task = container.ListBlobsSegmentedAsync(null);
            task.Wait();

            var result = task.Result.Results;
            foreach (var item in result)
            {
                ViewBag.list.Add(item.Uri.ToString());
            }





        }
    }
}
