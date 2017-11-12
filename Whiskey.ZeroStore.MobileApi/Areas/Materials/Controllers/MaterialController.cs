using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Areas.Materials.Controllers
{
    public class MaterialController : Controller
    {
        #region 声明业务层操作对象

        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MaterialController));
        //声明业务层操作对象
        protected readonly IGalleryContract _galleryContract;

        //构造函数-初始化业务层操作对象
        public MaterialController(IGalleryContract galleryContract)
        {
            _galleryContract = galleryContract;
        }
        #endregion
        string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        #region 获取搭配素材
        /// <summary>
        /// 获取搭配素材
        /// </summary>
        /// <returns></returns>
        public JsonResult GetList(GalleryFlag? MaterialType, int PageIndex = 1, int PageSize = 10)
        {
            try
            {

                if (MaterialType.HasValue)
                {

                    IQueryable<Gallery> listMaterial = _galleryContract.Gallerys.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.GalleryType == MaterialType);
                    listMaterial = listMaterial.OrderBy(x => x.Id).Skip((PageIndex - 1) * PageSize).Take(PageSize);
                    var listEntity = listMaterial.Select(x => new
                    {
                        MaterialId = x.Id,
                        MaterialName = x.PictureName,
                        IconPath = strWebUrl + x.OriginalPath
                    });
                    return Json(new OperationResult(OperationResultType.Success, "获取成功！", listEntity), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new OperationResult(OperationResultType.Error, "获取的素材不存在，请重试！"), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"), JsonRequestBehavior.AllowGet);
            }

        }
        #endregion
    }
}