using EImece.Domain.Models.Enums;
using EImece.Domain.Models.HelperModels;
using EImece.Domain.Services.IServices;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;

namespace EImece.Domain.Helpers
{
    public class FilesHelper
    {
        [Inject]
        public IFileStorageService FileStorageService { get; set; }

        public String DeleteURL { get; set; }
        public String DeleteType { get; set; }
        public String StorageRoot { get; set; }
        public String UrlBase { get; set; }
        public String tempPath { get; set; }
        //ex:"~/Files/something/";
        public String serverMapPath { get; set; }
        public void Init(String DeleteURL, String DeleteType, String StorageRoot, String UrlBase, String tempPath, String serverMapPath)
        {
            this.DeleteURL = DeleteURL;
            this.DeleteType = DeleteType;
            this.StorageRoot = StorageRoot;
            this.UrlBase = UrlBase;
            this.tempPath = tempPath;
            this.serverMapPath = serverMapPath;
        }

        public void DeleteFiles(String pathToDelete)
        {

            string path = HostingEnvironment.MapPath(pathToDelete);

            System.Diagnostics.Debug.WriteLine(path);
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo fi in di.GetFiles())
                {
                    System.IO.File.Delete(fi.FullName);
                    System.Diagnostics.Debug.WriteLine(fi.Name);
                }

