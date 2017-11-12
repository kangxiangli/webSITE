using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using AutoMapper;
using Antlr3.ST;
using Antlr3.ST.Language;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Models;
using System.Web.Script.Serialization;
using Whiskey.ZeroStore.ERP.Website.Areas.Products.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;
using ThoughtWorks.QRCode.Codec;
using System.Drawing;
using XKMath36;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.Utility;
using System.Threading;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Factory.Controllers
{
    [License(CheckMode.Verify)]
    public partial class SampleController : BaseController
    {
        #region 声明业务层操作对象

        protected static readonly ILogger _log = LogManager.GetLogger(typeof(SampleController));
        public static object objlock = new object();
        protected readonly IProductContract _productContract;
        protected readonly IBrandContract _brandContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IColorContract _colorContract;
        protected readonly ISeasonContract _seasonContract;
        protected readonly ISizeContract _sizeContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IProductDiscountContract _productDiscountContract;
        protected readonly IModuleContract _moduleContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IPermissionContract _permissionContract;
        protected readonly ITemplateContract _templateContract;
        protected readonly IProductOrigNumberContract _productOrigNumberContract;
        protected readonly IProductBigNumberAttrContract _productNumberAttrContract;
        protected readonly IProductAttributeContract _productAttributeContract;
        protected readonly ISalesCampaignContract _salesCampaignContract;
        protected readonly IMemberCollocationContract _memberCollocationContract;
        protected readonly IMemberColloEleContract _MemberColloEleContract;
        protected readonly IProductBuysaidAttributeContract _productBuysaidAttributeContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IMaintainContract _maintainContract;
        protected readonly IProductBarcodeDetailContract _productBarcodeDetailContract;
        protected readonly IBarCodeConfigContract _barCodeConfigContract;
        protected readonly IRetailItemContract _retailItemContract;
        protected readonly IOrderblankItemContract _orderblankItemContract;
        protected readonly IReturnedItemContract _returnedItemContract;
        protected readonly IProductTrackContract _productTrackContract;
        protected readonly IProductCrowdContract _productCrowdContract;
        protected readonly IProdcutOrigNumberProductDetailContract _ProductOrigNumberProductDetailContract;
        protected readonly IDesignerContract _DesignerContract;
        protected readonly IJobPositionContract _JobPositionContract;
        protected readonly IConfigureContract _configureContract;
        public SampleController(
             IProductContract productContract,
             IBrandContract brandContract,
             ICategoryContract categoryContract,
             IColorContract colorContract,
             ISeasonContract seasonContract,
             ISizeContract sizeContract,
             IStoreContract storeContract,
             IProductCrowdContract productCrowdContract,
             IProductDiscountContract productDiscountContract,
             IModuleContract moduleContract,
             IAdministratorContract administratorContract,
             IPermissionContract permissionContract,
             ITemplateContract templateContract,
             IProductOrigNumberContract productOrigNumberContract,
             IProductBigNumberAttrContract productNumberAttrContract,
             IProductAttributeContract productAttributeContract,
             ISalesCampaignContract salesCampaignContract,
             IProdcutOrigNumberProductDetailContract ProductOrigNumberProductDetailContract,
             IMemberCollocationContract memberCollocationContract,
             IMemberColloEleContract MemberColloEleContract,
             IProductBuysaidAttributeContract productBuysaidAttributeContract,
             IProductBigNumberAttrContract productBigNumberAttrContract,
             IInventoryContract inventoryContract,
             IMaintainContract maintainContract,
             IProductBarcodeDetailContract productBarcodeDetailContract,
             IBarCodeConfigContract barCodeConfigContract,
             IRetailItemContract retailItemContract,
             IOrderblankItemContract orderblankItemContract,
             IReturnedItemContract returnedItemContract,
             IDesignerContract DesignerContract,
             IJobPositionContract JobPositionContract,
             IProductTrackContract productTrackContract,
             IConfigureContract configureContract
             )
        {
            _productContract = productContract;
            _brandContract = brandContract;
            _categoryContract = categoryContract;
            _colorContract = colorContract;
            _seasonContract = seasonContract;
            _sizeContract = sizeContract;
            _storeContract = storeContract;
            _productDiscountContract = productDiscountContract;

            _moduleContract = moduleContract;
            _administratorContract = administratorContract;
            _permissionContract = permissionContract;
            _templateContract = templateContract;
            _productOrigNumberContract = productOrigNumberContract;
            _productNumberAttrContract = productNumberAttrContract;
            _productAttributeContract = productAttributeContract;
            _salesCampaignContract = salesCampaignContract;


            _memberCollocationContract = memberCollocationContract;
            _MemberColloEleContract = MemberColloEleContract;

            _productBuysaidAttributeContract = productBuysaidAttributeContract;
            _inventoryContract = inventoryContract;
            _maintainContract = maintainContract;
            _productBarcodeDetailContract = productBarcodeDetailContract;
            _barCodeConfigContract = barCodeConfigContract;
            _retailItemContract = retailItemContract;
            _orderblankItemContract = orderblankItemContract;
            _returnedItemContract = returnedItemContract;
            _productTrackContract = productTrackContract;
            _productCrowdContract = productCrowdContract;
            _ProductOrigNumberProductDetailContract = ProductOrigNumberProductDetailContract;
            _DesignerContract = DesignerContract;
            _JobPositionContract = JobPositionContract;
            _configureContract = configureContract;
        }

        #endregion

        #region 异步通知
        /// <summary>
        /// 发送待审核通知
        /// </summary>
        private void SendShouldVerifyNotification(List<int> PONIds)
        {
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                var permissionId = 7396;//样品发布审核权限Id

                var receiveAdminIds = PermissionHelper.HasPermissionAllAdmin(permissionId, EntityContract._adminContract, EntityContract._permissionContract);
                if (receiveAdminIds.IsNotNullOrEmptyThis())
                {
                    var modTN = EntityContract._templateNotificationContract.templateNotifications.FirstOrDefault(f => f.NotifciationType == TemplateNotificationType.ProductVerify);
                    if (modTN.IsNotNull())
                    {
                        var modTemp = modTN.Templates.FirstOrDefault(f => f.IsDefault && !f.IsDeleted && f.IsEnabled);
                        if (modTemp.IsNotNull() && modTemp.TemplateHtml.IsNotNullAndEmpty())
                        {
                            var listPon = EntityContract._productOriginNumberContract.OrigNumbs.Where(w => PONIds.Contains(w.Id)).ToList();
                            if (listPon.IsNotNullOrEmptyThis())
                            {
                                foreach (var item in listPon)
                                {
                                    var title = modTemp.TemplateName;
                                    var dic = new Dictionary<string, object>();
                                    dic.Add("DesignerName", item.Designer?.Admin?.Member?.RealName ?? string.Empty);
                                    dic.Add("ToFactory", item.Designer?.Factory?.FactoryName ?? string.Empty);
                                    dic.Add("ToBrand", item.Designer?.Factory?.Brand?.BrandName ?? string.Empty);
                                    dic.Add("BigProdNum", item.BigProdNum); dic.Add("OriginNumber", item.OriginNumber);
                                    dic.Add("ProductName", item.ProductName ?? string.Empty);

                                    var content = NVelocityHelper.Generate(modTemp.TemplateHtml, dic, "_template_productverify_");

                                    var result = EntityContract._notificationContract.Insert(sendNotificationAction, new NotificationDto()
                                    {
                                        Title = title,
                                        AdministratorIds = receiveAdminIds,
                                        Description = content,
                                        IsEnableApp = true,
                                        NoticeTargetType = (int)NoticeTargetFlag.Admin,
                                        NoticeType = (int)NoticeFlag.Immediate
                                    });
                                }
                            }
                        }
                    }
                }
            });
        }

        #endregion

        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            var modDesi = _DesignerContract.SelectDesigner.FirstOrDefault(f => f.AdminId == AuthorityHelper.OperatorId.Value && f.IsEnabled && !f.IsDeleted);
            if (modDesi.IsNull())
            {
                var vr = new ViewResult();
                vr.ViewName = "NotDesigner";
                return vr;
            }

            ViewBag.Category = CacheAccess.GetCategory(_categoryContract, true);
            ViewBag.Color =
                new JavaScriptSerializer().Serialize(
                    CacheAccess.GetColors(_colorContract, false).Select(c => new { c.Text, c.Value }).ToList());
            ViewBag.Color = (_colorContract.SelectList("选择颜色").Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
            ViewBag.Brand = CacheAccess.GetBrand(_brandContract, true, false);
            ViewBag.Season = CacheAccess.GetSeason(_seasonContract, true);
            ViewBag.Size = CacheAccess.GetSize(_sizeContract, _categoryContract, true);
            ViewBag.Discount = StaticHelper.DiscountList("选择折扣");
            ViewBag.OneCollo = CacheAccess.GetOneCollo(_productAttributeContract, false);
            ViewBag.Crowds = CacheAccess.GetProductCrowd(_productCrowdContract, true);

            var modbcc = _barCodeConfigContract.BarCodeConfigs.FirstOrDefault(f => !f.IsDeleted && f.IsEnabled);
            var prinProj = (int)(modbcc.IsNotNull() ? modbcc.PrinterPaperType : ERP.Models.Enums.PrinterPaperType._30_80);
            ViewBag.prinProj = prinProj;
            ViewBag.paperDirection = (int)(modbcc.IsNotNull() ? modbcc.PrinterPaperDirection : ERP.Models.Enums.PrinterPaperDirection._横版);

            return View();
        }

        [Layout]
        public ActionResult NotDesigner()
        {
            return View();
        }

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        [Log]
        [HttpGet]
        public ActionResult Create(string orignNum)
        {
            OperationResult res = new OperationResult(OperationResultType.Error, "");
            ViewBag.orignNum = orignNum;

            ProductOriginNumber dto = new ProductOriginNumber();

            #region MyRegion

            //在原始编号表中是否可以查到对应的数据，如果可以查到记录原始款号已存在
            var orignu = _productOrigNumberContract.OrigNumbs.Where(c => c.OriginNumber == orignNum).FirstOrDefault();
            if (orignu != null)
            {
                #region 原始款号已存在
                //var pronums = orignu.Products.Select(s => s.ProductNumber).ToList();
                //ViewBag.isUsed = IsUsedBarCode(pronums) ? 1 : 0;

                //ViewBag.Brand =
                //    CacheAccess.GetBrand(_brandContract).Where(c => c.Value == orignu.BrandId.ToString()).ToList();

                //ViewBag.Category =
                //    CacheAccess.GetCategory(_categoryContract, false)
                //        .Where(c => c.Value == orignu.CategoryId.ToString())
                //        .ToList();

                //ViewBag.Season =
                //    CacheAccess.GetSeason(_seasonContract, false)
                //        .Where(c => c.Value == orignu.SeasonId.ToString())
                //        .ToList();

                //dto = orignu;
                #endregion

                res.Message = "原始款号已存在";
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var modDesi = _DesignerContract.SelectDesigner.FirstOrDefault(f => f.AdminId == AuthorityHelper.OperatorId.Value && f.IsEnabled && !f.IsDeleted);
                var bid = modDesi.Factory.BrandId;
                ViewBag.Brand = _brandContract.Brands.Where(w => w.Id == bid && w.IsEnabled && !w.IsDeleted).Select(s => new SelectListItem() { Text = s.BrandName, Value = s.Id + "" }).ToList();
                ViewBag.Category = CacheAccess.GetCategory(_categoryContract, false);
                ViewBag.Season = CacheAccess.GetSeason(_seasonContract, false);
            }

            #endregion

            List<SelectListItem> cate = ViewBag.Category as List<SelectListItem>;
            if (cate != null && cate.Count > 0)
            {
                ViewBag.Size = CacheAccess.GetSize(_sizeContract, cate.FirstOrDefault(c => c.Value != "").Value);
            }
            else
                ViewBag.Size = new List<SelectListItem>();

            ViewBag.Templates = CacheAccess.GetTemplates(_templateContract, true, TemplateTypeFlag.PC);
            ViewBag.TemplatesPhone = CacheAccess.GetTemplates(_templateContract, true, TemplateTypeFlag.手机);
            ViewBag.Color =
                new JavaScriptSerializer().Serialize(
                    CacheAccess.GetColors(_colorContract, false).Select(c => new { c.Text, c.Value }).ToList());
            ViewBag.CollociColor = CacheAccess.GetColors(_colorContract);
            ViewBag.Styles = CacheAccess.GetCollocationStyle(_productAttributeContract);
            ViewBag.Situation = CacheAccess.GetCollocationSituation(_productAttributeContract);
            ViewBag.Crowds = CacheAccess.GetProductCrowd(_productCrowdContract, false);

            return PartialView(dto);
        }
        /// <summary>
        /// 新增商品
        /// </summary>
        /// <param name="prod"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(CProductInfo prod)
        {
            OperationResult res = new OperationResult(OperationResultType.Error, "");

            var modDesi = _DesignerContract.SelectDesigner.FirstOrDefault(f => f.AdminId == AuthorityHelper.OperatorId.Value && f.IsEnabled && !f.IsDeleted);
            if (modDesi.IsNull())
            {
                res.Message = "你还不是设计师";
            }
            else
            {
                List<Product> dtoli;
                ProductOriginNumber orignum;

                var addrs = AddProducts(prod);
                dtoli = addrs.Item1.ToList();
                orignum = addrs.Item2;
                res = addrs.Item3;

                if (res.ResultType == OperationResultType.Success)
                {
                    var exirog = _productOrigNumberContract.OrigNumbs.Where(c => c.OriginNumber == orignum.OriginNumber || c.BigProdNum == orignum.BigProdNum).FirstOrDefault();
                    if (exirog == null)
                    {
                        try
                        {
                            orignum.DesignerId = modDesi.Id;
                            res = _productOrigNumberContract.Insert(orignum);
                        }
                        catch (Exception ex)
                        {
                            res.Message = ex.Message;
                            res.ResultType = OperationResultType.Error;
                        }
                    }
                    else
                    {
                        res.Message = "原始款号已存在";
                        res.ResultType = OperationResultType.Error;
                    }
                    if (res.ResultType == OperationResultType.Success)
                    {
                        #region 生成静态页模板

                        var htmlRes = this.CreateHtml(orignum);
                        if (htmlRes.ResultType != OperationResultType.Success)
                        {
                            res = htmlRes;
                        }

                        #endregion
                    }
                }
            }

            return Json(res);
        }

        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult View(string Id)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            if (string.IsNullOrEmpty(Id))
            {
                return Json(resul);
            }
            Regex reg = new Regex(@"par(\d+)");
            Match mat = reg.Match(Id);
            if (mat != null && mat.Groups.Count > 0)
            {
                int _id = Convert.ToInt32(mat.Groups[1].Value);

                var result = _productOrigNumberContract.View(_id);

                var parents =
                    result.ProductAttributes.Where(c => c.ParentId != null).Select(c => c.Parent).ToList().DistinctBy(c => c.Id);
                var attrids = result.ProductAttributes.Select(c => c.Id).ToList();
                List<object> li = new List<object>();
                foreach (var par in parents)
                {
                    var childs = _productAttributeContract.ProductAttributes.Where(c => c.ParentId == par.Id && attrids.Contains(c.Id)).Select(c => new
                    {
                        name = c.AttributeName,
                        img = c.IconPath,

                    }).ToList();
                    li.Add(new
                    {
                        name = par.AttributeName,
                        img = par.IconPath,
                        child = childs
                    });
                }
                ViewBag.Attribu = new JavaScriptSerializer().Serialize(li);
                if (result.BuysaidAttributes != null)
                {
                    List<int> buysaidIds =
                        result.BuysaidAttributes
                            .Select(c => c.Id)
                            .ToList();
                    if (buysaidIds.Any())
                    {
                        var attnames =
                            _productBuysaidAttributeContract.BuysaidAttributes.Where(c => buysaidIds.Contains(c.Id))
                                .Select(c => "<li>" + c.AttriName + ":" + c.Descri + "</li>")
                                .ToList();

                        ViewBag.BuysaidAttriName = string.Join("", attnames);
                    }
                }
                else
                {
                    ViewBag.BuysaidAttriName = "";
                }

                string strUrl = Request.Url.Authority;

                ViewBag.Url = "Http://" + strUrl;
                //在查看时更新商品档案的审核人
                //if (result.IsVerified && result.VerifiedMembId == null)
                //{
                //    result.VerifiedMembId = result.OperatorId;
                //    result.VerifiedMemb = _administratorContract.Administrators.FirstOrDefault(c => c.Id == result.VerifiedMembId);
                //    _productOrigNumberContract.Update(result);
                //}

                #region 商品款号下的所有产品颜色

                List<SelectListItem> _curColorProd = result.Products.Where(s => s.IsEnabled && !s.IsDeleted).DistinctBy(d => d.ColorId).Select(s => new SelectListItem
                {
                    Text = s.Color.ColorName,
                    Value = s.BigProdNum + "_" + s.ColorId
                }).ToList();
                _curColorProd.Insert(0, new SelectListItem()
                {
                    Text = "共用",
                    Value = result.BigProdNum,
                    Selected = true
                });
                ViewBag.CurColorType = _curColorProd;

                #endregion

                return PartialView(result);
            }
            return PartialView();
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List(int? hasImg)
        {
            var listDesiIds = _DesignerContract.SelectDesigner.Where(w => w.IsEnabled && !w.IsDeleted && w.AdminId == AuthorityHelper.OperatorId.Value).Select(s => s.Id).ToList();//设计师自己的Id

            List<object> lis = new List<object>();
            GridRequest request = new GridRequest(Request);
            var isdelete = request.FilterGroup.Rules.Where(s => s.Field.Equals("IsDeleted")).Select(s => s.Value).FirstOrDefault();
            var boolIsDel = isdelete.IsNotNull() ? Convert.ToBoolean(isdelete) : false;
            GridData<object> data = await Task.Run(() =>
            {
                #region 商品款号逻辑

                Expression<Func<Product, bool>> predicate = FilterHelper.GetExpression<Product>(request.FilterGroup);

                List<ProductTree> parens = new List<ProductTree>();
                var query = _productContract.Products.Where(predicate).Where(w => !string.IsNullOrEmpty(w.BigProdNum)).Where(w => w.ProductOriginNumber != null)
                            .Where(w => w.ProductOriginNumber.DesignerId != null && listDesiIds.Contains(w.ProductOriginNumber.DesignerId.Value));
                //是否有图片筛选
                if (hasImg.HasValue)
                {
                    if (hasImg.Value == 1)
                    {
                        query = query.Where(p => !string.IsNullOrEmpty(p.ThumbnailPath));
                    }
                    else
                    {
                        query = query.Where(p => string.IsNullOrEmpty(p.ThumbnailPath));
                    }
                }

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
                            WholesalePrice = modPON.WholesalePrice,
                            PurchasePrice = modPON.PurchasePrice,
                            modPON.IsEnabled,
                            modPON.IsDeleted,
                            //ChilCou = modPON.Products.Count(c => c.IsEnabled && c.IsDeleted == boolIsDel),
                            ChilCou = item.Count(),
                            IsVerified = modPON.IsVerified,
                            JumpLink = modPON.JumpLink,
                            HtmlPath = modPON.HtmlPath,
                            HtmlPhonePath = modPON.HtmlPhonePath,
                            IsRecommend = modPON.IsRecommend
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
                Id = x.ProductNumber == null ? "org" + x.Id : x.Id.ToString(),
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
                TagPrice = "",// modPON.TagPrice,
                WholesalePrice = "",//  modPON.WholesalePrice,
                PurchasePrice = "",//  modPON.PurchasePrice,
                IsEnabled = x.IsEnabled,
                x.IsDeleted,
                HtmlPath = "",// modPON.HtmlPath,
                HtmlPhonePath = "",// modPON.HtmlPhonePath,
                JumpLink = "",//  modPON.JumpLink,
                ChilCou = "",
                IsVerified = modPON.IsVerified,
                IsRecommend = modPON.IsRecommend
            };
        }
        /// <summary>
        /// 获取商品的图片
        /// </summary>
        /// <param name="ProductNumber">商品货号,isbignum=true时为 商品款号</param>
        /// <returns></returns>
        public ActionResult GetCurProImg(string ProductNumber, int? ColorId, bool isbignum = false)
        {
            OperationResult res = new OperationResult(OperationResultType.QueryNull, "");
            List<object> detailImgs = new List<object>();
            string ProductCollocationImg = string.Empty;//搭配图
            string OriginalPath = string.Empty;//主图
            List<ProductImage> ProductImages = new List<ProductImage>();
            bool hasMod = false;
            if (isbignum)
            {
                #region 原始款号表
                var modOrg = _productOrigNumberContract.OrigNumbs.Where(w => w.BigProdNum == ProductNumber).FirstOrDefault();
                if (modOrg.IsNotNull())
                {
                    hasMod = true;
                    ProductCollocationImg = modOrg.ProductCollocationImg;
                    OriginalPath = modOrg.OriginalPath;
                    ProductImages = modOrg.ProductImages.ToList();
                }
                #endregion
            }
            else
            {
                #region 商品表
                var query = _productContract.Products.Where(w => w.BigProdNum == ProductNumber);
                if (ColorId.HasValue)
                {
                    query = query.Where(w => w.ColorId == ColorId);
                }
                var modPro = query.OrderByDescending(o => o.OriginalPath).FirstOrDefault();
                if (modPro.IsNotNull())
                {
                    hasMod = true;
                    ProductCollocationImg = modPro.ProductCollocationImg;
                    OriginalPath = modPro.OriginalPath;
                    ProductImages = modPro.ProductImages.ToList();
                }
                #endregion
            }
            if (hasMod)
            {
                res = new OperationResult(OperationResultType.Success);
                if (ProductImages.Any())
                {
                    foreach (var item in ProductImages)
                    {
                        var filePath = FileHelper.UrlToPath(item.OriginalPath);
                        if (System.IO.File.Exists(filePath))
                        {
                            FileInfo fileInfo = new FileInfo(filePath);
                            detailImgs.Add(new
                            {
                                OriginalPath = item.OriginalPath,
                                FileName = fileInfo.Name,
                                FileSize = fileInfo.Length
                            });
                        }
                    }
                }
                res.Data = new
                {
                    ProductCollocationImg = ProductCollocationImg,
                    OriginalPath = OriginalPath,
                    ProductImages = detailImgs
                };
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 保存商品图片
        /// </summary>
        /// <param name="ProductNumber"></param>
        /// <param name="isbignum"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveCurProImg(string ProductNumber, bool isbignum, string ProductCollocationImg, string OriginalPath, List<ProductImage> ProductImages, int? ColorId)
        {
            OperationResult res = new OperationResult(OperationResultType.QueryNull, "");
            List<object> detailImgs = new List<object>();
            List<string> delImgs = new List<string>();//待删除的图片

            if (ProductImages == null)
            {
                ProductImages = new List<ProductImage>();
            }
            ProductImages = ProductImages.Where(w => !string.IsNullOrEmpty(w.OriginalPath)).ToList();//去除空数据
            if (isbignum)
            {
                #region 原始款号表
                var modOrg = _productOrigNumberContract.OrigNumbs.Where(w => w.BigProdNum == ProductNumber).FirstOrDefault();
                if (modOrg.IsNotNull())
                {
                    #region 主图修改了
                    if (modOrg.OriginalPath != OriginalPath)
                    {
                        delImgs.Add(modOrg.OriginalPath);
                        delImgs.Add(modOrg.ThumbnailPath);
                        if (OriginalPath.IsNotNullAndEmpty())
                        {
                            modOrg.OriginalPath = DecodeStr(OriginalPath);
                            modOrg.ThumbnailPath = GenerateThumbnai(OriginalPath);
                        }
                        else
                        {
                            modOrg.OriginalPath = null;
                            modOrg.ThumbnailPath = null;
                        }
                    }
                    #endregion

                    #region 搭配图修改了
                    if (modOrg.ProductCollocationImg != ProductCollocationImg)
                    {
                        delImgs.Add(modOrg.ProductCollocationImg);
                        modOrg.ProductCollocationImg = DecodeStr(ProductCollocationImg);
                    }
                    #endregion

                    var orgImgs = modOrg.ProductImages.Select(s => s.OriginalPath);
                    var curImgs = ProductImages.Select(s => s.OriginalPath);
                    var shouldAddImgs = ProductImages.Where(w => !orgImgs.Contains(w.OriginalPath)).ToList();
                    var shouldDelImgs = modOrg.ProductImages.Where(w => !curImgs.Contains(w.OriginalPath)).ToList();

                    #region 需要添加
                    GenerateThumbnai(shouldAddImgs);
                    foreach (var item in shouldAddImgs)
                    {
                        modOrg.ProductImages.Add(item);
                    }
                    #endregion

                    #region 需要删除

                    for (int i = shouldDelImgs.Count - 1; i >= 0; i--)
                    {
                        var curdel = shouldDelImgs[i];
                        if (modOrg.ProductImages.Remove(curdel))
                        {
                            delImgs.Add(curdel.OriginalPath);
                            delImgs.Add(curdel.ThumbnailLargePath);
                            delImgs.Add(curdel.ThumbnailMediumPath);
                            delImgs.Add(curdel.ThumbnailSmallPath);
                        }
                    }

                    #endregion

                    res = _productOrigNumberContract.Update(modOrg);
                }
                #endregion
            }
            else
            {
                #region 商品表
                var query = _productContract.Products.Where(w => w.BigProdNum == ProductNumber);
                if (ColorId.HasValue)
                {
                    query = query.Where(w => w.ColorId == ColorId);
                }
                var listPro = query.ToList();
                if (listPro.IsNotNull())
                {
                    foreach (var modPro in listPro)
                    {
                        #region 主图修改了
                        if (modPro.OriginalPath != OriginalPath)
                        {
                            delImgs.Add(modPro.OriginalPath);
                            delImgs.Add(modPro.ThumbnailPath);
                            if (OriginalPath.IsNotNullAndEmpty())
                            {
                                modPro.OriginalPath = DecodeStr(OriginalPath);
                                modPro.ThumbnailPath = GenerateThumbnai(OriginalPath);
                            }
                            else
                            {
                                modPro.OriginalPath = null;
                                modPro.ThumbnailPath = null;
                            }
                        }
                        #endregion

                        #region 搭配图修改了
                        if (modPro.ProductCollocationImg != ProductCollocationImg)
                        {
                            delImgs.Add(modPro.ProductCollocationImg);
                            modPro.ProductCollocationImg = DecodeStr(ProductCollocationImg);
                        }
                        #endregion

                        var orgImgs = modPro.ProductImages.Select(s => s.OriginalPath);
                        var curImgs = ProductImages.Select(s => s.OriginalPath);
                        var shouldAddImgs = ProductImages.Where(w => !orgImgs.Contains(w.OriginalPath)).ToList();
                        var shouldDelImgs = modPro.ProductImages.Where(w => !curImgs.Contains(w.OriginalPath)).ToList();

                        #region 需要添加
                        GenerateThumbnai(shouldAddImgs);
                        foreach (var item in shouldAddImgs)
                        {
                            modPro.ProductImages.Add(item);
                        }
                        #endregion

                        #region 需要删除

                        for (int i = shouldDelImgs.Count - 1; i >= 0; i--)
                        {
                            var curdel = shouldDelImgs[i];
                            if (modPro.ProductImages.Remove(curdel))
                            {
                                delImgs.Add(curdel.OriginalPath);
                                delImgs.Add(curdel.ThumbnailLargePath);
                                delImgs.Add(curdel.ThumbnailMediumPath);
                                delImgs.Add(curdel.ThumbnailSmallPath);
                            }
                        }

                        #endregion

                        Product[] prod = new Product[] { modPro };

                    }
                    res = _productContract.Update(listPro.ToArray(), false, "商品图片修改");
                }
                else
                {
                    res.Message = "颜色商品不存在,图片修改失败";
                }

                #endregion
            }

            if (res.ResultType == OperationResultType.Success)
            {
                delImgs = delImgs.Where(w => !string.IsNullOrEmpty(w)).ToList();//移除空数据
                foreach (var item in delImgs)
                {
                    FileHelper.Delete(item);
                }
            }
            return Json(res);
        }

        /// <summary>
        /// 修改商品根据，款号或者原始货号或原始款号
        /// </summary>
        /// <param name="bitNumOrOrigNum">商品款号或者原始款号</param>
        /// <param name="numtype">0原始款号,1商品款号,2商品货号</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult BitNumberUpdate(string bitNumOrOrigNum, int numtype)
        {
            ViewBag.orignNum = bitNumOrOrigNum;
            ViewBag.isUsed = 0;//已经使用过吊牌,价格不允许修改,0未使用 1使用

            ProductOriginNumber dto = new ProductOriginNumber();
            #region MyRegion
            var query = _productOrigNumberContract.OrigNumbs;
            if (numtype == 1)
            {
                query = query.Where(w => w.BigProdNum == bitNumOrOrigNum);
            }
            else if (numtype == 0)
            {
                query = query.Where(w => w.OriginNumber == bitNumOrOrigNum);
            }
            else if (numtype == 2)
            {
                query = query.Where(w => w.Products.Count(s => s.ProductNumber == bitNumOrOrigNum) > 0);
            }
            //在原始编号表中是否可以查到对应的数据，如果可以查到记录，则以原始表中的数据为准
            var orignu = query.FirstOrDefault();
            if (orignu != null)
            {
                var pronums = orignu.Products.Select(s => s.ProductNumber).ToList();
                ViewBag.isUsed = IsUsedBarCode(pronums) ? 1 : 0;

                ViewBag.Brand =
                    CacheAccess.GetBrand(_brandContract).Where(c => c.Value == orignu.BrandId.ToString()).ToList();

                ViewBag.Category =
                    CacheAccess.GetCategory(_categoryContract, false)
                        .Where(c => c.Value == orignu.CategoryId.ToString())
                        .ToList();

                ViewBag.Season =
                    CacheAccess.GetSeason(_seasonContract, false)
                        //.Where(c => c.Value == orignu.SeasonId.ToString())
                        .ToList();

                dto = orignu;
            }
            else
            {
                return Json(numtype == 0 ? "原始款号" : numtype == 1 ? "商品款号" : numtype == 2 ? "商品货号" : "数据丢失", JsonRequestBehavior.AllowGet);
                //ViewBag.Brand = CacheAccess.GetBrand(_brandContract, false, false);
                //ViewBag.Category = CacheAccess.GetCategory(_categoryContract, false);
                //ViewBag.Season = CacheAccess.GetSeason(_seasonContract, false);
            }

            #endregion

            List<SelectListItem> cate = ViewBag.Category as List<SelectListItem>;
            if (cate != null && cate.Count > 0)
            {
                ViewBag.Size = CacheAccess.GetSize(_sizeContract, cate.FirstOrDefault(c => c.Value != "").Value);
            }
            else
                ViewBag.Size = new List<SelectListItem>();

            ViewBag.Templates = CacheAccess.GetTemplates(_templateContract, true, TemplateTypeFlag.PC);
            ViewBag.TemplatesPhone = CacheAccess.GetTemplates(_templateContract, true, TemplateTypeFlag.手机);
            ViewBag.Color =
                new JavaScriptSerializer().Serialize(
                    CacheAccess.GetColors(_colorContract, false).Select(c => new { c.Text, c.Value }).ToList());
            ViewBag.CollociColor = CacheAccess.GetColors(_colorContract);
            ViewBag.Styles = CacheAccess.GetCollocationStyle(_productAttributeContract);
            ViewBag.Situation = CacheAccess.GetCollocationSituation(_productAttributeContract);
            ViewBag.Crowds = CacheAccess.GetProductCrowd(_productCrowdContract, false);

            #region 商品款号下的所有产品颜色

            List<SelectListItem> _curColorProd = dto.Products.Where(s => s.IsEnabled && !s.IsDeleted).DistinctBy(d => d.ColorId).Select(s => new SelectListItem
            {
                Text = s.Color.ColorName,
                Value = s.BigProdNum + "_" + s.ColorId
            }).ToList();
            _curColorProd.Insert(0, new SelectListItem()
            {
                Text = "共用",
                Value = dto.BigProdNum,
                Selected = true
            });
            ViewBag.CurColorType = _curColorProd;

            #endregion

            return PartialView(dto);
        }
        /// <summary>
        /// 修改大款号或者原始货号
        /// </summary>
        /// <param name="prod"></param>
        /// <returns></returns>
        [HttpPost]
        [Log]
        public ActionResult BitNumberUpdate(CProductInfo prod)
        {
            OperationResult res = new OperationResult(OperationResultType.Error, "");
            var modDesi = _DesignerContract.SelectDesigner.FirstOrDefault(f => f.AdminId == AuthorityHelper.OperatorId.Value && f.IsEnabled && !f.IsDeleted);
            if (modDesi.IsNull())
            {
                res.Message = "你还不是设计师";
            }
            else
            {
                List<Product> dtoli;
                ProductOriginNumber orignum;
                List<string> delImgs = new List<string>();

                #region 修改之前的校验
                if (prod.TemplateId == -1 || prod.TemplateId == 0)
                {
                    res.Message = "模板不能为空，请在“商城属性”下选择模板";
                    return Json(res, JsonRequestBehavior.AllowGet);
                }
                #endregion

                var addrs = UpdateProducts(prod);
                dtoli = addrs.Item1.ToList();
                orignum = addrs.Item2;
                res = addrs.Item3;
                delImgs = addrs.Item4;

                if (res.ResultType == OperationResultType.Success)
                {
                    orignum.IsVerified = CheckStatusFlag.待审核;//样品修改后需要重新审核
                    res = _productOrigNumberContract.Update(orignum);
                    if (res.ResultType == OperationResultType.Success)
                    {
                        #region 生成静态页模板

                        var htmlRes = this.CreateHtml(orignum);
                        if (htmlRes.ResultType != OperationResultType.Success)
                        {
                            res = htmlRes;
                        }
                        else
                        {
                            var orgPON = _productOrigNumberContract.View(orignum.Id);
                            var orghp = orgPON.HtmlPath;
                            var orghpp = orgPON.HtmlPhonePath;
                            if (orignum.HtmlPath != orghp)
                            {
                                delImgs.Add(orghp);
                            }
                            if (orignum.HtmlPhonePath != orghpp)
                            {
                                delImgs.Add(orghpp);
                            }
                        }

                        #endregion

                        #region 删除图片

                        foreach (var item in delImgs)
                        {
                            FileHelper.Delete(item);
                        }

                        #endregion
                    }
                }
            }
            return Json(res);
        }

        /// <summary>
        /// 向ProductOriginNumber插入数据
        /// </summary>
        /// <param name="prod"></param>
        /// <returns>item1就是要转换后插入的Product集合，也是返回值之一,item2是ProductOriginNumber</returns>
        private Tuple<ICollection<Product>, ProductOriginNumber, OperationResult> AddProducts(CProductInfo prod)
        {
            OperationResult res = new OperationResult(OperationResultType.Error);
            ProductOriginNumber orignum = new ProductOriginNumber();
            List<string> errs = new List<string>();
            Math36 math36 = new Math36();

            #region 确定商品的原始款号
            if (!string.IsNullOrEmpty(prod.OrignNum))
            {
                orignum.OriginNumber = prod.OrignNum;
                var produ = _productOrigNumberContract.OrigNumbs.FirstOrDefault(c => c.OriginNumber == orignum.OriginNumber);

                // 商品款号
                if (produ != null)
                {
                    //orignum.AssistantNum = produ.AssistantNum;
                    //orignum.AssistantNumberOfInt = produ.AssistantNumberOfInt;
                    //orignum.BigProdNum = produ.BigProdNum;
                    //orignum = produ;

                    errs.Add("原始款号已存在");
                }
                else
                {
                    orignum.AssistantNum = CacheAccess.GetAssistantNum(prod.OrignNum, prod.BrandId, prod.CategoryId, _productOrigNumberContract, _productContract);
                    orignum.AssistantNumberOfInt = (int)math36.To10(orignum.AssistantNum, 0);

                    #region 从CproductInfo 转为ProductOriginNumber
                    orignum.TagPrice = prod.Tagprice;

                    var prods = _productContract.Products.Where(c => c.BigProdNum == prod.ProduNum);
                    var isprint = prods.Select(c => c.BarcodePrintCount).Any(g => g > 0);
                    if (isprint)
                    {
                        var orignumb = prods.FirstOrDefault().OriginNumber;
                        var orig = _productOrigNumberContract.OrigNumbs.FirstOrDefault(c => c.OriginNumber == orignumb);
                        orignum.TagPrice = orig.TagPrice;
                    }

                    orignum.BrandId = prod.BrandId;
                    orignum.CategoryId = prod.CategoryId;
                    orignum.CrowdId = prod.CrowdId;
                    orignum.SeasonId = prod.SeasonId;
                    orignum.Notes = DecodeStr(prod.Notes);
                    orignum.ProductName = DecodeStr(prod.ProduTit);
                    orignum.Summary = DecodeStr(prod.SalesTitle);
                    orignum.ProductCollocationImg = DecodeStr(prod.ProductCollocationImg);
                    orignum.OriginalPath = DecodeStr(prod.OriginalPath);
                    orignum.ThumbnailPath = GenerateThumbnai(orignum.OriginalPath);
                    orignum.TemplateId = prod.TemplateId;
                    orignum.TemplatePhoneId = prod.TemplatePhoneId;
                    orignum.Description = DecodeStr(prod.ProDescr);
                    if (prod.ProductOriginNumberTag.IsNotNull())
                    {
                        prod.ProductOriginNumberTag.OperatorId = AuthorityHelper.OperatorId;
                    }
                    orignum.ProductOriginNumberTag = prod.ProductOriginNumberTag;

                    #region 添加导航集合属性

                    #region 买手说
                    if (!prod.BuysaidAttrId.IsNullOrEmpty())
                    {
                        var buyids = prod.BuysaidAttrId.Split(",", true).ToList().ConvertAll<int>(c => Convert.ToInt32(c));
                        if (buyids.IsNotNullThis())
                        {
                            orignum.BuysaidAttributes = _productBuysaidAttributeContract.BuysaidAttributes.Where(w => buyids.Contains(w.Id)).ToList();
                        }
                    }
                    #endregion

                    #region 相关搭配
                    if (prod.OtherCollo.IsNotNullAndEmpty())
                    {
                        var collIds = prod.OtherCollo.Split(",", true).ToList().ConvertAll<int>(c => Convert.ToInt32(c));
                        if (collIds.IsNotNullThis())
                        {
                            orignum.MemberColloEles = _MemberColloEleContract.MemberColloEles.Where(w => collIds.Contains(w.Id)).ToList();
                        }
                    }
                    #endregion

                    #region 保养维护
                    if (!prod.MaintainIds.IsNullOrEmpty())
                    {
                        var mainids = prod.MaintainIds.Split(",", true).ToList().ConvertAll<int>(c => Convert.ToInt32(c));
                        if (mainids.IsNotNullThis())
                        {
                            orignum.MaintainExtends = _maintainContract.Maintains.Where(w => mainids.Contains(w.Id)).ToList();
                        }
                    }
                    #endregion

                    #region 原始款号表明细图(共用)
                    orignum.ProductImages = prod.ProductImages;
                    GenerateThumbnai(orignum.ProductImages);
                    #endregion

                    #region 搭配属性
                    if (!prod.PicAttIds.IsNullOrEmpty())
                    {
                        var picattids = prod.PicAttIds.Split(",", true).ToList().ConvertAll<int>(c => Convert.ToInt32(c));
                        if (picattids.IsNotNullThis())
                        {
                            orignum.ProductAttributes = _productAttributeContract.ProductAttributes.Where(w => picattids.Contains(w.Id)).ToList();
                        }
                    }
                    #endregion

                    #endregion

                    orignum.BuysaidText = DecodeStr(prod.Buysaid);
                    orignum.JumpLink = prod.JumpLink;
                    orignum.WholesalePrice = prod.WholesalePrice;
                    orignum.PurchasePrice = prod.PurchasePrice;

                    #region 生成商品款号
                    var brandcode = CacheAccess.GetBrands(_brandContract).FirstOrDefault(w => w.Id == prod.BrandId).BrandCode;
                    var categorycode = CacheAccess.GetCategorys(_categoryContract).FirstOrDefault(w => w.Id == prod.CategoryId).CategoryCode;
                    orignum.BigProdNum = brandcode.ToUpper() + orignum.AssistantNum.ToUpper() + categorycode.ToUpper();
                    #endregion

                    orignum.OperatorId = AuthorityHelper.OperatorId;
                    orignum.CreatedTime = DateTime.Now;
                    ////判断用户是否需要等待审核
                    //bool check = IsNeedReview();
                    //if (!check)
                    //{
                    //    orignum.IsVerified = true;
                    //    orignum.VerifiedMembId = AuthorityHelper.OperatorId;
                    //}

                    #endregion

                    #region 添加Products
                    prod.ProductNumbs = prod.ProductNumbs.DistinctBy(t => t.ProductNumber).ToList();//去重
                    foreach (var numb in prod.ProductNumbs)
                    {
                        Product dto = new Product();

                        numb.ProductNumber = orignum.BigProdNum + numb.ProductNumber.Substring(7);//生成 商品货号

                        dto.ProductNumber = numb.ProductNumber;
                        //存在同名的商品
                        if (orignum.Products.Any(c => c.ProductNumber == dto.ProductNumber))
                            continue;
                        if (_productContract.Products.Any(
                            c => c.ProductNumber == dto.ProductNumber && !c.IsDeleted))
                        {
                            errs.Add("商品已存在：" + dto.ProductNumber);
                            break;
                        }

                        #region
                        if (!string.IsNullOrEmpty(dto.ProductNumber))
                        {
                            string num = dto.ProductNumber;
                            if (num.Length < 11)
                            {
                                errs.Add("商品编号异常:" + dto.ProductNumber);
                                break;
                            }

                            string colorCode = num.Substring(7, 2);
                            dto.ColorId = CacheAccess.GetColorList(_colorContract).Where(c =>
                                c.ColorCode.ToUpper() == colorCode.ToUpper() &&
                                !c.IsDeleted && c.IsEnabled).Select(c => c.Id).FirstOrDefault();

                            //dto.SizeId
                            string sizeCode = num.Substring(9, 2).ToUpper();
                            if (sizeCode.StartsWith("0"))
                                sizeCode = sizeCode.Substring(1);
                            dto.SizeId = CacheAccess.GetSizes(_sizeContract).Where(c =>
                                !c.IsDeleted && c.IsEnabled &&
                                c.SizeCode.ToUpper() == sizeCode.ToUpper() && c.CategoryId == orignum.CategoryId).Select(c => c.Id).FirstOrDefault();

                            dto.BigProdNum = orignum.BigProdNum;
                            dto.OriginNumber = orignum.OriginNumber;
                            dto.ProductImages = numb.ProductImages;
                            dto.ProductCollocationImg = numb.ProductCollocationImg;
                            dto.OriginalPath = numb.OriginalPath;

                            #region 生成缩略图

                            dto.ThumbnailPath = GenerateThumbnai(dto.OriginalPath);
                            GenerateThumbnai(dto.ProductImages);

                            #endregion

                            #region 添加日志记录

                            ProductOperationLog log = new ProductOperationLog();
                            log.ProductNumber = dto.ProductNumber;
                            log.OperatorId = AuthorityHelper.OperatorId;
                            log.IsDeleted = false;
                            log.IsEnabled = true;
                            log.Description = "添加商品";
                            log.CreatedTime = DateTime.Now;

                            dto.ProductOperationLogs.Add(log);

                            #endregion

                            #region 商品追踪
                            ProductTrackDto pt = new ProductTrackDto();
                            pt.ProductNumber = dto.ProductNumber;
                            pt.ProductBarcode = "";
                            pt.Describe = ProductOptDescTemplate.ON_PRODUCT_ADD;

                            _productTrackContract.Insert(pt);
                            #endregion
                        }

                        #endregion

                        if (!orignum.Products.Any(c => c.ProductNumber == dto.ProductNumber))
                            orignum.Products.Add(dto);
                    }

                    #endregion

                    #region 分配宝贝详情页URL地址
                    //静态页路径
                    string strConfigPath = ConfigurationHelper.GetAppSetting("ProductHtmlPath");
                    DateTime current = DateTime.Now;
                    string strDate = current.Year.ToString() + "/" + current.Month.ToString() + "/" + current.Day.ToString() + "/" + current.Hour.ToString() + "/";
                    string htmlname = strConfigPath + strDate + orignum.BigProdNum;
                    orignum.HtmlPath = htmlname + ".html";
                    orignum.HtmlPhonePath = htmlname + "_phone.html";
                    #endregion

                    res.ResultType = OperationResultType.Success;
                }
            }
            else
            {
                errs.Add("原始款号丢失");
            }
            #endregion

            if (errs.Any())
            {
                res.Message = string.Join(";", errs);
            }
            return new Tuple<ICollection<Product>, ProductOriginNumber, OperationResult>(orignum.Products, orignum, res);
        }

        /// <summary>
        /// 更新ProductOriginNumber数据
        /// </summary>
        /// <param name="prod"></param>
        /// <returns></returns>
        private Tuple<ICollection<Product>, ProductOriginNumber, OperationResult, List<string>> UpdateProducts(CProductInfo prod)
        {
            OperationResult res = new OperationResult(OperationResultType.Error);
            ProductOriginNumber orignum = new ProductOriginNumber();
            List<string> errs = new List<string>();
            List<string> delImgs = new List<string>();//待删除的图片
            Math36 math36 = new Math36();

            #region 确定商品的商品款号
            if (!string.IsNullOrEmpty(prod.ProduNum))
            {
                orignum.BigProdNum = prod.ProduNum;
                orignum = _productOrigNumberContract.OrigNumbs.FirstOrDefault(c => c.BigProdNum == orignum.BigProdNum);
                if (orignum.IsNotNull())
                {
                    #region 从CproductInfo 转为 ProductOriginNumber

                    var pronums = orignum.Products.Select(s => s.ProductNumber).ToList();
                    if (IsUsedBarCode(pronums))
                    {
                        //打印过吊牌价格禁止修改
                    }
                    else
                    {
                        orignum.TagPrice = prod.Tagprice;
                    }
                    orignum.WholesalePrice = prod.WholesalePrice;
                    orignum.PurchasePrice = prod.PurchasePrice;

                    #region 禁止修改内容

                    //orignum.BrandId = prod.BrandId;
                    //orignum.CategoryId = prod.CategoryId;

                    #region 生成商品款号
                    //var brandcode = CacheAccess.GetBrands(_brandContract).FirstOrDefault(w => w.Id == prod.BrandId).BrandCode;
                    //var categorycode = CacheAccess.GetCategorys(_categoryContract).FirstOrDefault(w => w.Id == prod.CategoryId).CategoryCode;
                    //orignum.BigProdNum = brandcode.ToUpper() + orignum.AssistantNum.ToUpper() + categorycode.ToUpper();
                    #endregion

                    #endregion

                    orignum.SeasonId = prod.SeasonId;
                    orignum.CrowdId = prod.CrowdId;

                    orignum.Notes = DecodeStr(prod.Notes);
                    orignum.ProductName = DecodeStr(prod.ProduTit);
                    orignum.Summary = DecodeStr(prod.SalesTitle);

                    #region 吊牌属性修改
                    if (orignum.ProductOriginNumberTag.IsNotNull())
                    {
                        var pontOrg = orignum.ProductOriginNumberTag;
                        var pontNew = prod.ProductOriginNumberTag;

                        #region 水洗符号图片修改了
                        if (pontOrg.WashingSymbols != pontNew.WashingSymbols)
                        {
                            delImgs.Add(pontOrg.WashingSymbols);
                        }
                        #endregion

                        pontOrg.batching = pontNew.batching;
                        pontOrg.Category = pontNew.Category;
                        pontOrg.Fabric = pontNew.Fabric;
                        pontOrg.Inspector = pontNew.Inspector;
                        pontOrg.Level = pontNew.Level;
                        pontOrg.Material = pontNew.Material;
                        pontOrg.ProductionPlace = pontNew.ProductionPlace;
                        pontOrg.Standard = pontNew.Standard;
                        pontOrg.Stuffing = pontNew.Stuffing;
                        pontOrg.WashingSymbols = pontNew.WashingSymbols;
                        pontOrg.Manufacturer = pontNew.Manufacturer;
                        pontOrg.PostCode = pontNew.PostCode;
                        pontOrg.CateName = pontNew.CateName;
                        pontOrg.UpdatedTime = DateTime.Now;
                        pontOrg.OperatorId = AuthorityHelper.OperatorId;

                    }
                    else
                    {
                        orignum.ProductOriginNumberTag = prod.ProductOriginNumberTag;
                    }


                    #endregion

                    #region 主图修改了
                    if (orignum.OriginalPath != prod.OriginalPath)//图片修改了
                    {
                        delImgs.Add(orignum.OriginalPath);
                        delImgs.Add(orignum.ThumbnailPath);
                        if (prod.OriginalPath.IsNotNullAndEmpty())
                        {
                            orignum.OriginalPath = DecodeStr(prod.OriginalPath);
                            orignum.ThumbnailPath = GenerateThumbnai(orignum.OriginalPath);
                        }
                        else
                        {
                            orignum.OriginalPath = null;
                            orignum.ThumbnailPath = null;
                        }
                    }
                    #endregion

                    #region 搭配图修改了
                    if (orignum.ProductCollocationImg != prod.ProductCollocationImg)//图片修改了
                    {
                        delImgs.Add(orignum.ProductCollocationImg);
                        orignum.ProductCollocationImg = DecodeStr(prod.ProductCollocationImg);
                    }
                    #endregion

                    orignum.TemplateId = prod.TemplateId;
                    orignum.TemplatePhoneId = prod.TemplatePhoneId;
                    orignum.Description = DecodeStr(prod.ProDescr);

                    #region 添加导航集合属性

                    #region 买手说
                    orignum.BuysaidAttributes.Clear();
                    if (!prod.BuysaidAttrId.IsNullOrEmpty())
                    {
                        var buyids = prod.BuysaidAttrId.Split(",", true).ToList().ConvertAll<int>(c => Convert.ToInt32(c));
                        if (buyids.IsNotNullThis())
                        {
                            orignum.BuysaidAttributes = _productBuysaidAttributeContract.BuysaidAttributes.Where(w => buyids.Contains(w.Id)).ToList();
                        }
                    }
                    #endregion

                    #region 相关搭配
                    orignum.MemberColloEles.Clear();
                    if (prod.OtherCollo.IsNotNullAndEmpty())
                    {
                        var collIds = prod.OtherCollo.Split(",", true).ToList().ConvertAll<int>(c => Convert.ToInt32(c));
                        if (collIds.IsNotNullThis())
                        {
                            orignum.MemberColloEles = _MemberColloEleContract.MemberColloEles.Where(w => collIds.Contains(w.Id)).ToList();
                        }
                    }
                    #endregion

                    #region 保养维护
                    orignum.MaintainExtends.Clear();
                    if (!prod.MaintainIds.IsNullOrEmpty())
                    {
                        var mainids = prod.MaintainIds.Split(",", true).ToList().ConvertAll<int>(c => Convert.ToInt32(c));
                        if (mainids.IsNotNullThis())
                        {
                            orignum.MaintainExtends = _maintainContract.Maintains.Where(w => mainids.Contains(w.Id)).ToList();
                        }
                    }
                    #endregion

                    #region 原始款号表明细图(共用)

                    #region 需要转换缩略图
                    var curImgs = prod.ProductImages.Select(s => s.OriginalPath);
                    var orgImgs = orignum.ProductImages.Select(s => s.OriginalPath);
                    var shouldAddImgs = prod.ProductImages.Where(w => !orgImgs.Contains(w.OriginalPath)).ToList();
                    var shouldDelImgs = orignum.ProductImages.Where(w => !curImgs.Contains(w.OriginalPath)).ToList();
                    #region 需要添加
                    GenerateThumbnai(shouldAddImgs);
                    foreach (var item in shouldAddImgs)
                    {
                        orignum.ProductImages.Add(item);
                    }
                    #endregion

                    #region 需要删除

                    for (int i = shouldDelImgs.Count - 1; i >= 0; i--)
                    {
                        var curdel = shouldDelImgs[i];
                        if (orignum.ProductImages.Remove(curdel))
                        {
                            delImgs.Add(curdel.OriginalPath);
                            delImgs.Add(curdel.ThumbnailLargePath);
                            delImgs.Add(curdel.ThumbnailMediumPath);
                            delImgs.Add(curdel.ThumbnailSmallPath);
                        }
                    }

                    #endregion

                    #endregion

                    #endregion

                    #region 搭配属性
                    orignum.ProductAttributes.Clear();
                    if (!prod.PicAttIds.IsNullOrEmpty())
                    {
                        var picattids = prod.PicAttIds.Split(",", true).ToList().ConvertAll<int>(c => Convert.ToInt32(c));
                        if (picattids.IsNotNullThis())
                        {
                            orignum.ProductAttributes = _productAttributeContract.ProductAttributes.Where(w => picattids.Contains(w.Id)).ToList();
                        }
                    }
                    #endregion

                    #endregion

                    orignum.BuysaidText = DecodeStr(prod.Buysaid);
                    orignum.JumpLink = prod.JumpLink;

                    #endregion

                    #region 添加商品

                    foreach (var numb in prod.ProductNumbs)
                    {
                        Product dto = new Product();
                        dto.ProductNumber = numb.ProductNumber;
                        //存在同名的商品
                        if (orignum.Products.Any(c => c.ProductNumber == dto.ProductNumber))
                            continue;
                        if (_productContract.Products.Any(
                            c => c.ProductNumber == dto.ProductNumber && !c.IsDeleted))
                        {
                            errs.Add("商品已存在：" + dto.ProductNumber);
                            break;
                        }

                        #region 生成商品实体
                        if (!string.IsNullOrEmpty(dto.ProductNumber))
                        {
                            string num = dto.ProductNumber;
                            if (num.Length < 11)
                            {
                                errs.Add("商品货号异常:" + dto.ProductNumber);
                                break;
                            }

                            string colorCode = num.Substring(7, 2);
                            dto.ColorId = CacheAccess.GetColorList(_colorContract).Where(c =>
                                c.ColorCode.ToUpper() == colorCode.ToUpper() &&
                                !c.IsDeleted && c.IsEnabled).Select(c => c.Id).FirstOrDefault();

                            //dto.SizeId
                            string sizeCode = num.Substring(9, 2).ToUpper();
                            if (sizeCode.StartsWith("0"))
                                sizeCode = sizeCode.Substring(1);
                            dto.SizeId = CacheAccess.GetSizes(_sizeContract).Where(c =>
                                !c.IsDeleted && c.IsEnabled &&
                                c.SizeCode.ToUpper() == sizeCode.ToUpper() && c.CategoryId == orignum.CategoryId).Select(c => c.Id).FirstOrDefault();

                            dto.BigProdNum = orignum.BigProdNum;
                            dto.OriginNumber = orignum.OriginNumber;

                            #region 判断此颜色商品的图片是否已存在
                            var modhas = orignum.Products.FirstOrDefault(f => f.ColorId == dto.ColorId);
                            if (modhas.IsNotNull())
                            {
                                dto.OriginalPath = modhas.OriginalPath;
                                dto.ThumbnailPath = modhas.ThumbnailPath;
                                dto.ProductCollocationImg = modhas.ProductCollocationImg;
                                if (modhas.ProductImages.IsNotNullOrEmptyThis())
                                {
                                    var listimgs = modhas.ProductImages.Select(s => new ProductImage()
                                    {
                                        OriginalPath = s.OriginalPath,
                                        ThumbnailLargePath = s.ThumbnailLargePath,
                                        ThumbnailMediumPath = s.ThumbnailMediumPath,
                                        ThumbnailSmallPath = s.ThumbnailSmallPath,
                                        OperatorId = AuthorityHelper.OperatorId
                                    }).ToList();
                                    dto.ProductImages = listimgs;
                                }
                            }
                            else
                            {
                                dto.ProductImages = numb.ProductImages;
                                dto.ProductCollocationImg = numb.ProductCollocationImg;
                                dto.OriginalPath = numb.OriginalPath;

                                #region 生成缩略图

                                dto.ThumbnailPath = GenerateThumbnai(dto.OriginalPath);
                                GenerateThumbnai(dto.ProductImages);

                                #endregion
                            }
                            #endregion

                            #region 添加日志记录

                            ProductOperationLog log = new ProductOperationLog();
                            log.ProductNumber = dto.ProductNumber;
                            log.OperatorId = AuthorityHelper.OperatorId;
                            log.IsDeleted = false;
                            log.IsEnabled = true;
                            log.Description = "添加商品";
                            log.CreatedTime = DateTime.Now;

                            dto.ProductOperationLogs.Add(log);

                            #endregion
                        }

                        #endregion

                        if (!orignum.Products.Any(c => c.ProductNumber == dto.ProductNumber))
                            orignum.Products.Add(dto);
                    }

                    #endregion

                    orignum.OperatorId = AuthorityHelper.OperatorId;
                    orignum.UpdatedTime = DateTime.Now;

                    res.ResultType = OperationResultType.Success;
                }
                else
                {
                    errs.Add("商品款号丢失");
                }
            }
            else
            {
                errs.Add("商品款号丢失");
            }
            #endregion

            if (errs.Any())
            {
                res.Message = string.Join(";", errs);
            }
            return new Tuple<ICollection<Product>, ProductOriginNumber, OperationResult, List<string>>(orignum.Products, orignum, res, delImgs);
        }

        private string DecodeStr(string st)
        {
            if (!st.IsNullOrEmpty())
            {
                return st.Replace("%lt%", "<").Replace("%gt%", ">");
            }
            return st;
        }

        public JsonResult GetProductNumberNotBigProNum(int? colorId, int? sizeId)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error, "颜色和尺码信息丢失！");
            var ProduNumber = "xxxxxxx";
            if (colorId.HasValue && sizeId.HasValue)
            {
                var sizeNum = CacheAccess.GetSizes(_sizeContract).Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == sizeId).Select(c => c.SizeCode).FirstOrDefault();
                var colorNum = CacheAccess.GetColorList(_colorContract).Where(c => c.IsDeleted == false && c.IsEnabled == true && c.Id == colorId).Select(c => c.ColorCode).FirstOrDefault();
                if (sizeNum.IsNotNull() && colorNum.IsNotNull())
                {
                    ProduNumber = ProduNumber + colorNum + sizeNum.PadLeft(2, '0');
                    resul = new OperationResult(OperationResultType.Success, "", ProduNumber);
                }
            }
            return Json(resul);
        }

        public ActionResult GetChildByBigNum()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Product, bool>> predicate = FilterHelper.GetExpression<Product>(request.FilterGroup);
            var prods = _productContract.Products.Where(predicate);

            var retProds = prods.OrderByDescending(c => c.CreatedTime).GroupBy(c => c.BigProdNum);
            List<object> lis = new List<object>();
            foreach (var item in retProds)
            {
                #region MyRegion

                var childs = item.OrderByDescending(c => c.CreatedTime).Select(x => new
                {
                    Id = x.Id.ToString(),
                    ParentId = "",
                    BrandName = x.ProductOriginNumber.Brand.BrandName,
                    CategoryName = x.ProductOriginNumber.Category.CategoryName,
                    SeasonName = x.ProductOriginNumber.Season.SeasonName,
                    ProductNumber = x.ProductNumber,
                    SizeName = x.Size.SizeName,
                    ThumbnailPath = x.ThumbnailPath, //显示子类的第一张图
                    ColorName = _colorContract.Colors.Where(m => m.Id == x.ColorId).FirstOrDefault().ColorName,
                    HtmlPath = x.ProductOriginNumber.HtmlPath,
                    TagPrice = x.ProductOriginNumber.TagPrice,
                    WholesalePrice = x.ProductOriginNumber.WholesalePrice,
                    PurchasePrice = x.ProductOriginNumber.PurchasePrice,
                    UseDefaultDiscount = "",
                    IsEnabled = x.IsEnabled,
                    ChilCou = ""
                });
                #endregion

                lis.AddRange(childs);
            }
            return Json(lis);
        }

        /// <summary>
        /// 移除商品
        /// </summary>
        /// <param name="ids">商品ID集合</param>
        /// <param name="isDele">是否物理删除</param>
        /// <returns></returns>
        [Log]
        public ActionResult Remove(string[] ids, bool isDele)
        {
            OperationResult result = new OperationResult(OperationResultType.Error, "操作失败");
            if (ids == null || ids.Count() == 0)
            {
                result.Message = "请选择需要移除的商品货号或原始款号";
            }
            else
            {
                try
                {
                    List<int> li = new List<int>();
                    List<int> orgli = new List<int>();
                    foreach (var ite in ids)
                    {
                        if (ite.IndexOf("org") > -1)
                        {
                            int _id = Convert.ToInt32(ite.Substring(3));
                            orgli.Add(_id);
                        }
                        else if (ite.IndexOf("par") > -1)
                        {
                            int bigNumId = Convert.ToInt32(ite.Substring(3));
                            var curPON = _productOrigNumberContract.OrigNumbs.Where(w => w.Id == bigNumId).FirstOrDefault();
                            if (curPON.IsNotNull())
                            {
                                var childP = curPON.Products.Select(s => s.Id).ToList();
                                if (childP.IsNotNull())
                                {
                                    li.AddRange(childP);
                                }
                                orgli.Add(bigNumId);
                            }
                        }
                        else
                        {
                            li.Add(Convert.ToInt32(ite));
                        }
                    }

                    if (li.Count() > 0)
                    {
                        if (!isDele)
                        {
                            _productOrigNumberContract.Remove(true, orgli.ToArray());
                            result = _productContract.Remove(li.ToArray());
                        }
                        else
                        {
                            _productOrigNumberContract.Delete(true, orgli.ToArray());
                            result = _productContract.Delete(li.ToArray());
                        }

                    }
                    else
                    {
                        if (!isDele)
                        {
                            result = _productOrigNumberContract.Remove(false, orgli.ToArray());
                        }
                        else
                        {
                            result = _productOrigNumberContract.Delete(false, orgli.ToArray());
                        }
                    }

                }
                catch (Exception ex)
                {
                    result.Message = ex.Message;
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Delete(int[] Id)
        {
            var result = _productContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Recovery(string[] ids)
        {
            OperationResult result;
            List<int> li = new List<int>();
            List<int> origIds = new List<int>();
            foreach (var id in ids)
            {
                if (id.StartsWith("org"))
                {
                    var teid = id.Substring(3);
                    origIds.Add(Convert.ToInt32(teid));
                }
                else if (id.StartsWith("par"))
                {
                    int bigNumId = Convert.ToInt32(id.Substring(3));
                    var curPON = _productOrigNumberContract.OrigNumbs.Where(w => w.Id == bigNumId).FirstOrDefault();
                    if (curPON.IsNotNull())
                    {
                        var childP = curPON.Products.Select(s => s.Id).ToList();
                        if (childP.IsNotNull())
                        {
                            li.AddRange(childP);
                        }
                        origIds.Add(bigNumId);
                    }
                }
                else
                {
                    li.Add(Convert.ToInt32(id));
                }
            }
            if (origIds.Any())
            {
                var origs = _productOrigNumberContract.OrigNumbs.Where(c => origIds.Contains(c.Id));
                origs.Each(c =>
                {
                    c.IsDeleted = false;
                    c.UpdatedTime = DateTime.Now;
                });
                result = _productOrigNumberContract.Update(origs.ToArray());
                _productContract.Recovery(li.ToArray());
            }
            else
            {
                result = _productContract.Recovery(li.ToArray());
                if (result.ResultType == OperationResultType.Success)
                {
                    var parPON = _productContract.Products.Where(s => li.Contains(s.Id)).Select(s => s.ProductOriginNumber).ToList();
                    if (parPON.IsNotNull())
                    {
                        parPON.Each(e =>
                        {
                            e.IsDeleted = false;
                            e.UpdatedTime = DateTime.Now;
                        });
                    }
                    result = _productOrigNumberContract.Update(parPON.ToArray());
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Enable(int[] Id)
        {
            var result = _productContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Disable(int[] Id)
        {
            var result = _productContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _productContract.Products.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _productContract.Products.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 条码数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [License(CheckMode.Check)]
        public ActionResult Barcode(int[] Id)
        {
            var result = _productContract.Products.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            return PartialView(result);
        }


        /// <summary>
        /// 审核数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [License(CheckMode.Check)]
        public ActionResult Publish(int[] Id)
        {
            var result = _productContract.Publish(Id);
            if (result.ResultType == OperationResultType.Success)
            {
                var listPONIds = _productContract.Products.Where(w => Id.Contains(w.Id) && !w.IsEnabled && !w.IsDeleted && w.ProductOriginNumber != null).Select(s => s.ProductOriginNumber.Id).ToList();
                if (listPONIds.IsNotNullOrEmptyThis())
                {
                    SendShouldVerifyNotification(listPONIds);
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult VerifyReason(int Id)
        {
            var result = _productOrigNumberContract.View(Id);
            return PartialView(result);
        }

        /// <summary>
        /// 缩略图数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [License(CheckMode.Check)]
        public ActionResult Thumbnails(int Id)
        {
            var result = new List<object>();
            var entity = _productContract.Products.FirstOrDefault(m => m.Id == Id);

            if (entity != null)
            {
                var counter = 1;
                foreach (var item in entity.ProductImages)
                {
                    var filePath = FileHelper.UrlToPath(item.OriginalPath);
                    if (System.IO.File.Exists(filePath))
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        result.Add(new
                        {
                            ID = counter.ToString(),
                            FileName = item.OriginalPath,
                            FilePath = item.OriginalPath,
                            FileSize = fileInfo.Length
                        });
                        counter++;
                    }
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取原始款号对应的商品图片明细
        /// </summary>
        /// <param name="origNum"></param>
        /// <returns></returns>
        public ActionResult ThumbnailsForOrigNum(string origNum)
        {
            var result = new List<object>();
            List<string> imgarr = new List<string>();

            imgarr = _productOrigNumberContract.OrigNumbs.Where(c => c.OriginNumber == origNum).SelectMany(c => c.ProductImages).Select(s => s.OriginalPath).ToList();
            if (imgarr.Any())
            {
                var counter = 1;
                foreach (var item in imgarr)
                {
                    var filePath = FileHelper.UrlToPath(item);
                    if (System.IO.File.Exists(filePath))
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        result.Add(new
                        {
                            ID = counter.ToString(),
                            FileName = item,
                            FilePath = item,
                            FileSize = fileInfo.Length
                        });
                        counter++;
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取产品列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetProducts()
        {
            var list = _productContract.Selectlist("请选择商品", c => c.IsDeleted == false && c.IsEnabled == true).Select(m => new SelectListItem { Text = m.Value, Value = m.Key });
            return new JsonResult() { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        /// <summary>
        /// 根据商品编号返回商品基本信息
        /// </summary>
        /// <param name="produNum"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetProductByNum(string produNum)
        {
            var data = await Task.Run(() =>
            {
                return _productContract.Products.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.ProductNumber == produNum).Select(c => new
                {
                    Id = c.Id,
                    ProductName = c.ProductName,
                    CategoryId = c.ProductOriginNumber.CategoryId,
                    BrandId = c.ProductOriginNumber.BrandId,
                    SizeId = c.SizeId,
                    SeasonId = c.ProductOriginNumber.SeasonId,
                    ColorId = c.ColorId,
                    ProductType = c.ProductType,
                    //Description=c.Description,
                    ThumbnailPath = c.ThumbnailPath

                }).FirstOrDefault();
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProductList()
        {
            Response.Cache.SetOmitVaryStar(true);
            ViewBag.Brand = CacheAccess.GetBrand(_brandContract, true);
            return PartialView();

        }
        /// <summary>
        /// 根据搜索条件获取商品信息
        /// </summary>
        /// <returns></returns>
        public JsonResult GetProductArr()
        {
            GridRequest gr = new GridRequest(Request);
            Expression<Func<Product, bool>> predic = FilterHelper.GetExpression<Product>(gr.FilterGroup);

            string numOrName = "";
            string Brand = "";

            int daCou = 0;

            if (gr.FilterGroup.Rules.Count > 0)
            {
                for (int i = 0; i < gr.FilterGroup.Rules.Count; i++)
                {
                    if (gr.FilterGroup.Rules.ToList()[i].Field == "AttributeNameOrNum")
                    {
                        var te = gr.FilterGroup.Rules.ToList()[i].Value;
                        if (te != null && te.ToString() != "")
                        {
                            // pageind = "1";
                            numOrName = te.ToString();
                        }
                    }
                    if (gr.FilterGroup.Rules.ToList()[i].Field == "BrandId")
                    {
                        var te = gr.FilterGroup.Rules.ToList()[i].Value;
                        if (te != null)
                            Brand = te.ToString();
                    }
                }
            }

            var resul = _productContract.Products.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.ProductOriginNumber.IsVerified == CheckStatusFlag.通过).Select(c => new
            {
                Id = c.Id,
                Name = c.ProductName,
                ProNum = c.ProductNumber,
                BrandId = c.ProductOriginNumber.BrandId,
                Brand = c.ProductOriginNumber.Brand.BrandName,
                Size = c.Size.SizeName,
                Seaso = c.ProductOriginNumber.Season.SeasonName,
                Colo = c.Color.IconPath,
                Thumbnail = c.ThumbnailPath
            });

            if (!string.IsNullOrEmpty(numOrName))
            {
                resul = resul.Where(c => c.Name == numOrName || c.ProNum == numOrName);

            }
            if (!string.IsNullOrEmpty(Brand) && Brand != "-1")
            {
                int brandid = Convert.ToInt32(Brand);
                resul = resul.Where(c => c.BrandId == brandid);
            }
            daCou = resul.Count();

            resul = resul.OrderBy(c => c.Id).Skip(gr.PageCondition.PageIndex).Take(gr.PageCondition.PageSize);


            var da = new GridData<object>(resul.ToList(), daCou, Request);
            return Json(da, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 初始化设置商品属性界面
        /// </summary>
        /// <returns></returns>
        public ActionResult SetAttr()
        {
            return PartialView();
        }
        /// <summary>
        /// 判断指定的2商品货号或者0原始款号是否存在
        /// </summary>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult ExistNumber()
        {
            OperationResult res = new OperationResult(OperationResultType.Success);
            string origiNum = Request["Num"];
            string isOrigi = Request["isOrigi"];
            if (origiNum != null)
            {
                var modDesi = _DesignerContract.SelectDesigner.FirstOrDefault(f => f.AdminId == AuthorityHelper.OperatorId.Value && f.IsEnabled && !f.IsDeleted);
                if (modDesi.IsNotNull())
                {
                    if (isOrigi == "0") //商品原始款号
                    {
                        var mod = _productOrigNumberContract.OrigNumbs.FirstOrDefault(w => w.OriginNumber == origiNum);
                        if (mod.IsNotNull())
                        {
                            var isDesigner = mod.DesignerId == modDesi.Id;//是设计师产品,可以修改
                            res.Data = new
                            {
                                isDesigner = isDesigner,
                            };
                        }
                    }
                    else
                    {
                        var query = _productContract.Products.Where(w => w.ProductNumber == origiNum);
                        if (query.Any())
                        {
                            var isDesigner = query.Any(w => w.ProductOriginNumber.DesignerId == modDesi.Id);//是设计师产品,可以修改
                            res.Data = new
                            {
                                isDesigner = isDesigner,
                            };
                        }
                    }
                }
                else {
                    res.ResultType = OperationResultType.Error;
                    res.Message = "你还不是设计师";
                }
            }
            else
            {
                res.ResultType = OperationResultType.ValidError;
                res.Message = "参数无效";
            }
            return Json(res);
        }
        /// <summary>
        /// 获取折扣分类的详细信息
        /// </summary>
        /// <param name="Id">0:店铺;1:季节;2:品牌;3:颜色;4:尺码</param>
        /// <returns></returns>
        public JsonResult GetDetail(int Id)
        {
            switch (Id)
            {
                case 0:
                    var store = _storeContract.Stores.Where(x => x.IsDeleted == false && x.IsEnabled == true).Select(x => new
                    {
                        Id = x.Id,
                        Name = x.StoreName
                    });
                    return Json(store, JsonRequestBehavior.AllowGet);
                case 1:
                    var season = _seasonContract.Seasons.Where(x => x.IsDeleted == false && x.IsEnabled == true).Select(x => new
                    {
                        Id = x.Id,
                        Name = x.SeasonName
                    });
                    return Json(season, JsonRequestBehavior.AllowGet);
                case 2:
                    var brand = _brandContract.Brands.Where(x => x.IsDeleted == false && x.IsEnabled == true).Select(x => new
                    {
                        Id = x.Id,
                        Name = x.BrandName
                    });
                    return Json(brand, JsonRequestBehavior.AllowGet);
                case 3:
                    var color = _colorContract.Colors.Where(x => x.IsDeleted == false && x.IsEnabled == true).Select(x => new
                    {
                        Id = x.Id,
                        Name = x.ColorName
                    });
                    return Json(color, JsonRequestBehavior.AllowGet);
                case 4:
                    var size = _sizeContract.Sizes.Where(x => x.IsDeleted == false && x.IsEnabled == true).Select(x => new
                    {
                        Id = x.Id,
                        Name = x.SizeName
                    });
                    return Json(size, JsonRequestBehavior.AllowGet);
                default:
                    return null;
            }
        }
        /// <summary>
        /// 获取产品的编号"品牌2+款式3+品类2（0x16)+颜色2+尺码(2)"
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetProductNumber()
        {//categoryId: categoryId, brandId: brandId, sizeId: sizeId, colorId: colorId, seasonId: seasonId,proNum:proNum
            string categoryId = Request["categoryId"];
            string brandId = Request["brandId"];
            string sizeId = Request["sizeId"];
            string seasonId = Request["seasonId"];
            string colorId = Request["colorId"];
            string proNum = Request["proNum"];
            string orignum = Request["orignum"];//原始ID
            bool isorig = (Request["isorig"] == "0");//orignum表示的是否是0原始款号1商品款号

            OperationResult resul = new OperationResult(OperationResultType.Error, "商品货号生成异常！");
            string err = "";

            var origNumber = "";
            var brandNum = "";
            #region 获取品牌代码

            if (!string.IsNullOrEmpty(brandId))
            {
                brandNum =
                    CacheAccess.GetBrands(_brandContract)
                        .Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id.ToString() == brandId)
                        .Select(c => c.BrandCode)
                        .FirstOrDefault();
                if (brandNum == null)
                    brandNum = "";
            }
            else
            {
                err += "品牌信息为空；";
            }
            #endregion

            var categoryNum = "";
            #region 获取品类代码

            if (!string.IsNullOrEmpty(categoryId))
            {
                categoryNum =
                    CacheAccess.GetCategorys(_categoryContract)
                        .Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id.ToString() == categoryId)
                        .Select(c => c.CategoryCode)
                        .FirstOrDefault();

                if (categoryNum == null)
                    categoryNum = "";
            }
            else
            {
                err += "品类信息为空；";
            }
            #endregion
            string assistantNum = "";
            #region 获取款式代码

            if (!string.IsNullOrEmpty(orignum))
            {
                if (!string.IsNullOrEmpty(brandId) && !string.IsNullOrEmpty(categoryId))
                {
                    int _brandId = Convert.ToInt32(brandId);
                    int _categoryId = Convert.ToInt32(categoryId);
                    if (!isorig)
                    {//orignum表示的是商品款号
                        var orig = _productOrigNumberContract.OrigNumbs.FirstOrDefault(c => c.BigProdNum == orignum);
                        if (orig != null)
                            origNumber = orig.OriginNumber;
                        else
                        {
                            var prod = _productContract.Products.FirstOrDefault(c => c.BigProdNum == orignum);
                            if (prod != null)
                                origNumber = prod.OriginNumber;
                        }
                    }
                    else
                        origNumber = orignum;
                    assistantNum = CacheAccess.GetAssistantNum(origNumber, _brandId, _categoryId, _productOrigNumberContract, _productContract);
                }
            }
            else
            {
                //商品货号为空，说明输入的是原始款号，所以原始款号不会为空
            }

            #endregion
            var sizeNum = "";
            #region 获取尺寸代码

            if (!string.IsNullOrEmpty(sizeId))
            {
                sizeNum =
                    CacheAccess.GetSizes(_sizeContract)
                        .Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id.ToString() == sizeId)
                        .Select(c => c.SizeCode)
                        .FirstOrDefault();
                if (sizeNum == null)
                    sizeNum = "";
            }
            else
            {
                err += "尺寸信息为空；";
            }
            #endregion

            string colorCode = "";
            #region 颜色代码
            if (!string.IsNullOrEmpty(colorId))
            {
                int _id = Convert.ToInt32(colorId);
                colorCode = CacheAccess.GetColorList(_colorContract).Where(c => c.IsDeleted == false && c.IsEnabled == true && c.Id == _id).Select(c => c.ColorCode).FirstOrDefault();
                if (colorCode == null)
                    colorCode = "";
            }
            else
            {
                err += "颜色信息为空；";
            }
            #endregion

            if (!string.IsNullOrEmpty(brandNum) && !string.IsNullOrEmpty(assistantNum) &&
                !string.IsNullOrEmpty(categoryNum) && !string.IsNullOrEmpty(colorCode) && !string.IsNullOrEmpty(sizeNum))
            {
                string _code = brandNum + assistantNum + categoryNum + colorCode + sizeNum.PadLeft(2, '0'); //基准号

                //如果原始货号相对，且商品基准编号相等，则认为商品编号重复
                var pronumb = _productContract.Products.FirstOrDefault(
                     c => c.ProductNumber == _code);

                //判断指定的商品编码是否已存在
                var prod =
                    _productContract.Products.FirstOrDefault(
                        c => c.ProductNumber == _code && c.IsDeleted == false && c.IsEnabled == true);

                resul = prod != null ? new OperationResult(OperationResultType.NameRepeat, "同名的商品货号已经存在", _code) :
                    new OperationResult(OperationResultType.Success, "", _code);
            }
            else
            {
                resul = new OperationResult(OperationResultType.Error, err);
            }

            return Json(resul);


        }

        /// <summary>
        /// 通过款号获取原始商品号
        /// </summary>
        /// <param name="bigNum"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetProductOriginByBigNum(string bigNum)
        {
            string orignNum = "";
            var prod = _productContract.Products.Where(c => c.BigProdNum == bigNum).FirstOrDefault();
            if (prod != null)
                orignNum = prod.OriginNumber ?? "";
            return Json(orignNum);
        }
        /// <summary>
        /// 根据categoryId获取对应的尺码
        /// </summary>
        /// <param name="catId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetSizeByCategId(int catId)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error, "操作异常");
            var t = CacheAccess.GetSize(_sizeContract, catId.ToString()).Select(c => new { tx = c.Text, vl = c.Value }).ToList();
            if (t.Any())
                resul = new OperationResult(OperationResultType.Success, "ok") { Data = t };
            return Json(resul);
        }
        /// <summary>
        /// 根据商品编号获取商品信息
        /// </summary>
        /// <param name="nums"></param>
        /// <returns></returns>
        public ActionResult GetProductsByNums(string[] nums)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error, "操作异常");
            if (nums != null && nums.Count() > 0)
            {

                var t = _productContract.Products.Where(c => nums.Contains(c.ProductNumber) && c.IsDeleted == false && c.IsEnabled == true && c.ProductOriginNumber.IsVerified == CheckStatusFlag.通过).Select(c => new
                {
                    c.Id,
                    c.ProductNumber,
                    c.ProductName,
                    c.ProductOriginNumber.TagPrice,
                    c.ProductOriginNumber.Brand.BrandName,
                    c.ProductOriginNumber.Category.CategoryName,
                    c.Color.IconPath,
                    c.Color.ColorName,
                    c.Size.SizeName,
                    c.ProductOriginNumber.Season.SeasonName,
                    c.ThumbnailPath,

                }).ToList().Select(c => new
                {

                    c.Id,
                    c.ProductNumber,
                    c.ProductName,
                    c.TagPrice,
                    c.BrandName,
                    c.CategoryName,
                    c.IconPath,
                    c.ColorName,
                    c.SizeName,
                    c.SeasonName,
                    c.ThumbnailPath,
                    saleCampaigns = GetSalesCampaignsByProId(c.Id)
                }).ToList();

                if (t.Count > 0)
                    resul = new OperationResult(OperationResultType.Success, "ok") { Data = t };
            }
            return Json(resul);
        }
        /// <summary>
        /// 根据商品id获取该商品可以参与的未过期活动
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private object GetSalesCampaignsByProId(int id)
        {
            var num = _productContract.Products.Single(c => c.Id == id).ProductNumber;
            return CacheAccess.GetSalesCampaign(_salesCampaignContract).Where(x => !x.ISPass && x.BigProdNums.Contains(num)).Select(g => new
            {
                g.Id,
                g.CampaignName,
                g.MemberDiscount,
                g.NoMmebDiscount,
                g.OtherCashCoupon
            }).ToList();
        }
        /// <summary>
        /// 商品搭配图上传
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CollocationImgUpload()
        {
            var file = Request.Files[0];
            OperationResult resul = new OperationResult(OperationResultType.Error);

            string[] extens = { ".png", ".jpg" };
            string ext = Path.GetExtension(file.FileName);
            if (extens.Contains(ext))
            {
                string absoulPath = "/Content/Images/ProductCollImgs/" + DateTime.Now.ToString("yyyyMMdd") + "/" + DateTime.Now.Hour + "/";
                string saveDire = Server.MapPath(absoulPath);
                if (!Directory.Exists(saveDire))
                {
                    Directory.CreateDirectory(saveDire);
                }
                string savePath = saveDire + "/" + file.FileName;
                if (System.IO.File.Exists(savePath))
                {
                    System.IO.File.Delete(savePath);
                }
                file.SaveAs(savePath);
                resul = new OperationResult(OperationResultType.Success, "ok") { Data = absoulPath + "/" + file.FileName };
            }
            return Json(resul);
        }
        /// <summary>
        /// 获取搭配图
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCollocationInfo()
        {
            GridRequest rq = new GridRequest(Request);
            Expression<Func<MemberCollocation, bool>> predicate = FilterHelper.GetExpression<MemberCollocation>(rq.FilterGroup);
            var li = _memberCollocationContract.MemberCollocations.Where(predicate).Select(c => new
            {
                c.CollocationName,
                collpics = c.MemberColloEles.Where(w => w.ImagePath != "" && w.ImagePath != null).Select(x => new { x.Id, x.TextInfo, ImagePath = ApiUrl + x.ImagePath }).ToList()
            }).ToList();
            return Json(li);
        }
        /// <summary>
        /// 根据搭配ID获取搭配图
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ActionResult GetCollocationByIds(string ids)
        {
            OperationResult resul = new OperationResult(OperationResultType.Success, "");

            if (!string.IsNullOrEmpty(ids))
            {
                var _ids = ids.Split(",", true).Select(c => Convert.ToInt32(c)).ToList();
                resul.Data = _MemberColloEleContract.MemberColloEles.Where(c => _ids.Contains(c.Id)).Select(c => new { ImagePath = ApiUrl + c.ImagePath, c.Id, c.TextInfo }).ToList();
            }
            return Json(resul);
        }
        /// <summary>
        /// 根据父级的ID获取子集
        /// </summary>
        /// <param name="parIds">,1,2,3,</param>
        /// <returns></returns>
        public ActionResult GetProductAttriByParentIds(string parIds)
        {
            List<SelectListItem> li = new List<SelectListItem>();
            if (!string.IsNullOrEmpty(parIds))
            {
                List<int> ids = parIds.Split(",", true).Select(c => Convert.ToInt32(c)).ToList();
                li = CacheAccess.GetProductAttributeDtos(_productAttributeContract)
                     .Where(c => c.ParentId != null && ids.Contains(c.ParentId ?? -1)).Select(c => new SelectListItem()
                     {
                         Text = StringHelper.GetPrefix(1) + c.AttributeName,
                         Value = c.Id.ToString()
                     }).ToList();
            }
            return Json(li);
        }

        /// <summary>
        /// 生成静态页
        /// </summary>
        /// <param name="pon"></param>
        /// <returns></returns>
        private OperationResult CreateHtml(ProductOriginNumber pon)
        {
            if (pon.TemplateId == 0 || pon.TemplatePhoneId == 0)
                return new OperationResult(OperationResultType.Error, "模板不存在");
            try
            {
                //静态页路径
                string strConfigPath = ConfigurationHelper.GetAppSetting("ProductHtmlPath");
                DateTime current = DateTime.Now;
                string strDate = current.Year.ToString() + "/" + current.Month.ToString() + "/" + current.Day.ToString() + "/" + current.Hour.ToString() + "/";
                IQueryable<Template> listTemplate = _templateContract.Templates.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.TemplateType == (int)TemplateFlag.Product);
                var modAdmin = _administratorContract.View(AuthorityHelper.OperatorId.Value);

                #region PC模板

                int templateId = pon.TemplateId;
                var template = listTemplate.FirstOrDefault(f => f.Id == templateId);
                if (template == null)
                {
                    return new OperationResult(OperationResultType.Error, "生成失败，请选择模板");
                }


                var dicPC = new Dictionary<string, object>();
                dicPC.Add("PONInfo", pon);
                dicPC.Add("AdminInfo", modAdmin);

                var strResultPC = NVelocityHelper.Generate(template.TemplateHtml, dicPC, "productDetail_PC");

                strResultPC = GetFashionHtml(pon, TemplateTypeFlag.PC, strResultPC);

                if (pon.Id == 0 || pon.HtmlPath.IsNullOrEmpty())
                    pon.HtmlPath = strConfigPath + strDate + pon.BigProdNum + ".html";

                bool isSave = FileHelper.SavePath(pon.HtmlPath, strResultPC);
                if (!isSave)
                {
                    return new OperationResult(OperationResultType.Error, "生成静态页失败！");
                }

                #region 页面自动转换生成二维码
                //OperationResult operRes = this.CreateQRCode(pon.BigProdNum, pon.HtmlPath);
                //if (operRes.ResultType != OperationResultType.Success)
                //{
                //    return new OperationResult(OperationResultType.Error, "生成二维码失败！");
                //}
                //else
                //{
                //    pon.QRCodePath = operRes.Data.ToString();
                //}
                #endregion

                #endregion

                #region 手机模板

                var templatepId = pon.TemplatePhoneId ?? 0;
                var templatep = listTemplate.FirstOrDefault(f => f.Id == templatepId);
                if (templatep == null)
                {
                    return new OperationResult(OperationResultType.Error, "生成失败，请选择模板");
                }

                var dicPhone = new Dictionary<string, object>();
                dicPhone.Add("PONInfo", pon);
                dicPhone.Add("AdminInfo", modAdmin);

                var strResultPhone = NVelocityHelper.Generate(templatep.TemplateHtml, dicPhone, "productDetail_Phone");

                strResultPhone = GetFashionHtml(pon, TemplateTypeFlag.手机, strResultPhone);

                if (pon.Id == 0 || pon.HtmlPhonePath.IsNullOrEmpty())
                    pon.HtmlPhonePath = strConfigPath + strDate + pon.BigProdNum + "_phone.html";

                isSave = FileHelper.SavePath(pon.HtmlPhonePath, strResultPhone);
                if (!isSave)
                {
                    return new OperationResult(OperationResultType.Error, "生成静态页失败！");
                }

                #region 页面自动转换生成二维码
                //OperationResult operRes = this.CreateQRCode(pon.BigProdNum, pon.HtmlPath);
                //if (operRes.ResultType != OperationResultType.Success)
                //{
                //    return new OperationResult(OperationResultType.Error, "生成二维码失败！");
                //}
                //else
                //{
                //    pon.QRCodePath = operRes.Data.ToString();
                //}
                #endregion

                #endregion

                return new OperationResult(OperationResultType.Success, "生成成功");
            }
            catch (Exception ex)
            {
                _log.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "生成静态页失败，程序异常！");
            }
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <returns></returns>
        private OperationResult CreateQRCode(string fileName, string link)
        {
            string strDomain = ConfigurationHelper.GetAppSetting("Domain");
            string strQrCodePath = ConfigurationHelper.GetAppSetting("ProductQRCodePath");
            string strDate = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "/";
            string strUrl = strDomain + link;
            string savePath = strQrCodePath + strDate + fileName + ".jpg";
            try
            {
                QRCodeEncoder qr = new QRCodeEncoder();
                //编码方式
                qr.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
                qr.QRCodeScale = 4;//大小(值越大生成的二维码图片像素越高)
                qr.QRCodeVersion = 0;//版本(注意：设置为0主要是防止编码的字符串太长时发生错误)
                qr.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;//错误效验、错误更正(有4个等级)
                Bitmap bmp = qr.Encode(strUrl);
                bmp.Save(FileHelper.UrlToPath(savePath));
                return new OperationResult(OperationResultType.Success, "生成成功！", savePath);
            }
            catch (Exception ex)
            {
                _log.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "生成失败！");
            }
        }

        /// <summary>
        /// 条码是否已使用
        /// </summary>
        /// <returns></returns>
        private bool IsUsedBarCode(List<string> productNumbers)
        {
            //var isUsed = _productBarcodeDetailContract.productBarcodeDetails.Count(w => productNumbers.Contains(w.ProductNumber) && w.Status == (int)ProductBarcodeDetailFlag.AddStorage);
            var isUsed = _productBarcodeDetailContract.productBarcodeDetails.Count(w => productNumbers.Contains(w.ProductNumber));
            return isUsed > 0;
        }
        /// <summary>
        /// 生成明细缩略图
        /// </summary>
        /// <param name="originalImg"></param>
        /// <param name="imgs"></param>
        private void GenerateThumbnai(IEnumerable<ProductImage> imgs)
        {
            if (imgs.IsNotNullThis())
            {
                foreach (var item in imgs)
                {
                    if (!item.OriginalPath.IsNullOrEmpty())
                    {
                        string exten = Path.GetExtension(item.OriginalPath);
                        string imgtype = exten.Substring(1);
                        var ThumbnailLargePath = EnvironmentHelper.ProductPath + "/p_s_l_" +
                                            DateTime.Now.ToString("HHmmssffff", DateTimeFormatInfo.InvariantInfo) + exten;
                        var ThumbnailMediumPath = EnvironmentHelper.ProductPath + "/p_s_m_" +
                                            DateTime.Now.ToString("HHmmssffff", DateTimeFormatInfo.InvariantInfo) + exten;
                        var ThumbnailSmallPath = EnvironmentHelper.ProductPath + "/p_s_s_" +
                                            DateTime.Now.ToString("HHmmssffff", DateTimeFormatInfo.InvariantInfo) + exten;
                        ImageHelper.MakeThumbnail(item.OriginalPath, ThumbnailLargePath, 204, 325, "W", imgtype);
                        ImageHelper.MakeThumbnail(item.OriginalPath, ThumbnailMediumPath, 204, 325, "W", imgtype);
                        ImageHelper.MakeThumbnail(item.OriginalPath, ThumbnailSmallPath, 204, 325, "W", imgtype);

                        item.ThumbnailLargePath = ThumbnailLargePath;
                        item.ThumbnailMediumPath = ThumbnailMediumPath;
                        item.ThumbnailSmallPath = ThumbnailSmallPath;
                    }
                }
            }
        }

        /// <summary>
        /// 根据原图,生成缩略图
        /// </summary>
        /// <param name="originalImg"></param>
        /// <returns></returns>
        private string GenerateThumbnai(string originalImg)
        {
            if (originalImg.IsNotNullAndEmpty())
            {
                string exten = Path.GetExtension(originalImg);
                string imgtype = exten.Substring(1);
                var thumbnailPath = EnvironmentHelper.ProductPath + "/p_s_" +
                                    DateTime.Now.ToString("HHmmssffff", DateTimeFormatInfo.InvariantInfo) + exten;

                ImageHelper.MakeThumbnail(originalImg, thumbnailPath, 204, 325, "W", imgtype);
                return thumbnailPath;
            }
            return null;
        }

        /// <summary>
        /// 获取宝贝模板详情，共用
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="phoneOrpc">0为pc，1为手机</param>
        /// <returns></returns>
        public ActionResult ProductDetailOnly(int Id, TemplateTypeFlag phoneOrpc)
        {
            var modPON = _productOrigNumberContract.View(Id);
            ViewBag.Templates = CacheAccess.GetTemplates(_templateContract, true, phoneOrpc);
            ViewBag.phoneOrpc = (int)phoneOrpc;
            return PartialView(modPON);
        }
        /// <summary>
        /// 获取宝贝模板详情
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="pid"></param>
        /// <param name="phoneOrpc">0为pc，1为手机</param>
        /// <param name="refresh">刷新商品模板</param>
        /// <returns></returns>
        public ActionResult TempldateDetail(int templateId, int ponid, TemplateTypeFlag phoneOrpc, bool refresh = false)
        {
            var modPON = _productOrigNumberContract.View(ponid);
            var modTemp = _templateContract.Templates.FirstOrDefault(f => f.Id == templateId);
            var modAdmin = _administratorContract.View(AuthorityHelper.OperatorId.Value);

            var orgContent = modTemp.TemplateHtml;

            var dic = new Dictionary<string, object>();
            dic.Add("PONInfo", modPON);
            dic.Add("AdminInfo", modAdmin);

            var strResult = NVelocityHelper.Generate(orgContent, dic, "TempldateDetail");

            if (!refresh)
            {
                strResult = GetFashionHtml(modPON, phoneOrpc, strResult);
            }

            return Json(new { Content = strResult });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ProductDetailOnly(ProductDetail detail, TemplateTypeFlag phoneOrpc)
        {
            OperationResult res = new OperationResult(OperationResultType.Error, "更新宝贝详情失败");
            try
            {
                detail.CheckNotNull("detail");
                var modelPON = _productOrigNumberContract.View(detail.Id);

                if (phoneOrpc == TemplateTypeFlag.PC)
                    modelPON.TemplateId = detail.TemplateId;
                else
                    modelPON.TemplatePhoneId = detail.TemplatePhoneId;

                var result = CreateHtml(detail, modelPON, phoneOrpc);
                if (result.ResultType == OperationResultType.Success)
                {
                    res = _productOrigNumberContract.Update(modelPON);
                }
                else
                {
                    res = result;
                }
            }
            catch (Exception ex)
            {
                res = new OperationResult(OperationResultType.Error, "更新宝贝失败，原因：", ex.Message);
            }
            return Json(res);
        }

        public OperationResult CreateHtml(ProductDetail detail, ProductOriginNumber modPON, TemplateTypeFlag phoneOrpc)
        {
            var oResult = new OperationResult(OperationResultType.Error, "生成静态页失败！");

            try
            {
                //静态页路径
                string strConfigPath = ConfigurationHelper.GetAppSetting("ProductHtmlPath");
                DateTime current = DateTime.Now;
                string strDate = current.Year.ToString() + "/" + current.Month.ToString() + "/" + current.Day.ToString() + "/" + current.Hour.ToString() + "/";
                var modAdmin = _administratorContract.View(AuthorityHelper.OperatorId.Value);

                if (phoneOrpc == TemplateTypeFlag.PC)
                {
                    #region PC模板
                    var dicPC = new Dictionary<string, object>();
                    dicPC.Add("PONInfo", modPON);
                    dicPC.Add("AdminInfo", modAdmin);

                    var strResultPC = NVelocityHelper.Generate(detail.TemplateContent, dicPC, "productDetail_PC");

                    strResultPC = UpdateFashionHtml(modPON, phoneOrpc, strResultPC);

                    if (modPON.HtmlPath.IsNullOrEmpty())
                        modPON.HtmlPath = strConfigPath + strDate + modPON.BigProdNum + ".html";

                    bool isSave = FileHelper.SavePath(modPON.HtmlPath, strResultPC);
                    if (!isSave)
                    {
                        return oResult;
                    }
                    #endregion
                }
                else
                {
                    #region 手机模板

                    var dicPhone = new Dictionary<string, object>();
                    dicPhone.Add("PONInfo", modPON);
                    dicPhone.Add("AdminInfo", modAdmin);

                    var strResultPhone = NVelocityHelper.Generate(detail.TemplatePhoneContent, dicPhone, "productDetail_Phone");

                    strResultPhone = UpdateFashionHtml(modPON, phoneOrpc, strResultPhone);

                    if (modPON.HtmlPhonePath.IsNullOrEmpty())
                        modPON.HtmlPhonePath = strConfigPath + strDate + modPON.BigProdNum + "_phone.html";

                    var isSave = FileHelper.SavePath(modPON.HtmlPhonePath, strResultPhone);
                    if (!isSave)
                    {
                        return oResult;
                    }
                    #endregion
                }

                oResult.ResultType = OperationResultType.Success;
                oResult.Message = "";
                return oResult;
            }
            catch (Exception ex)
            {
                _log.Error<string>(ex.ToString());
                oResult.ResultType = OperationResultType.Error; oResult.Message = "生成静态页失败！程序异常";
                return oResult;
            }
        }
        /// <summary>
        /// 选择要编辑宝贝的类型页面
        /// </summary>
        /// <returns></returns>
        public ActionResult SelectPhoneOrPc(int Id)
        {
            ViewBag.PONId = Id;
            return PartialView();
        }

        private string UpdateFashionHtml(ProductOriginNumber modelPON, TemplateTypeFlag phoneOrpc, string tempContent)
        {
            if (tempContent.IsNotNullAndEmpty() && modelPON.IsNotNull())
            {
                HtmlAgilityPack.HtmlDocument hd = new HtmlAgilityPack.HtmlDocument();
                hd.LoadHtml(tempContent);

                var root = hd.DocumentNode;
                var fashonNods = root.SelectNodes("//*[@fashion]");
                if (fashonNods.IsNotNull())
                {
                    foreach (var item in fashonNods)
                    {
                        var tag = item.Attributes["fashion"];

                        #region HTML片段解析为纯内容

                        string txt = null;
                        if (tag.Value.StartsWith("img"))
                        {
                            var allimgs = item.SelectNodes("img[@src]");
                            if (allimgs.IsNotNull())
                            {
                                txt = string.Join(",", allimgs.Select(s => s.Attributes["src"].Value).Where(w => w.IsNotNullAndEmpty()));
                            }
                        }
                        else
                        {
                            txt = item.InnerText;
                            if (txt.IsNotNullAndEmpty())
                            {
                                txt = MvcHtmlString.Create(txt.Trim()).ToHtmlString();
                            }
                        }

                        #endregion

                        var modPd = modelPON.ProdcutOrigNumberProductDetails.FirstOrDefault(f => f.Mark == tag.Value && f.TemplateTypeFlag == phoneOrpc);
                        if (modPd.IsNotNull())
                        {
                            modPd.Content = item.InnerHtml;
                            modPd.Text = txt;
                        }
                        else
                        {
                            modelPON.ProdcutOrigNumberProductDetails.Add(new ProdcutOrigNumberProductDetail()
                            {
                                Mark = tag.Value,
                                Content = item.InnerHtml,
                                Text = txt,
                                TemplateTypeFlag = phoneOrpc
                            });
                        }
                    }

                    tempContent = root.OuterHtml;
                }
            }

            return tempContent;
        }

        private string GetFashionHtml(ProductOriginNumber modelPON, TemplateTypeFlag phoneOrpc, string tempContent)
        {
            if (tempContent.IsNotNullAndEmpty() && modelPON.IsNotNull())
            {
                if (modelPON.ProdcutOrigNumberProductDetails.Count > 0)
                {
                    HtmlAgilityPack.HtmlDocument hd = new HtmlAgilityPack.HtmlDocument();
                    hd.LoadHtml(tempContent);

                    var root = hd.DocumentNode;
                    var fashonNods = root.SelectNodes("//*[@fashion]");
                    if (fashonNods.IsNotNull())
                    {
                        if (phoneOrpc == TemplateTypeFlag.PC)
                        {
                            var sync = ConfigurationHelper.GetAppSetting("ProdcutDetailTemplateSync", "false").CastTo<bool>();//PC内容与手机内容同步，可能fashion=txt1 这样的标签不一致，内容错乱（慎用）
                            phoneOrpc = sync ? TemplateTypeFlag.手机 : phoneOrpc;
                        }

                        foreach (var item in fashonNods)
                        {
                            var tag = item.Attributes["fashion"];

                            var modPd = modelPON.ProdcutOrigNumberProductDetails.FirstOrDefault(f => f.Mark == tag.Value && f.TemplateTypeFlag == phoneOrpc);
                            if (modPd.IsNotNull())
                            {
                                item.InnerHtml = modPd.Content;

                                if (modPd.Content.IsNotNullAndEmpty() && modPd.Text.IsNullOrEmpty())
                                {
                                    #region HTML片段解析为纯内容

                                    string txt = null;
                                    if (tag.Value.StartsWith("img"))
                                    {
                                        var allimgs = item.SelectNodes("img[@src]");
                                        if (allimgs.IsNotNull())
                                        {
                                            txt = string.Join(",", allimgs.Select(s => s.Attributes["src"].Value).Where(w => w.IsNotNullAndEmpty()));
                                        }
                                    }
                                    else
                                    {
                                        txt = item.InnerText;
                                        if (txt.IsNotNullAndEmpty())
                                        {
                                            txt = MvcHtmlString.Create(txt.Trim()).ToHtmlString();
                                        }
                                    }

                                    #endregion

                                    modPd.Text = txt;
                                    _ProductOrigNumberProductDetailContract.Update(modPd);
                                }
                            }
                        }

                        tempContent = root.OuterHtml;
                    }
                }
            }
            return tempContent;
        }

        public JsonResult GetPrintTimeOutInfo()
        {
            //Utility.XmlHelper helper = new Utility.XmlHelper("Product", "PrintTimeOut");
            //var lastadminid = (helper.GetElement("LastAdminId")?.Value ?? "0").CastTo<int>();
            //var isstarted = (helper.GetElement("IsStarted")?.Value ?? "false").CastTo<bool>();
            var lastadminid = _configureContract.GetConfigureValue("Product", "PrintTimeOut", "LastAdminId", "0").CastTo<int>();
            var isstarted = _configureContract.GetConfigureValue("Product", "PrintTimeOut", "IsStarted", "false").CastTo<bool>();
            if (lastadminid != AuthorityHelper.OperatorId && isstarted)
            {
                //var lattime = helper.GetElement("LastTime");
                //if (lattime?.Value != null)
                //{
                //    var gapvalue = (helper.GetElement("GapValue")?.Value ?? "300").CastTo<int>();
                //    var dtlast = lattime.Value.CastTo<DateTime>();
                //    if (dtlast.AddSeconds(Math.Abs(gapvalue)) > DateTime.Now)//未超时
                //    {
                //        var TimeOut = (dtlast.AddSeconds(Math.Abs(gapvalue)) - DateTime.Now).TotalSeconds;//距超时剩于秒数
                //        var PrintLastName = helper.GetElement("LastName")?.Value;//最后打印的操作人

                //        return Json(new { Timeout = TimeOut, LastName = PrintLastName }, JsonRequestBehavior.AllowGet);
                //    }
                //}
                var lattime = _configureContract.GetConfigureValue("Product", "PrintTimeOut", "LastTime", null).CastTo<DateTime?>();
                if (lattime != null)
                {
                    var gapvalue = _configureContract.GetConfigureValue("Product", "PrintTimeOut", "GapValue", "300").CastTo<int>();
                    var dtlast = lattime.Value.CastTo<DateTime>();
                    if (dtlast.AddSeconds(Math.Abs(gapvalue)) > DateTime.Now)//未超时
                    {
                        var TimeOut = (dtlast.AddSeconds(Math.Abs(gapvalue)) - DateTime.Now).TotalSeconds;//距超时剩于秒数
                        var PrintLastName = _configureContract.GetConfigureValue("Product", "PrintTimeOut", "LastName", "");//最后打印的操作人

                        return Json(new { Timeout = TimeOut, LastName = PrintLastName }, JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SetPrintTimeOutInfo(bool started = true)
        {
            //Utility.XmlHelper helper = new Utility.XmlHelper("Product", "PrintTimeOut");
            //helper.ModifyElement("IsStarted", started.ToString());
            //helper.ModifyElement("LastAdminId", (AuthorityHelper.OperatorId ?? 0).ToString());
            //helper.ModifyElement("LastTime", DateTime.Now.ToString());
            //helper.ModifyElement("LastName", AuthorityHelper.RealName);
            //helper.Save();

            _configureContract.SetConfigure("Product", "PrintTimeOut", "IsStarted", started.ToString());
            _configureContract.SetConfigure("Product", "PrintTimeOut", "LastAdminId", (AuthorityHelper.OperatorId ?? 0).ToString());
            _configureContract.SetConfigure("Product", "PrintTimeOut", "LastTime", DateTime.Now.ToString());
            _configureContract.SetConfigure("Product", "PrintTimeOut", "LastName", AuthorityHelper.RealName);
            return Json(new OperationResult(OperationResultType.Success));
        }

        /// <summary>
        /// 设置每件商品的打印件数
        /// </summary>
        /// <returns></returns>
        public ActionResult SetPrintCount(bool jumpPrinter = false)
        {
            ViewBag.jumpPrinter = jumpPrinter;
            return PartialView();
        }
    }

    public partial class SampleController : BaseController
    {
        [Layout]
        public ActionResult SampleInventory()
        {
            ViewBag.Color = _colorContract.ParentSelectList("请选择");
            ViewBag.Category = CacheAccess.GetCategory(_categoryContract, true);
            return View();
        }

        public async Task<ActionResult> SampleInventoryList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<ProductOriginNumber, bool>> predicate = FilterHelper.GetExpression<ProductOriginNumber>(request.FilterGroup);
            var OperatorId = AuthorityHelper.OperatorId.Value;
            var data = await Task.Run(() =>
            {
                var count = 0;
                var listdesignerids = _DesignerContract.SelectDesigner.Where(w => w.IsEnabled && !w.IsDeleted && w.AdminId == OperatorId).Select(s => s.Id).ToList();
                var query = _productOrigNumberContract.OrigNumbs.Where(w => w.IsEnabled && !w.IsDeleted && w.DesignerId.HasValue && listdesignerids.Contains(w.DesignerId.Value));

                var list = (from s in query.Where<ProductOriginNumber, int>(predicate, request.PageCondition, out count)
                            let proids = s.Products.Where(w => w.IsEnabled && !w.IsDeleted).Select(ss => ss.Id)
                            let invcount = _inventoryContract.Inventorys.Count(w => w.IsEnabled && !w.IsDeleted && proids.Contains(w.ProductId) && !w.IsLock && w.Status == (int)InventoryStatus.Default)
                            select new
                            {
                                s.Id,
                                s.ThumbnailPath,
                                s.ProductName,
                                s.BigProdNum,
                                InvCount = invcount

                            }).ToList();

                return new GridData<object>(list, count, request.RequestInfo);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult InventoryInfo()
        {
            return PartialView();
        }
        /// <summary>
        /// 获取指定款号颜色尺码和库存数
        /// </summary>
        /// <param name="BigProdNum"></param>
        /// <returns></returns>
        public JsonResult GetInventoryInfo(string BigProdNum)
        {
            var query = _productOrigNumberContract.OrigNumbs.Where(w => w.IsEnabled && !w.IsDeleted && w.BigProdNum == BigProdNum && w.Designer.AdminId == AuthorityHelper.OperatorId.Value);
            var queryinv = _inventoryContract.Inventorys.Where(w => w.IsEnabled && !w.IsDeleted && !w.IsLock && w.Status == (int)InventoryStatus.Default);
            var data = (from s in query.SelectMany(s => s.Products.Where(w => w.IsEnabled && !w.IsDeleted))
                        let invcount = queryinv.Count(w => w.ProductId == s.Id)
                        select new
                        {
                            s.Color.ColorName,
                            s.Size.SizeName,
                            s.Color.IconPath,
                            InvCount = invcount,

                        }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}