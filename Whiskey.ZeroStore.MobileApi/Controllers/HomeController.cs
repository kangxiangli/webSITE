using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;


namespace Whiskey.ZeroStore.MobileApi.Controllers
{
    public class HomeController : BaseController
    {
        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(HomeController));

        protected readonly IColorContract _colorContract;

        protected readonly IAppArticleContract _appArticleContract;

        protected readonly IMemberSingleProductContract _memberSingleProductContract;

        protected readonly IMemberColloEleContract _memberColloEleContract;
        protected readonly IAppVerManageContract _appVerManageContract;

        public HomeController(IColorContract colorContract,
            IMemberSingleProductContract memberSingleProductContract,
            IMemberColloEleContract memberColloEleContract,
            IAppVerManageContract _appVerManageContract,
            IAppArticleContract appArticleContract)
        {
            _colorContract = colorContract;
            _memberSingleProductContract = memberSingleProductContract;
            _memberColloEleContract = memberColloEleContract;
            _appArticleContract = appArticleContract;
            this._appVerManageContract = _appVerManageContract;
        }
        #endregion
        // GET: Home
        public ActionResult Index()
        {
            var tem = _colorContract.Colors;
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonResult Error()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "操作异常");
            return Json(oper, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Set()
        {
            var list = _memberColloEleContract.MemberColloEles.Where(x => x.EleType == (int)MemberColloEleFlag.ImageEle && !string.IsNullOrEmpty(x.ImagePath));
            List<MemberColloEle> listEle = new List<MemberColloEle>();
            string strApiUrl = ConfigurationHelper.GetAppSetting("ApiUrl");
            foreach (var item in list)
            {
                if (item.ImagePath.Contains(strApiUrl))
                {
                    item.ImagePath = item.ImagePath.Replace(strApiUrl, string.Empty);
                    listEle.Add(item);
                }
            }
            _memberColloEleContract.Update(listEle);
            return Json("Ok");
        }


        public ActionResult Upload()
        {
            var rand = new Random().Next(1, 9999);

            if (Request.Files != null && Request.Files.Count > 0)
            {
                var keys = Request.Files.AllKeys;
                foreach (var key in keys)
                {
                    var savePath = "~/Content/images/" + key + rand + ".png";
                    var res = ImageHelper.SaveOriginImg(Request.Files[key].InputStream, savePath);
                    if (!res)
                    {
                        return Json(new OperationResult(OperationResultType.Error, "上传图片失败"));
                    }
                }
                return Json(new { files = string.Join(",", Request.Files.AllKeys) }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { files = new string[] { } }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult CheckUpdate(string version, AppTypeFlag? AppFlag)
        {
            var result = _appVerManageContract.CheckUpdate(version, AppFlag);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}