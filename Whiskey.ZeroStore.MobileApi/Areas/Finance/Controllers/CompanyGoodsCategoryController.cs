using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Finance;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.Web.Helper;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Whiskey.ZeroStore.MobileApi.Areas.Finance.Controllers
{
    public class CompanyGoodsCategoryController : Controller
    {
        #region 初始化操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CompanyGoodsCategoryController));

        protected readonly ICompanyGoodsCategoryContract _CompanyGoodsCategoryContract;
        protected readonly IAdministratorContract _administratorContract;

        public CompanyGoodsCategoryController(
            ICompanyGoodsCategoryContract CompanyGoodsCategoryContract,
            IAdministratorContract administratorContract
            )
        {
            this._CompanyGoodsCategoryContract = CompanyGoodsCategoryContract;
            this._administratorContract = administratorContract;
        }
        #endregion

        #region 添加类别或物品
        /// <summary>
        /// 添加类别或物品
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult InsertCompanyGoodsCategory(CompanyGoodsCategoryDto dto)
        {
            if (!_administratorContract.CheckExists(a => a.Id == dto.OperatorId && a.IsEnabled && !a.IsDeleted))
            {
                return Json(new OperationResult(OperationResultType.Error, "用户不存在"), JsonRequestBehavior.AllowGet);
            }

            HttpFileCollectionBase files = Request.Files;

            if ((dto.ParentId == 0 && (files == null || files.Count == 0)) || (dto.ParentId > 0 && !dto.IsUniqueness && (files == null || files.Count == 0)))
            {
                return Json(new OperationResult(OperationResultType.Error, "请上传展示图片"), JsonRequestBehavior.AllowGet);
            }

            string weburl = ConfigurationHelper.GetAppSetting("WebUrl");
            if (files != null && files.Count > 0)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("ExtType", "Image");
                dic.Add("SaveDir", "Goods");
                dic.Add("FileName", DateTime.Now.Ticks.ToString());
                var opera = ConfigurationHelper.Upload<OperationResult>("Upload/UploadGoodsImg", files, dic, 0);

                if (opera.ResultType == OperationResultType.Success)
                {
                    var aaa = JsonConvert.SerializeObject(opera.Data);
                    var bb = JsonConvert.DeserializeObject<UploadResult>(aaa);

                    dto.ImgAddress = bb.file;
                }
                else
                {
                    return Json(opera, JsonRequestBehavior.AllowGet);
                }
            }

            var result = _CompanyGoodsCategoryContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取物品类型
        /// <summary>
        /// 获取物品类型
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCompanyGoodsCategoryTypes()
        {
            var CompanyGoodsCategoryTypes = new List<SelectListItem>();

            foreach (var value in Enum.GetValues(typeof(CompanyGoodsCategoryTypeFlag)))
            {
                CompanyGoodsCategoryTypes.Add(new SelectListItem() { Value = Convert.ToInt32(value).ToString(), Text = (EnumHelper.GetValue<string>((CompanyGoodsCategoryTypeFlag)value)) });
            }
            return Json(new OperationResult(OperationResultType.Success, "", CompanyGoodsCategoryTypes), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取物品类别
        /// <summary>
        /// 获取物品类别
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCompanyGoodsCategorys(int type)
        {
            try
            {
                var list = (from x in _CompanyGoodsCategoryContract.Entities.Where(g => g.Type == type && (g.ParentId == null || g.ParentId == 0) && !g.IsDeleted && g.IsEnabled).ToList()
                            select new
                            {
                                x.Id,
                                x.CompanyGoodsCategoryName,
                                x.IsUniqueness,
                                x.ImgAddress
                            }).ToList();
                return Json(new OperationResult(OperationResultType.Success, "", list), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new OperationResult(OperationResultType.Error, "网络异常，请稍候再试"), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 获取物品详情
        /// <summary>
        /// 获取物品详情
        /// </summary>
        /// <param name="Id">物品Id</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetGoodInfo(int Id)
        {
            try
            {
                var model =
                    (from x in _CompanyGoodsCategoryContract.Entities.Where(g => g.Id == Id && !g.IsDeleted && g.IsEnabled).ToList()
                     select new
                     {
                         x.Id,
                         x.CompanyGoodsCategoryName,
                         x.IsUniqueness,
                         x.ImgAddress,
                         x.Notes,
                         OperatorName = x.Operator != null ? x.Operator.Member.RealName : "",
                         ParentId = x.ParentId ?? 0,
                         ParentName = x.Parent != null ? x.Parent.CompanyGoodsCategoryName : "",
                         Price = x.Price == null ? "0.00" : Convert.ToDecimal(x.Price).ToString("f2"),
                         x.Status,
                         TotalQuantity = x.TotalQuantity ?? 0,
                         TypeName = EnumHelper.GetValue<string>((CompanyGoodsCategoryTypeFlag)x.Type),
                         x.UniqueIdentification,
                         UsedQuantity = x.UsedQuantity ?? 0,
                         CreatedTime = x.CreatedTime.ToString("yyyy-MM-dd")
                     }).FirstOrDefault();
                return Json(new OperationResult(OperationResultType.Success, "", model), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new OperationResult(OperationResultType.Error, "网络异常，请稍候再试"), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 获取物品列表
        /// <summary>
        /// 获取物品列表
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="parentId">类别</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">条数</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetGoods(int type = -1, int parentId = 0, int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                int totalCount = 0;
                int totalPage = 0;

                IQueryable<CompanyGoodsCategory> query;
                if (type == -1 && parentId == 0)
                {
                    query = _CompanyGoodsCategoryContract.Entities.Where(g => !g.IsDeleted && g.IsEnabled && (g.ParentId ?? 0) > 0).OrderByDescending(g => g.CreatedTime);
                }
                else if (type > -1 && parentId == 0)
                {
                    query = _CompanyGoodsCategoryContract.Entities.Where(g => g.Type == type && !g.IsDeleted && g.IsEnabled && (g.ParentId ?? 0) > 0).OrderByDescending(g => g.CreatedTime);
                }
                else if (type == -1 && parentId > 0)
                {
                    query = _CompanyGoodsCategoryContract.Entities.Where(g => g.ParentId == parentId && !g.IsDeleted && g.IsEnabled).OrderByDescending(g => g.CreatedTime);
                }
                else
                {
                    query = _CompanyGoodsCategoryContract.Entities.Where(g => g.Type == type && g.ParentId == parentId && !g.IsDeleted && g.IsEnabled).OrderByDescending(g => g.CreatedTime);
                }

                var list = (from x in query.Where<CompanyGoodsCategory, int>(pageIndex, pageSize, out totalCount, out totalPage).ToList()
                            select new
                            {
                                x.Id,
                                x.CompanyGoodsCategoryName,
                                x.IsUniqueness,
                                x.ImgAddress,
                                x.Notes,
                                OperatorName = x.Operator != null && x.Operator.Member != null ? x.Operator.Member.RealName : "",
                                ParentId = x.ParentId ?? 0,
                                ParentName = x.Parent != null ? x.Parent.CompanyGoodsCategoryName : "",
                                Price = x.Price == null ? "0.00" : Convert.ToDecimal(x.Price).ToString("f2"),
                                x.Status,
                                TotalQuantity = x.TotalQuantity ?? 0,
                                TypeName = EnumHelper.GetValue<string>((CompanyGoodsCategoryTypeFlag)x.Type),
                                UniqueIdentification = x.UniqueIdentification,
                                UsedQuantity = x.UsedQuantity ?? 0,
                                CreatedTime = x.CreatedTime.ToString("yyyy-MM-dd")
                            }).ToList();

                OperationResult opera = new OperationResult(OperationResultType.Success, "", list);
                IDictionary<string, int> dic = new Dictionary<string, int>();
                dic.Add("totalCount", totalCount);
                dic.Add("totalPage", totalPage);
                opera.Other = dic;
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new OperationResult(OperationResultType.Error, "网络异常，请稍候再试"), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }

    public class UploadResult
    {
        public string file { get; set; }
    }
}