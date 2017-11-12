using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Web.Helper;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Contracts.Warehouse;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Property;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{
    public class CheckupController : BaseController
    {

        #region 初始化操作对象

        protected readonly IStoreContract _storeContract;
        protected readonly IStorageContract _storageContract;
        protected readonly ICheckerContract _checkerContract;
        protected readonly ICheckerItemContract _checkerItemContract;
        protected readonly IProductContract _productContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly ICheckupItemContract _checkupItemContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IColorContract _colorContract;
        protected readonly IAdministratorContract _adminContract;

        public CheckupController(IStoreContract storeContract,
            IStorageContract storageContract,
            ICheckerContract checkerContract,
            ICheckerItemContract checkerItemContract,
            IProductContract productContract,
            IInventoryContract inventoryContract,
            ICheckupItemContract checkupItemContract,
            ICategoryContract categoryContract,
            IAdministratorContract _adminContract,
            IColorContract colorContract)
        {
            _storeContract = storeContract;
            _storageContract = storageContract;
            _checkerContract = checkerContract;
            _checkerItemContract = checkerItemContract;
            _productContract = productContract;
            _inventoryContract = inventoryContract;
            _checkupItemContract = checkupItemContract;
            _categoryContract = categoryContract;
            _colorContract = colorContract;
            this._adminContract = _adminContract;
        }
        #endregion

        #region 初始化界面

        // GET: /Warehouses/Checkup/
        [Layout]
        public ActionResult Index()
        {
            string cheitenid = Request["chitnum"];
            if (!string.IsNullOrEmpty(cheitenid))
            {
                //int _id = int.Parse(cheitenid);
                var checkiem = _checkerItemContract.CheckerItems.Where(c => c.CheckGuid == cheitenid).FirstOrDefault();
                if (checkiem != null)
                {
                    var check = _checkerContract.Checkers.Where(c => c.CheckGuid == checkiem.CheckGuid).FirstOrDefault();
                    ViewBag.CheckItemId = cheitenid;
                    if (check != null)
                    {
                        ViewBag.Stores = check.StoreId;

                        ViewBag.Storages = new List<SelectListItem>() {
                        new SelectListItem(){
                         Text=check.Storage.StorageName,
                         Value=check.StorageId.ToString()
                        }
                       };
                        ViewBag.CheckNum = check.CheckGuid;
                        ViewBag.CheckName = check.CheckerName;
                        ViewBag.StartDate = check.CreatedTime.ToString("yyyy/MM/dd HH:mm:ss");
                        ViewBag.EndDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    }

                }
            }
            else
            {
                ViewBag.Stores = string.Empty;
                ViewBag.CheckItemId = -1;
                var stores = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
                if (!stores.Any())
                {
                    ViewBag.Storages = new List<SelectListItem>();
                }
                else
                {
                    var firStore = stores.FirstOrDefault();
                    ViewBag.Storages = CacheAccess.GetManagedStorageByStoreId(_storageContract, _adminContract, firStore, true);
                }


            }

            return View();
        }
        #endregion

        private List<CheckupDto_t> GetCheckItem(string st)
        {
            List<CheckupDto_t> li = new List<CheckupDto_t>();
            if (!string.IsNullOrEmpty(st))
            {
                string[] chils = st.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string item in chils)
                {
                    string[] child = item.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    string assNum = child[0];
                    if (int.Parse(child[1]) <= 0)
                        continue;
                    Product pro = _productContract.Products.Where(c => c.ProductNumber == assNum).FirstOrDefault();
                    if (pro != null)
                    {
                        li.Add(new CheckupDto_t()
                        {
                            Id = pro.Id,
                            ProName = pro.ProductName,
                            ProNum = pro.ProductNumber,
                            SizeName = pro.Size.SizeName,
                            ColorName = pro.Color.ColorName,
                            Count = int.Parse(child[1])
                        });
                    }
                }
            }
            return li;
        }

        #region 注销代码 获取缺货和余货列表


        /// <summary>
        /// 获取缺货和余货列表
        /// </summary>
        /// <returns></returns>
        //public ActionResult GetMisWithResProd()
        //{
        //    GridData<object> dat;
        //    List<CheckupDto_t> li = new List<CheckupDto_t>();
        //    var rq = Request["da"];
        //    if (rq != null)
        //    {
        //        string num = rq.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries)[0];
        //        string ty = rq.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries)[1];

        //        var chec = _checkerItemContract.CheckerItems.Where(c => c.CheckGuid == num).OrderByDescending(c => c.CreatedTime).FirstOrDefault();


        //        if (chec != null)
        //        {
        //            if (ty == "mis")
        //            {
        //                li = GetCheckItem(chec.MissingCount);
        //            }
        //            if (ty == "res")
        //            {
        //                li = GetCheckItem(chec.ResidueCount);
        //            }
        //        }
        //    }
        //    dat = new GridData<object>(li, li.Count, Request);
        //    return Json(dat);
        //    //GridRequest request = new GridRequest(Request);
        //    //try
        //    //{
        //    //    Expression<Func<Checker, bool>> predicate = FilterHelper.GetExpression<Checker>(request.FilterGroup);
        //    //    List<CheckedDto_t> li = new List<CheckedDto_t>();
        //    //    var data = await Task.Run(() =>
        //    //    {
        //    //        var count = 0;
        //    //        var list = _checkerContract.Checkers.Where<Checker, int>(predicate, request.PageCondition, out count).Select(m => new CheckedDto_t()
        //    //        {
        //    //            Id = m.CheckGuid,
        //    //            ParentId = "",
        //    //            StoreName = m.Store.StoreName,
        //    //            StorageName = m.Storage.StorageName,
        //    //            CheckerName = m.CheckerName,
        //    //            CheckCount = m.CheckCount,
        //    //            CheckedCount = m.CheckedCount,
        //    //            ValidCount = m.ValidCount,
        //    //            InvalidCount = m.InvalidCount,
        //    //            ResidueCount = m.ResidueCount,
        //    //            MissingCount = m.MissingCount,
        //    //            Notes = m.Notes,
        //    //            CreatedTime = m.UpdatedTime,
        //    //            CheckerState = m.CheckerState,
        //    //            AdminName = m.Operator.AdminName
        //    //        }).ToList();
        //    //        foreach (var item in list)
        //    //        {

        //    //            string checkGuid = item.Id;
        //    //            var chil = _checkerItemContract.CheckerItems.Where(c => c.CheckGuid == checkGuid).ToList().Select(c => new CheckedDto_t()
        //    //            {
        //    //                Id = c.Id.ToString(),
        //    //                ParentId = c.CheckGuid,
        //    //                StoreName = item.StoreName,
        //    //                StorageName = item.StorageName,
        //    //                CheckerName = item.CheckerName,
        //    //                CheckCount = GetCheckItemCou(c.CheckCount),
        //    //                CheckedCount = GetCheckItemCou(c.CheckedCount),
        //    //                ValidCount = GetCheckItemCou(c.ValidCount),
        //    //                InvalidCount = GetCheckItemCou(c.InvalidCount),
        //    //                ResidueCount = GetCheckItemCou(c.ResidueCount),
        //    //                MissingCount = GetCheckItemCou(c.MissingCount),
        //    //                Notes = c.Notes,
        //    //                CreatedTime = c.CreatedTime,
        //    //                CheckerState = c.CheckerState,
        //    //                AdminName = item.AdminName
        //    //            }).ToList();
        //    //            chil = chil.OrderByDescending(c => c.CreatedTime).ToList();
        //    //            li.Add(item);
        //    //            li.AddRange(chil);

        //    //        }
        //    //        return new GridData<object>(li, count, request.RequestInfo);
        //    //    });
        //    //    return Json(data, JsonRequestBehavior.AllowGet);
        //    //}
        //    //catch (Exception)
        //    //{

        //    //    return Json(new GridData<object>(new List<object>(), 0, request.RequestInfo));
        //    //}

        //}

        #endregion

        #region 注销代码-获取数据列表

        /// <summary>
        /// 获取盘点校对信息
        /// </summary>
        /// <returns></returns>
        //public async Task<ActionResult> List()
        //{
        //    //_checkupItemContract.CheckupItems.Where(c=>c.CheckGuid)
        //    GridRequest request = new GridRequest(Request);

        //    GridData<object> dat = null;
        //    var t = request.FilterGroup.Rules.Where(c => c.Field == "Id").FirstOrDefault();

        //    if (t != null)
        //    {
        //        #region MyRegion
        //        List<object> li = new List<object>();
        //        string val = t.Value.ToString();
        //        var checkupitem = _checkupItemContract.CheckupItems.Where(c => c.CheckItemId == val).FirstOrDefault();
        //        if (checkupitem != null)
        //        {
        //            var check = _checkerContract.Checkers.Where(x => x.CheckGuid == checkupitem.CheckGuid).OrderByDescending(c => c.CreatedTime).FirstOrDefault();

        //            li.Add(new
        //            {
        //                Id = check.Id.ToString(),
        //                ParentId = "",
        //                cheguid = check.CheckGuid,
        //                Num = checkupitem.ProductNum,
        //                CheckName = check.CheckerName,
        //                StoreName = check.Store.StoreName,
        //                StorageName = check.Storage.StorageName,
        //                CheckTime = DateTime.Now,
        //                CheckupAdmin = "",
        //                CheckCount = "",
        //                CheckedCount = "",
        //                CheckType = "",
        //                Notes = ""
        //            });
        //            li.Add(new
        //            {
        //                Id = "c" + checkupitem.Id.ToString(),
        //                ParentId = check.Id.ToString(),
        //                cheguid = checkupitem.Id, //////
        //                Num = checkupitem.ProductNum,
        //                CheckName = check.CheckerName,
        //                StoreName = check.Store.StoreName,
        //                StorageName = check.Storage.StorageName,
        //                CheckTime = checkupitem.CreatedTime,
        //                CheckupAdmin = checkupitem.OperatorId.ToString(),
        //                CheckCount = checkupitem.CheckupBeforeCou.ToString(),
        //                CheckedCount = checkupitem.CheckupAfterCou.ToString(),
        //                CheckType = checkupitem.CheckupType.ToString(),
        //                Notes = checkupitem.Notes
        //            });
        //        }
        //        dat = new GridData<object>(li, li.Count, request.RequestInfo);
        //        #endregion
        //    }
        //    else
        //    {
        //        #region MyRegion
        //        Expression<Func<Checker, bool>> predicate = FilterHelper.GetExpression<Checker>(request.FilterGroup);

        //        List<object> li = new List<object>();
        //        dat = await Task.Run(() =>
        //         {
        //             var count = 0;

        //             List<string> pars = _checkerContract.Checkers.Where<Checker>(predicate).Where(x => x.IsDeleted == false && x.IsEnabled == true).Select(c => c.CheckGuid
        //                ).Distinct().ToList();
        //             foreach (string pa in pars)
        //             {

        //                 var che = _checkupItemContract.CheckupItems.Where(c => c.CheckGuid == pa).OrderByDescending(x => x.CreatedTime).FirstOrDefault();
        //                 var check = _checkerContract.Checkers.Where(x => x.CheckGuid == pa).OrderByDescending(c => c.CreatedTime).FirstOrDefault();
        //                 if (che != null)
        //                 {
        //                     li.Add(new
        //                     {
        //                         Id = che.Id.ToString(),
        //                         ParentId = "",
        //                         cheguid = pa,
        //                         Num = che.ProductNum,
        //                         CheckName = check.CheckerName,
        //                         StoreName = check.Store.StoreName,
        //                         StorageName = check.Storage.StorageName,
        //                         CheckTime = DateTime.Now,
        //                         CheckupAdmin = "",
        //                         CheckCount = "",
        //                         CheckedCount = "",
        //                         CheckType = "",
        //                         Notes = ""
        //                     });
        //                     li.AddRange(_checkupItemContract.CheckupItems.Where(c => c.CheckGuid == pa).Select(c => new
        //                     {
        //                         Id = c.CheckGuid,
        //                         ParentId = che.Id.ToString(),
        //                         cheguid = c.Id,
        //                         Num = che.ProductNum,
        //                         CheckName = check.CheckerName,
        //                         StoreName = check.Store.StoreName,
        //                         StorageName = check.Storage.StorageName,
        //                         CheckTime = c.CreatedTime,
        //                         CheckupAdmin = c.OperatorId.ToString(),
        //                         CheckCount = c.CheckupBeforeCou.ToString(),
        //                         CheckedCount = c.CheckupAfterCou.ToString(),
        //                         CheckType = c.CheckupType.ToString(),
        //                         Notes = ""
        //                     }).OrderByDescending(c => c.CheckTime).ToList());
        //                 }

        //             }
        //             return new GridData<object>(li, li.Count, Request);
        //         });
        //        #endregion
        //    }
        //    return Json(dat);

        //}
        #endregion

        #region 校对

        /// <summary>
        /// 校对
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Checkup(string num)
        {
            num = InputHelper.SafeInput(num);
            if (!string.IsNullOrEmpty(num))
            {
                var chec = _checkerContract.Checkers.Where(c => c.CheckGuid == num).FirstOrDefault();
                if (chec != null)
                {
                    ViewBag.CheckName = chec.CheckerName;
                    ViewBag.CheckNum = chec.CheckGuid;
                    ViewBag.StoreName = chec.Store.StoreName;
                    ViewBag.StorageName = chec.Storage.StorageName;
                    ViewBag.CheckTime = chec.UpdatedTime.ToShortDateString() + " " + chec.UpdatedTime.ToLongTimeString();
                    ViewBag.AdminName = chec.Operator.Member.MemberName;
                }
            }
            else
            {
                return RedirectToAction("Index", "Checked", new { area = "Warehouses" });
            }
            ViewBag.CheckGuid = num;
            return View();
        }
        #endregion

        #region 获取缺货和余货的数量

        /// <summary>
        /// 获取缺货和余货的数量
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCheckMisWithRedsCou(string num)
        {
            int misCou = 0;
            int resiCou = 0;
            if (!string.IsNullOrEmpty(num))
            {
                Checker checker = _checkerContract.Checkers.Where(c => c.CheckGuid == num).FirstOrDefault();
                if (checker != null)
                {
                    List<CheckerItem> listCheckerItem = checker.CheckerItems.Where(x => x.IsEnabled == true && x.IsDeleted == false).ToList();
                    misCou = listCheckerItem.Where(x => x.CheckerItemType == (int)CheckerItemFlag.Lack).Count();
                    resiCou = listCheckerItem.Where(x => x.CheckerItemType == (int)CheckerItemFlag.Surplus).Count();
                }
            }
            return Json(new { MisCou = misCou, ResidCou = resiCou });
        }

        #endregion

        #region 获取缺货和余货商品列表
        public async Task<ActionResult> CheckerList()
        {
            GridRequest request = new GridRequest(Request);
            IQueryable<CheckerItem> listCheckerItem = _checkerItemContract.CheckerItems.Where(x => x.ProductId != null);
            listCheckerItem = SearchParams(listCheckerItem, request.FilterGroup);
            Expression<Func<CheckerItem, bool>> predicate = FilterHelper.GetExpression<CheckerItem>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = listCheckerItem.Where<CheckerItem, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.ProductBarcode,
                    m.Product.ProductOriginNumber.Category.CategoryName,
                    m.Product.Color.ColorName,
                    m.Product.Size.SizeName,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                    Count = 1,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 搜索参数
        private IQueryable<CheckerItem> SearchParams(IQueryable<CheckerItem> listCheckerItem, FilterGroup filterGroup)
        {

            object obj = RemoveParams(filterGroup, "ProductNumber");
            if (obj != null)
            {
                string num = obj.ToString();
                listCheckerItem = listCheckerItem.Where(x => x.Product.ProductNumber == num);
            }
            obj = RemoveParams(filterGroup, "ProductName");
            if (obj != null)
            {
                string name = obj.ToString();
                listCheckerItem = listCheckerItem.Where(x => x.Product.ProductName.Contains(name));
            }
            obj = RemoveParams(filterGroup, "CategoryId");
            if (obj != null)
            {
                int categoryId = Convert.ToInt32(obj);
                listCheckerItem = listCheckerItem.Where(x => x.Product.ProductOriginNumber.CategoryId == categoryId);
            }
            obj = RemoveParams(filterGroup, "ColorId");
            if (obj != null)
            {
                int colorId = Convert.ToInt32(obj);
                listCheckerItem = listCheckerItem.Where(x => x.Product.ColorId == colorId);
            }
            return listCheckerItem;
        }
        private object RemoveParams(FilterGroup filterGroup, string strParam)
        {
            FilterRule filterRule = filterGroup.Rules.FirstOrDefault(x => x.Field == strParam);
            object obj = null;
            if (filterRule != null)
            {
                obj = filterRule.Value;
                filterGroup.Rules.Remove(filterRule);
            }
            return obj;
        }

        #endregion

        #region 获取数据列表
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Checker, bool>> predicate = FilterHelper.GetExpression<Checker>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _checkerContract.Checkers.Where<Checker, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.CheckerName,
                    m.CheckGuid,
                    m.Id,
                    m.AfterCheckQuantity,
                    m.ValidQuantity,
                    m.BeforeCheckQuantity,
                    m.Store.StoreName,
                    m.Storage.StorageName,
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

        #region 更改全部缺货和余货
        public JsonResult ChangeAllChecker(int Id, int Flag)
        {

            OperationResult oper = _checkupItemContract.ChangeAllChecker(Id, Flag);
            return Json(oper);
        }
        #endregion

        #region 初始化缺货界面
        /// <summary>
        /// 初始化缺货界面
        /// </summary>
        /// <returns></returns>

        public ActionResult Lack(string num)
        {
            string title = "请选择";
            IEnumerable<SelectListItem> colors = _colorContract.ParentSelectList(title);
            ViewBag.CheckGuid = num;
            ViewBag.Colors = colors;
            return PartialView();
        }
        #endregion

        #region 初始化余货界面
        public ActionResult Surplus(string num)
        {
            string title = "请选择";
            IEnumerable<SelectListItem> colors = _colorContract.ParentSelectList(title);
            //ViewBag.Categories = categories;
            ViewBag.Colors = colors;
            ViewBag.CheckGuid = num;
            return PartialView();
        }
        #endregion

        #region 获取品类
        /// <summary>
        /// 获取品类
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetCategory()
        {
            List<Category> listCategory = _categoryContract.Categorys.Where(x => x.IsEnabled == true && x.IsDeleted == false && x.ParentId == null).ToList();
            List<ChildCategory> parents = listCategory.Select(x => new ChildCategory
            {
                Id = x.Id,
                CategoryName = x.CategoryName,
                Categories = x.Children.Select(k => new BaseCategory()
                {
                    Id = k.Id,
                    CategoryName = k.CategoryName,
                }).ToList(),
            }).ToList();
            return Json(parents);
        }
        #endregion

        #region 移除缺货数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Remove(int[] Id)
        {
            OperationResult oper = _checkupItemContract.RemoveLack(Id);
            return Json(oper);
        }

        [HttpPost]
        public JsonResult RemoveAll(string checkerGuid)
        {
            var checkerEntity = _checkerContract.Checkers.FirstOrDefault(c => !c.IsDeleted && c.IsEnabled && c.CheckGuid == checkerGuid);

            // 获取所有标记为[缺货]的盘点明细
            var itemIds = checkerEntity.CheckerItems.Where(i => i.CheckerItemType == (int)CheckerItemFlag.Lack).Select(item => item.Id).ToArray();
            var oper = _checkupItemContract.RemoveLack(itemIds);
            return Json(oper);
        }
        #endregion

        #region 将余货添加到自己的仓库
        public JsonResult AddInventory(int[] Id)
        {
            OperationResult oper = _checkupItemContract.AddInventory(Id);
            return Json(oper);
        }

        public JsonResult AddAllInventory(string checkerGuid)
        {
            var checkerEntity = _checkerContract.Checkers.FirstOrDefault(c => !c.IsDeleted && c.IsEnabled && c.CheckGuid == checkerGuid);
            // 获取所有标记为[缺货]的盘点明细
            var itemIds = checkerEntity.CheckerItems.Where(i => i.CheckerItemType == (int)CheckerItemFlag.Surplus).Select(item => item.Id).ToArray();
            OperationResult oper = _checkupItemContract.AddInventory(itemIds);
            return Json(oper);
        }
        #endregion

        #region 结束校验
        /// <summary>
        /// 结束校验
        /// </summary>
        /// <param name="CheckerOver"></param>
        /// <returns></returns>
        public JsonResult CheckerOver(string CheckNumber)
        {
            OperationResult oper = _checkerContract.CheckerOver(CheckNumber);
            return Json(oper);
        }
        #endregion

        #region 注销代码-移除缺货记录


        /// <summary>
        /// 移除缺货记录
        /// </summary>
        /// <returns></returns>
        //public JsonResult DelMisProd()
        //{
        //    //1123|2,231|1
        //    OperationResult resul = new OperationResult(OperationResultType.Error);
        //    string pnumWithCou = Request["pnumWithcou"];
        //    string checkNum = Request["checkNum"];
        //    if (!string.IsNullOrEmpty(checkNum))
        //    {
        //        if (string.IsNullOrEmpty(pnumWithCou))
        //        {
        //            var checite = _checkerItemContract.CheckerItems.Where(c => c.CheckGuid == checkNum).OrderByDescending(c => c.CreatedTime).FirstOrDefault();
        //            if (checite != null)
        //                pnumWithCou = checite.MissingCount;
        //        }
        //        string[] chi = pnumWithCou.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);
        //        Checker chec = _checkerContract.Checkers.Where(x => x.CheckGuid == checkNum).FirstOrDefault();
        //        CheckerItem checkerItem = _checkerItemContract.CheckerItems.Where(d => d.CheckGuid == checkNum).OrderByDescending(d => d.CreatedTime).FirstOrDefault();

        //        var cheitedto = AutoMapper.Mapper.Map<CheckerItemDto>(checkerItem);

        //        List<InventoryDto> li = new List<InventoryDto>();
        //        List<CheckupItem> checkupitemLis = new List<CheckupItem>();
        //        List<CheckerDto> cheli = new List<CheckerDto>();
        //        List<CheckerItemDto> checkeritemLis = new List<CheckerItemDto>();
        //        foreach (var itm in chi)
        //        {
        //            string[] childs = itm.Split("|".ToArray(), StringSplitOptions.RemoveEmptyEntries);

        //            string num = childs[0];
        //            int cou = int.Parse(childs[1]);
        //            if (cou <= 0) continue;
        //            //从库存记录中移除记录
        //            #region MyRegion
        //            var invent = _inventoryContract.Inventorys.Where(c => c.Product.ProductNumber == num).FirstOrDefault();
        //            if (invent != null)
        //            {
        //                //invent.Quantity -= cou;
        //                //if (invent.Quantity < 0)
        //                //    invent.Quantity = 0;
        //                InventoryDto dto = AutoMapper.Mapper.Map<InventoryDto>(invent);
        //                resul = _inventoryContract.Update(new InventoryDto[]{dto});
        //            }
        //            #endregion
        //            //变更盘点记录
        //            #region MyRegion

        //            if (chec != null && invent != null)
        //            {
        //                chec.MissingCount -= cou;
        //                if (chec.MissingCount < 0)
        //                    chec.MissingCount = 0;
        //                if (checkerItem != null)
        //                {
        //                    var t = checkerItem.MissingCount;
        //                    string re = ",?(" + invent.Product.ProductNumber + "\\|(\\d+)),?.*";
        //                    Regex reg = new Regex(re);
        //                    var mat = reg.Match(t);
        //                    if (mat != null)
        //                    {
        //                        string rs = mat.Result("$1");
        //                        int _cou = Convert.ToInt32(mat.Result("$2")) - cou;
        //                        if (_cou < 0) _cou = 0;
        //                        string newstr = invent.Product.ProductNumber + "|" + _cou;
        //                        cheitedto.MissingCount = checkerItem.MissingCount.Replace(rs, newstr);

        //                    }
        //                }
        //            }
        //            chec.CheckerState = 4;
        //            var checkdto = AutoMapper.Mapper.Map<CheckerDto>(chec);
        //            resul = _checkerContract.Insert(checkdto);

        //            cheitedto.CheckerState = 5;
        //            cheitedto.Id = 0;

        //            resul = _checkerItemContract.Insert(cheitedto);

        //            CheckupItem checkupitem = new CheckupItem()
        //            {
        //                CheckGuid = checkNum,
        //                CheckItemId = "",
        //                CheckupType = 1,
        //                IsDeleted = false,
        //                IsEnabled = true,
        //                OperatorId = AuthorityHelper.OperatorId,
        //                ProductNum = invent.Product.ProductNumber,
        //                UpdatedTime = DateTime.Now,
        //                CreatedTime = DateTime.Now,
        //                CheckupBeforeCou = GetCheckupBeforeCou(checkNum),
        //                CheckupAfterCou = GetCheckupAfterCou(chec.SelectWhere),
        //                CheckupBeforeMissCou = GetCheckupBeforeMissCou(checkNum),
        //                CheckupAfterMissCou = GetCheckupAfterMissCou(checkerItem),
        //                CheckupBeforeResCou = GetCheckupBeforeResCou(checkNum),
        //                CheckupAfterResCou = GetCheckupAfterResCou(checkerItem),
        //            };
        //            if (resul.ResultType == OperationResultType.Success)
        //            {
        //                var ids = resul.Data as int[];
        //                if (ids != null)
        //                    checkupitem.CheckItemId = ids[0].ToString();
        //            }
        //            resul = _checkupItemContract.Insert(checkupitem);
        //            #endregion
        //        }
        //    }
        //    return Json(resul);
        //}
        #endregion

        #region 注销代码--将余货插入库存记录


        /// <summary>
        /// 将余货插入库存记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        //public JsonResult InsertResidProd()
        //{
        //    //1123|2,231|1
        //    //pnumWithcou: pnumWithcous, checkNum: checkNum
        //    OperationResult res = new OperationResult(OperationResultType.Error);
        //    string checkNum = Request["checkNum"];
        //    string pnumWithcou = Request["pnumWithcous"];
        //    if (!string.IsNullOrEmpty(checkNum))
        //    {
        //        if (string.IsNullOrEmpty(pnumWithcou))
        //        {

        //            var che = _checkerItemContract.CheckerItems.Where(c => c.CheckGuid == checkNum).OrderByDescending(c => c.CreatedTime).FirstOrDefault();
        //            if (che != null)
        //                pnumWithcou = che.ResidueCount;
        //        }
        //        string[] chils = pnumWithcou.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);

        //        Checker ch = _checkerContract.Checkers.Where(c => c.CheckGuid == checkNum).FirstOrDefault();
        //        var checkerItem = _checkerItemContract.CheckerItems.Where(c => c.CheckGuid == checkNum).OrderByDescending(c => c.CreatedTime).FirstOrDefault();
        //        var cheitemdto = AutoMapper.Mapper.Map<CheckerItemDto>(checkerItem);
        //        CheckerDto dto = null;
        //        // List<InventoryDto> li = new List<InventoryDto>();
        //        foreach (string te in chils)
        //        {
        //            string[] childs = te.Split("|".ToArray(), StringSplitOptions.RemoveEmptyEntries);
        //            string num = childs[0];
        //            int cou = int.Parse(childs[1]);
        //            if (cou <= 0) continue;
        //            //修改库存
        //            #region MyRegion
        //            Inventory invent = _inventoryContract.Inventorys.Where(c => c.Product.ProductNumber == num).FirstOrDefault();
        //            if (invent == null)
        //            {
        //                //在库存记录中未找到，去商品信息中查找
        //                var pro = _productContract.Products.Where(c => c.ProductNumber == num).FirstOrDefault();
        //                if (pro != null)
        //                {
        //                    invent = new Inventory()
        //                    {
        //                        ProductId = pro.Id,
        //                        StoreId = ch.StoreId,
        //                        StorageId = ch.StorageId,
        //                       // Quantity = cou,
        //                        UpdatedTime = DateTime.Now,
        //                        CreatedTime = DateTime.Now,
        //                        IsDeleted = false,
        //                        IsEnabled = true,
        //                        Description = "余货入库",
        //                        Product = pro

        //                    };
        //                }
        //            }
        //            else
        //            {
        //               // invent.Quantity = invent.Quantity + cou;
        //                invent.IsDeleted = false;
        //                invent.IsEnabled = true;
        //            }
        //            //修改盘点记录
        //            ch.ResidueCount -= cou;
        //            //修改 checkeritem
        //            if (checkerItem != null)
        //            {
        //                var t = checkerItem.ResidueCount;
        //                string re = ",?(" + invent.Product.ProductNumber + "\\|(\\d+)),?.*";
        //                Regex reg = new Regex(re);
        //                var mat = reg.Match(t);
        //                if (mat != null)
        //                {
        //                    string rs = mat.Result("$1");
        //                    int _cou = Convert.ToInt32(mat.Result("$2")) - cou;
        //                    if (_cou < 0)
        //                        _cou = 0;
        //                    string newstr = invent.Product.ProductNumber + "|" + _cou;
        //                    cheitemdto.ResidueCount = checkerItem.ResidueCount.Replace(rs, newstr);
        //                }
        //            }
        //            //checkupitem 
        //            CheckupItem cheupitem = new CheckupItem()
        //            {
        //                CheckGuid = checkNum,
        //                CheckItemId = "",
        //                CheckupType = 2,
        //                IsDeleted = false,
        //                IsEnabled = true,
        //                OperatorId = AuthorityHelper.OperatorId,
        //                ProductNum = invent.Product.ProductNumber,
        //                UpdatedTime = DateTime.Now,
        //                CreatedTime = DateTime.Now,
        //                CheckupBeforeCou = GetCheckupBeforeCou(checkNum),
        //                // CheckupAfterCou = GetCheckupAfterCou(checkerItem),
        //                CheckupBeforeMissCou = GetCheckupBeforeMissCou(checkNum),
        //                CheckupAfterMissCou = GetCheckupAfterMissCou(checkerItem),
        //                CheckupBeforeResCou = GetCheckupBeforeResCou(checkNum),
        //                CheckupAfterResCou = GetCheckupAfterResCou(checkerItem),
        //            };
        //            var inventDto = AutoMapper.Mapper.Map<InventoryDto>(invent);

        //            res = _inventoryContract.Insert(inventDto);
        //            if (res.ResultType == OperationResultType.Success)
        //            {

        //                cheupitem.CheckupAfterCou = GetCheckupAfterCou(ch.SelectWhere);
        //                if (ch.ResidueCount < 0)
        //                    ch.ResidueCount = 0;
        //                ch.CheckerState = 4;
        //                dto = AutoMapper.Mapper.Map<CheckerDto>(ch);
        //                res = _checkerContract.Update(dto);
        //                if (res.ResultType == OperationResultType.Success)
        //                {
        //                    cheitemdto.CheckerState = 5;
        //                    cheitemdto.Id = 0;
        //                    res = _checkerItemContract.Insert(cheitemdto);
        //                    if (res.ResultType == OperationResultType.Success)
        //                    {
        //                        var ids = res.Data as int[];
        //                        if (ids != null)
        //                            cheupitem.CheckItemId = ids[0].ToString();
        //                        res = _checkupItemContract.Insert(cheupitem);
        //                    }
        //                }
        //            }


        //            #endregion

        //        }
        //    }
        //    return Json(res);

        //}
        #endregion

        #region 注销代码--获取校对后的余货


        /// <summary>
        /// 获取校对后的余货
        /// </summary>
        /// <param name="checkNum"></param>
        /// <returns></returns>
        //private int GetCheckupAfterResCou(CheckerItem checkerItem)
        //{
        //    int cou = 0;

        //    if (checkerItem != null)
        //    {
        //        string tx = checkerItem.ResidueCount;
        //        string[] chils = tx.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);
        //        foreach (string item in chils)
        //        {
        //            string[] childs = item.Split("|".ToArray(), StringSplitOptions.RemoveEmptyEntries);
        //            cou += Convert.ToInt32(childs[1]);
        //        }
        //    }
        //    return cou;
        //}
        #endregion

        #region 注销代码-获取校对前的余货

        /// <summary>
        /// 获取校对前的余货
        /// </summary>
        /// <param name="checkNum"></param>
        /// <returns></returns>
        //private int GetCheckupBeforeResCou(string checkNum)
        //{
        //    int cou = 0;
        //    var che = _checkerItemContract.CheckerItems.Where(c => c.CheckGuid == checkNum && c.CheckerState != 5).OrderByDescending(c => c.CreatedTime).FirstOrDefault();
        //    if (che != null)
        //    {
        //        string tx = che.ResidueCount;
        //        string[] chils = tx.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);
        //        foreach (string item in chils)
        //        {
        //            string[] childs = item.Split("|".ToArray(), StringSplitOptions.RemoveEmptyEntries);
        //            cou += Convert.ToInt32(childs[1]);
        //        }
        //    }
        //    return cou;
        //}
        #endregion

        #region 注销代码-获取校对后的缺货数量


        /// <summary>
        /// 获取校对后的缺货数量
        /// </summary>
        /// <param name="checkNum"></param>
        /// <returns></returns>
        //private int GetCheckupAfterMissCou(CheckerItem checkeritem)
        //{
        //    int cou = 0;
        //    if (checkeritem != null)
        //    {
        //        string tx = checkeritem.MissingCount;
        //        string[] chis = tx.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);
        //        foreach (string item in chis)
        //        {
        //            string[] childs = item.Split("|".ToArray(), StringSplitOptions.RemoveEmptyEntries);
        //            cou += Convert.ToInt32(childs[1]);
        //        }

        //    }
        //    return cou;
        //}
        #endregion

        #region 注销代码-获取校对前的缺货数量

        /// <summary>
        /// 获取校对前的缺货数量
        /// </summary>
        /// <param name="checkNum"></param>
        /// <returns></returns>
        //private int GetCheckupBeforeMissCou(string checkNum)
        //{
        //    int cou = 0;
        //    var che = _checkerItemContract.CheckerItems.Where(c => c.CheckGuid == checkNum && c.CheckerState != 5).OrderByDescending(c => c.CreatedTime).FirstOrDefault();
        //    if (che != null)
        //    {
        //        string tx = che.MissingCount;
        //        string[] chis = tx.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);
        //        foreach (string item in chis)
        //        {
        //            string[] childs = item.Split("|".ToArray(), StringSplitOptions.RemoveEmptyEntries);
        //            cou += Convert.ToInt32(childs[1]);
        //        }
        //    }
        //    return cou;
        //}
        #endregion

        #region 获取校对后的总数

        /// <summary>
        /// 获取校对后的总数
        /// </summary>
        /// <param name="checkNum"></param>
        /// <returns></returns>
        private int GetCheckupAfterCou(string selectWhere)
        {
            if (string.IsNullOrEmpty(selectWhere))
                return 0;
            int cou = 0;
            CheckedType chety = new JavaScriptSerializer().Deserialize<CheckedType>(selectWhere) as CheckedType;
            var che = _inventoryContract.Inventorys.Where(c => c.IsDeleted == false && c.IsEnabled == true);
            if (chety != null)
            {
                if (Convert.ToInt32(chety.StoreId) > 0)
                {
                    int storeId = Convert.ToInt32(chety.StoreId);
                    che = che.Where(c => c.StoreId == storeId);
                }
                if (Convert.ToInt32(chety.StorageId) > 0)
                {
                    int storageId = Convert.ToInt32(chety.StorageId);
                    che = che.Where(c => c.StorageId == storageId);
                }
                if (chety.BrandId > 0)
                {
                    che = che.Where(c => c.Product.ProductOriginNumber.BrandId == chety.BrandId);
                }

                if (chety.SeasonId > 0)
                {
                    che = che.Where(c => c.Product.ProductOriginNumber.SeasonId == chety.SeasonId);
                }
                if (chety.ColorId > 0)
                {
                    che = che.Where(c => c.Product.ColorId == chety.ColorId);
                }
            }
            cou = 1;//che.Select(c => c.Quantity).Sum();

            return cou;
        }
        #endregion

        #region 注销代码-获取校对开始前的总数

        /// <summary>
        /// 获取校对开始前的总数
        /// </summary>
        /// <param name="cheNum"></param>
        /// <returns></returns>
        //private int GetCheckupBeforeCou(string cheNum)
        //{
        //    int cou = 0;
        //    if (!string.IsNullOrEmpty(cheNum))
        //    {
        //        var checCouStr = _checkerItemContract.CheckerItems.Where(c => c.CheckGuid == cheNum).OrderBy(c => c.CreatedTime).FirstOrDefault().CheckCount;
        //        if (!string.IsNullOrEmpty(checCouStr))
        //        {
        //            string[] str = checCouStr.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);

        //            foreach (string item in str)
        //            {
        //                string[] chil = item.Split("|".ToArray(), StringSplitOptions.RemoveEmptyEntries);
        //                cou += int.Parse(chil[1]);
        //            }
        //        }
        //    }
        //    return cou;
        //}
        #endregion

        #region 注销代码-修改指定的盘点记录的校对状态


        /// <summary>
        /// 修改指定的盘点记录的校对状态
        /// </summary>
        /// <param name="cheNum">盘点编号</param>
        /// <returns></returns>
        //[HttpPost]
        //[Log]
        //public JsonResult EmptCheckupSaveState(string cheNum)
        //{
        //    OperationResult opr = new OperationResult(OperationResultType.Error, "未查找到盘点记录");
        //    cheNum = InputHelper.SafeInput(cheNum);
        //    if (!string.IsNullOrEmpty(cheNum))
        //    {
        //        Checker ck = _checkerContract.Checkers.Where(c => c.CheckGuid == cheNum).OrderByDescending(x => x.CreatedTime).FirstOrDefault();
        //        if (ck != null)
        //        {
        //            if (ck.CheckerState != 4)
        //            {
        //                ck.CheckerState = 4;
        //                ck.UpdatedTime = DateTime.Now;
        //                CheckerDto chedto = AutoMapper.Mapper.Map<CheckerDto>(ck);
        //                opr = _checkerContract.Update(chedto);
        //                if (opr.ResultType == OperationResultType.Success)
        //                {
        //                    //更新checkeritem
        //                    CheckerItem cheitem = _checkerItemContract.CheckerItems.Where(c => c.CheckGuid == cheNum).OrderByDescending(c => c.CreatedTime).FirstOrDefault();
        //                    if (cheitem != null)
        //                    {
        //                        cheitem.CheckerState = 5;
        //                        cheitem.CreatedTime = DateTime.Now;
        //                    }
        //                    CheckerItemDto cheitemDto = AutoMapper.Mapper.Map<CheckerItemDto>(cheitem);
        //                    opr = _checkerItemContract.Insert(cheitemDto);
        //                }
        //            }
        //            else opr = new OperationResult(OperationResultType.Success, "ok");
        //        }
        //    }
        //    return Json(opr);
        //}
        #endregion

    }
}