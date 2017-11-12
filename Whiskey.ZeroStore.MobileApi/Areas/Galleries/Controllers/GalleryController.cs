using System.Linq;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;

namespace Whiskey.ZeroStore.MobileApi.Areas.Galleries.Controllers
{
    [License(CheckMode.Verify)]
    public class GalleryController : Controller
    {
        #region 初始化业务层操作对象
        
        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(GalleryController));
        
        //声明业务层操作对象        
        protected readonly IGalleryContract _galleryContract;

         
        //构造函数-初始化业务层操作对象
        public GalleryController(IGalleryContract galleryContract)
        {
            _galleryContract = galleryContract;                         
        }
        #endregion
        public string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");
        #region 获取动态图
        [HttpPost]
        public JsonResult Get()
        {                         
            IQueryable<Gallery> listGallery = _galleryContract.Gallerys.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.GalleryType == GalleryFlag.Dynamic);
            var entity = listGallery.Select(x => new
            { 
                x.Id,
                ThumbnailPath=strWebUrl+x.ThumbnailPath,
            });
            OperationResult oper = new OperationResult(OperationResultType.Success, "获取成功", entity);
            return Json(oper);
        }
        #endregion        
    }
}