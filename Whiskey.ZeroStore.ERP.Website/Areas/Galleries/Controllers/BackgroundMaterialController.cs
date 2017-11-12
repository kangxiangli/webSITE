using System;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Galleries.Controllers
{
    [License(CheckMode.Verify)]
    public class BackgroundMaterialController : Controller
    {
        #region 声明数据层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(BackgroundMaterialController));

        protected readonly IGalleryContract _galleryContract;

        protected readonly IGalleryAttributeContract _galleryAttrContract;
        public BackgroundMaterialController(IGalleryContract galleryContract,
            IGalleryAttributeContract galleryAttrContract)
        {
            _galleryContract = galleryContract;
            _galleryAttrContract = galleryAttrContract;
        }
        #endregion

        #region 初始化操作界面

        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 查询数据
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
         
        public async Task<ActionResult> Waterfall()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Gallery, bool>> predicate = FilterHelper.GetExpression<Gallery>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _galleryContract.Gallerys.Where(x => x.GalleryType == (int)GalleryFlag.Background).Where<Gallery, int>(predicate, request.PageCondition, out count).Select(m => new
                {                   
                    m.Id,
                    m.PictureName,
                    m.ThumbnailPath,
                    m.OriginalPath,                   
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                }).ToList();
                return new { total = list.Count, result = list };
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 添加数据

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            //ViewBag.GalleryAttribute = (_galleryAttrContract.SelectList("选择分类").Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
            return PartialView();
        }


        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(GalleryDto dto)
        {
            var result = _galleryContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 查看详情
        public ActionResult View(int Id)
        {
            var dto = _galleryContract.Edit(Id);
            return PartialView(dto);
        }
        #endregion

        #region 编辑数据
        /// <summary>
        /// 编辑数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id) 
        {
            var entity = _galleryContract.Edit(Id);
            return PartialView(entity);
        }

        /// <summary>
        /// 保存编辑
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(GalleryDto dto)
        {
            var res = _galleryContract.Update(dto);
            return Json(res);
        }
        #endregion

        #region 删除数据
        public  JsonResult Remove(int Id)
        {
            var res = _galleryContract.Remove(Id);
            return Json(res);
        }
        #endregion

        #region 下载图片
        public ActionResult DownloadImage()
        {
            string strPath=Request["Path"];
                      
            ////文件路径   //文件类型   
            //return File(savePath, "application/vnd.ms-excel");  
            if (string.IsNullOrEmpty(strPath)) return Content("文件不存在，无法下载！");
            string savePath = FileHelper.UrlToPath(strPath);  
            if (!System.IO.File.Exists(savePath)) return Content("文件不存在，可能已被删除！");
            string suffix=savePath.Substring(savePath.LastIndexOf(".")+1);
            var fileName = "123." + suffix;                        
            Response.Clear();
            Response.ClearHeaders();
            Response.AppendHeader("Content-Disposition", "attachment;filename=" + fileName);
           
            //Response.Redirect("http://localhost:5679/");
            return File(new FileStream(savePath, FileMode.Open), "application/octet-stream");
        }
        #endregion


    }
}