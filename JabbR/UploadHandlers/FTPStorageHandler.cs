using JabbR.Services;
using Ninject;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using JabbR.Infrastructure;

namespace JabbR.UploadHandlers
{
    public class FTPStorageHandler : IUploadHandler
    {
        private readonly IKernel _kernel;

         [ImportingConstructor]
        public FTPStorageHandler(IKernel kernel)
        {
            _kernel = kernel;
        }

        public bool IsValid(string fileName, string contentType)
        {
            var settings = _kernel.Get<ApplicationSettings>();
            return !String.IsNullOrEmpty(settings.FTPServerLocation);
        }

        public async Task<UploadResult> UploadFile(string fileName, string contentType, Stream stream)
        {
            var settings = _kernel.Get<ApplicationSettings>();

            // Randomize the filename everytime so we don't overwrite files
            string randomFile = Path.GetFileNameWithoutExtension(fileName) + "_" +
                                Guid.NewGuid().ToString().Substring(0, 4) + Path.GetExtension(fileName);

            String uploadPath = String.Format("{0}{1}", settings.FTPServerLocation, randomFile);

            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uploadPath);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential ("anonymous","email@example.com");

            byte[] fileContents = new byte[stream.Length];
            stream.Read(fileContents, 0, fileContents.Length);

            Stream requestStream = await request.GetRequestStreamAsync();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close(); // necessary??

            String responseUrl = response.ResponseUri.ToString();
            if (responseUrl.IndexOf("ftp://") == 0)
            {
                responseUrl = responseUrl.Replace("ftp://", "http://");
            }

            var result = new UploadResult
            {
                Url = responseUrl,
                Identifier = randomFile
            };

            return result;
        }

    }
}