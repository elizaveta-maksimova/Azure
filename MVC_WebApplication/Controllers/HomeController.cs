using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;
using MVC_WebApplication.BL.Account;
using MVC_WebApplication.BL.Models;
using MVC_WebApplication.Data.DataAccess;
using MVC_WebApplication.Models;

namespace MVC_WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductService _productService;
        private readonly BlobStorageDataAccess _storageDataAccess;

        public HomeController()
        {
            _productService = new ProductService();
            _storageDataAccess = new BlobStorageDataAccess();
        }

        public ActionResult Index()
        {
            List<ProductEntity> products = _productService.GetAllProducts();
            var viewModel = new ProductViewModel { Products = products, NewProduct = new Product() };
            return View(viewModel);
        }

        public ActionResult GetBlobSas()
        {
            string sas = _storageDataAccess.GetBlobSas("finn.jpg");
            ViewBag.SasLink = sas;

            return PartialView("Link");
        }

        public ActionResult GetContainerSas()
        {
            string sas = _storageDataAccess.GetContainerSas();
            ViewBag.SasLink = sas;

            return PartialView("Link");
        }

        public ActionResult GetBlobListWithSas()
        {
            List<IListBlobItem> blobs = _storageDataAccess.GetBlobListWithSas();
            ViewBag.Blobs = blobs.Select(b => (CloudBlockBlob)b);

            return PartialView("Link");
        }

        public ActionResult CreateAccountSas()
        {
            string sas = _storageDataAccess.CreateAccountSasUrl();
            ViewBag.SasLink = sas;

            return PartialView("Link");
        }

        public ActionResult CreatePolicy()
        {
            string sas = _storageDataAccess.CreateStoredAccessPolicy();
            ViewBag.SasLink = sas;

            return PartialView("Link");
        }

        public ActionResult ClearPolicy()
        {
             _storageDataAccess.ClearStoredAccessPolicies();
            ViewBag.SasLink = "Clear completed";

            return PartialView("Link");
        }

        public ActionResult GetInvalidSas()
        {
            string sas = _storageDataAccess.GetInvalidContainerSasUriBasedOnPolicy();
            ViewBag.SasLink = sas;

            return PartialView("Link");
        }

        public ActionResult GetBlobListAnomimously()
        {
            List<IListBlobItem> blobs = _storageDataAccess.ListBlobsAnonymously();
            ViewBag.Blobs = blobs.Select(b => (CloudBlockBlob)b);

            return PartialView("Link");
        }

        public ActionResult Demo()
        {
            _storageDataAccess.TableLinqOperations();

            return View();
        }

        public FileResult Download()
        {
            byte[] fileBytes = _storageDataAccess.DownloadFileInBlocks("50MBFile1.zip");
            string fileName = "50MBFile1.zip";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}