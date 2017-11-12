using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Entities.Template;
using System.Net;
using System.IO;
using System.Text;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Templates.Controllers
{
    [License(CheckMode.Verify)]
    public class TemplateController : BaseController
    {
        #region 声明业务层操作对象
        /// <summary>
        /// 初始化日志
        /// </summary>
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(TemplateController));

        /// <summary>
        /// 声明业务层操作对象
        /// </summary>
        protected readonly ITemplateContract _templateContract;

        protected readonly IHtmlItemContract _htmlItemContract;

        protected readonly IHtmlPartContract _htmlPartContract;
        /// <summary>
        /// 构造函数-初始化操作对象 
        /// </summary>
        /// <param name="templateContract">业务层操作对象</param>
        public TemplateController(ITemplateContract templateContract, 
             IHtmlPartContract htmlPartContract,
            IHtmlItemContract htmlItemContract)
        {
            _templateContract = templateContract;
            _htmlItemContract = htmlItemContract;
            _htmlPartContract = htmlPartContract;
        }
        #endregion

        #region 初始化界面
        /// <summary>
        /// 初始化模版界面
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
        /// 根据条件获取模版数据集
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Template, bool>> predicate = FilterHelper.GetExpression<Template>(request.FilterGroup);
            Article a = new Article();
            int count = 0;
            var data = await Task.Run(() =>
            {
                var list = _templateContract.Templates.Where<Template, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.TemplateName,                    
                    m.UpdatedTime,
                    m.TemplatePath,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.IsDefault,
                    RealName = m.Operator == null ? string.Empty : m.Operator.Member.RealName,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 初始化创建模版界面
        /// </summary>        
        /// <returns></returns>
        public ActionResult Create()
        {
            string title="请选择";
            var listHtmlPart = _htmlPartContract.SelectList(title);
            ViewBag.HtmlPart = listHtmlPart;
            return PartialView();
        }
        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="template">数据载体对象</param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Create(TemplateDto dto)
        {
            dto.TemplateType = (int)TemplateFlag.Article;
            var res =  _templateContract.Insert(dto);
            return Json(res);
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 初始化修改数据界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Update(int Id) 
        {
            string title = "请选择";
            var parts = _htmlPartContract.SelectList(title);
            var result = _templateContract.Edit(Id);
            ViewBag.HtmlPart = parts;
            return PartialView(result);
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="template">数据载体对象</param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Update(TemplateDto dto)
        {
            dto.TemplateType = (int)TemplateFlag.Article;
            var res = _templateContract.Update(dto);
            return Json(res);
        }
        #endregion

        #region 获取数据详情
        public ActionResult View(int Id) 
        {
                
            var result = _templateContract.Templates.Where(x => x.Id == Id).FirstOrDefault();
            return PartialView(result);
        }
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
            var result = _templateContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
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
            var result = _templateContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 设为默认模板
        /// <summary>
        /// 设置默认模板
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult SetDefault(int Id)
        {
            var result = _templateContract.SetDefault(Id, TemplateFlag.Article);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}