using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities.StoreCollocation;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Contracts.StoreCollocation;
using Whiskey.ZeroStore.ERP.Transfers.Entities.StoreCollocation;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Models;

namespace Whiskey.ZeroStore.ERP.Website.Areas.StoreCollocation.Controllers
{
    public class StoreController : BaseController
    {


        private const string SESSION_KEY_INVALID_LIST = "StoreCollocation_Invalid";
        private const string SESSION_KEY_SC_LIST = "StoreCollocation_data";
        private const string SESSION_KEY_VALID_LIST = "StoreCollocation_Valid";

        protected readonly IStoreProductCollocationContract _storeProductCollocationContract;
        protected readonly IStoreCollocationInfoContract _storeCollocationInfoContract;
        protected readonly IProductContract _productContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IStoreContract _storeContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IColorContract _colorContract;
        protected readonly ISeasonContract _seasonContract;
        protected readonly ISizeContract _sizeContract;
        protected readonly IProductAttributeContract _productAttributeContract;

        public StoreController(IStoreProductCollocationContract storeProductCollocationContract, IStoreCollocationInfoContract storeCollocationInfoContract,
            IProductContract productContract, IBrandContract brandContract,
            IStoreContract storeContract, ICategoryContract categoryContract,
            IColorContract colorContract, ISeasonContract seasonContract,
            ISizeContract sizeContract, IProductAttributeContract productAttributeContract)
        {
            _storeProductCollocationContract = storeProductCollocationContract;
            _storeCollocationInfoContract = storeCollocationInfoContract;
            _productContract = productContract;
            _brandContract = brandContract;
            _storeContract = storeContract;
            _categoryContract = categoryContract;
            _colorContract = colorContract;
            _seasonContract = seasonContract;
            _sizeContract = sizeContract;
            _productAttributeContract = productAttributeContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            GridRequest request = new GridRequest(Request);
            string storeId = Request["StoreId"];
            Expression<Func<StoreProductCollocation, bool>> predicate = FilterHelper.GetExpression<StoreProductCollocation>(request.FilterGroup);
            var spd = _storeProductCollocationContract.StoreProductCollocations.Where(predicate).Where(x => x.CollocationName != "").Include(x => x.Operator).ToList();
            var filterList = new List<StoreProductCollocation>();
            if (!string.IsNullOrEmpty(storeId) && storeId != "0")
            {
                foreach (var item in spd)
                {
                    var arry = GetStoreNameArry(item.StoreId);
                    if (arry.Contains(storeId))
                    {
                        filterList.Add(item);
                    }
                }
            }
            else
            {
                filterList = spd.ToList();
            }
            int count = filterList.ToList().Count();
            var source = filterList.OrderByDescending(c => c.CreatedTime)
                   .Skip(request.PageCondition.PageIndex)
                   .Take(request.PageCondition.PageSize);
            List<object> lis = new List<object>();

            foreach (var item in source)
            {
                if (item.Guid != null)
                {
                    var id = _storeProductCollocationContract.StoreProductCollocations.Where(o => o.Guid == item.Guid).Select(x => x.Id).FirstOrDefault();
                    var querySouce = from Chitem in _storeCollocationInfoContract.StoreCollocationInfos
                                     join product in _productContract.Products on Chitem.ProductOrigNumberId equals product.Id into Joinitem
                                     from product in Joinitem.DefaultIfEmpty()
                                     where Chitem.IsDeleted == item.IsDeleted && Chitem.IsEnabled == item.IsEnabled
                                     && Chitem.StoreCollocationId == id
                                     select new
                                     {
                                         Chitem.Id,
                                         product.ThumbnailPath,
                                         product.ProductNumber,
                                         product.ProductOriginNumber,
                                         product.Size,
                                         product.Color,
                                         product.CreatedTime,
                                         product.IsDeleted,
                                         product.IsEnabled,
                                         Chitem.Operator
                                     };



                    var da = querySouce.OrderByDescending(c => c.Id)
                        .Select(c => new
                        {
                            Id = "childStore" + c.Id,
                            ParentId = "par" + id,
                            Guid = "",
                            c.ThumbnailPath,
                            CollocationName = c.ProductNumber,
                            c.ProductOriginNumber.TagPrice,
                            StoreId = "",
                            CreatedTime = c.CreatedTime,
                            c.IsDeleted,
                            c.IsEnabled,
                            c.Operator.Member.MemberName
                        }).ToList();
                    int childCount = da.ToList().Count();
                    count += childCount;
                    var par = new
                    {
                        Id = "par" + item.Id,
                        Guid = item.Guid,
                        ParentId = "",
                        ThumbnailPath = item.ThumbnailPath,
                        CollocationName = item.CollocationName,
                        count = childCount,
                        TagPrice = "",
                        StoreId = GetCount(item.StoreId),
                        CreatedTime = item.CreatedTime,
                        item.IsDeleted,
                        item.IsEnabled,
                        MemberName = item.Operator == null ? "" : item.Operator.Member.MemberName

                    };
                    lis.Add(par);
                    lis.AddRange(da);
                }
                else
                {
                    var par = new
                    {
                        Id = "par" + item.Id,
                        Guid = item.Guid,
                        ParentId = "",
                        ThumbnailPath = item.ThumbnailPath,
                        CollocationName = item.CollocationName,
                        count = 0,
                        TagPrice = "",
                        StoreId = GetCount(item.StoreId),
                        CreatedTime = item.CreatedTime,
                        item.Operator.Member.MemberName
                    };
                    lis.Add(par);
                }
            }
            GridData<object> data = new GridData<object>(lis, count, request.RequestInfo);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public int GetCount(string storeId)
        {
            return storeId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().Count();
        }

        public string GetStoreName(string storeId)
        {
            var store = storeId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var list = _storeContract.Stores.Where(x => store.Contains(x.Id.ToString())).Select(x => x.StoreName).ToList();
            return string.Join(",", list.ToArray());
        }
        public List<string> GetStoreNameArry(string storeId)
        {
            if (!string.IsNullOrEmpty(storeId))
            {
                var store = storeId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                return store;
            }
            else
            {
                return null;
            }
        }

        public ActionResult Create()
        {
            string guid = Guid.NewGuid().ToString().Replace("-", "");
            //删除无效数据
            var date = DateTime.Now.Date.AddDays(-1);
            var items = _storeProductCollocationContract.StoreProductCollocations.Where(
                x => string.IsNullOrEmpty(x.CollocationName)).Where(x => x.CreatedTime <= date).Select(x => x.Id).ToArray();
            if (items.Any())
            {
                _storeCollocationInfoContract.DeleteByCollocationId(items);
                _storeProductCollocationContract.TrueRemove(items);
            }
            //foreach (var a in items) {
            //    if (DateTime.Now.Subtract(a.CreatedTime).TotalDays > 1)
            //    {

            //    }
            //}

            _storeProductCollocationContract.Insert(new StoreProductCollocation() { CollocationName = "", Guid = guid });
            ViewBag.Styles = GetSelect("风格");//风格
            ViewBag.Situation = GetSelect("场合");//场合
            ViewBag.Season = GetSelect("季节");
            ViewBag.Shape = GetSelect("体型");
            ViewBag.Effect = GetSelect("效果");
            ViewBag.Colour = GetSelect("颜色");
            ViewBag.uid = guid;
            return PartialView();
        }

        public List<SelectListItem> GetSelect(string name)
        {
            var id = _productAttributeContract.ProductAttributes.Where(x => x.AttributeName == name).Select(x => x.Id).FirstOrDefault();
            var list = _productAttributeContract.ProductAttributes.Where(x => x.ParentId == id && x.IsDeleted == false && x.IsEnabled == true)
                .Select(x => new SelectListItem
                {
                    Text = x.AttributeName,
                    Value = x.Id.ToString()
                }).ToList();
            return list;
        }



        public ActionResult BatchImport()
        {
            return PartialView();
        }

        #region 上传Excel表格
        public JsonResult ExcelFileUpload()
        {
            var res = new OperationResult(OperationResultType.Error);
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                string fileName = file.FileName;
                string savePath = Server.MapPath("/Content/UploadFiles/Excels/") + DateTime.Now.ToString("yyyyMMddHH");
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                string fullName = savePath + "\\" + fileName;

                if (System.IO.File.Exists(fullName))
                {
                    System.IO.File.Delete(fullName);
                }
                file.SaveAs(fullName);
                var reda = ExcelToJson(fullName);
                System.IO.File.Delete(fullName);
                if (reda.Any())
                {
                    var list = reda.Select(s => s.First()).ToList();
                    res = new OperationResult(OperationResultType.Success, string.Empty, list);
                }
            }
            return Json(res);
        }
        #endregion

