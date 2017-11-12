using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Utility.Data;
using System.Linq.Expressions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.Utility.Filter;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Class;
namespace Whiskey.ZeroStore.ERP.Website.Areas.Commons.Controllers
{
    [License(CheckMode.Verify)]
    public class SensitiveWordController : BaseController
    {
        #region 初始化业务层操作对象
        
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(SensitiveWordController));

        protected readonly ISensitiveWordContract _sensitiveWordContract;

        public SensitiveWordController(ISensitiveWordContract sensitiveWordContract)
        {
            _sensitiveWordContract = sensitiveWordContract;
		}
        #endregion

        #region 初始化界面
                
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 获取数据集
        /// <summary>
        /// 获取数据集
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<SensitiveWord, bool>> predicate = FilterHelper.GetExpression<SensitiveWord>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _sensitiveWordContract.SensitiveWords.Where<SensitiveWord, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.WordPattern,
                    m.ReplaceWord,
                    m.IsForbid,
                    m.IsNeutral,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 添加数据
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
        public ActionResult Create(SensitiveWordDto dto)
        {
            var result = _sensitiveWordContract.Insert(dto);
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
            var result = _sensitiveWordContract.Edit(Id);
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
        public ActionResult Update(SensitiveWordDto dto)
        {
            var result = _sensitiveWordContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region 删除数据

        public JsonResult Delete(int Id)
        {
            var res = _sensitiveWordContract.Delete(Id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}