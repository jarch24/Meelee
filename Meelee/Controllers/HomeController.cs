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
using Microsoft.WindowsAzure.Storage.Table;

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
            //task.Wait();
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
            await InsertAsync();
        }

        public async Task InsertAsync()
        {
            try
            {
                var connectionString = "DefaultEndpointsProtocol=https;AccountName=tester24;AccountKey=ulJeySdffkNHZWfIb5WTYyuqM6vauhzhnknIPbqNkAqxCu3J3f94WQPujtloniYoZysZby6zC1366OX0WY0AoQ==;TableEndpoint=https://tester24.table.cosmos.azure.com:443/;";
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference("Demo");
                await table.CreateIfNotExistsAsync();
                TableOperation insert = TableOperation.Insert(new User("Q", "B")
                {
                    Phone = "123456789",
                    Email = "q@p.com"
                });
                await table.ExecuteAsync(insert);
                
                //TableOperation tb = TableOperation.Retrieve<User>("K", "K");
                TableQuery<User> query = new TableQuery<User>().Where(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Q"));


                var task = table.ExecuteQuerySegmentedAsync<User>(query, null);
                task.Wait();
                var user = task.Result;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
            
        }
    }
}
