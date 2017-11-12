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
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    //[License(CheckMode.Verify)]
    public class PartnerLevelController : BaseController
    {
        #region 声明业务层操作对象
                
        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(ChangeRestController));

        protected readonly IPartnerLevelContract _partnerLevelContract;

        public PartnerLevelController(IPartnerLevelContract partnerLevelContract)
        {
            _partnerLevelContract = partnerLevelContract;            
        }
        #endregion

        #region 初始化界面
                
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 获取数据列表
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<PartnerLevel, bool>> predicate = FilterHelper.GetExpression<PartnerLevel>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _partnerLevelContract.PartnerLevels.Where<PartnerLevel, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.LevelName,
                    m.Level,
                    m.Experience,
                    m.Price,
                    m.CouponQuantity,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,                     
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加数据
        [HttpGet]
        public ActionResult Create()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult Create(PartnerLevelDto dto)
        {
            OperationResult oper = _partnerLevelContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        #region 编辑数据
        public ActionResult Update(int Id)
        {
            PartnerLevelDto dto = _partnerLevelContract.Edit(Id);
            return PartialView(dto);
        }

        [HttpPost]
        public JsonResult Update(PartnerLevelDto dto)
        {
            OperationResult oper =  _partnerLevelContract.Update(dto);
            return Json(oper);
        }
        #endregion

        #region 查看数据
        public ActionResult View(int Id)
        {
            PartnerLevel entity = _partnerLevelContract.View(Id);
            return PartialView(entity);
        }
        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult Remove(int Id)
        {
            var res = _partnerLevelContract.Remove(Id);
            return Json(res);
        }
        #endregion

        #region 恢复数据
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult Recovery(int Id)
        {
            var res = _partnerLevelContract.Recovery(Id);
            return Json(res);
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
            var result = _partnerLevelContract.Enable(Id);
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
            var result = _partnerLevelContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}