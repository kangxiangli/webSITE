using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Filter;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Data;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Templates.Controllers
{
    public class HtmlPartController : Controller
    {
        #region 声明业务层操作对象
        /// <summary>
        /// 声明业务层操作对象
        /// </summary>
        protected readonly IHtmlPartContract _htmlPartContract;
        /// <summary>
        /// 构造函数-初始化业务层操作对象
        /// </summary>
        /// <param name="templateCSSContract"></param>
        public HtmlPartController(IHtmlPartContract htmlPartContract) 
        {
            _htmlPartContract = htmlPartContract;
        }

        #endregion

        #region 初始化界面
        /// <summary>
        /// 初始化界面
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
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<HtmlPart, bool>> predicate = FilterHelper.GetExpression<HtmlPart>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                int count = 0;
                var list = _htmlPartContract.HtmlParts.AsQueryable().Where<HtmlPart, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.PartName,                    
                    m.UpdatedTime,
                    m.IsEnabled,
                    m.IsDeleted,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);

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
        /// <param name="Summary">简介</param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Create(HtmlPartDto dto)
        {
            var oper = _htmlPartContract.Insert(dto);
            return Json(dto);
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 初始化修改界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var css = _htmlPartContract.Edit(Id);
            return PartialView(css);
        }
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="CSS"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Update(HtmlPartDto dto)
        {
            var oper = _htmlPartContract.Update(dto);
            return Json(dto);
        }

        #endregion

        #region 查看详情
        public ActionResult View(int id)
        {
            HtmlPart htmlPart = _htmlPartContract.View(id);
            return PartialView(htmlPart);
        }
        #endregion

        #region 删除数据
        public JsonResult Delete(int Id)
        {
            OperationResult result = _htmlPartContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
         
    }
}