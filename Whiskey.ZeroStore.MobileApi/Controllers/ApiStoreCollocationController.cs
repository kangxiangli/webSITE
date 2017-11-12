using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts.StoreCollocation;

namespace Whiskey.ZeroStore.MobileApi.Controllers
{
    public class ApiStoreCollocationController : Controller
    {
        protected readonly IStoreProductCollocationContract _storeProductCollocationContract;


        public ApiStoreCollocationController(IStoreProductCollocationContract storeProductCollocationContract)
        {
            _storeProductCollocationContract = storeProductCollocationContract;
        }
        [HttpPost]
        public JsonResult Insert(StoreProductCollocation collocationInfo)
        {
            try
            {
                string guid = Guid.NewGuid().ToString().Replace("-", "");
                collocationInfo.Guid = guid;
                collocationInfo.IsDeleted = false;
                collocationInfo.IsEnabled = true;
                HttpFileCollectionBase files = Request.Files;
                OperationResult oper = new OperationResult(OperationResultType.Error);

                if (files.Count == 0)
                {
                    oper.Message = "请选择图片";
                    return Json(oper);
                }
                else
                {
                    string conPath = "/Content/UploadFiles/StoreCollocation";
                    DateTime now = DateTime.Now;
                    Guid gid = Guid.NewGuid();
                    string fileName = gid.ToString();
                    conPath = conPath + now.Year.ToString() + "/" + now.Month.ToString() + "/" + now.Day.ToString() + "/" + now.Hour.ToString() + "/" + now.ToString("yyyyMMddHHmmss") + ".jpg";
                    files[0].SaveAs(FileHelper.UrlToPath(conPath));
                    collocationInfo.ThumbnailPath = conPath;
                    OperationResult rs = _storeProductCollocationContract.Insert(collocationInfo);
                    return Json(rs);
                }
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "获取失败！", e.Message));
            }
        }

        [HttpPost]
        public JsonResult UploadImg(int imgType)
        {
            HttpFileCollectionBase files = Request.Files;
            OperationResult oper = new OperationResult(OperationResultType.Error);

            if (files.Count == 0)
            {
                oper.Message = "请选择图片";
                return Json(oper);
            }
            else
            {
                string conPath = "/Content/UploadFiles/Entry/";
                switch (imgType)
                {
                    case 1:
                        conPath += "Bankcard/";
                        break;
                    case 2:
                        conPath += "IdCard/";
                        break;
                    case 3:
                        conPath += "HealthCertificate/";
                        break;
                    case 4:
                        conPath += "Photo/";
                        break;
                }
                DateTime now = DateTime.Now;
                Guid gid = Guid.NewGuid();
                string fileName = gid.ToString();
                conPath = conPath + now.Year.ToString() + "/" + now.Month.ToString() + "/" + now.Day.ToString() + "/" + now.Hour.ToString() + "/" + now.ToString("yyyyMMddHHmmss") + ".jpg";
                string savePath = FileHelper.UrlToPath(conPath);
                files[0].SaveAs(savePath);
                var data = new { imgPath = conPath };
                oper = new OperationResult(OperationResultType.Success, "上传成功！");
                oper.Data = data;
            }
            return Json(oper);
        }
    }
}