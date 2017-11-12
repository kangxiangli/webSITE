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

namespace Whiskey.ZeroStore.ERP.Website.Areas.Subjects.Controllers
{

    [License(CheckMode.Verify)]
	public class SubjectController : BaseController
    {
        #region 声明操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(SubjectController));

        protected readonly ISubjectContract _subjectContract;

		public SubjectController(ISubjectContract subjectContract) {
			_subjectContract = subjectContract;
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

        #region 展示数据列表
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Subject, bool>> predicate = FilterHelper.GetExpression<Subject>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                Func<List<Subject>, List<Subject>> getTree = null;
                getTree = (source) =>
                {
                    var children = source.OrderBy(o => o.Sequence).ThenBy(o => o.Id);
                    List<Subject> tree = new List<Subject>();
                    foreach (var child in children)
                    {
                        tree.Add(child);
                        var chil = _subjectContract.Subjects.Where(c => c.ParentId == child.Id).ToList();
                        tree.AddRange(getTree(chil));
                    }
                    return tree;
                };

                var parents = _subjectContract.Subjects.Where(m => m.ParentId == null).ToList();
                var count = 0;
                var list = getTree(parents).AsQueryable().Where(predicate).Select(m => new
                {
                    m.SubjectName,
                    m.Summary,
                    m.Id,
                    m.ParentId,
                    m.Sequence,
                    m.UpdatedTime,
                    m.Operator.Member.MemberName,
                    m.Path,
                    m.IsEnabled,
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
        public ActionResult Create(string SubjectName, string Summary)
        {
            Subject subject = new Subject();
            subject.SubjectName = SubjectName;
            subject.Summary = Summary;
            HttpFileCollectionBase listFile = Request.Files;
            var result = _subjectContract.Insert(subject, listFile);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _subjectContract.Subjects.Where(x => x.Id == Id).FirstOrDefault();
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
        public ActionResult Update(string SubjectName, string Summary, string SubjectId)
        {
            Subject subject = new Subject();
            subject.SubjectName = SubjectName;
            subject.Summary = Summary;
            subject.Id = int.Parse(SubjectId);
            HttpFileCollectionBase listFile = Request.Files;
            var result = _subjectContract.Update(subject, listFile);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _subjectContract.View(Id);
            return PartialView(result);
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
			var result = _subjectContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region 删除数据
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Delete(int[] Id)
        {
			var result = _subjectContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Recovery(int[] Id)
        {
			var result = _subjectContract.Recovery(Id);
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
			var result = _subjectContract.Enable(Id);
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
			var result = _subjectContract.Disable(Id);
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
            var list = _subjectContract.Subjects.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
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
            var list = _subjectContract.Subjects.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #region 设为首页
        /// <summary>
        /// 设为首页
        /// </summary>
        /// <param name="Id">主键ID</param>
        /// <returns></returns>
        public JsonResult SetHomePage(int Id)
        {
           var result= _subjectContract.SetHomePage(Id);
           return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion







    }
}
