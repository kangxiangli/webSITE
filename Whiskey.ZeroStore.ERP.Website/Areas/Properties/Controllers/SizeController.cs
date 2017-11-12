using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using AutoMapper;
using Antlr3;
using Antlr3.ST;
using Antlr3.ST.Language;
using Antlr3.ST.Extensions;
using Newtonsoft.Json;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc.Binders;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Properties.Controllers
{

    [License(CheckMode.Verify)]
	public class SizeController : BaseController
    {
        #region 初始化操作对象
                
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(SizeController));

        protected readonly ISizeContract _sizeContract;

        protected readonly ICategoryContract _categoryContract;
        protected readonly ISizeExtentionContract _sizeExtentionContract;
		public SizeController(ISizeContract sizeContract,
            ISizeExtentionContract _sizeExtentionContract,
            ICategoryContract categoryContract)
        {            
            _categoryContract = categoryContract;
			_sizeContract = sizeContract;
            ViewBag.Size = (_sizeContract.SelectList("选择尺码").Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
            this._sizeExtentionContract = _sizeExtentionContract;
        }
        #endregion

        #region 初始化界面
        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            string title = string.Empty;
            IEnumerable<SelectListItem> listParent = _categoryContract.ParentSelectList(title);
            int categoryId = int.Parse(listParent.FirstOrDefault().Value);
            IEnumerable<SelectListItem> listChildren = _categoryContract.ChildrenSelectList(categoryId,title);
            ViewBag.ParentCategory = listParent;
            ViewBag.ChildrenCategory = listChildren;
            ViewBag.SizeExtentions = _sizeExtentionContract.SelectListItem(true);

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
        public ActionResult Create(SizeDto dto)
        {
            var result = _sizeContract.Insert(dto);
            
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 更新数据

        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {            
            var result = _sizeContract.Edit(Id);
            string title = string.Empty;
            IEnumerable<SelectListItem> listParent = _categoryContract.ParentSelectList(title);
            int categoryId = result.CategoryId;
            var category =  _categoryContract.Categorys.Where(x => x.Id == categoryId).FirstOrDefault();
            categoryId = category.Parent.Id;            
            IEnumerable<SelectListItem> listChildren = _categoryContract.ChildrenSelectList(categoryId, title);
            ViewBag.ParentCategory = listParent;
            ViewBag.ChildrenCategory = listChildren;
            ViewBag.ParentCategoryId = categoryId;
            ViewBag.SizeExtentions = _sizeExtentionContract.SelectListItem(true);
            return PartialView(result);
        }


        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(SizeDto dto)
        {
            var result = _sizeContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 查看详情
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _sizeContract.View(Id);
            return PartialView(result);
        }
        #endregion

        #region 获取数据列表
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Size, bool>> predicate = FilterHelper.GetExpression<Size>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                IQueryable<Category> listCategory = _categoryContract.Categorys.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId != null);
                IQueryable<Size> listSize = _sizeContract.Sizes;
                int id = listSize.Select(x => x.Id).Max();
                listSize = listSize.Where(predicate);
                List<SizeInfo> listSizeInfo = new List<SizeInfo>();                
                var cateli=listCategory.OrderByDescending(c=>c.CreatedTime).ThenByDescending(c=>c.Id).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize);
                foreach (var category in cateli)
                {
                    ++id;
                    SizeInfo sizeInfo = new SizeInfo();
                    sizeInfo.Name = category.CategoryName;
                    sizeInfo.Id = id;
                    sizeInfo.UpdatedTime = category.UpdatedTime;
                    sizeInfo.IsDeleted = category.IsDeleted;
                    sizeInfo.IsEnabled = category.IsEnabled;

                    var tempSize = listSize.Where(x => x.CategoryId == category.Id);

                    if (tempSize.Count() > 0)
                    {
                        listSizeInfo.Add(sizeInfo);
                        foreach (var size in tempSize)
                        {
                            listSizeInfo.Add(new SizeInfo { Id = size.Id, Name = size.SizeName, ParentId = sizeInfo.Id, IsDeleted = size.IsDeleted, IsEnabled = size.IsEnabled, UpdatedTime = size.UpdatedTime, SizeCode = size.SizeCode, SizeExtentionName = size.SizeExtention?.Name ?? string.Empty });
                        }
                    }
                }
                return new GridData<object>(listSizeInfo,listCategory.Count(), request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 物理删除数据

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Delete(int[] Id)
        {
            var result = _sizeContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 移除和恢复数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Remove(int[] Id)
        {
			var result = _sizeContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Recovery(int[] Id)
        {
			var result = _sizeContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 启用和禁用数据
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Enable(int[] Id)
        {
			var result = _sizeContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Disable(int[] Id)
        {
			var result = _sizeContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 打印和导出数据
        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _sizeContract.Sizes.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _sizeContract.Sizes.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 上传图片
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        public JsonResult UploadImage()
        {
            string conPath = ConfigurationHelper.GetAppSetting("SizeIconPath");
            string savePath = conPath;
            Guid gid = Guid.NewGuid();
            string fileName = gid.ToString();
            fileName = fileName.Substring(0, 15) + ".png";
            savePath = savePath + fileName;
            var file = Request.Files;
            bool result = false;
            for (int i = 0; i < file.Count; i++)
            {
                result = FileHelper.SaveUpload(file[i].InputStream, savePath);
            }
            if (result)
            {
                return Json(new { ResultType = OperationResultType.Success, Path = savePath }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { ResultType = OperationResultType.Error, path = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region 获取二级品类
        [HttpPost]
        public JsonResult GetCategory(int CategoryId)
        {
            var entity = _categoryContract.Categorys.Where(x => x.ParentId == CategoryId).Select(x => new
            {
                x.CategoryName,
                x.Id
            });
            return Json(entity);
        }
        #endregion
    }
}
