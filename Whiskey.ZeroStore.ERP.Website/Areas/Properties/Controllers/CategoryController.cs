




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

namespace Whiskey.ZeroStore.ERP.Website.Areas.Properties.Controllers
{

    [License(CheckMode.Verify)]
	public class CategoryController : BaseController
    {
        #region 初始化操作对象
                
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CategoryController));

        protected readonly ICategoryContract _categoryContract;

		public CategoryController(ICategoryContract categoryContract) {
			_categoryContract = categoryContract;
            
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

        #region 获取数据列表

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Category, bool>> predicate = FilterHelper.GetExpression<Category>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                Func<ICollection<Category>, List<Category>> getTree = null;
                getTree = (source) =>
                {
                    var children = source.OrderBy(o => o.Sequence).ThenBy(o => o.Id);
                    List<Category> tree = new List<Category>();
                    foreach (var child in children)
                    {
                        tree.Add(child);
                        tree.AddRange(getTree(child.Children));
                    }
                    return tree;
                };
                int count;
                var parents = _categoryContract.Categorys.Where(m => m.ParentId == null).Where<Category,int>(predicate,request.PageCondition,out count).ToList();
                var list = getTree(parents).AsQueryable().Where(predicate).Select(m => new
                {
                    m.ParentId,
                    m.CategoryName,
                    m.Description,
                    m.CategoryCode,
                    //m.CategoryLevel,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.Operator.Member.MemberName,
                    m.IconPath,                   
                }).ToList();

                return new GridData<object>(list, count, request.RequestInfo);
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
            string title = "请选择";
            ViewBag.Category = _categoryContract.ParentSelectList(title);            
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
        public ActionResult Create(CategoryDto dto)
        {             
            var result = _categoryContract.Insert(dto);             
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
            string title = "请选择";
            ViewBag.Category = _categoryContract.ParentSelectList(title);            
            var result = _categoryContract.Edit(Id);                        
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
        public ActionResult Update(CategoryDto dto)
        {            
            var result = _categoryContract.Update(dto);            
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
            var result = _categoryContract.View(Id);
            return PartialView(result);
        }

        #endregion

        #region 注释代码-查询数据
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        //public async Task<ActionResult> List()
        //{
        //    GridRequest request = new GridRequest(Request);
        //    Expression<Func<Category, bool>> predicate = FilterHelper.GetExpression<Category>(request.FilterGroup);
        //    var data = await Task.Run(() =>
        //    {
        //        var count = 0;
        //        var list = _categoryContract.Categorys.Where<Category, int>(predicate, request.PageCondition, out count).Select(m => new
        //        {
        //            m.ParentId,
        //            m.CategoryName,
        //            m.CategoryCode,
        //            m.CategoryLevel,
        //            m.Description,
        //            m.Id,
        //            m.IsDeleted,
        //            m.IsEnabled,
        //            m.Sequence,
        //            m.UpdatedTime,
        //            m.CreatedTime,
        //            m.Operator.AdminName,
        //        }).ToList();
        //        return new GridData<object>(list, count, request.RequestInfo);
        //    });
        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region 注释代码-查询数据
        
        ///// <summary>
        ///// AjaxTree查询数据
        ///// </summary>
        ///// <returns></returns>
        //public async Task<ActionResult> AjaxTree(int? id)
        //{
        //    var data = await Task.Run(() =>
        //    {
        //        var list = _productcategoryContract.Categorys.Where(m => m.ParentId == (id == 0 ? new Nullable<int>() : id)).Select(m => new
        //        {
        //            id = m.Id,
        //            name = m.CategoryName,
        //            level = m.CategoryLevel,
        //            type = "default"
        //        }).ToList();
        //        return new { nodes = list };
        //    });
        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}



        ///// <summary>
        ///// TreeGrid查询数据
        ///// </summary>
        ///// <returns></returns>
        //public async Task<ActionResult> TreeGrid()
        //{

        //    Func<Category, ICollection<Category>, Tree> getTree = null;
        //    getTree = (category, source) =>
        //    {
        //        Tree tree = new Tree(category);

        //        List<Category> children = source.Where(m => m.ParentId == tree.Id).ToList();

        //        foreach (Category child in children)
        //        {
        //            Tree childTree = getTree(child, source);
        //            tree.Children.Add(childTree);
        //        }
        //        return tree;
        //    };

        //    var data = await Task.Run(() =>
        //    {
        //        var roots = _productcategoryContract.Categorys.Where(m => m.ParentId == null).OrderBy(o => o.Sequence).ToList();

        //        List<Tree> list = (from root in roots
        //                           let source = _productcategoryContract.Categorys.ToList()
        //                           select getTree(root, source)).ToList();
        //        return list;
        //    });
        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Remove(int[] Id)
        {
			var result = _categoryContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 删除数据
        ///// <summary>
        ///// 删除数据
        ///// </summary>
        ///// <param name="Id"></param>
        ///// <returns></returns>
        //[Log]
        //[HttpPost]
        //public ActionResult Delete(int[] Id)
        //{
        //    var result = ""//_categoryContract.Delete(Id);
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region 恢复数据
                
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Recovery(int[] Id)
        {
			var result = _categoryContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 启用数据
                
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Enable(int[] Id)
        {
			var result = _categoryContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 禁用数据
                
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Disable(int[] Id)
        {
			var result = _categoryContract.Disable(Id);
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
            var list = _categoryContract.Categorys.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
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
            var list = _categoryContract.Categorys.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
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
            string savePath = ConfigurationHelper.GetAppSetting("CategoryIconPath") + DateTime.Now.ToString("yyyyMMddhhmmss") + ".png";
            var file = Request.Files;
            bool result = false;
            for (int i = 0; i < file.Count; i++)
            {
                result = FileHelper.SaveUpload(file[i].InputStream, savePath);
            }
            if (result)
            {
                string url = ConfigurationHelper.GetAppSetting("WebUrl") + savePath;
                return Json(new { ResultType = OperationResultType.Success, Path = savePath }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { ResultType = OperationResultType.Error, path = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion


    }
}
