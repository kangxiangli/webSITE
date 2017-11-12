using System;
using System.IO;
using System.Web;
using Whiskey.Web.Helper;
using Antlr3.ST;
using Antlr3.ST.Language;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Services.Content;
using System.Threading.Tasks;
using System.Collections.Generic;
using Whiskey.ZeroStore.ERP.Website.Areas.Products.Models;
using AutoMapper;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Data;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Commons.Controllers
{
    [License(CheckMode.Verify)]
    public class AppointmentGenController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AppointmentGenController));

        protected readonly IAppointmentGenContract _AppointmentGenContract;
        protected readonly IBrandContract _brandContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IColorContract _colorContract;
        protected readonly ISeasonContract _seasonContract;
        protected readonly IProductCrowdContract _productCrowdContract;
        protected readonly IProductContract _productContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IRetailContract _retailContract;

        public AppointmentGenController(
            IAppointmentGenContract _AppointmentGenContract,
            IBrandContract _brandContract,
            ICategoryContract _categoryContract,
            IColorContract _colorContract,
            ISeasonContract _seasonContract,
            IProductCrowdContract _productCrowdContract,
            IStoreContract _storeContract,
            IMemberContract _memberContract,
            IRetailContract _retailContract,
            IProductContract _productContract
            )
        {
            this._AppointmentGenContract = _AppointmentGenContract;
            this._brandContract = _brandContract;
            this._categoryContract = _categoryContract;
            this._colorContract = _colorContract;
            this._seasonContract = _seasonContract;
            this._productCrowdContract = _productCrowdContract;
            this._productContract = _productContract;
            this._storeContract = _storeContract;
            this._memberContract = _memberContract;
            this._retailContract = _retailContract;
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
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(AppointmentGenDto dto)
        {
            var result = _AppointmentGenContract.Insert(sendPopLoadingAction,dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(AppointmentGenDto dto)
        {
            var result = _AppointmentGenContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _AppointmentGenContract.Edit(Id);
            return PartialView(result);
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _AppointmentGenContract.View(Id);
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<AppointmentGen, bool>> predicate = FilterHelper.GetExpression<AppointmentGen>(request.FilterGroup);
            var count = 0;

            var list = (from s in _AppointmentGenContract.Entities.Where<AppointmentGen, int>(predicate, request.PageCondition, out count)
                        select new
                        {
                            s.Id,
                            s.IsDeleted,
                            s.IsEnabled,
                            s.CreatedTime,
							OperatorName = s.Operator.Member.RealName,
							s.AllCount,
                            s.SuccessCount,

                        }).ToList();
            var data = new GridData<object>(list, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _AppointmentGenContract.DeleteOrRecovery(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _AppointmentGenContract.DeleteOrRecovery(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _AppointmentGenContract.EnableOrDisable(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
		
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _AppointmentGenContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult Export()
        {
			var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            GridRequest request = new GridRequest(Request);
            Expression<Func<AppointmentGen, bool>> predicate = FilterHelper.GetExpression<AppointmentGen>(request.FilterGroup);
			var query = _AppointmentGenContract.Entities.Where(predicate);
			var list = (from s in query
						select new
						{
							s.UpdatedTime,
							OperatorName = s.Operator.Member.RealName,
                            s.AllCount,
                            s.SuccessCount,

                        }).ToList();
			var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return FileExcel(st, "预约数据生成");
        }

        #region 选择商品

        public ActionResult VProduct()
        {
            ViewBag.Color = CacheAccess.GetColorsName(_colorContract, true);
            ViewBag.Brand = CacheAccess.GetBrand(_brandContract, true, false);
            ViewBag.Category = CacheAccess.GetCategory(_categoryContract, true);
            ViewBag.Season = CacheAccess.GetSeason(_seasonContract, true);
            ViewBag.Crowds = CacheAccess.GetProductCrowd(_productCrowdContract, true);

            return PartialView();
        }

        public async Task<ActionResult> ProductList()
        {
            List<object> lis = new List<object>();
            GridRequest request = new GridRequest(Request);
            GridData<object> data = await Task.Run(() =>
            {
                #region 商品款号逻辑

                Expression<Func<Product, bool>> predicate = FilterHelper.GetExpression<Product>(request.FilterGroup);

                List<ProductTree> parens = new List<ProductTree>();
                var query = _productContract.Products.Where(predicate).Where(w => !string.IsNullOrEmpty(w.BigProdNum)).Where(w => w.ProductOriginNumber != null && w.ProductOriginNumber.IsVerified == CheckStatusFlag.通过);

                //其他信息筛选
                int cou = query.GroupBy(c => c.BigProdNum).Count();
                var proli = query.GroupBy(c => c.BigProdNum)
                                 .OrderByDescending(c => c.Max(g => g.CreatedTime))
                                 .Skip(request.PageCondition.PageIndex)
                                 .Take(request.PageCondition.PageSize);

                int rec = 0;
                proli.Each(c =>
                {
                    rec += c.Select(g => g.Id).Count();
                });

                foreach (var item in proli)
                {
                    #region 新版逻辑

                    var modPON = item.Select(f => f.ProductOriginNumber).FirstOrDefault();
                    if (modPON.IsNotNull())
                    {
                        var par = new
                        {
                            Id = "par" + modPON.Id,
                            BigProdNum = modPON.BigProdNum,
                            ParentId = "",
                            BrandName = modPON.Brand.BrandName,
                            CategoryName = modPON.Category.CategoryName,
                            SeasonName = modPON.Season.SeasonName,
                            ProductNumber = "",
                            SizeName = "",
                            ThumbnailPath = modPON.ThumbnailPath,
                            ColorName = "",
                            TagPrice = modPON.TagPrice,
                            AllPrice = modPON.TagPrice * modPON.Products.Count(c => c.IsEnabled && !c.IsDeleted),
                            modPON.IsEnabled,
                            ProductCount = modPON.Products.Count(c => c.IsEnabled && !c.IsDeleted)
                        };
                        lis.Add(par);

                        var childs = item.OrderByDescending(c => c.UpdatedTime).Where(x => !string.IsNullOrEmpty(x.BigProdNum) && !x.BigProdNum.StartsWith("-"))
                                .Select(x => GetDataByProduct(x, par.Id));

                        lis.AddRange(childs);
                    }
                    else
                    {
                        continue;
                    }

                    #endregion
                }

                return new GridData<object>(lis, cou, request.RequestInfo);

                #endregion
            });
            return Json(data);
        }
        /// <summary>
        /// 根据product得到返回的数据
        /// </summary>
        /// <param name="x">product</param>
        /// <param name="parid">父类ID，</param>
        /// <param name="isverif">是否通过审核</param>
        /// <returns></returns>
        private object GetDataByProduct(Product x, string parid)
        {
            var modPON = x.ProductOriginNumber;//原始款号是不应该为NULL的
            return new
            {
                Id = x.Id + "",
                BigProdNum = x.BigProdNum,
                ParentId = parid,
                BrandName = "",//modPON.Brand.BrandName,
                CategoryName = "",//modPON.Category.CategoryName,
                SeasonName = "",//modPON.Season.SeasonName,
                ProductNumber = x.ProductNumber,
                SizeName = x.Size == null ? "" : x.Size.SizeName,
                ThumbnailPath = x.ThumbnailPath, //显示子类的第一张图
                ColorName = x.ColorId == null ? "" : _colorContract.Colors.FirstOrDefault(m => m.Id == x.ColorId).ColorName,
                ColorImg = x.ColorId == null ? "" : _colorContract.Colors.Where(m => m.Id == x.ColorId).FirstOrDefault().IconPath,
                TagPrice = modPON.TagPrice,
                IsEnabled = x.IsEnabled,
                ProductCount = "",
            };
        }


        #endregion

        #region 选择会员

        public ActionResult VMember()
        {
            return PartialView();
        }

        public ActionResult MemberList(DateTime? RetailStart, DateTime? RetailEnd)
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Member, bool>> predicate = FilterHelper.GetExpression<Member>(request.FilterGroup);
            var count = 0;
            RetailEnd = RetailEnd.HasValue ? RetailEnd.Value.AddDays(1) : RetailEnd;
            var hasretail = RetailStart.HasValue && RetailEnd.HasValue;

            var qr = _retailContract.Retails.Where(w => w.IsEnabled && !w.IsDeleted);
            if (hasretail)
            {
                qr = qr.Where(w => w.CreatedTime >= RetailStart && w.CreatedTime < RetailEnd);
            }

            var query = from s in _memberContract.Members.Where(w => w.StoreId.HasValue)
                        let r = qr.Where(w => w.ConsumerId == s.Id)
                        where hasretail ? r.Any() : true
                        select s;

            var list = (from s in query.Where<Member, int>(predicate, request.PageCondition, out count)
                        let r = qr.Where(w => w.ConsumerId == s.Id)
                        select new
                        {
                            s.Id,
                            s.MemberName,
                            s.MobilePhone,
                            s.UserPhoto,
                            Gender = s.Gender == 0 ? "女" : "男",
                            s.MemberType.MemberTypeName,
                            s.RealName,
                            s.CreatedTime,
                            hasRetail = r.Any(),

                        }).ToList();

            var data = new GridData<object>(list, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 批量导入

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
                    var list = reda.Select(s => new { RealName = s[0], MobilePhone = s[1], ProductNumber = s[2], AppointmentTime = s[3] }).ToList();

                    list = list.Where(w =>
                    {
                        var isnotempty = w.MobilePhone.IsNotNullAndEmpty() && w.ProductNumber.IsNotNullAndEmpty() && w.AppointmentTime.IsNotNullAndEmpty();
                        if (isnotempty)
                        {
                            DateTime _time;
                            return DateTime.TryParse(w.AppointmentTime, out _time);
                        }
                        return isnotempty;
                    }).ToList();

                    res = new OperationResult(OperationResultType.Success, string.Empty, list);
                }
            }
            return Json(res);
        }

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
                    da = excel.ExcelToArray(fileName);
                }
                return da;
            }
            return null;
        }
        #endregion

        #endregion

        [Log]
        [HttpPost]
        public ActionResult BatchImport(params AppointmentGenBatchDto[] dtos)
        {
            var result = _AppointmentGenContract.BatchImpot(sendPopLoadingAction, dtos);
            return Json(result);
        }

        #endregion
    }
}

