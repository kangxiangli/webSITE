
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
using Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse;
using System.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{

    [License(CheckMode.Verify)]
    public class CheckedController : BaseController
    {

        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CheckedController));

        protected readonly ICheckerContract _checkerContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IStorageContract _storageContract;
        protected readonly ICheckerItemContract _checkerItemContract;
        protected readonly IBrandContract _brandContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IProductTrackContract _productTrackContract;
        protected readonly IAdministratorContract _administratorContract;

        public CheckedController(ICheckerContract checkerContract,
            IStoreContract storeContract,
            IStorageContract storageContract,
            ICheckerItemContract checkerItemContract,
            IBrandContract brandContract,
            ICategoryContract categoryContract,
            IProductTrackContract productTrackContract,
            IAdministratorContract administratorContract)
        {
            _administratorContract = administratorContract;
            _checkerContract = checkerContract;
            _storeContract = storeContract;
            _storageContract = storageContract;
            _checkerItemContract = checkerItemContract;
            _brandContract = brandContract;
            _categoryContract = categoryContract;
            _productTrackContract = productTrackContract;
        }
        #endregion

        #region 初始化界面

        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {

            return View();
        }
        #endregion

        #region 添加数据

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
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
        public ActionResult Create(CheckerDto dto)
        {
            var result = _checkerContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 提交数据

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(CheckerDto dto)
        {
            OperationResult ope = new OperationResult(OperationResultType.Error);
            var chec = _checkerContract.Checkers.Where(c => c.CheckGuid == dto.CheckGuid).FirstOrDefault();
            if (chec != null)
            {
                chec.CheckerName = dto.CheckerName;
                chec.Notes = dto.Notes;
                CheckerDto cheDto = AutoMapper.Mapper.Map<CheckerDto>(chec);
                ope = _checkerContract.Update(cheDto);
            }
            return Json(ope, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public ActionResult Update(string num)
        {

            var res = _checkerContract.Checkers.Where(c => c.CheckGuid == num).FirstOrDefault();
            if (res != null)
            {
                ViewBag.CheckerName = res.CheckerName;
                ViewBag.Notes = res.Notes;
                ViewBag.Num = num;
            }
            return PartialView();
        }
        #endregion

        #region 查看数据详情

        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public new ActionResult View(string number)
        {
            var result = _checkerContract.Checkers.FirstOrDefault(x => x.CheckGuid == number);
            return PartialView(result);
        }
        #endregion

        #region 注释代码- 获取数据列表

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        //public async Task<ActionResult> List()
        //{
        //    GridRequest request = new GridRequest(Request);
        //    try
        //    {
        //        Expression<Func<Checker, bool>> predicate = FilterHelper.GetExpression<Checker>(request.FilterGroup);
        //        List<CheckedDto_t> li = new List<CheckedDto_t>();
        //        var data = await Task.Run(() =>
        //        {
        //            var count = 0;
        //            var list = _checkerContract.Checkers.Where<Checker, int>(predicate, request.PageCondition, out count).Select(m => new CheckedDto_t()
        //            {
        //                Id = m.CheckGuid,
        //                ParentId = "",
        //                StoreName = m.Store.StoreName,
        //                StorageName = m.Storage.StorageName,
        //                CheckerName = m.CheckerName,
        //                CheckCount = m.CheckCount,
        //                CheckedCount = m.CheckedCount,
        //                ValidCount = m.ValidCount,
        //                InvalidCount = m.InvalidCount,
        //                ResidueCount = m.ResidueCount,
        //                MissingCount = m.MissingCount,
        //                Notes = m.Notes,
        //                CreatedTime = m.UpdatedTime,
        //                CheckerState = m.CheckerState,
        //                AdminName = m.Operator.AdminName
        //            }).OrderByDescending(c=>c.CreatedTime).ToList();
        //            foreach (var item in list)
        //            {
        //                string checkGuid = item.Id;
        //                var chil = _checkerItemContract.CheckerItems.Where(c => c.CheckGuid == checkGuid).ToList().Select(c => new CheckedDto_t()
        //                {
        //                    Id = c.Id.ToString(),
        //                    ParentId = c.CheckGuid,
        //                    StoreName = item.StoreName,
        //                    StorageName = item.StorageName,
        //                    CheckerName = item.CheckerName,
        //                    CheckCount = GetCheckItemCou(c.CheckCount),
        //                    CheckedCount = GetCheckItemCou(c.CheckedCount),
        //                    ValidCount = GetCheckItemCou(c.ValidCount),
        //                    InvalidCount = GetCheckItemCou(c.InvalidCount),
        //                    ResidueCount = GetCheckItemCou(c.ResidueCount),
        //                    MissingCount = GetCheckItemCou(c.MissingCount),
        //                    Notes = c.Notes,
        //                    CreatedTime = c.CreatedTime,
        //                    CheckerState = c.CheckerState,
        //                    AdminName = item.AdminName
        //                }).OrderByDescending(c=>c.CreatedTime).ToList();
        //               // chil.Remove(chil[0]);
        //                li.Add(item);
        //                li.AddRange(chil);

        //            }
        //            return new GridData<object>(li, count, request.RequestInfo);
        //        });
        //        return Json(data, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {

        //        return Json(new GridData<object>(new List<object>(), 0, request.RequestInfo));
        //    }

        //}
        #endregion

        #region 获取数据列表
        public ActionResult List()
        {
            GridRequest request = new GridRequest(Request);
            try
            {
                var optId = AuthorityHelper.OperatorId;
                if (!optId.HasValue)
                {
                    throw new Exception("optId is null");
                }
                Expression<Func<Checker, bool>> predicate = FilterHelper.GetExpression<Checker>(request.FilterGroup);



                List<CheckedDto_t> li = new List<CheckedDto_t>();

                var count = 0;
                IQueryable<Checker> listChecker = _checkerContract.Checkers.OrderByDescending(c => c.CreatedTime)
                    .Where(x => x.IsEnabled == true && x.IsDeleted == false);
                Dictionary<string, Checker> dic = new Dictionary<string, Checker>();
                foreach (Checker checker in listChecker)
                {
                    int index = dic.Where(x => x.Value.StoreId == checker.StoreId && x.Value.StorageId == checker.StorageId).Count();
                    if (index > 0)
                    {
                        continue;
                    }
                    else
                    {
                        if (!dic.ContainsKey(checker.CheckGuid))
                        {
                            dic.Add(checker.CheckGuid, new Checker() { StoreId = checker.StoreId, StorageId = checker.StorageId });
                        }
                    }
                }

                IQueryable<Brand> listBrand = _brandContract.Brands.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                IQueryable<Category> listCategory = _categoryContract.Categorys.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                var query = _checkerContract.Checkers;
                if (request.FilterGroup.Rules.Count(i => i.Field == "StorageId") <= 0)
                {
                    var enableStoreIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
                    query = query.Where(c => enableStoreIds.Contains(c.StoreId));
                }
                var list = query.OrderByDescending(c => c.CreatedTime).Where<Checker, int>(predicate, request.PageCondition, out count).Select(m => new CheckedDto_t()
                {
                    Id = m.CheckGuid,
                    ParentId = "",
                    StoreName = m.Store.StoreName,
                    StorageName = m.Storage.StorageName,
                    BrandName = listBrand.FirstOrDefault(x => x.Id == m.BrandId) == null ? "全部" : listBrand.FirstOrDefault(x => x.Id == m.BrandId).BrandName,
                    CategoryName = listCategory.FirstOrDefault(x => x.Id == m.CategoryId) == null ? "全部" : listCategory.FirstOrDefault(x => x.Id == m.CategoryId).CategoryName,
                    //Quantity=m.Quantity,
                    CheckerName = m.CheckerName,
                    //CheckCount = m.CheckQuantity,
                    BeforeCheckQuantity = m.BeforeCheckQuantity,
                    CheckedQuantity = m.CheckedQuantity,
                    CheckedCount = m.ValidQuantity + m.ResidueQuantity + m.MissingQuantity,
                    ValidCount = m.ValidQuantity,
                    //InvalidCount = m.InvalidQuantity,
                    ResidueCount = m.ResidueQuantity,
                    MissingCount = m.MissingQuantity,
                    Notes = m.Notes,
                    CreatedTime = m.UpdatedTime,
                    CheckerState = m.CheckerState,
                    AdminName = m.Operator.Member.MemberName,

                }).ToList();
                foreach (var item in list)
                {
                    item.IsIndex = dic.ContainsKey(item.Id);
                }

                var data = new GridData<object>(list, count, request.RequestInfo);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(new GridData<object>(new List<object>(), 0, request.RequestInfo));
            }
        }
        #endregion

        /// <summary>
        /// 获取盘点每一项的数量
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        private int GetCheckItemCou(string st)
        {
            int cou = 0;
            if (!string.IsNullOrEmpty(st))
            {
                string[] childs = st.Split(",", true);
                foreach (string c in childs)
                {
                    cou += Convert.ToInt32(c.Split("|", true)[1]);
                }

            }
            return cou;
        }

        #region 注释代码

        /// <summary>
        /// 继续盘点
        /// </summary>
        /// <param name="num">盘点编号</param>
        /// <returns></returns>
        //public ActionResult ContinuChecker(string num)
        //{
        //    /*

        //    Session["checkCount_li"] = null;
        //    Session["checkedCount_li"] = null;
        //    Session["validCount_li"] = null;
        //    Session["residueCount_li"] = null;
        //    Session["invalidCount_li"] = null;
        //    Session["currCheckGuid"] = null;
        //    Session["_checkedInfo"] = null;
        //     */
        //    var che = _checkerContract.Checkers.Where(c => c.CheckGuid == num).FirstOrDefault();
        //    if (che != null)
        //    {
        //        SetCheckerItem(num);
        //    }
        //    return Redirect(@"\Warehouses\Checker\Index");

        //}
        #endregion

        #region 注释代码


        //private void SetCheckerItem(string num)
        //{
        //    if (!string.IsNullOrEmpty(num))
        //    {
        //        var chcItem = _checkerItemContract.CheckerItems.Where(c => c.CheckGuid == num).OrderByDescending(c => c.CreatedTime).FirstOrDefault();
        //        var chcinfo = _checkerContract.Checkers.Where(c => c.CheckGuid == num).FirstOrDefault();
        //        if (chcItem != null)
        //        {
        //            Session["checkCount_li"] = GetCheckItem(chcItem.CheckCount);
        //            Session["checkedCount_li"] = GetCheckItem(chcItem.CheckedCount);
        //            Session["validCount_li"] = GetCheckItem(chcItem.ValidCount);
        //            Session["residueCount_li"] = GetCheckItem(chcItem.ResidueCount);
        //            Session["invalidCount_li"] = GetCheckItem(chcItem.InvalidCount);
        //            Session["currCheckGuid"] = num;
        //            #region  Session["_checkedInfo"]
        //            if (chcinfo != null)
        //            {
        //                Session["_checkedInfo"] = new CheckedType()
        //                {
        //                    UUID = num,
        //                    CheckCount = chcinfo.CheckCount,
        //                    CheckedCount = chcinfo.CheckedCount,
        //                    InvalidCount = chcinfo.InvalidCount,
        //                    ValidCount = chcinfo.ValidCount,
        //                    StoreId = chcinfo.StoreId.ToString(),
        //                    StorageId = chcinfo.StorageId.ToString(),
        //                    MissingCount = chcinfo.MissingCount,
        //                    Notes = chcinfo.Notes,
        //                    ResidueCount = chcinfo.ResidueCount,
        //                };
        //            }
        //            #endregion

        //        }
        //        /*
        //          ViewBag.StoreList = StoreList;
        //    int defaultId = int.Parse(StoreList[0].Value);
        //    ViewBag.StorageList = CacheAccess.GetStorages(_storageContract, defaultId);
        //    ViewBag.BrandList = CacheAccess.GetBrand(_brandContract, true, true);
        //    ViewBag.CategoryList = CacheAccess.GetCategory(_categoryContract, true);
        //    ViewBag.SeasonList = CacheAccess.GetSeason(_seasonContract, true);
        //    ViewBag.SizeList = CacheAccess.GetSize(_sizeContract);
        //    ViewBag.ColorList = CacheAccess.GetColors(_colorContract, true, true);
        //         */
        //        TempData["_StoreList_conti"] = _storeContract.Stores.Where(c => c.Id == chcinfo.StoreId).Select(c => new SelectListItem()
        //        {
        //            Value = c.Id.ToString(),
        //            Text = c.StoreName
        //        }).ToList();
        //        TempData["_StorageList_conti"] = _storageContract.Storages.Where(c => c.Id == chcinfo.StorageId).Select(c => new SelectListItem()
        //        {
        //            Value = c.Id.ToString(),
        //            Text = c.StorageName
        //        }).ToList();

        //    }
        //    else { }
        //}
        #endregion

        private List<CheckDto_t> GetCheckItem(string tname)
        {
            List<CheckDto_t> li = new List<CheckDto_t>();
            if (!string.IsNullOrEmpty(tname))
            {
                string[] childs = tname.Split(",", true);
                foreach (string chi in childs)
                {
                    string[] chs = chi.Split("|", true);
                    if (chs.Count() == 2)
                    {
                        li.Add(new CheckDto_t()
                        {
                            ProductNumber = chs[0],
                            Quantity = int.Parse(chs[1])

                        });
                    }
                }
            }

            return li;
        }

        #region 移除数据

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        //[Log]
        //[HttpPost]
        //public ActionResult Remove(int[] Id)
        //{
        //    var result = _checkerContract.Remove(Id);
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Remove(string[] CheckGuids)
        {
            var result = _checkerContract.Remove(CheckGuids);
            if (result.ResultType == OperationResultType.Success)
            {
                var CheckerItems = _checkerContract.Checkers.Where(x => CheckGuids.Contains(x.CheckGuid)).SelectMany(s => s.CheckerItems).Select(s => new
                {
                    s.ProductBarcode,
                    s.Product.ProductNumber,
                }).ToList();
                List<ProductTrack> listpt = new List<ProductTrack>();
                foreach (var checkitem in CheckerItems)
                {
                    #region 商品追踪
                    ProductTrack pt = new ProductTrack();
                    if (!string.IsNullOrEmpty(checkitem.ProductBarcode))
                    {
                        pt.ProductNumber = checkitem.ProductNumber;
                        pt.ProductBarcode = checkitem.ProductBarcode;
                        pt.Describe = ProductOptDescTemplate.ON_PRODUCT_CHECKER_DELETE;
                        listpt.Add(pt);
                        #endregion
                    }
                }
                _productTrackContract.BulkInsert(listpt);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 删除数据

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Delete(int[] Id)
        {
            var result = _checkerContract.Delete(Id);
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
            var result = _checkerContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
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
            var result = _checkerContract.Enable(Id);
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
            var result = _checkerContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 打印数据

        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _checkerContract.Checkers.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 导出数据

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));

            GridRequest request = new GridRequest(Request);
            var optId = AuthorityHelper.OperatorId;
            if (!optId.HasValue)
            {
                throw new Exception("optId is null");
            }
            Expression<Func<Checker, bool>> predicate = FilterHelper.GetExpression<Checker>(request.FilterGroup);

            IQueryable<Checker> listChecker = _checkerContract.Checkers.OrderByDescending(c => c.CreatedTime)
                .Where(x => x.IsEnabled == true && x.IsDeleted == false);
            Dictionary<string, Checker> dic = new Dictionary<string, Checker>();
            foreach (Checker checker in listChecker)
            {
                int index = dic.Where(x => x.Value.StoreId == checker.StoreId && x.Value.StorageId == checker.StorageId).Count();
                if (index > 0)
                {
                    continue;
                }
                else
                {
                    if (!dic.ContainsKey(checker.CheckGuid))
                    {
                        dic.Add(checker.CheckGuid, new Checker() { StoreId = checker.StoreId, StorageId = checker.StorageId });
                    }
                }
            }

            IQueryable<Brand> listBrand = _brandContract.Brands.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            IQueryable<Category> listCategory = _categoryContract.Categorys.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            var query = _checkerContract.Checkers;
            if (request.FilterGroup.Rules.Count(i => i.Field == "StorageId") <= 0)
            {
                var enableStoreIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
                query = query.Where(c => enableStoreIds.Contains(c.StoreId));
            }
            var list = query.OrderByDescending(c => c.CreatedTime).Where(predicate).Select(m => new
            {
                Id = m.CheckGuid,
                ParentId = "",
                StoreName = m.Store.StoreName,
                StorageName = m.Storage.StorageName,
                BrandName = listBrand.FirstOrDefault(x => x.Id == m.BrandId) == null ? "全部" : listBrand.FirstOrDefault(x => x.Id == m.BrandId).BrandName,
                CategoryName = listCategory.FirstOrDefault(x => x.Id == m.CategoryId) == null ? "全部" : listCategory.FirstOrDefault(x => x.Id == m.CategoryId).CategoryName,
                CheckerName = m.CheckerName,
                BeforeCheckQuantity = m.BeforeCheckQuantity,
                CheckedQuantity = m.CheckedQuantity,
                CheckedCount = m.ValidQuantity + m.ResidueQuantity + m.MissingQuantity,
                ValidCount = m.ValidQuantity,
                ResidueCount = m.ResidueQuantity,
                MissingCount = m.MissingQuantity,
                Notes = m.Notes,
                CreatedTime = m.UpdatedTime,
                CheckerState = m.CheckerState,
                AdminName = m.Operator.Member.MemberName,
            }).ToList();

            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return FileExcel(st, "盘点管理");
        }
        /// <summary>
        /// 导出余货缺货数据
        /// </summary>
        /// <returns></returns>
        [Log]
        public ActionResult ExportQuantity(string CheckGuid, CheckerItemFlag Flag)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _checkerItemContract.CheckerItems.Where(w => w.IsEnabled && !w.IsDeleted).Where(w => w.CheckGuid == CheckGuid && w.CheckerItemType == (int)Flag).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("ExporterQuantity");
            st.SetAttribute("list", list);
            return FileExcel(st, "盘点详情列表");
        }
        #endregion

        #region 修改盘点状态

        /// <summary>
        /// 修改盘点状态
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateState()
        {
            var num = Request["num"];
            var stat = Request["stat"]; //1:成功 2：中断
            OperationResult res = new OperationResult(OperationResultType.Error);
            if (stat != null && num != null)
            {
                var che = _checkerContract.Checkers.Where(c => c.CheckGuid == num).FirstOrDefault();
                if (che != null)
                {
                    Checker checker = _checkerContract.Checkers.Where(x => x.IsEnabled == true && x.IsDeleted == false)
                         .OrderByDescending(x => x.CreatedTime)
                         .FirstOrDefault(x => x.StorageId == che.StorageId && x.StoreId == che.StoreId);
                    if (checker != null && che.CheckGuid != checker.CheckGuid)
                    {
                        res.Message = "校验过的数据无法更改";
                        return Json(res);
                    }


                    if (stat == "1")
                    {
                        che.CheckerState = CheckerFlag.Checked;
                        var ord = _storageContract.Storages.Where(c => c.Id == che.StorageId && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
                        if (ord != null)
                        {
                            ord.CheckLock = false;
                            StorageDto dto = AutoMapper.Mapper.Map<StorageDto>(ord);
                            _storageContract.Update(dto);
                        }
                    }
                    else if (stat == "2")
                    {
                        che.CheckerState = CheckerFlag.Interrupt;
                    }
                    else
                    {
                        che.CheckerState = CheckerFlag.Other;
                    }
                }
                if (che.CheckerState != CheckerFlag.Other)
                {
                    CheckerDto dto = AutoMapper.Mapper.Map<CheckerDto>(che);
                    res = _checkerContract.Update(dto);
                }
            }
            return Json(res);

        }
        #endregion

        #region 上传Excel
        public JsonResult UploadExcel()
        {
            return null;
        }
        #endregion

        #region 判断是否为最新盘点
        /// <summary>
        /// 判断是否为最新盘点
        /// </summary>
        /// <param name="CheckerUid"></param>
        /// <returns></returns>
        public JsonResult IsIndex(string CheckerUid)
        {
            OperationResult oper = _checkerContract.IsIndex(CheckerUid);
            return Json(oper);
        }
        #endregion

        #region 展示盘点数量
        /// <summary>
        /// 展示盘点数量
        /// </summary>
        /// <param name="Id">盘点Id</param>
        /// <param name="Flag">参看CheckerItemFlag</param>
        /// <returns></returns>
        public ActionResult ShowQuantity(string Id, int Flag)
        {
            ViewBag.CheckGuid = Id;
            ViewBag.CheckerItemType = Flag;
            return PartialView();
        }

        public async Task<ActionResult> QuantityList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<CheckerItem, bool>> predicate = FilterHelper.GetExpression<CheckerItem>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _checkerItemContract.CheckerItems.Where<CheckerItem, int>(predicate, request.PageCondition, out count).OrderByDescending(c => c.CreatedTime).Select(m => new
                {
                    m.Product.ProductOriginNumber.ProductName,
                    m.Product.ProductOriginNumber.TagPrice,
                    m.ProductBarcode,
                    m.Product.Size.SizeName,
                    ColorImg = m.Product.Color.IconPath,
                    ColorName = m.Product.Color.ColorName,
                    ThumbnailPath = m.Product.ThumbnailPath != null ? m.Product.ThumbnailPath : m.Product.ProductOriginNumber.ThumbnailPath,
                    m.CheckerItemType,
                    m.Id,
                    m.CheckGuid,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    AdminName = m.Operator.Member.MemberName,
                }).ToList();

                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}
