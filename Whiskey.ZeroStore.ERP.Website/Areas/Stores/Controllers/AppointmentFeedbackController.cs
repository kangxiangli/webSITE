
using System;
using System.Linq;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.Utility.Data;
using System.Data.Entity;
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using Whiskey.ZeroStore.ERP.Models;
using System.Collections.Generic;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models.DTO;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    [License(CheckMode.Verify)]
    public class AppointmentFeedbackController : BaseController
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(AppointmentFeedbackController));

        private readonly IAppointmentFeedbackContract _contract;
        private readonly IAdministratorContract _adminContract;
        private readonly IModuleContract _moduleContract;
        private readonly IPermissionContract _permissionContract;
        private readonly ICollocationTemplateContract _collocationTemplateContract;
        private readonly IProductContract _productContract;
        private readonly IProductOrigNumberContract _productOrigNumberContract;
        private readonly IMemberCollocationContract _memberCollocationContract;
        private readonly IAppointmentContract _appointmentContract;
        private readonly IStoreContract _storeContract;


        public AppointmentFeedbackController(IAppointmentFeedbackContract contract,
            IAdministratorContract adminContract,
            IModuleContract moduleContract,
            IPermissionContract permissionContract,
            ICollocationTemplateContract collocationTemplateContract,
            IProductContract productContract,
            IProductOrigNumberContract productOrigNumberContract,
            IMemberCollocationContract memberCollocationContract,
            IAppointmentContract appointmentContract
            , IStoreContract storeContract
            )
        {
            _adminContract = adminContract;
            _contract = contract;
            _moduleContract = moduleContract;
            _permissionContract = permissionContract;
            _collocationTemplateContract = collocationTemplateContract;
            _productContract = productContract;
            _productOrigNumberContract = productOrigNumberContract;
            _memberCollocationContract = memberCollocationContract;
            _appointmentContract = appointmentContract;
            _storeContract = storeContract;
        }

        [Layout]
        public ActionResult Index(Int64? number)
        {
            if (number.HasValue)
            {
                ViewBag.number = number.Value.ToString();
            }
            else
            {
                ViewBag.number = string.Empty;
            }
            return View();
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
            var dto = JsonHelper.FromJson<List<AppointmentFeedbackOptionDto>>(options);

            var res = _contract.UpdateOptions(dto);
            return Json(res);
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

        private string SaveImg(string imgBase64Str)
        {
            var mimeType = imgBase64Str.Substring(0, imgBase64Str.IndexOf(';'));
            var extention = mimeType.Substring(mimeType.IndexOf('/') + 1);
            var serverPath = "/Content/Images/CollocationPlan/" + Guid.NewGuid().ToString("N") + "." + extention;
            bool res = ImageHelper.SaveBase64Image(imgBase64Str, serverPath);
            return serverPath;
        }



        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(string number, DateTime? startDate, DateTime? endDate, string ids, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {
            var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);

            var query = _contract.Entities;
            query = query.Where(e => e.IsEnabled == isEnabled && storeIds.Contains(e.Appointment.StoreId));


            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime <= endDate.Value);
            }


            if (!string.IsNullOrEmpty(number) && number.Length > 0)
            {
                query = query.Where(e => e.Appointment.Number.StartsWith(number));
            }



            if (!string.IsNullOrEmpty(ids) && ids.Length > 0)
            {
                var idArr = ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => int.Parse(i));
                query = query.Where(e => idArr.Contains(e.Id));
            }

            var list = query.OrderByDescending(e => e.UpdatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                e.ProductNumber,
                                e.Appointment.Number,
                                ProductCollocationImg = e.Product.ProductCollocationImg ?? e.Product.ProductOriginNumber.ProductCollocationImg,
                                e.Feedbacks,
                                e.Id,
                                e.Appointment.Store.StoreName,
                                e.Appointment.Member.RealName,
                                e.IsDeleted,
                                e.IsEnabled,
                                e.CreatedTime,
                                e.UpdatedTime,
                                e.Operator.Member.MemberName
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
                _logger.Error("权限包含的页面元素加载出错，错误如下：" + ex.Message + "。");
                throw new Exception("error");
            }

        }




    }
}
