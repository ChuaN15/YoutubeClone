using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Enums;
using Xabe.FFmpeg.Model;

namespace YoutubeClone3.Controllers
{
    public class HomeController : Controller
    {
        YoutubeCloneEntities ent = new YoutubeCloneEntities();


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Search(string search)
        {
            List<VIDEO> videolist = ent.VIDEOs.Where(x => x.Name.Contains(search)).ToList();

            return View(videolist);
        }

        [HttpGet]
        public ActionResult Index()
        {
            List<VIDEO> videolist = ent.VIDEOs.OrderByDescending(x => x.ViewCount).ToList();

            return View(videolist);
        }

        [HttpPost]
        public ActionResult UploadVideo(HttpPostedFileBase fileupload)
        {
            if (fileupload != null)
            {
                string fileName = Path.GetFileName(fileupload.FileName);
                int fileSize = fileupload.ContentLength;
                int Size = fileSize / 1000;
                fileupload.SaveAs(Server.MapPath("~/Controllers/VideoFileUpload/" + fileName));

                VIDEO video = new VIDEO();
                video.Name = fileName;
                video.FileSize = fileSize;
                video.FilePath = "~/Controllers/VideoFileUpload/" + fileName;
                video.UploadDateTime = DateTime.Now;
                video.Owner = fileName;
                ent.VIDEOs.Add(video);
                ent.SaveChanges();

            }
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult RegisterVideo()
        {
                return View();
        }

        public ActionResult EditVideo(int id)
        {
            Session["VideoId2"] = id;
            var whichvideo = ent.VIDEOs.FirstOrDefault(x => x.ID == id);

            return View(whichvideo);
        }

        public ActionResult DeleteVideo(int id)
        {
            Session["VideoId2"] = id;
            var whichvideo = ent.VIDEOs.FirstOrDefault(x => x.ID == id);
            ent.VIDEOs.Remove(whichvideo);
            ent.SaveChanges();

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult EditVideo(VIDEO video2, HttpPostedFileBase UserPhoto, HttpPostedFileBase fileupload)
        {

            var whichVideo = ent.VIDEOs.FirstOrDefault(x => x.ID == video2.ID);

            whichVideo.Name = video2.Name;
            ent.SaveChanges();

            whichVideo.UploadDateTime = DateTime.Now;
            ent.SaveChanges();

            whichVideo.Owner = video2.Owner;
            ent.SaveChanges();

            if (fileupload != null)
            {
                string fileName = Path.GetFileName(fileupload.FileName);
                int fileSize = fileupload.ContentLength;
                int Size = fileSize / 1000;
                fileupload.SaveAs(Server.MapPath("~/Controllers/VideoFileUpload/" + fileName));

                whichVideo.FileSize = fileSize;
                whichVideo.FilePath = "~/Controllers/VideoFileUpload/" + fileName;
                
                

                if (UserPhoto != null)
                {
                    MemoryStream target = new MemoryStream();
                    UserPhoto.InputStream.CopyTo(target);
                    byte[] data = target.ToArray();
                    whichVideo.CoverImage = data;
                }

                ent.SaveChanges();

            }
            return RedirectToAction("Index");
        }

        public ActionResult ViewVideo(int id)
        {
            Session["VideoId"] = id;
            var whichvideo = ent.VIDEOs.FirstOrDefault(x => x.ID == id);
            whichvideo.ViewCount = whichvideo.ViewCount + 1;
            ent.SaveChanges();

            return View(whichvideo);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterVideo(VIDEO video2, HttpPostedFileBase UserPhoto, HttpPostedFileBase fileupload)
        {

            if (fileupload != null)
            {
                string fileName = Path.GetFileName(fileupload.FileName);
                int fileSize = fileupload.ContentLength;
                int Size = fileSize / 1000;
                fileupload.SaveAs(Server.MapPath("~/Controllers/VideoFileUpload/" + fileName));

                VIDEO video = new VIDEO();
                video.Name = video2.Name;
                video.FileSize = fileSize;
                video.FilePath = "~/Controllers/VideoFileUpload/" + fileName;
                video.UploadDateTime = DateTime.Now;
                video.Owner = video2.Owner;
                video.ViewCount = 0;
                ent.VIDEOs.Add(video);

                if (UserPhoto != null)
                {
                    MemoryStream target = new MemoryStream();
                    UserPhoto.InputStream.CopyTo(target);
                    byte[] data = target.ToArray();
                    video.CoverImage = data;
                }


                ent.SaveChanges();

            }
            return RedirectToAction("Index");
        }

        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        public FileContentResult GetUserImage(int id)
        {
            if (id.ToString() != null)
            {
                int userId = id;

                var userImage = ent.VIDEOs.Where(x => x.ID == userId).FirstOrDefault();

                if (userImage.CoverImage == null)
                {
                    string fileName = HttpContext.Server.MapPath(@"~/Images/noImg.png");


                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    

                    var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
                    ffMpeg.GetVideoThumbnail(userImage.FilePath, fs, 1);


                    return File(ReadToEnd(fs), "image/png");
                }

                return new FileContentResult(userImage.CoverImage, "image/jpeg");
            }
            else
            {
                

                string fileName = HttpContext.Server.MapPath(@"~/Images/noImg.png");

                byte[] imageData = null;
                FileInfo fileInfo = new FileInfo(fileName);
                long imageFileLength = fileInfo.Length;
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                imageData = br.ReadBytes((int)imageFileLength);
                return File(imageData, "image/png");

            }
        }

    }
}