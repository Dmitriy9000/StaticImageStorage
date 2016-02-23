using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kudinov.ImageStorage.Controllers
{
    public class HomeController : Controller
    {
        static string _imagesPath;
        static string _webPrefix;

        static HomeController()
        {
            _imagesPath = ConfigurationManager.AppSettings["ImagesDirectory"];
            _webPrefix = ConfigurationManager.AppSettings["ImagesWebPrefix"];
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(int? chunk, string name)
        {
            var fileUpload = Request.Files[0];
            chunk = chunk ?? 0;

            var fname = GetFreeName(name);
            var fileName = Path.GetFileName(fname);

            using (var fs = new FileStream(fname, chunk == 0 ? FileMode.Create : FileMode.Append))
            {
                var buffer = new byte[fileUpload.InputStream.Length];
                fileUpload.InputStream.Read(buffer, 0, buffer.Length);
                fs.Write(buffer, 0, buffer.Length);
            }
            return Json(new
            {
                Status = "Chunk uploaded",
                Filename = Path.Combine(_webPrefix, fileName).Replace('\\', '/')
            });
        }

        private string GetFreeName(string name, int count = 0)
        {
            var ext = Path.GetExtension(name);
            var fn = Path.GetFileNameWithoutExtension(name);
            var filePath = Path.Combine(_imagesPath, string.Format("{0}-{1}{2}", fn, count, ext));
            if (System.IO.File.Exists(filePath))
            {
                return GetFreeName(name, ++count);
            }
            return filePath;
        }
    }
}
