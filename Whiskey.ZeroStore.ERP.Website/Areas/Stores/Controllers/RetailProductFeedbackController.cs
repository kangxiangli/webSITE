using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{

    [License(CheckMode.Verify)]
    public class RetailProductFeedbackController : BaseController
    {
        private static readonly ILogger _Logger = LogManager.GetLogger(typeof(RetailProductFeedbackController));

        private readonly IRetailProductFeedbackContract _contract;
        private readonly IAdministratorContract _adminContract;
        private readonly IModuleContract _moduleContract;
        private readonly IPermissionContract _permissionContract;

        public RetailProductFeedbackController(IRetailProductFeedbackContract contract,
            IAdministratorContract adminContract,
            IModuleContract moduleContract,
            IPermissionContract permissionContract)
        {
            _contract = contract;
            _adminContract = adminContract;
            _moduleContract = moduleContract;
            _permissionContract = permissionContract;
        }

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
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        public ActionResult View(int Id)
        {
            var result = _contract.Entities.Where(e => e.Id == Id).FirstOrDefault();
            return PartialView(result);
        }
        /// <summary>
        /// 查看/创建/修改数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit(int? id)
        {
            if (id.HasValue)
            {
                var model = _contract.View(id.Value);
                return PartialView("Edit", model);
            }
            else
            {
                return PartialView("Edit", new CollocationPlan());
            }
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(string productNumber, string retailNumber, DateTime? startDate, DateTime? endDate, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {

            var query = _contract.Entities;
            query.Where(e => e.IsEnabled == isEnabled);
            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(productNumber) && productNumber.Length > 0)
            {

                query = query.Where(e => e.Product.ProductNumber.StartsWith(productNumber));
            }

            if (!string.IsNullOrEmpty(retailNumber) && retailNumber.Length > 0)
            {

                query = query.Where(e => e.Retail.RetailNumber.StartsWith(retailNumber));

            }


            var list = query.OrderByDescending(e => e.UpdatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                e.RatePoints,
                                e.Retail.RetailNumber,
                                e.Product.ProductNumber,
                                e.Retail.Store.StoreName,
                                e.Retail.Consumer.RealName,
                                ThumbnailPath = e.Product.ThumbnailPath??e.Product.ProductOriginNumber.ThumbnailPath,
                                e.Feedbacks,
                                e.Id,
                                e.IsDeleted,
                                e.IsEnabled,
                                e.CreatedTime,
                                e.UpdatedTime,
                                OptName = e.Operator.Member.MemberName
                            }).ToList();


            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = list,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = query.Count(),
                }
            });

            return Json(res, JsonRequestBehavior.AllowGet);
        }





        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _contract.DeleteOrRecovery(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _contract.DeleteOrRecovery(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _contract.EnableOrDisable(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _contract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 300)]
        public ActionResult QueryPageFlag(bool isValid)
        {
            var area = RouteData.DataTokens.ContainsKey("area") ? RouteData.DataTokens["area"].ToString() : string.Empty;
            var controller = RouteData.Values["controller"].ToString();

            var pageUrl = string.Format("{0}/{1}/Index", area, controller);
            var permisstionList = new List<Permission>();
            try
            {
                if (isValid)  // 获取拥有的权限标识
                {
                    var res = PermissionHelper.GetCurrentUserPagePermission(pageUrl, _adminContract, _moduleContract, _permissionContract);
                    if (res != null && res.Any())
                    {
                        permisstionList.AddRange(res);
                    }

                }
                else  // 获取需要屏蔽的权限标识
                {
                    var res = PermissionHelper.GetCurrentUserPageNoPermission(pageUrl, _adminContract, _moduleContract, _permissionContract);
                    if (res != null && res.Any())
                    {
                        permisstionList.AddRange(res);
                    }
                }


                var flags = permisstionList.Where(p => !string.IsNullOrEmpty(p.OnlyFlag))
                                            .Select(p => p.OnlyFlag)
                                            .ToList();
                return Json(new OperationResult(OperationResultType.Success, string.Empty, flags), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _Logger.Error("权限包含的页面元素加载出错，错误如下：" + ex.Message + "。");
                throw new Exception("error");
            }

        }



        public ActionResult ConfigOption()
        {

            return PartialView();
        }

        public ActionResult GetOptions()
        {
            var dto = _contract.GetOptions();
            var res = new OperationResult(OperationResultType.Success, string.Empty, dto);

            return Json(res, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult UpdateOptions(string options)
        {
            var dto = JsonHelper.FromJson<List<RetailProductFeedbackOptionDto>>(options);
            var res = _contract.UpdateOptions(dto);
            return Json(res);
        }


        [HttpPost]
        public ActionResult SubmitFeedbacks(int memberId, string retailNumber, string feedbacks)
        {

            var entries = JsonHelper.FromJson<List<RetailProductFeedbackEntry>>(feedbacks);
            var res = _contract.SubmitFeedbacks(memberId, retailNumber, entries);
            return Json(res);
        }


    }

}