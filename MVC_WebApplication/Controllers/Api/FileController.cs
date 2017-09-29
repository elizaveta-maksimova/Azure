using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AutoMapper;
using MVC_WebApplication.BL.Account;
using MVC_WebApplication.BL.Models;
using MVC_WebApplication.Models;
using Newtonsoft.Json;

namespace MVC_WebApplication.Controllers.Api
{
    public class FileController : ApiController
    {
        private readonly ProductService _productService;

        public FileController()
        {
            _productService = new ProductService();
        }

        [HttpPost]
        public async Task<IHttpActionResult> Upload()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            MultipartMemoryStreamProvider memoryStreamProvider = await Request.Content.ReadAsMultipartAsync();

            string jsonContent = null;
            byte[] fileContent = null;
            string fileName = null;
            foreach (HttpContent content in memoryStreamProvider.Contents)
            {
                string contentDispositionName = content.Headers.ContentDisposition.Name.Trim('"');
                if (contentDispositionName == "jsonData")
                {
                    jsonContent = await content.ReadAsStringAsync();
                    continue;
                }

                if (contentDispositionName.StartsWith("file"))
                {
                    fileName = content.Headers.ContentDisposition.FileName.Trim('"');
                    fileContent = await content.ReadAsByteArrayAsync();
                }
            }

            if (string.IsNullOrEmpty(jsonContent))
            {
                _productService.InsertProduct(null, fileContent, fileName);
               return Ok("File successfully uploaded");
            }

            var productRequest = JsonConvert.DeserializeObject<Product>(jsonContent);
            var newProduct = Mapper.Map<ProductEntity>(productRequest);
            _productService.InsertProduct(newProduct, fileContent, fileName);

            return Ok("File successfully uploaded and data saved");
        }
    }
}