        #region 读取Excel文件

        private List<List<String>> ExcelToJson(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                var da = new List<List<String>>();
                if (Path.GetExtension(fileName) == ".txt")
                {
                    string st = System.IO.File.ReadAllText(fileName);
                    var retda = st.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var li = new List<List<string>>();
                    retda.Each(c =>
                    {
                        var t = new List<string>() { c };
                        li.Add(t);
                    });
                    da = li;
                }
                else
                {
                    YxkSabri.ExcelUtility excel = new YxkSabri.ExcelUtility();
                    da = excel.ExcelToArray(fileName, 0, 0);
                }
                return da;
            }
            return null;
        }
        #endregion

        /// <summary>
        /// 批量导入校验
        /// </summary>
        public ActionResult MultitudeVaild(string nums, string uid)
        {
            var dat = nums.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int storeCollocationId = _storeProductCollocationContract.StoreProductCollocations.Where(o => o.Guid == uid).Select(x => x.Id).FirstOrDefault();
            var result = new OperationResult(OperationResultType.Error, "");
            var validListFromCache = (List<Product_Model>)SessionAccess.Get(SESSION_KEY_VALID_LIST + uid) ?? new List<Product_Model>();
            var invalidlistFromCache = (List<Product_Model>)SessionAccess.Get(SESSION_KEY_INVALID_LIST + uid) ?? new List<Product_Model>();
            var storeCollocation = _storeProductCollocationContract.StoreProductCollocations.Where(o => !o.IsDeleted && o.IsEnabled)
                                                                  .Where(o => o.Id == storeCollocationId)
                                                                  .Include(o => o.StoreCollocationInfoItems)
                                                                  .FirstOrDefault();


            string strMessage = string.Empty;

            var modelList = dat.Select(barcode => new Product_Model { ProductBarcode = barcode, UUID = Guid.NewGuid().ToString() }).ToList();


            var checkRes = CheckCollcationEntity(storeCollocation);
            if (!checkRes.Item1)
            {
                invalidlistFromCache.Add(new Product_Model { ProductBarcode = string.Empty });
            }
            else //批量校验
            {
                var tmpValidModels = new List<Product_Model>();
                //var allValid = true;
                var orderblankItemsFromDb = storeCollocation.StoreCollocationInfoItems.ToList();
                foreach (var modelToCheck in modelList)
                {
                    var res = CheckBarcode(modelToCheck, validListFromCache, invalidlistFromCache, orderblankItemsFromDb, storeCollocationId);
                    if (!res.Item1)
                    {
                        //allValid = false;
                        modelToCheck.Notes = res.Item2;
                        invalidlistFromCache.Add(modelToCheck);
                    }
                    else
                    {
                        tmpValidModels.Add(modelToCheck);
                        validListFromCache.Add(modelToCheck);
                    }

                }
                if (tmpValidModels.Count > 0)
                {
                    var optRes = BatchAddCollocationItem(storeCollocation, orderblankItemsFromDb, tmpValidModels.ToArray());
                    if (optRes.ResultType != OperationResultType.Success)
                    {
                        invalidlistFromCache.Add(new Product_Model { ProductBarcode = string.Empty, Notes = optRes.Message });
                    }

                }
            }

            SessionAccess.Set(SESSION_KEY_VALID_LIST + uid, validListFromCache);
            SessionAccess.Set(SESSION_KEY_INVALID_LIST + uid, invalidlistFromCache);
            result.Data = new { validCount = validListFromCache.Count, invalidCount = invalidlistFromCache.Count };
            result.ResultType = OperationResultType.Success;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //校验店铺搭配是否存在
        private Tuple<bool, string> CheckCollcationEntity(StoreProductCollocation entity)
        {
            if (entity == null)
            {
                return Tuple.Create(false, "搭配不存在");
            }


            if (entity.IsDeleted)
            {
                return Tuple.Create(false, "搭配已经被删除");
            }
            if (!entity.IsEnabled)
            {
                return Tuple.Create(false, "搭配被禁用");
            }
            return Tuple.Create(true, string.Empty);
        }

        //校验单品
        private Tuple<bool, string> CheckBarcode(Product_Model model, List<Product_Model> validList, List<Product_Model> invalidlist, List<StoreCollocationInfo> CollocationInfo, int storeCollocationId)
        {
            var result = new OperationResult(OperationResultType.Error, "");
            var number = model.ProductBarcode;
            if (invalidlist.Any(c => c.ProductBarcode == number))
            {
                return Tuple.Create(false, "商品条码无效，并且已重复出现：" + number);
            }
            // 长度校验
            if (number.Length != 11)
            {
                return Tuple.Create(false, "扫入的商品条码不符合要求");
            }

            // 排重
            if (validList.Any(c => c.ProductBarcode == number))
            {
                return Tuple.Create(false, "商品条码已经扫入：" + number);
            }

            #region 校验单品信息
            var inventoryEntity = CollocationInfo.FirstOrDefault(c => c.ProductBarcode == number);
            if (inventoryEntity == null)
            {
                var prouct = _productContract.Products.Where(x => x.ProductNumber == number)
                    .FirstOrDefault();
                // 装载数据
                model.Id = prouct.Id;
                model.ProductId = storeCollocationId;
                model.IsValided = true;
                model.ProductNumber = number;
                model.Thumbnail = prouct.ProductOriginNumber.ThumbnailPath;
                model.Brand = prouct.ProductOriginNumber.Brand.BrandName;
                model.Size = prouct.Size.SizeName;
                model.Color = prouct.Color.ColorName;
                model.Season = prouct.ProductOriginNumber.Season.SeasonName;
                model.Category = prouct.ProductOriginNumber.Category.CategoryName;
                return Tuple.Create(true, string.Empty);
            }
            else
            {

                if (inventoryEntity.IsDeleted)
                {
                    return Tuple.Create(false, "在该搭配中存在该商品条码：" + number + ",但该库存已经被移至回收站");
                }
                if (!inventoryEntity.IsEnabled)
                {
                    return Tuple.Create(false, "在该搭配中存在该商品条码：" + number + ",但该库存处于禁用状态");
                }
                else
                {
                    return Tuple.Create(false, "在该搭配中存在该商品条码：" + number);
                }
            }
            #endregion
        }

        private OperationResult BatchAddCollocationItem(StoreProductCollocation orderblankEntity, List<StoreCollocationInfo> inventoryList, params Product_Model[] models)
        {
            var barcodes = models.Select(m => m.ProductBarcode).ToList();
            var orderblankItemsFromDb = orderblankEntity.StoreCollocationInfoItems.ToList();
            var barcodseFromDb = orderblankItemsFromDb.Select(x => x.ProductBarcode).ToList();
            var errorCodes = barcodseFromDb.Intersect(barcodes);
            if (errorCodes.Any())
            {
                return new OperationResult(OperationResultType.Error, "存在已添加的条码" + string.Join(",", errorCodes));
            }
            List<StoreCollocationInfo> sci = new List<StoreCollocationInfo>();
            foreach (var groupItem in models)
            {
                var itemEntity = orderblankItemsFromDb.FirstOrDefault(i => i.StoreCollocationId == groupItem.ProductId
                && i.ProductOrigNumberId == groupItem.Id
                );
                if (itemEntity == null)
                {
                    //新增
                    var entity = new StoreCollocationInfo()
                    {
                        StoreCollocationId = groupItem.ProductId, //OrderblankId = orderblankEntity.Id,
                        ProductOrigNumberId = groupItem.Id,
                        ProductBarcode = groupItem.ProductNumber,

                        OperatorId = AuthorityHelper.OperatorId
                    };
                    sci.Add(entity);
                }
            }

            //删除原有
            //_storeCollocationInfoContract.DeleteByCollocationId(orderblankEntity.Id);
            //添加
            return _storeCollocationInfoContract.Insert(sci.ToArray());
        }

        public ActionResult GetProductList()
        {
            ViewBag.Brand = CacheAccess.GetBrand(_brandContract, true);
            return PartialView();
        }

        public JsonResult ProductList(string productNumber, int? brandId)
        {

            var querySouce = _productContract.Products.Where(x => !x.IsDeleted && x.IsEnabled);

            var requ = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<Product>(requ.FilterGroup);

            if (!string.IsNullOrEmpty(productNumber))
            {
                querySouce = querySouce.Where(i => i.ProductNumber == productNumber);
            }
            if (brandId.HasValue && brandId != -1)
            {
                querySouce = querySouce.Where(i => i.ProductOriginNumber.BrandId == brandId.Value);
            }
            var da = querySouce.OrderByDescending(c => c.Id)
                .Skip(requ.PageCondition.PageIndex)
                .Take(requ.PageCondition.PageSize)
                .Select(c => new
                {
                    Id = c.Id,
                    ParentId = "",
                    ThumbnailPath = c.ThumbnailPath ?? c.ProductOriginNumber.ThumbnailPath,
                    c.ProductNumber,
                    c.ProductOriginNumber.Brand.BrandName,
                    c.Size.SizeName,
                    c.Color.ColorName,
                    c.ProductOriginNumber.Season.SeasonName,
                    c.ProductOriginNumber.Category.CategoryName
                }).ToList();
            GridData<Object> objdata = new GridData<object>(da, querySouce.Count(), Request);
            return Json(objdata);
        }

        public JsonResult CollcationViewList(string uid)
        {
            var id = _storeProductCollocationContract.StoreProductCollocations.Where(o => o.Guid == uid).Select(x => x.Id).FirstOrDefault();
            var requ = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<Product>(requ.FilterGroup);

            var querySouce = from item in _storeCollocationInfoContract.StoreCollocationInfos
                             join product in _productContract.Products on item.ProductOrigNumberId equals product.Id into Joinitem
                             from product in Joinitem.DefaultIfEmpty()
                             where item.IsDeleted == false && item.IsEnabled == true
                             && product.IsDeleted == false && product.IsEnabled == true
                             && item.StoreCollocationId == id
                             select new
                             {
                                 item.Id,
                                 product.ThumbnailPath,
                                 product.ProductNumber,
                                 product.ProductOriginNumber,
                                 product.Size,
                                 product.Color
                             };



            var da = querySouce.OrderByDescending(c => c.Id)
                .Skip(requ.PageCondition.PageIndex)
                .Take(requ.PageCondition.PageSize)
                .Select(c => new
                {
                    Id = c.Id,
                    ParentId = "",
                    c.ThumbnailPath,
                    c.ProductNumber,
                    c.ProductOriginNumber.Brand.BrandName,
                    c.Size.SizeName,
                    c.Color.ColorName,
                    c.ProductOriginNumber.Season.SeasonName,
                    c.ProductOriginNumber.Category.CategoryName
                }).ToList();
            GridData<Object> objdata = new GridData<object>(da, querySouce.Count(), Request);
            return Json(objdata);
        }

        //查看无效数据
        public ActionResult InValid(string uid)
        {
            ViewBag.uuid = uid;
            return PartialView();
        }
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public ActionResult InValidList(string uid)
        {

            var request = new GridRequest(Request);
            int count = 0;
            int pageIndex = request.PageCondition.PageIndex;
            int pageSize = request.PageCondition.PageSize;
            var invalidlistFromCache = (List<Product_Model>)SessionAccess.Get(SESSION_KEY_INVALID_LIST + uid) ?? new List<Product_Model>();
            count = invalidlistFromCache.Count;
            var index = 1;
            var resData = invalidlistFromCache.Skip(pageIndex).Take(pageSize).Select(p => new { Id = index++, ProductBarcode = p.ProductBarcode, Notes = p.Notes }).ToList();
            var data = new GridData<object>(resData, count, request.RequestInfo);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //查看无效数据
        public ActionResult VaildView(string uid)
        {
            ViewBag.uuid = uid;
            return PartialView();
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public ActionResult VaildViewList(string uid)
        {
            GridRequest request = new GridRequest(Request);
            int count = 0;
            int pageIndex = request.PageCondition.PageIndex;
            int pageSize = request.PageCondition.PageSize;
            var validListFromCache = (List<Product_Model>)SessionAccess.Get(SESSION_KEY_VALID_LIST + uid) ?? new List<Product_Model>();
            count = validListFromCache.Count;
            var index = 1;
            var resData = validListFromCache.Skip(pageIndex).Take(pageSize).Select(p => new { Id = index++, ProductBarcode = p.ProductBarcode }).ToList();
            var data = new GridData<object>(resData, count, request.RequestInfo);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //单个商品移除
        public ActionResult InfoRemove(int infoId)
        {
            var res = _storeCollocationInfoContract.TrueRemove(infoId);
            return Json(res);
        }

        //修改数据

        public ActionResult Update(StoreCollocationDto scdo)
        {
            var id = _storeProductCollocationContract.StoreProductCollocations.Where(o => o.Guid == scdo.Guid).Select(x => x.Id).FirstOrDefault();
            scdo.Id = id;
            var res = _storeProductCollocationContract.Update(scdo);
            return Json(res);
        }

        // 删除 恢复数据

        public ActionResult RemoveAll(string idArry, bool statues)
        {
            List<string> list = idArry.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            OperationResult resul = new OperationResult(OperationResultType.Error);
            foreach (var item in list)
            {
                if (item.Contains("par"))
                {
                    int id = Convert.ToInt32(item.Replace("par", ""));
                    var infoList = _storeCollocationInfoContract.StoreCollocationInfos.Where(x => x.StoreCollocationId == id).Select(x => x.Id).ToList();
                    foreach (var a in infoList)
                    {
                        resul = _storeCollocationInfoContract.Remove(statues, a);
                    }
                    resul = _storeProductCollocationContract.Remove(statues, id);
                }
                else
                {
                    int id = Convert.ToInt32(item.Replace("childStore", ""));
                    resul = _storeCollocationInfoContract.Remove(statues, id);
                }
            }
            return Json(resul);
        }

        public ActionResult Disable(string idArry, bool statues)
        {
            List<string> list = idArry.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            OperationResult resul = new OperationResult(OperationResultType.Error);
            foreach (var item in list)
            {
                if (item.Contains("par"))
                {
                    int id = Convert.ToInt32(item.Replace("par", ""));
                    var infoList = _storeCollocationInfoContract.StoreCollocationInfos.Where(x => x.StoreCollocationId == id).Select(x => x.Id).ToList();
                    foreach (var a in infoList)
                    {
                        resul = _storeCollocationInfoContract.Disable(statues, a);
                    }
                    resul = _storeProductCollocationContract.Disable(statues, id);
                }
                else
                {
                    int id = Convert.ToInt32(item.Replace("childStore", ""));
                    resul = _storeCollocationInfoContract.Disable(statues, id);
                }
            }
            return Json(resul);
        }

        public ActionResult ShowImg(string img)
        {
            ViewBag.img = img;
            return PartialView();
        }

        public ActionResult ViewCollocation(int id)
        {
            var item = _storeProductCollocationContract.StoreProductCollocations.Where(x => x.Id == id).FirstOrDefault();
            var styleArry = GetStoreNameArry(item.Styles);
            var Situation = GetStoreNameArry(item.Situation);
            var Season = GetStoreNameArry(item.Season);
            var Shape = GetStoreNameArry(item.Shape);
            var Effect = GetStoreNameArry(item.Effect);
            var Colour = GetStoreNameArry(item.Colour);
            var Store = GetStoreName(item.StoreId);
            if (!string.IsNullOrEmpty(Store))
            {
                ViewData["Store"] = Store.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            else { ViewData["Store"] = new List<string>(); }
            if (styleArry != null)
            {
                ViewData["Styles"] = GetSelect("风格").Where(x => styleArry.Contains(x.Value)).Select(x => x.Text).ToList();
            }
            else
            {
                ViewData["Styles"] = new List<string>();
            }
            if (Situation != null)
            { ViewData["Situation"] = GetSelect("场合").Where(x => Situation.Contains(x.Value)).Select(x => x.Text).ToList(); }
            else
            {
                ViewData["Situation"] = new List<string>();
            }
            if (Season != null)
            { ViewData["Season"] = GetSelect("季节").Where(x => Season.Contains(x.Value)).Select(x => x.Text).ToList(); }
            else
            {
                ViewData["Season"] = new List<string>();
            }
            if (Shape != null)
            { ViewData["Shape"] = GetSelect("体型").Where(x => Shape.Contains(x.Value)).Select(x => x.Text).ToList(); }
            else
            {
                ViewData["Shape"] = new List<string>();
            }
            if (Effect != null)
            { ViewData["Effect"] = GetSelect("效果").Where(x => Effect.Contains(x.Value)).Select(x => x.Text).ToList(); }
            else
            {
                ViewData["Effect"] = new List<string>();
            }
            if (Colour != null)
            {
                ViewData["Colour"] = GetSelect("颜色").Where(x => Colour.Contains(x.Value)).Select(x => x.Text).ToList(); ;
            }
            else
            {
                ViewData["Colour"] = new List<string>();
            }
            return PartialView(item);
        }

        public ActionResult View(int id)
        {
            var item = _storeCollocationInfoContract.StoreCollocationInfos.Where(x => x.Id == id).FirstOrDefault();
            //on item.ProductOrigNumberId
            var product = _productContract.Products.Where(x => x.Id == item.ProductOrigNumberId).FirstOrDefault();
            ViewBag.ProductNumber = product.ProductNumber;
            ViewBag.ThumbnailPath = product.ProductOriginNumber.ThumbnailPath;
            ViewBag.CategoryName = product.ProductOriginNumber.Category.CategoryName;
            ViewBag.Colour = product.Color.ColorName;
            ViewBag.SeasonName = product.ProductOriginNumber.Season.SeasonName;
            ViewBag.TagPrice = product.ProductOriginNumber.TagPrice;
            ViewBag.CreatedTime = item.CreatedTime;
            ViewBag.UpdatedTime = item.UpdatedTime;
            return PartialView();
        }

        public List<SelectListItem> SelectedItem(List<SelectListItem> listSel, List<string> names)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if (names != null && names.Count > 0)
            {
                foreach (var selItem in listSel)
                {
                    foreach (string item in names)
                    {
                        if (item == selItem.Value)
                        {
                            selItem.Selected = true;
                        }
                    }
                    list.Add(selItem);
                }
            }
            return list;
        }
        public ActionResult UpdateCollocation(int id)
        {
            var enty = _storeProductCollocationContract.StoreProductCollocations.Where(x => x.Id == id).FirstOrDefault();
            List<SelectListItem> list = _storeContract.Stores.Where(x => x.IsDeleted == false && x.IsEnabled == true).Select(x => new SelectListItem
            {
                Text = x.StoreName,
                Value = x.Id.ToString(),
                Disabled = false,
                Selected = false
            }).ToList();
            ViewBag.CollocationName = enty.CollocationName;
            ViewData["Store"] = list;
            ViewBag.StoreId = enty.StoreId == null ? "" : enty.StoreId;

            //ViewData["Styles"] = GetSelect("风格").Where(x => styleArry.Contains(x.Value)).Select(x => x.Text).ToList();
            ViewData["Styles"] = GetSelect("风格");
            ViewBag.StylesId = enty.Styles == null ? "" : enty.Styles;

            ViewData["Situation"] = GetSelect("场合");
            ViewBag.SituationId = enty.Situation == null ? "" : enty.Situation;

            ViewData["Season"] = GetSelect("季节");
            ViewBag.SeasonId = enty.Season == null ? "" : enty.Season;


            ViewData["Shape"] = GetSelect("体型");
            ViewBag.ShapeId = enty.Shape == null ? "" : enty.Shape;

            ViewData["Effect"] = GetSelect("效果");
            ViewBag.EffectId = enty.Effect == null ? "" : enty.Effect;

            ViewData["Colour"] = GetSelect("颜色");
            ViewBag.ColourId = enty.Colour == null ? "" : enty.Colour;

            ViewBag.uid = enty.Guid;
            return PartialView(enty);
        }
        //_storeProductCollocationContract.Insert(new StoreProductCollocation() { CollocationName = "", Guid = guid });

        public ActionResult Thumbnails(int Id)
        {
            var result = new List<object>();
            var entity = _productContract.Products.FirstOrDefault(m => m.Id == Id);
            var item = _storeProductCollocationContract.StoreProductCollocations.Where(x => x.Id == Id).FirstOrDefault();
            if (item != null && !string.IsNullOrEmpty(item.ThumbnailPath))
            {
                var counter = 1;
                var filePath = FileHelper.UrlToPath(item.ThumbnailPath);
                if (System.IO.File.Exists(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    result.Add(new
                    {
                        ID = counter.ToString(),
                        FileName = item.ThumbnailPath,
                        FilePath = item.ThumbnailPath,
                        FileSize = fileInfo.Length
                    });
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSelectStore(string id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public ActionResult GetSelectStoreList()
        {
            int id = 0;
            Int32.TryParse(Request["id"], out id);
            var storeId = _storeProductCollocationContract.StoreProductCollocations.Where(x => x.Id == id).Select(x => x.StoreId).FirstOrDefault();
            string Name = Request["StoreName"];
            Name = Name == null ? "" : Name;
            var requ = new GridRequest(Request);
            var strArry = storeId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var querySource = _storeContract.Stores.Where(x => x.IsDeleted == false && x.IsEnabled == true && strArry.Contains(x.Id.ToString())).Where(x => x.StoreName.Contains(Name));
            int count = querySource.Count();
            var da = querySource.OrderByDescending(x => x.UpdatedTime).Skip(requ.PageCondition.PageIndex)
                  .Take(requ.PageCondition.PageSize)
                      .Select(x => new
                      {
                          x.StoreName,
                          x.StoreType,
                          x.StoreCredit,
                          x.Balance,
                          x.IsAttached
                      });

            GridData<Object> objdata = new GridData<object>(da, count, Request);
            return Json(objdata);
        }

        public string CheckName(string name)
        {
            var count = _storeProductCollocationContract.StoreProductCollocations.Where(x =>
             x.IsDeleted == false && x.IsEnabled == true && x.CollocationName == name).ToList().Count();
            return count.ToString();
        }
    }
}