using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Routing;
using System.Web.UI;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Mvc.Binders;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.Files.Controllers
{
    public class UploadController : Controller
    {
        private string UploadRoot
        {
            get
            {
                var saveDir = InputHelper.SafeInput(Request["SaveDir"]);
                if (saveDir == null) { saveDir = "Unknowns"; }
                var uploadRoot = "/Content/UploadFiles/" + saveDir + "/";
                uploadRoot += DateTime.Now.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo) + "/";
                return uploadRoot;
            }
        }

        private string FileName
        {
            get
            {
                var fileName = InputHelper.SafeInput(Request["FileName"]);
                return fileName;
            }
        }

        private string FileType
        {
            get
            {
                var fileType = InputHelper.SafeInput(Request["ExtType"]);
                if (fileType == null) { fileType = "Image"; }
                return fileType;
            }
        }

        private Hashtable Extensions
        {
            get
            {
                var extensions = new Hashtable();
                extensions.Add("Image", "gif,jpg,jpeg,png,bmp");
                extensions.Add("Excel", "txt,xls,xlsx");
                extensions.Add("Flash", "swf,flv");
                extensions.Add("Media", "swf,flv,mp3,wav,wma,wmv,mid,avi,mpg,asf,rm,rmvb");
                extensions.Add("File", "doc,docx,xls,xlsx,ppt,pptx,txt,zip,rar,gz,bz2,pdf");
                return extensions;
            }
        }

        private string SavePath
        {
            get
            {
                var savePath = FileHelper.UrlToPath(UploadRoot);
                return savePath;
            }
        }

        private string ShowPath
        {
            get
            {
                return UploadRoot;
            }
        }

        private int MaxSize
        {
            get { return 10485760; }
        }

        [HttpPost]
        public JsonResult Multiple()
        {
            var errors = new ArrayList();
            var files = new ArrayList();
            if (Request.Files.Count > 0)
            {
                try
                {
                    for (int i = 0; i <= Request.Files.Count - 1; i++)
                    {
                        HttpPostedFileBase file = Request.Files[i];
                        if (file.ContentLength > 0)
                        {
                            var fileExt = Path.GetExtension(file.FileName).ToLower();
                            if (file.InputStream == null || file.InputStream.Length > MaxSize)
                            {
                                errors.Add("文件大小不能超过：" + MaxSize / 1024 / 1024 + "M！文件名：" + file.FileName + "。");
                                continue;
                            }
                            if (String.IsNullOrEmpty(fileExt) || Array.IndexOf(((String)Extensions[FileType]).Split(','), fileExt.Substring(1).ToLower()) == -1)
                            {
                                errors.Add("系统只允许上传类型为" + ((String)Extensions[FileType]) + "格式文件！文件名：" + file.FileName + "。");
                                continue;
                            }
                            var orginalFileName = "o_" + i + "_" + DateTime.Now.ToString("HHmmssffff", DateTimeFormatInfo.InvariantInfo) + fileExt;
                            //string strUrl = ConfigurationHelper.GetAppSetting("WebUrl");
                            files.Add(ShowPath + orginalFileName);
                            file.SaveAs(SavePath + orginalFileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                    throw;
                }
            }

            return Json(new { files = files, errors = errors, }, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult Partial()
        {
            var result = new OperationResult(OperationResultType.Error, "");
            try
            {
                HttpPostedFileBase file = Request.Files[0];
                var fileExt = Path.GetExtension(file.FileName).ToLower();
                var newName = "f_" + FileName + fileExt;
                if (file.InputStream == null || file.InputStream.Length > MaxSize)
                {
                    result = new OperationResult(OperationResultType.Error, "文件大小不能超过：" + MaxSize / 1024 / 1024 + "M！");
                }
                else if (String.IsNullOrEmpty(fileExt) || Array.IndexOf(((String)Extensions[FileType]).Split(','), fileExt.Substring(1).ToLower()) == -1)
                {
                    result = new OperationResult(OperationResultType.Error, "系统只允许上传类型为" + ((String)Extensions[FileType]) + "格式文件！");
                }
                else
                {
                    var stream = file.InputStream;
                    using (var fs = new FileStream(SavePath + newName, FileMode.Append, FileAccess.Write))
                    {
                        var buffer = new byte[1024];
                        var l = stream.Read(buffer, 0, 1024);
                        while (l > 0)
                        {
                            fs.Write(buffer, 0, l);
                            l = stream.Read(buffer, 0, 1024);
                        }
                        fs.Flush();
                        fs.Close();
                    }
                    result = new OperationResult(OperationResultType.Success, "文件分块上传成功！", new { file = ShowPath + newName });
                }

            }
            catch (Exception ex)
            {
                result = new OperationResult(OperationResultType.Error, "文件分块上传失败，错误如下：" + ex.Message, ex.ToString());
                throw;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Whole()
        {
            var result = new OperationResult(OperationResultType.Error, "");
            try
            {
                HttpPostedFileBase file = Request.Files[0];
                var fileExt = Path.GetExtension(file.FileName).ToLower();
                if (file.InputStream == null || file.InputStream.Length > MaxSize)
                {
                    result = new OperationResult(OperationResultType.Error, "文件大小不能超过：" + MaxSize / 1024 / 1024 + "M！");
                }
                else if (String.IsNullOrEmpty(fileExt) || Array.IndexOf(((String)Extensions[FileType]).Split(','), fileExt.Substring(1).ToLower()) == -1)
                {
                    result = new OperationResult(OperationResultType.Error, "系统只允许上传类型为" + ((String)Extensions[FileType]) + "格式文件！");
                }
                else
                {
                    var newName = "f_" + DateTime.Now.ToString("HHmmssffff", DateTimeFormatInfo.InvariantInfo) + fileExt;
                    file.SaveAs(SavePath + newName);
                    result = new OperationResult(OperationResultType.Success, "文件上传成功！", new { file = ShowPath + newName });
                }

            }
            catch (Exception ex)
            {
                result = new OperationResult(OperationResultType.Error, "文件上传失败，错误如下：" + ex.Message, ex.ToString());
                throw;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}