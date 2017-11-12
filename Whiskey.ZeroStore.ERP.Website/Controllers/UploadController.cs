using System;
using System.Collections;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.IO;
using System.Drawing;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.Web.Extensions;
using System.Collections.Generic;
using System.Web.Security;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    [AllowCross]
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

        /// <summary>
        /// 来源
        /// </summary>
        private string Origin
        {
            get
            {
                var Origin = InputHelper.SafeInput(Request["Origin"] ?? "");
                return Origin;
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
                extensions.Add("Word", "doc,docx");
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
                            if (FileType == "Image")
                            {
                                Image image = Image.FromStream(file.InputStream);
                                var orginalFileName = "o_" + i + "_" + DateTime.Now.ToString("HHmmssffff", DateTimeFormatInfo.InvariantInfo) + "_" + image.Width.ToString() + "_" + image.Height.ToString() + fileExt;
                                //string strUrl = ConfigurationHelper.GetAppSetting("WebUrl");
                                files.Add(ShowPath + orginalFileName);
                                file.SaveAs(SavePath + orginalFileName);
                            }
                            else
                            {
                                var orginalFileName = "o_" + i + "_" + DateTime.Now.ToString("HHmmssffff", DateTimeFormatInfo.InvariantInfo) + "_" + fileExt;
                                files.Add(ShowPath + orginalFileName);
                                file.SaveAs(SavePath + orginalFileName);
                            }
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

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <remarks>暂时为工单、充值规则上传方法</remarks>
        /// <returns></returns>
        [HttpPost]
        public JsonResult WorkOrderImg()
        {
            var result = new OperationResult(OperationResultType.Error, "");
            try
            {
                HttpPostedFileBase file = Request.Files[0];
                var fileExt = Path.GetExtension(file.FileName).ToLower();
                string newName = "";
                switch (Origin)
                {
                    case "StoreValueRule":      //充值积分规则
                        newName = "r_" + FileName + fileExt;
                        break;
                    default:
                        newName = "w_" + FileName + fileExt;            //工单
                        break;
                }
                if (String.IsNullOrEmpty(fileExt) || Array.IndexOf(((String)Extensions[FileType]).Split(','), fileExt.Substring(1).ToLower()) == -1)
                {
                    return Json(new OperationResult(OperationResultType.Error, "系统只允许上传类型为" + ((String)Extensions[FileType]) + "格式文件！"), JsonRequestBehavior.AllowGet);
                }
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
            catch (Exception ex)
            {
                result = new OperationResult(OperationResultType.Error, "文件分块上传失败，错误如下：" + ex.Message, ex.ToString());
                throw;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private static int IsFirst = 0;                 //是否第一次进入分块上传图片的方法
        private static string errorMsg = "";                //错误原因
        private static string fileName = "";
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <remarks>公司物品</remarks>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UploadGoodsImg()
        {
            var result = new OperationResult(OperationResultType.Error, "");
            try
            {
                int max_width = 500;
                int max_height = 500;

                HttpPostedFileBase file = Request.Files[0];
                var fileExt = Path.GetExtension(file.FileName).ToLower();
                string newName = "goods_" + FileName + fileExt;

                bool IsExists = System.IO.File.Exists(SavePath + newName);          //文件是否存在

                if(fileName!=SavePath+newName)
                {
                    IsFirst = 0;
                    fileName = SavePath + newName;
                }

                IsFirst++;

                if (IsFirst != 1 && !IsExists)
                {
                    return Json(new OperationResult(OperationResultType.Error, errorMsg),JsonRequestBehavior.AllowGet);
                }

                if (String.IsNullOrEmpty(fileExt) || Array.IndexOf(((String)Extensions[FileType]).Split(','), fileExt.Substring(1).ToLower()) == -1)
                {
                    return Json(new OperationResult(OperationResultType.Error, "系统只允许上传类型为" + ((String)Extensions[FileType]) + "格式文件！"), JsonRequestBehavior.AllowGet);
                }

                int maxSize = 307200;
                if (file.InputStream == null || file.InputStream.Length > maxSize)
                {
                    return Json(new OperationResult(OperationResultType.Error, "文件大小不能超过：" + maxSize / 1024 + "KB！"),JsonRequestBehavior.AllowGet);
                }

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

                if (IsFirst == 1)
                {
                    var size = GetSize(file);
                    if (size["Width"] == 0 || size["Height"] == 0)
                    {
                        System.IO.File.Delete(SavePath + newName);
                        errorMsg = "文件分块上传失败，无法获取图片尺寸";
                        return Json(new OperationResult(OperationResultType.Error, "文件分块上传失败，无法获取图片尺寸"),JsonRequestBehavior.AllowGet);
                    }

                    if (size["Width"] > max_width || size["Height"] > max_height)
                    {
                        System.IO.File.Delete(SavePath + newName);
                        errorMsg = "上传图片长宽不可超过" + max_width + "*" + max_height;
                        return Json(new OperationResult(OperationResultType.Error, "上传图片长宽不可超过" + max_width + "*" + max_height),JsonRequestBehavior.AllowGet);
                    }
                }
                result = new OperationResult(OperationResultType.Success, "文件分块上传成功！", new { file = ShowPath + newName });

            }
            catch (Exception ex)
            {
                result = new OperationResult(OperationResultType.Error, "文件分块上传失败，错误如下：" + ex.Message, ex.ToString());
                throw;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private IDictionary<string, int> GetSize(HttpPostedFileBase file)
        {
            IDictionary<string, int> dic = new Dictionary<string, int>();
            dic.Add("Width", 0);
            dic.Add("Height", 0);

            //建立图片对象
            System.Drawing.Image myimage = System.Drawing.Image.FromStream(file.InputStream);
            if (myimage == null)
            {
                return dic;
            }

            dic["Width"] = myimage.Width;
            dic["Height"] = myimage.Height;
            return dic;
        }
    }
}