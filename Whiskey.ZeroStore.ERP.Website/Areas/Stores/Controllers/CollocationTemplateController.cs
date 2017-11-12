
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{



    [License(CheckMode.Verify)]
    public class CollocationTemplateController : BaseController
    {
        private static readonly ILogger _Logger = LogManager.GetLogger(typeof(CollocationTemplateController));

        private readonly ICollocationTemplateContract _contract;
        private readonly IAdministratorContract _adminContract;
        private readonly IModuleContract _moduleContract;
        private readonly IPermissionContract _permissionContract;
        private readonly ICategoryContract _categoryContract;
        private readonly IProductAttributeContract _productAttributeContract;
        private readonly IProductContract _productContract;
        private readonly IProductOrigNumberContract _productOrigNumberContract;

        public CollocationTemplateController(ICollocationTemplateContract contract,
            IAdministratorContract adminContract,
            IModuleContract moduleContract,
            IPermissionContract permissionContract,
            ICategoryContract categoryContract,
            IProductAttributeContract productAttributeContract,
            IProductContract productContract,
            IProductOrigNumberContract productOrigNumberContract)
        {
            _contract = contract;
            _adminContract = adminContract;
            _moduleContract = moduleContract;
            _permissionContract = permissionContract;
            _categoryContract = categoryContract;
            _productAttributeContract = productAttributeContract;
            _productContract = productContract;
            _productOrigNumberContract = productOrigNumberContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
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
                return PartialView("Edit", new CollocationTemplate());
            }
        }

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(int? id, string name, CollocationRulesEntry[] rules)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Json(OperationResult.Error("模版名不可为空"));

            }
            if (rules == null || rules.Length <= 0)
            {
                return Json(OperationResult.Error("规则不能为空"));
            }
            if (rules.Any(r => r.CategoryId <= 0 || string.IsNullOrEmpty(r.CategoryName)))
            {
                return Json(OperationResult.Error("规则中选择的品类无效"));
            }

            

            if (id.HasValue && id.Value > 0)
            {
                var model = _contract.View(id.Value);
                model.Name = name;
                model.CollocationRules = JsonHelper.ToJson(rules,true);
                model.RuleCount = rules.Length;
                var res = _contract.Update(model);
                return Json(res);
            }
            else
            {
                var res = _contract.Insert(new CollocationTemplate()
                {
                    Name = name,
                    CollocationRules = JsonHelper.ToJson(rules,true),
                    RuleCount = rules.Length
                });

                return Json(res);
            }



        }



        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(string name, DateTime? startDate, DateTime? endDate, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {

            var query = _contract.Entities;
            query = query.Where(e => e.IsEnabled == isEnabled);

            if (!string.IsNullOrEmpty(name) && name.Length > 0)
            {

                query = query.Where(e => e.Name.StartsWith(name));

            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime <= endDate.Value);
            }
            var list = query.OrderByDescending(e => e.UpdatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                e.Name,
                                e.Id,
                                e.RuleCount,
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
                _Logger.Error("权限包含的页面元素加载出错，错误如下：" + ex.Message + "。");
                throw new Exception("error");
            }

        }

        [HttpGet]
        public ActionResult ProductSelect(int categoryId, string categoryName, string tags)
        {
            ViewBag.categoryId = categoryId;
            ViewBag.categoryName = categoryName;
            ViewBag.tags = tags;
            return PartialView();
        }

        public ActionResult ProductList(int categoryId, string tags,string productNumber, int pageIndex = 1, int pageSize = 10)
        {
            var tagArr = tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var query = _productOrigNumberContract.OrigNumbs.Where(p => !p.IsDeleted && p.IsEnabled)
                .Where(p => p.IsRecommend.Value)
                .Where(p => p.Category.ParentId == categoryId)
                .Where(p => tagArr.All(t => p.ProductAttributes.Any(a => a.AttributeName == t)))
                .Select(p => p.BigProdNum);

            var productQuery = _productContract.Products.Where(p => !p.IsDeleted && p.IsEnabled && query.Contains(p.BigProdNum));

            if (!string.IsNullOrEmpty(productNumber) && productNumber.Length > 0)
            {
                productQuery = productQuery.Where(p => p.ProductNumber.StartsWith(productNumber));
            }

            var list = productQuery.OrderByDescending(p => p.UpdatedTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.BigProdNum,
                    p.ProductNumber,
                    ThumbnailPath = p.ThumbnailPath ?? p.ProductOriginNumber.ThumbnailPath,
                    p.Color.ColorName,
                    p.Size.SizeName,
                    p.ProductOriginNumber.Brand.BrandName,
                    p.ProductOriginNumber.TagPrice

                })
                .ToList();
            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = list,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = productQuery.Count(),
                }
            });

            return Json(res, JsonRequestBehavior.AllowGet);
        }


    }



}