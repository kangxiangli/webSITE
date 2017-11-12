using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Notices;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Website.Areas.Notices.Models;
using Whiskey.Web.Helper;
using System.Data.Entity;
using Whiskey.Utility.Helper;
using Whiskey.Web.Extensions;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Utility.Class;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Models;
using System.Text;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    [AllowCross]
    public class WorkController : BaseController
    {
        protected readonly IWorkLogContract _workLogContract;
        protected readonly IWorkLogAttributeContract _workLogAttributeContract;
        protected readonly IStoreSpendStatisticsContract _storeSpendStatisticsContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IAdministratorContract _adminContract;
        protected readonly IArticleItemContract _articleItemContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IStoreCheckRecordContract _storeCheckRecordContract;
        protected readonly IStoreCheckItemContract _storeCheckItemContract;
        protected readonly IStoreStatisticsContract _storeStatisticsContract;
        protected readonly IProductTrackContract _productTrackContract;
        protected readonly IOrderblankContract _orderblankContract;
        protected readonly IInventoryContract _inventoryContract;


        public WorkController(IWorkLogAttributeContract workLogAttributeContract, IWorkLogContract workLogContract,
            IStoreSpendStatisticsContract storeSpendStatisticsContract,
            IStorageContract storageContract, IAdministratorContract adminContract,
            IArticleItemContract articleItemContract,
            IStoreContract storeContract,
            IStoreCheckRecordContract storeCheckRecordContract,
            IStoreCheckItemContract storeCheckItemContract,
            IProductTrackContract productTrackContract,
            IStoreStatisticsContract storeStatisticsContract,
            IOrderblankContract orderblankContract,
            IInventoryContract inventoryContract)
        {
            _workLogAttributeContract = workLogAttributeContract;
            _workLogContract = workLogContract;
            _storeSpendStatisticsContract = storeSpendStatisticsContract;
            _storageContract = storageContract;
            _adminContract = adminContract;
            _articleItemContract = articleItemContract;
            _storeContract = storeContract;
            _storeCheckRecordContract = storeCheckRecordContract;
            _storeCheckItemContract = storeCheckItemContract;
            _productTrackContract = productTrackContract;
            _storeStatisticsContract = storeStatisticsContract;
            _orderblankContract = orderblankContract;
            _inventoryContract = inventoryContract;
        }

        [HttpPost]
        public ActionResult GetWorkLogAttribute()
        {
            try
            {
                var list = _workLogAttributeContract.WorkLogAttributes.Where(t => t.ParentId.HasValue).Select(t => new
                {
                    t.Id,
                    t.WorkLogAttributeName
                }).ToList();
                return Json(new OperationResult(OperationResultType.Success, string.Empty, list));
            }
            catch (Exception e)
            {

                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }

        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddWorkLog(WorkLogDto dto)
        {
            try
            {
                OperationResult oper = _workLogContract.Insert(dto);
                return Json(oper);
            }
            catch (Exception)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }

        }

        [HttpPost]
        public ActionResult GetStoreSpendType()
        {
            try
            {
                Dictionary<int, string> dict = new Dictionary<int, string>();
                foreach (int myCode in Enum.GetValues(typeof(SpendType)))
                {
                    //string strName = Enum.GetName(typeof(SpendType), myCode);
                    var desc = ((SpendType)myCode).ToDescription();//获取名称
                    dict.Add(myCode, desc);
                }
                var data = dict.Select(d => new
                {
                    value = d.Key,
                    desc = d.Value
                }).ToList();
                return Json(new OperationResult(OperationResultType.Success, string.Empty, data));
            }
            catch (Exception)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }

        [HttpPost]
        public ActionResult GetEnableStore(int? adminId)
        {
            try
            {
                if (!adminId.HasValue)
                {
                    return Json(new OperationResult(OperationResultType.Error, "参数错误"));
                }
                var list = _storeContract.QueryManageStore(AuthorityHelper.OperatorId.Value).Select(s => new { StoreId = s.Id, StoreName = s.StoreName });
                return Json(new OperationResult(OperationResultType.Success, string.Empty, list));
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }


        }


        [CheckCookieAttrbute]
        [HttpPost]
        public ActionResult GetAllStore()
        {
            try
            {

                var list = _storeContract.QueryManageStore(AuthorityHelper.OperatorId.Value).Where(s => s.StoreTypeName == "直营店")
                    .Select(s => new
                    {
                        s.Id,
                        s.StoreName,
                        s.StorePhoto
                    }).ToList().Select(s => new
                    {

                        s.Id,
                        s.StoreName,
                        StorePhoto = string.IsNullOrEmpty(s.StorePhoto) ? string.Empty : WebUrl + s.StorePhoto
                    }).ToList();
                return Json(new OperationResult(OperationResultType.Success, string.Empty, list));
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }


        }

        [HttpPost]
        public ActionResult AddStoreSpend(StoreSpendStatisticsDto storeSpenddto)
        {
            try
            {
                var storespend = AutoMapper.Mapper.Map<StoreSpendStatistics>(storeSpenddto);
                storespend.StartYear = storeSpenddto.StartTime.Year;
                storespend.StartMonth = (byte)storeSpenddto.StartTime.Month;
                storespend.StartDay = (byte)storeSpenddto.StartTime.Day;

                storespend.EndYear = storeSpenddto.EndTime.Year;
                storespend.EndMonth = (byte)storeSpenddto.EndTime.Month;
                storespend.EndDay = (byte)storeSpenddto.EndTime.Day;

                var res = _storeSpendStatisticsContract.Insert(storespend);
                return Json(res);
            }
            catch (Exception)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }

        [HttpPost]
        public ActionResult GetArticleType()
        {
            try
            {
                //获取[公司信息]栏目
                var item = _articleItemContract.ArticleItems.Where(i => !i.ParentId.HasValue && i.ArticleItemName == "公司信息").Include(i => i.Children).FirstOrDefault();
                if (item == null || item.Children.Count == 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "栏目或子栏目尚未添加"));
                }
                var data = item.Children.Select(i => new { i.ArticleItemName, i.Id }).ToList();
                return Json(new OperationResult(OperationResultType.Success, string.Empty, data));
            }
            catch (Exception)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }

        }
        [HttpPost]
        public ActionResult GetArticle(int? articleItemId)
        {
            try
            {
                if (!articleItemId.HasValue)
                {
                    return Json(new OperationResult(OperationResultType.Error, "参数错误"));
                }
                //获取[公司信息]栏目
                var item = _articleItemContract.ArticleItems.Include(i => i.Articles).FirstOrDefault(i => i.Id == articleItemId.Value);
                if (item == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "栏目不存在"));
                }
                Func<string, string> getFullPath = path =>
                {
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        return string.Empty;
                    }
                    return WebUrl + path;
                };

                var data = item.Articles.Select(i => new
                {
                    i.Id,
                    i.Title,
                    i.Summary,
                    i.Content,
                    i.Operator.Member.MemberName,
                    i.ArticleItem.ArticleItemName,
                    i.ArticlePath,
                    i.CoverImagePath,

                }).ToList().Select(i => new
                {
                    Id = i.Id,
                    Title = i.Title,//文章标题
                    Summary = i.Summary,//简介
                    Content = i.Content,//文章内容
                    Author = i.MemberName,//发布人
                    ArticleItemName = i.ArticleItemName,//栏目名称
                    ArticlePath = getFullPath(i.ArticlePath),//文章路径
                    CoverImagePath = getFullPath(i.CoverImagePath)//封面图路径
                });


                return Json(new OperationResult(OperationResultType.Success, string.Empty, data));
            }
            catch (Exception)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }
        [HttpPost]
        public ActionResult GetArticleNoContent(int? articleItemId)
        {
            try
            {
                if (!articleItemId.HasValue)
                {
                    return Json(new OperationResult(OperationResultType.Error, "参数错误"));
                }
                //获取[公司信息]栏目
                var item = _articleItemContract.ArticleItems.Include(i => i.Articles).FirstOrDefault(i => i.Id == articleItemId.Value);
                if (item == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "栏目不存在"));
                }
                Func<string, string> getFullPath = path =>
                {
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        return string.Empty;
                    }
                    return WebUrl + path;
                };

                var data = item.Articles.Select(i => new
                {
                    i.Id,
                    i.Title,
                    i.ArticleItem.ArticleItemName,
                    i.ArticlePath,
                    i.CoverImagePath,

                }).ToList().Select(i => new
                {
                    Id = i.Id,
                    Title = i.Title,//文章标题
                    ArticleItemName = i.ArticleItemName,//栏目名称
                    ArticlePath = getFullPath(i.ArticlePath),//文章路径
                    CoverImagePath = getFullPath(i.CoverImagePath)//封面图路径
                });

                return Json(new OperationResult(OperationResultType.Success, string.Empty, data));
            }
            catch (Exception)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }

        [CheckCookieAttrbute]
        public ActionResult GetStoreChecks()
        {
            try
            {

                var list = _storeCheckItemContract.Entities.Where(s => !s.IsDeleted && s.IsEnabled)
                    .Select(s => new
                    {

                        s.Id,
                        s.CheckName,
                        s.Desc,
                        s.ItemsCount,
                        s.Items,
                        s.PunishScore,
                        s.Standard,

                    }).ToList()
                .Select(s => new
                {
                    s.Id,
                    s.CheckName,
                    s.Desc,
                    s.ItemsCount,
                    Items = JsonHelper.FromJson<CheckDetail[]>(s.Items),
                    s.PunishScore,
                    s.Standard,

                }).ToList();
                return Json(new OperationResult(OperationResultType.Success, string.Empty, list));
            }
            catch (Exception)
            {

                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }

        }


        [CheckCookieAttrbute]

        public ActionResult GetStoreCheckRecords(int? storeId, int? top)
        {
            try
            {
                if (!storeId.HasValue)
                {
                    return Json(OperationResult.Error("参数错误"));
                }
                var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
                if (storeIds == null || storeIds.Count <= 0 || !storeIds.Contains(storeId.Value))
                {
                    return Json(OperationResult.Error("权限不足"));
                }
                if (!top.HasValue)
                {
                    top = 30;
                }

                var startTime = DateTime.Now.Date.AddDays(-1 * top.Value);
                var endTime = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
                var list = _storeCheckRecordContract.Entities.Where(s => !s.IsDeleted && s.IsEnabled && s.StoreId == storeId.Value)
                                                    .Where(s => s.CheckTime >= startTime && s.CheckTime <= endTime)
                                                    .OrderByDescending(s => s.Id)
                                                    .Select(s => new
                                                    {
                                                        s.StoreName,
                                                        s.CheckTime,
                                                        s.Operator.Member.MemberName,
                                                        s.Remark,
                                                        s.Images,
                                                        s.CheckDetails,
                                                        s.RatingPoints,
                                                        s.TotalPunishScore
                                                    })
                                                    .Take(top.Value)
                                                    .ToList()
                                                    .Select(s => new
                                                    {
                                                        s.StoreName,
                                                        CheckTime = s.CheckTime.ToUnixTime(),
                                                        s.MemberName,
                                                        s.Remark,
                                                        s.Images,
                                                        CheckDetails = JsonHelper.FromJson<StoreCheckRecordSerializeModel[]>(s.CheckDetails),
                                                        s.RatingPoints,
                                                        s.TotalPunishScore
                                                    })
                                                    .ToList();
                return Json(new OperationResult(OperationResultType.Success, string.Empty, list));
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }


        }

        [CheckCookieAttrbute]
        public ActionResult AddStoreCheckRecord(AddStoreCheckRecordDTO dto)
        {
            try
            {
                if (!dto.AdminId.HasValue)
                {
                    return Json(OperationResult.Error("参数错误"));
                }
                if (string.IsNullOrEmpty(dto.CheckDetails))
                {
                    return Json(OperationResult.Error("勾选详情不能为空"));
                }
                var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
                if (storeIds == null || storeIds.Count <= 0 || !storeIds.Contains(dto.StoreId))
                {
                    return Json(OperationResult.Error("权限不足"));
                }
                var checkDetails = JsonHelper.FromJson<List<CheckInfoModel>>(dto.CheckDetails);

                var addDto = new StoreCheckRecordDTO()
                {
                    CheckTime = dto.CheckTime,
                    Images = dto.Images,
                    Remark = dto.Remark,
                    StoreName = dto.StoreName,
                    StoreId = dto.StoreId,
                    OperatorId = dto.AdminId.Value
                };

                var res = _storeCheckRecordContract.Insert(addDto, checkDetails);
                return Json(res);
            }
            catch (Exception e)
            {

                return Json(OperationResult.Error("系统错误"));
            }
        }

        [CheckCookieAttrbute]
        public ActionResult StoreStat(int? date)
        {
            var optId = AuthorityHelper.OperatorId;
            var statdate = int.Parse(DateTime.Now.Date.AddDays(-1).ToString("yyyyMMdd"));
            if (date.HasValue && date.Value <= statdate)
            {
                statdate = date.Value;
            }

            var storeQuery = _storeContract.QueryManageStoreId(optId.Value);
            if (storeQuery == null || storeQuery.Count <= 0)
            {
                return Json(OperationResult.Error("权限不足"));
            }
            var adminQuery = _adminContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            var query = _storeStatisticsContract.StoreStatistics.Where(s => !s.IsDeleted && s.IsEnabled)
                                                                .Where(s => storeQuery.Contains(s.StoreId) && s.StatDate == statdate);
            if (!query.Any())
            {
                return Json(OperationResult.Error("暂无数据"));
            }
            var data = query.Where(s => !s.IsDeleted && s.IsEnabled && s.StatDate == date)
                                                                .Select(s => new StoreStatRes
                                                                {
                                                                    StoreId = s.StoreId,
                                                                    StoreName = s.StoreName,
                                                                    InventoryCount = s.InventoryCount ?? 0,
                                                                    OrderblankDeliverCount = s.OrderblankDeliverCount,
                                                                    OrderblankAcceptCount = s.OrderblankAcceptCount,
                                                                    EmployeeCount = adminQuery.Count(a => a.DepartmentId == s.Store.DepartmentId),
                                                                    StatDate = s.StatDate,
                                                                }).ToList();


            //统计店铺员工数量
            return Json(new OperationResult(OperationResultType.Success, string.Empty, data));

        }

        [CheckCookieAttrbute]
        public ActionResult ProductTract(string barcode)
        {
            if (string.IsNullOrEmpty(barcode))
            {
                return Json(OperationResult.Error("流水号不能为空"));
            }
            var thumbnail = _inventoryContract.Inventorys.Where(i => !i.IsDeleted && i.IsEnabled && i.ProductBarcode == barcode).Select(s => s.Product.ThumbnailPath ?? s.Product.ProductOriginNumber.ThumbnailPath).FirstOrDefault();
            if (thumbnail == null)
            {
                thumbnail = string.Empty;
            }
            else
            {
                thumbnail = WebUrl + thumbnail;
            }

            var data = _productTrackContract.Tracks.Where(t => !t.IsDeleted && t.IsEnabled && t.ProductBarcode == barcode)
                                                    .OrderByDescending(p => p.CreatedTime)
                                                    .Select(p => new
                                                    {

                                                        p.ProductBarcode,
                                                        p.CreatedTime,
                                                        p.Describe,
                                                        p.Operator.Member.MemberName

                                                    }).ToList()
                                                    .Select(p => new
                                                    {
                                                        TimeStamp = p.CreatedTime.ToUnixTime(),
                                                        Desc = p.Describe,
                                                        Operator = p.MemberName,
                                                        ProductBarcode = p.ProductBarcode
                                                    }).ToList();
            var res = new
            {
                barcode = barcode,
                tracks = data,
                thumbnail = thumbnail
            };
            return Json(new OperationResult(OperationResultType.Success, string.Empty, res));
        }


        [CheckCookieAttrbute]
        public ActionResult GetEmployeeInfo(int storeId)
        {
            var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            if (storeIds == null || storeIds.Count <= 0 || !storeIds.Contains(storeId))
            {
                return Json(OperationResult.Error("权限不足"));
            }
            var key = storeId.ToString();
            var storeEntry = RedisCacheHelper.GetValueFromHash<StoreCacheEntry>(RedisCacheHelper.KEY_ALL_STORE, key);
            if (storeEntry == null)
            {
                return Json(OperationResult.Error("店铺信息未找到"));
            }
            var data = _adminContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true)
                .Where(x => x.DepartmentId.Value == storeEntry.DepartmentId.Value)
                .Select(a => new
                {
                    a.JobPosition.JobPositionName,
                    a.Member.MobilePhone,
                    a.Member.UserPhoto,
                    a.Member.RealName
                }).ToList()
                .Select(a => new
                {
                    a.JobPositionName,
                    a.MobilePhone,
                    UserPhoto = string.IsNullOrEmpty(a.UserPhoto) ? string.Empty : WebUrl + a.UserPhoto,
                    a.RealName
                }).ToList();
            return Json(new OperationResult(OperationResultType.Success, string.Empty, data));
        }

        [CheckCookieAttrbute]

        public ActionResult GetOrderblankDeliverInfo(DateTime? date, int storeId, OderblankActionEnum context, int pageIndex = 1, int offset = 10)
        {
            var optId = AuthorityHelper.OperatorId;
            var statdate = DateTime.Now.Date.AddDays(-1);
            if (date.HasValue && date.Value <= statdate)
            {
                statdate = date.Value;
            }


            var endTime = statdate.Date.AddDays(1).AddSeconds(-1);


            var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            if (storeIds == null || storeIds.Count <= 0 || !storeIds.Contains(storeId))
            {
                return Json(OperationResult.Error("权限不足"));
            }
            var query = _orderblankContract.Orderblanks.Where(o => !o.IsDeleted && o.IsEnabled);
            object res = null;
            switch (context)
            {
                case OderblankActionEnum.Deliver:
                    res = query.Where(o => o.OutStoreId == storeId)
                               .Where(o => o.Status == OrderblankStatus.发货中)
                               .Where(o => o.DeliveryTime.Value >= statdate && o.DeliveryTime <= endTime)
                               .OrderByDescending(o => o.UpdatedTime)
                               .Skip((pageIndex - 1) * offset)
                               .Take(offset)
                               .Select(o => new
                               {
                                   o.OrderBlankNumber,
                                   o.DeliverAdmin.Member.RealName,
                                   DeliveryTime = o.DeliveryTime.Value
                               })
                               .ToList()
                               .Select(o => new
                               {
                                   OrderBlankNumber = o.OrderBlankNumber,
                                   Deliver = o.RealName,
                                   DeliveryTime = o.DeliveryTime.ToUnixTime()
                               }).ToList();
                    break;
                case OderblankActionEnum.Accept:

                    res = query.Where(o => o.ReceiverStoreId == storeId)
                               .Where(o => o.Status == OrderblankStatus.已完成)
                               .Where(o => o.ReceiveTime >= statdate && o.ReceiveTime <= endTime)
                               .OrderByDescending(o => o.UpdatedTime)
                               .Skip((pageIndex - 1) * offset)
                               .Take(offset)
                               .Select(o => new
                               {
                                   o.ReceiverAdmin.Member.RealName,
                                   o.OrderBlankNumber,
                                   ReceiveTime = o.ReceiveTime.Value
                               })
                               .ToList()
                               .Select(o => new
                               {
                                   Reciever = o.RealName,
                                   OrderBlankNumber = o.OrderBlankNumber,
                                   ReceiveTime = o.ReceiveTime.ToUnixTime()
                               }).ToList();


                    break;
                default:
                    break;
            }
            return Json(new OperationResult(OperationResultType.Success, string.Empty, res));

        }


        [CheckCookieAttrbute]

        public ActionResult GetOrderblankItems(int storeId, string orderblankNumber)
        {
            var optId = AuthorityHelper.OperatorId;
            var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            if (storeIds == null || storeIds.Count <= 0 || !storeIds.Contains(storeId))
            {
                return Json(OperationResult.Error("权限不足"));
            }
            var query = _orderblankContract.Orderblanks.Where(o => !o.IsDeleted && o.IsEnabled)
                                                        .Where(o => o.ReceiverStoreId == storeId || o.OutStoreId == storeId)
                                                        .Where(o => o.OrderBlankNumber == orderblankNumber);
            if (!query.Any())
            {
                return Json(OperationResult.Error("未找到配货单"));
            }


            var items = query.SelectMany(o => o.OrderblankItems.Select(i => new
            {
                i.OrderBlankBarcodes,
                i.Product.ProductOriginNumber.Brand.BrandName,
                i.Product.Color.IconPath,
                i.Product.Size.SizeName,
                ThumbnailPath = i.Product.ThumbnailPath ?? i.Product.ProductOriginNumber.ThumbnailPath ?? string.Empty
            })).ToList();

            var res = new List<object>();
            foreach (var item in items)
            {
                var barcodeArr = item.OrderBlankBarcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                res.AddRange(barcodeArr.Select(s => new
                {
                    ProductBarcode = s,
                    BrandName = item.BrandName,
                    SizeName = item.SizeName,
                    ThumbnailPath = string.IsNullOrEmpty(item.ThumbnailPath) ? string.Empty : WebUrl + item.ThumbnailPath,
                    IconPath = WebUrl + item.IconPath
                }).ToList());
            }

            return Json(new OperationResult(OperationResultType.Success, string.Empty, res));

        }

        public enum OderblankActionEnum
        {
            Deliver = 0,
            Accept = 1
        }

        public class StoreStatRes
        {
            public int StoreId { get; set; }
            public string StoreName { get; set; }
            public int InventoryCount { get; set; }
            public int OrderblankDeliverCount { get; set; }
            public int OrderblankAcceptCount { get; set; }
            public int EmployeeCount { get; set; }

            public int StatDate { get; set; }
        }





    }
}