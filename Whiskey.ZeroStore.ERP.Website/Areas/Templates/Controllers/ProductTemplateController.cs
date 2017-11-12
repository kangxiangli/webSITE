using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using System.Collections.Generic;
using Whiskey.Web.Helper;
using Whiskey.Utility.Class;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Templates.Controllers
{
    [License(CheckMode.Verify)]
    public class ProductTemplateController : BaseController
    {
        #region 声明业务层操作对象
        /// <summary>
        /// 初始化日志
        /// </summary>
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ProductTemplateController));

        /// <summary>
        /// 声明业务层操作对象
        /// </summary>
        protected readonly ITemplateContract _templateContract;
        protected readonly IHtmlItemContract _htmlItemContract;
        protected readonly IProductOrigNumberContract _productOrigNumberContract;
        protected readonly IAdministratorContract _administratorContract;

        /// <summary>
        /// 构造函数-初始化操作对象 
        /// </summary>
        /// <param name="templateContract">业务层操作对象</param>
        public ProductTemplateController(ITemplateContract templateContract,
             IProductOrigNumberContract _productOrigNumberContract,
             IAdministratorContract _administratorContract,
            IHtmlItemContract htmlItemContract)
        {
            _templateContract = templateContract;
            _htmlItemContract = htmlItemContract;
            this._productOrigNumberContract = _productOrigNumberContract;
            this._administratorContract = _administratorContract;
        }
        #endregion

        #region 初始化界面
        
        
        // GET: Templates/ProductTemplate
        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ContentDetail()
        {
            return PartialView();
        }

        #endregion

        #region 获取数据列表

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Template, bool>> predicate = FilterHelper.GetExpression<Template>(request.FilterGroup);
            var data = await Task.Run(() =>
            {                 
                IQueryable<Template> listTemplate = _templateContract.Templates.Where(m => m.TemplateType == (int)TemplateFlag.Product);
                var list = listTemplate.Where(predicate).Select(m => new
                {
                    m.Id,
                    m.TemplateName,
                    m.TemplatePath,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.IsDefault,
                    m.IsDefaultPhone,
                    m.UpdatedTime,
                    RealName = m.Operator == null ? string.Empty : m.Operator.Member.RealName,
                }).ToList();
                return new GridData<object>(list, list.Count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 初始化添加界面
        /// </summary>
        /// <returns></returns>        
        public ActionResult Create()
        {
            
            return PartialView();
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Log]
        [ValidateInput(false)]
        public JsonResult Create(TemplateDto dto)
        {
            dto.TemplateType = (int)TemplateFlag.Product;
            var result = _templateContract.Insert(dto);
            return Json(result,JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 更新数据
        /// <summary>
        /// 更新数据界面
        /// </summary>
        /// <returns></returns>        
        public ActionResult Update(int Id)
        {

            TemplateDto dto = _templateContract.Edit(Id);
            string strHeaderName = string.Empty;
            string strFooterName = string.Empty;
            
            ViewBag.HeaderName = strHeaderName;
            ViewBag.FooterName = strFooterName;
            return PartialView(dto);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [Log]
        [ValidateInput(false)]
        public JsonResult Update(TemplateDto dto)
        {
            dto.TemplateType = (int)TemplateFlag.Product;
            var result = _templateContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 查看数据详情
        /// <summary>
        /// 查看数据详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            var result = _templateContract.View(Id);
            return PartialView(result);
        }
        #endregion

        #region 设为默认模板
        /// <summary>
        /// 设置默认模板
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public  JsonResult SetDefault(int Id,TemplateTypeFlag type)
        {
            var result = _templateContract.SetDefault(Id,TemplateFlag.Product, type);
            return Json(result, JsonRequestBehavior.AllowGet);
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

        #region 检测模板有效性

        /// <summary>
        /// 检测模板有效性
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CheckValid(string content)
        {
            string strResult = "";
            var modPro = _productOrigNumberContract.OrigNumbs.FirstOrDefault();
            var modAdmin = _administratorContract.View(AuthorityHelper.OperatorId.Value);
            if (modPro.IsNotNull())
            {
                var dic = new Dictionary<string, object>();
                dic.Add("PONInfo", modPro);
                dic.Add("AdminInfo", modAdmin);

                strResult = NVelocityHelper.Generate(content, dic, "checkValid");

                #region 弃用
                //StringTemplate strTemplate = new StringTemplate(content);
                //strTemplate.SetAttribute("PONInfo", modPro);
                //strResult = strTemplate.ToString();
                #endregion
            }
            strResult = strResult.IsNullOrEmpty() ? "内容建设中..." : strResult;
            return Json(strResult);
        }

        #endregion
    }
}