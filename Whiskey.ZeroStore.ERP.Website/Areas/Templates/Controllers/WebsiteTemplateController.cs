using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Entities.Template;
using Whiskey.ZeroStore.ERP.Transfers;
using System.Text.RegularExpressions;
using System.Text;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Templates.Controllers
{
    [License(CheckMode.Verify)]
    public class WebsiteTemplateController : BaseController
    {
        #region 声明业务层操作对象
        /// <summary>
        /// 初始化日志
        /// </summary>
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(WebsiteTemplateController));

        /// <summary>
        /// 声明业务层操作对象
        /// </summary>
        protected readonly ITemplateContract _templateContract;

        protected readonly IHtmlItemContract _htmlItemContract;

        protected readonly IArticleItemContract _articleItemContract;

        protected readonly IArticleContract _ArticleContract;

        
        /// <summary>
        /// 构造函数-初始化操作对象 
        /// </summary>
        /// <param name="templateContract">业务层操作对象</param>
        public WebsiteTemplateController(ITemplateContract templateContract,
            IHtmlItemContract htmlItemContract,
            IArticleItemContract articleItemContract,
            IArticleContract articleContract)
        {
            _templateContract = templateContract;
            _htmlItemContract = htmlItemContract;
            _articleItemContract = articleItemContract;
            _ArticleContract = articleContract;            
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

        #region 获取模版列表
        /// <summary>
        /// 根据条件获取模版数据集
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Template, bool>> predicate = FilterHelper.GetExpression<Template>(request.FilterGroup);
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

        #region 修改数据
        /// <summary>
        /// 初始化修改数据界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {

            ViewBag.ArticleItem = _articleItemContract.SelectList("请选择");
            var result = _templateContract.Edit(Id);
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
            dto.TemplateType=(int)TemplateFlag.Website;
            var res = _templateContract.Update(dto);
            return Json(res);
        }
        #endregion

        #region 查看详情
        public ActionResult View(int Id)
        {
            string strId = Request["Id"];
            var result = _templateContract.Templates.Where(x => x.Id == Id).FirstOrDefault();
            return PartialView(result);
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 初始化创建模版界面
        /// </summary>        
        /// <returns></returns>
        public ActionResult Create()
        {
            var articleItem = _articleItemContract.SelectList("请选择");
            ViewBag.ArticleItem = articleItem;
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
            dto.TemplateType = (int)TemplateFlag.Website;
            var res = _templateContract.Insert(dto);
            return Json(res);

        }

        #endregion

        #region 生成静态页面
        /// <summary>
        /// 生成静态页面
        /// </summary>
        /// <param name="Id">主键Id</param>
        /// <returns></returns>
        public JsonResult Build(int[] Id)
        {
            OperationResult oper = _templateContract.Build(Id);
            return Json(oper);
        }

        #endregion

        #region 校验模版名称是否重复
        /// <summary>
        /// 校验模版名称是否重复
        /// </summary>
        /// <returns></returns>
        public JsonResult CheckTemplateName()
        {
            string templateName = Request["templateName"];
            int result = 0;//0表示接受的模版名称为空，1表示模版名称不存在，2表示模版名称已存在
            if (string.IsNullOrEmpty(templateName))
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                result = _templateContract.CheckTemplateName(templateName, TemplateFlag.Website );
                return Json(result, JsonRequestBehavior.AllowGet);
            }
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

        #region 生成首页
        public ActionResult SetIndex(int Id)
        {
            OperationResult oper = _templateContract.SetIndex(Id);
            return Json(oper);             
        }
        #endregion

        #region 获取文章二级栏目
        /// <summary>
        /// 获取文章栏目二级栏目列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetArticleItemChild()
        {
            int Id = int.Parse(Request["Id"]);

            var listArticleAttr = _articleItemContract.ArticleItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId == Id).Select(x => new
            {
                x.Id,
                Name=x.ArticleItemName,
            });
            return Json(listArticleAttr, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}