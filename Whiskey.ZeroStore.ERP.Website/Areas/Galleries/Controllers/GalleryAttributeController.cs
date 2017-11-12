




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

namespace Whiskey.ZeroStore.ERP.Website.Areas.Galleries.Controllers
{

    [License(CheckMode.Verify)]
	public class GalleryAttributeController : BaseController
	{
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(GalleryAttributeController));

        protected readonly IGalleryAttributeContract _galleryattributeContract;

        public GalleryAttributeController(IGalleryAttributeContract galleryattributeContract)
        {
            _galleryattributeContract = galleryattributeContract;
            ViewBag.GalleryAttribute = (_galleryattributeContract.SelectList("选择分类").Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
        }


        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
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
        public ActionResult Create(GalleryAttributeDto dto)
        {
            var result = _galleryattributeContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(GalleryAttributeDto dto)
        {
            var result = _galleryattributeContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result= _galleryattributeContract.Edit(Id);
            return PartialView(result);
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _galleryattributeContract.View(Id);
            return PartialView(result);
        }


        ///// <summary>
        ///// 查询数据
        ///// </summary>
        ///// <returns></returns>
        //public async Task<ActionResult> List()
        //{
        //    GridRequest request = new GridRequest(Request);
        //    Expression<Func<GalleryAttribute, bool>> predicate = FilterHelper.GetExpression<GalleryAttribute>(request.FilterGroup);
        //    var data = await Task.Run(() =>
        //    {
        //        var count = 0;
        //        var list = _galleryattributeContract.GalleryAttributes.Where<GalleryAttribute, int>(predicate, request.PageCondition, out count).Select(m => new
        //        {
        //            m.ParentId,
        //            m.AttributeName,
        //            m.Description,
        //            m.AttributeLevel,
        //            m.AttributeImage,
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

        /// <summary>
        /// 数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<GalleryAttribute, bool>> predicate = FilterHelper.GetExpression<GalleryAttribute>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                Func<ICollection<GalleryAttribute>, List<GalleryAttribute>> getTree = null;
                getTree = (source) =>
                {
                    var children=source.OrderBy(o=>o.Sequence).ThenBy(o=>o.Id);
                    List<GalleryAttribute> tree = new List<GalleryAttribute>();
                    foreach (var child in children)
                    {
                        tree.Add(child);
                        tree.AddRange(getTree(child.Children));
                    }
                    return tree;
                };
              
                var parents = _galleryattributeContract.GalleryAttributes.Where(m => m.ParentId == null).ToList();
                var list = getTree(parents).AsQueryable().Where(predicate).Select(m => new
                {
                    m.ParentId,
                    m.AttributeName,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.Operator.Member.MemberName,
                    m.IconPath,
                }).ToList();

                return new GridData<object>(list, list.Count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 树状列表
        /// </summary>
        /// <returns></returns>
        [License(CheckMode.Check)]
        public async Task<ActionResult> TreeList()
        {
            var data = await Task.Run(() =>
            {
                var list = _galleryattributeContract.GalleryAttributes.Where(o =>o.IsEnabled&&!o.IsDeleted).OrderBy(o => o.Sequence).ThenBy(t => t.CreatedTime).Select(m => new
                {
                    id = m.Id,
                    pid = m.ParentId,
                    name = m.AttributeName
                });
                return list;
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Remove(int[] Id)
        {
			var result = _galleryattributeContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Delete(int[] Id)
        {
			var result = _galleryattributeContract.Delete(Id);
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
			var result = _galleryattributeContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Enable(int[] Id)
        {
			var result = _galleryattributeContract.Enable(Id);
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
			var result = _galleryattributeContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _galleryattributeContract.GalleryAttributes.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
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
            var list = _galleryattributeContract.GalleryAttributes.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 属性描述
        /// </summary>
        /// <param name="Id"></param>  
        /// <returns></returns>
        [License(CheckMode.Check)]
        public ActionResult Tooltip(int Id)
        {
            var result = new OperationResult(OperationResultType.Error, "加载属性描述失败！");
            var entity = _galleryattributeContract.View(Id);
            if (entity != null)
            {
                result = new OperationResult(OperationResultType.Success, "加载属性描述成功！", new { Description = entity.Description });
            }
            return Json(result);
        }




    }
}
