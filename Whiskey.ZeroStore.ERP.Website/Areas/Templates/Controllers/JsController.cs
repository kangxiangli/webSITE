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
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Data;
using System.Text;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Templates.Controllers
{
    [License(CheckMode.Verify)]
    public class JsController : BaseController
    {
        #region 声明业务层操作对象
        /// <summary>
        /// 声明业务层操作对象
        /// </summary>
        protected readonly IHtmlItemContract _htmlItemContract;
        /// <summary>
        /// 构造函数-初始化业务层操作对象
        /// </summary>
        /// <param name="templateJSContract"></param>
        public JsController(IHtmlItemContract htmlItemContract) 
        {
            _htmlItemContract = htmlItemContract;
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

        #region 获取JS列表
        /// <summary>
        /// 获取JS列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<HtmlItem, bool>> predicate = FilterHelper.GetExpression<HtmlItem>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                int count = 0;
                var list = _htmlItemContract.HtmlItems.AsQueryable().Where<HtmlItem, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.ItemName,
                    m.SavePath,                     
                    m.UpdatedTime,
                    m.IsEnabled,
                    m.IsDeleted,
                    RealName = m.Operator == null ? string.Empty : m.Operator.Member.RealName,
                    m.Notes,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);                

            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加JS
        /// <summary>
        /// 初始化添加JS界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Create() 
        {
            return PartialView();
        }

       
        /// <summary>
        /// 添加JS
        /// </summary>
        /// <param name="Summary">简介</param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Create(HtmlItemDto JSDto) 
        {                       
            HttpFileCollectionBase listFile = Request.Files;
            //获取配置文件下JS的保存路径
            string jsPath=ConfigurationHelper.GetAppSetting("JSPath");
            List<OperationResult> listOper  = _htmlItemContract.Insert(listFile,JSDto,HtmlItemFlag.Js,jsPath);
            return Json(listOper);
        }
        #endregion

        #region 修改JS
        /// <summary>
        /// 初始化修改界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            HtmlItemDto js=_htmlItemContract.Edit(Id);
            return PartialView(js);
        }
        /// <summary>
        /// 修改JS文件
        /// </summary>
        /// <param name="js"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Update(HtmlItemDto dto)
        {
            HttpFileCollectionBase listFile = Request.Files;
            var entity = _htmlItemContract.View(dto.Id);
            string path = string.Empty;
            if (entity != null)
            {
                path = entity.SavePath;
            }
            dto.HtmlItemType = (int)HtmlItemFlag.Js;
            List<OperationResult> listOper = _htmlItemContract.Update(listFile, dto, HtmlItemFlag.Js, path);
            return Json(listOper);

        }

        #endregion

        #region 查看详情
        public ActionResult ViewDetail(int id) 
        {
            HtmlItem js= _htmlItemContract.HtmlItems.Where(x => x.Id == id).FirstOrDefault();
            return PartialView(js);
        }
        #endregion

        #region 删除数据
        public JsonResult Delete(int Id)
        {
            OperationResult result = _htmlItemContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取JS列表
        /// <summary>
        /// 获取JS列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetList()
        {
            return PartialView();
        }
        #endregion

        #region 获取JS
        /// <summary>
        /// 获取JS集合
        /// </summary>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GetJSList()
        {
            var result = _htmlItemContract.HtmlItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.HtmlItemType==(int)HtmlItemFlag.Js).Select(x => new
            {
                JSName=x.ItemName,
                JSPath=x.SavePath,
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}