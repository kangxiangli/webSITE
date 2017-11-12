using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.Web.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    public class ApiMemberController : Controller
    {
        #region 初始化操作对象
        /// <summary>
        /// 初始化日志
        /// </summary>
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ApiMemberController));
        
        protected readonly IMemberContract _memberContract;         

        /// <summary>
        /// 初始化业务层操作对象
        /// </summary>
        public ApiMemberController(IMemberContract memberContract)
        {
            _memberContract = memberContract;                      
		}
        #endregion

        #region 修改头像
        [HttpPost]
        [AllowCross]
        public JsonResult Upload(int MemberId)
        {
            HttpFileCollectionBase files = Request.Files;
            OperationResult oper = new OperationResult(OperationResultType.Error);
            
            if (files.Count==0)
            {
                oper.Message = "请选择图片";
                return Json(oper);
            }
            else
            {
                string conPath = ConfigurationHelper.GetAppSetting("SaveMemberPhoto");
                string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");
                DateTime now = DateTime.Now;
                Guid gid = Guid.NewGuid();
                string fileName = gid.ToString();
                conPath = conPath + now.Year.ToString() + "/" + now.Month.ToString() + "/" + now.Day.ToString() + "/" + now.Hour.ToString() + "/" + now.ToString("yyyyMMddHHmmss") + ".jpg";
                files[0].SaveAs(FileHelper.UrlToPath(conPath));
                oper = _memberContract.UploadImage(MemberId, conPath);
                if (oper.ResultType==OperationResultType.Success)
                {
                    oper.Data =strWebUrl+ conPath;
                }
            }
            return Json(oper);
        }
        #endregion
    }
}