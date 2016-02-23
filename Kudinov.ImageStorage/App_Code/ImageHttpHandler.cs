using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace Kudinov.ImageStorage
{
    public class ImageHttpHandler : IHttpHandler
    {
        static string _imagesPath;
        static string _cacheFolder;

        static ImageHttpHandler()
        {
            _imagesPath = ConfigurationManager.AppSettings["ImagesDirectory"];
            _cacheFolder = ConfigurationManager.AppSettings["CacheFolder"];
        }

        const string WidthParam = "w";
        const string HeightParam = "h";

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            var requestPath = context.Request.Path;
            var fileName = Path.GetFileName(requestPath);

            var pathToLook = Path.Combine(_imagesPath, fileName);
            if (File.Exists(pathToLook))
            {
                int width = -1;
                if (context.Request.QueryString.AllKeys.Contains(WidthParam))
                {
                    int.TryParse(context.Request.QueryString[WidthParam], out width);
                }

                int height = -1;
                if (context.Request.QueryString.AllKeys.Contains(HeightParam))
                {
                    int.TryParse(context.Request.QueryString[HeightParam], out height);
                }

                var fileExt = Path.GetExtension(fileName).Trim('.').ToLower();
                System.Drawing.Imaging.ImageFormat fileFormat;
                if (fileExt == "png")
                {
                    context.Response.ContentType = "image/png";
                    fileFormat = System.Drawing.Imaging.ImageFormat.Png;
                }
                else
                {
                    context.Response.ContentType = "image/jpeg";
                    fileFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                }

                if (width != -1 && height != -1)
                {
                    GetFromCache(width, height, pathToLook, fileFormat, context);
                }
                else
                {
                    Bitmap bitOutput;
                    bitOutput = new Bitmap(pathToLook);
                    bitOutput.Save(context.Response.OutputStream, fileFormat);
                }
            }
        }

        private void GetFromCache(int width, int height, string pathToLook, ImageFormat fileFormat, HttpContext context)
        {
            var cachePath = Path.Combine(_imagesPath, _cacheFolder);
            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            var fileName = Path.GetFileName(pathToLook);
            var filenameFromCache = string.Format("{0}{1}{2}", width, height, fileName);
            var filepathFromCache = Path.Combine(cachePath, filenameFromCache);
            if (File.Exists(filepathFromCache))
            {
                // return from cache
                var bitOutput = new Bitmap(filepathFromCache);
                bitOutput.Save(context.Response.OutputStream, fileFormat);
            }
            else
            {
                // create cache entry & return image

                Bitmap src = Image.FromFile(pathToLook) as Bitmap;

                var left = (src.Width - width) / 2;
                var top = (src.Height - height) / 2;

                Rectangle cropRect = new Rectangle(new Point(left, top), new Size() { Width = width, Height = height });

                Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                     cropRect, GraphicsUnit.Pixel);
                    g.Save();
                    target.Save(filepathFromCache, fileFormat);
                    target.Save(context.Response.OutputStream, fileFormat);
                }
            }
        }

    }
}