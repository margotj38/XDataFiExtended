using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI_final.Models.Data;
using System.IO;
using System.Web;
using System.Xml.Linq;
using System.Threading.Tasks;
using WebAPI_final.Models.Parser;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI_final.Models.Data;
using WebAPI_final.Models;
using WebAPI_final.Models.GestionErreurs;
using WebAPI_final.Models.Service;

namespace WebAPI_final.Controllers
{
    public class XMLController : ApiController
    {
        private static readonly string ServerUploadFolder =  Path.GetTempPath();
        public DataRetour donnees = new DataRetour();

        [Route("files")]
        [HttpPost]
        [ValidateMimeMultipartContentFilter]
        public async Task<FileResult> UploadSingleFile()
        {
            var streamProvider = new MultipartFormDataStreamProvider(ServerUploadFolder);
            await Request.Content.ReadAsMultipartAsync(streamProvider);
    
            return new FileResult
            {
                FileNames = streamProvider.FileData.Select(entry => entry.LocalFileName),
                Names = streamProvider.FileData.Select(entry => entry.Headers.ContentDisposition.FileName),
                ContentTypes = streamProvider.FileData.Select(entry => entry.Headers.ContentType.MediaType),
                Description = streamProvider.FormData["description"],
                CreatedTimestamp = DateTime.UtcNow,
                UpdatedTimestamp = DateTime.UtcNow, 
                DownloadLink = "TODO, will implement when file is persisited"
            };
        }
    }
    }

    