                di.Delete(true);
            }
        }

        public String DeleteFile(String file, HttpContextBase ContentBase)
        {

            var request = ContentBase.Request;
            int contentId = request.QueryString["contentId"].ToInt();
            var imageType = EnumHelper.Parse<EImeceImageType>(request.QueryString["imageType"].ToStr());
            var mod = EnumHelper.Parse<MediaModType>(request.QueryString["mod"].ToStr());


            System.Diagnostics.Debug.WriteLine("DeleteFile");
            //    var req = HttpContext.Current;
            System.Diagnostics.Debug.WriteLine(file);

            String fullPath = Path.Combine(StorageRoot, file);
            System.Diagnostics.Debug.WriteLine(fullPath);
            System.Diagnostics.Debug.WriteLine(System.IO.File.Exists(fullPath));
            String thumbPath = "/thb" + file + "";
            String partThumb1 = Path.Combine(StorageRoot, "thumbs");
            String partThumb2 = Path.Combine(partThumb1, "thb" + file);

            System.Diagnostics.Debug.WriteLine(partThumb2);
            System.Diagnostics.Debug.WriteLine(System.IO.File.Exists(partThumb2));
            if (System.IO.File.Exists(fullPath))
            {
                //delete thumb 
                if (System.IO.File.Exists(partThumb2))
                {
                    System.IO.File.Delete(partThumb2);
                }
                System.IO.File.Delete(fullPath);
                String succesMessage = "Ok";

                FileStorageService.DeleteUploadImage(file, contentId, imageType, mod);
                Thread.Sleep(100);
                return succesMessage;
            }
            String failMessage = "Error Delete";
            return failMessage;
        }
        public JsonFiles GetFileList(HttpContextBase ContentBase)
        {
            var request = ContentBase.Request;
            int Id = request.QueryString["contentId"].ToInt();
            var imageType = EnumHelper.Parse<EImeceImageType>(request.QueryString["imageType"].ToStr());
            var mod = EnumHelper.Parse<MediaModType>(request.QueryString["mod"].ToStr());

            var r = new List<ViewDataUploadFilesResult>();

            String fullPath = Path.Combine(StorageRoot);
            if (Directory.Exists(fullPath))
            {
                DirectoryInfo dir = new DirectoryInfo(fullPath);
                foreach (FileInfo file in dir.GetFiles())
                {
                    int SizeInt = unchecked((int)file.Length);
                    r.Add(UploadResult(file.Name, SizeInt, file.FullName, ContentBase));
                }

            }
            JsonFiles files = new JsonFiles(r);

            return files;
        }

        public void UploadAndShowResults(HttpContextBase ContentBase, List<ViewDataUploadFilesResult> resultList)
        {
            var httpRequest = ContentBase.Request;
            System.Diagnostics.Debug.WriteLine(Directory.Exists(tempPath));

            String fullPath = Path.Combine(StorageRoot);
            Directory.CreateDirectory(fullPath);
            // Create new folder for thumbs
            Directory.CreateDirectory(fullPath + "/thumbs/");

            foreach (String inputTagName in httpRequest.Files)
            {

                var headers = httpRequest.Headers;

                var file = httpRequest.Files[inputTagName];
                System.Diagnostics.Debug.WriteLine(file.FileName);

                if (string.IsNullOrEmpty(headers["X-File-Name"]))
                {

                    UploadWholeFile(ContentBase, resultList);
                }
                else
                {

                    UploadPartialFile(headers["X-File-Name"], ContentBase, resultList);
                }
            }
        }


        private void UploadWholeFile(HttpContextBase requestContext, List<ViewDataUploadFilesResult> statuses)
        {

            var request = requestContext.Request;



            for (int i = 0; i < request.Files.Count; i++)
            {
                var file = request.Files[i];

                var ext = Path.GetExtension(file.FileName);
                var fileBase = Path.GetFileNameWithoutExtension(file.FileName);
                Random random = new Random();
                var randomNumber = random.Next(0, int.MaxValue).ToString();
                var newFileName = string.Format(@"{0}_{1}{2}", fileBase, randomNumber, ext);


                String pathOnServer = Path.Combine(StorageRoot);
                //   var fullPath = Path.Combine(pathOnServer, Path.GetFileName(file.FileName));
                var fullPath = Path.Combine(pathOnServer, newFileName);
                file.SaveAs(fullPath);

                //Create thumb
                string[] imageArray = file.FileName.Split('.');
                if (imageArray.Length != 0)
                {
                    String extansion = imageArray[imageArray.Length - 1];
                    if (extansion != "jpg" && extansion != "png") //Do not create thumb if file is not an image
                    {

                    }
                    else
                    {

                        var newFileNameThb = string.Format(@"thb{0}_{1}{2}", fileBase, randomNumber, ext);
                        var ThumbfullPath = Path.Combine(pathOnServer, "thumbs");
                        // String fileThumb = file.FileName + ".80x80.jpg";
                        String fileThumb = newFileNameThb;
                        var ThumbfullPath2 = Path.Combine(ThumbfullPath, fileThumb);
                        using (MemoryStream stream = new MemoryStream(System.IO.File.ReadAllBytes(fullPath)))
                        {
                            var thumbnail = new WebImage(stream).Resize(80, 80);
                            thumbnail.Save(ThumbfullPath2, "jpg");
                        }

                    }
                }
                // statuses.Add(UploadResult(file.FileName, file.ContentLength, file.FileName));
                statuses.Add(UploadResult(newFileName, file.ContentLength, newFileName, requestContext));
            }
        }



        private void UploadPartialFile(string fileName, HttpContextBase requestContext, List<ViewDataUploadFilesResult> statuses)
        {
            var request = requestContext.Request;
            if (request.Files.Count != 1) throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
            var file = request.Files[0];
            var inputStream = file.InputStream;
            String patchOnServer = Path.Combine(StorageRoot);
            var fullName = Path.Combine(patchOnServer, Path.GetFileName(file.FileName));
            var ThumbfullPath = Path.Combine(fullName, Path.GetFileName(file.FileName + ".80x80.jpg"));
            ImageHelper handler = new ImageHelper();

            var ImageBit = ImageHelper.LoadImage(fullName);
            handler.Save(ImageBit, 80, 80, 10, ThumbfullPath);
            using (var fs = new FileStream(fullName, FileMode.Append, FileAccess.Write))
            {
                var buffer = new byte[1024];

                var l = inputStream.Read(buffer, 0, 1024);
                while (l > 0)
                {
                    fs.Write(buffer, 0, l);
                    l = inputStream.Read(buffer, 0, 1024);
                }
                fs.Flush();
                fs.Close();
            }
            statuses.Add(UploadResult(file.FileName, file.ContentLength, file.FileName, requestContext));
        }
        public ViewDataUploadFilesResult UploadResult(String FileName, int fileSize, String FileFullPath, HttpContextBase requestContext)
        {
            var request = requestContext.Request;
            int contentId = request.Form["contentId"].ToInt();
            var imageType = EnumHelper.Parse<EImeceImageType>(request.Form["imageType"].ToStr());
            var mod = EnumHelper.Parse<MediaModType>(request.Form["mod"].ToStr());


            String getType = System.Web.MimeMapping.GetMimeMapping(FileFullPath);

            var result = new ViewDataUploadFilesResult()
            {
                name = FileName,
                size = fileSize,
                type = getType,
                url = UrlBase + FileName,
                deleteUrl = String.Format(DeleteURL, FileName, contentId, mod, imageType),
                thumbnailUrl = CheckThumb(getType, FileName),
                deleteType = DeleteType,
            };
            return result;
        }

        public String CheckThumb(String type, String FileName)
        {
            var splited = type.Split('/');
            if (splited.Length == 2)
            {
                string extansion = splited[1];
                if (extansion.Equals("jpeg") || extansion.Equals("jpg") || extansion.Equals("png") || extansion.Equals("gif"))
                {
                    //   String thumbnailUrl = UrlBase + "/thumbs/" + FileName + ".80x80.jpg";
                    String thumbnailUrl = UrlBase + "/thumbs/thb" + FileName;
                    return thumbnailUrl;
                }
                else
                {
                    if (extansion.Equals("octet-stream")) //Fix for exe files
                    {
                        return "/Content/Free-file-icons/48px/exe.png";

                    }
                    if (extansion.Contains("zip")) //Fix for exe files
                    {
                        return "/Content/Free-file-icons/48px/zip.png";
                    }
                    String thumbnailUrl = "/Content/Free-file-icons/48px/" + extansion + ".png";
                    return thumbnailUrl;
                }
            }
            else
            {
                return UrlBase + "/thumbs/" + FileName + ".80x80.jpg";
            }

        }
        public List<String> FilesList()
        {

            List<String> Filess = new List<String>();
            string path = HostingEnvironment.MapPath(serverMapPath);
            System.Diagnostics.Debug.WriteLine(path);
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo fi in di.GetFiles())
                {
                    Filess.Add(fi.Name);
                    System.Diagnostics.Debug.WriteLine(fi.Name);
                }

            }
            return Filess;
        }
    }
    public class JsonFiles
    {
        public ViewDataUploadFilesResult[] files;
        public string TempFolder { get; set; }
        public JsonFiles(List<ViewDataUploadFilesResult> filesList)
        {
            files = new ViewDataUploadFilesResult[filesList.Count];
            for (int i = 0; i < filesList.Count; i++)
            {
                files[i] = filesList.ElementAt(i);
            }

        }
    }
}
