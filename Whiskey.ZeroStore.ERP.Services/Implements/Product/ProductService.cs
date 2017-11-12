


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Linq.Expressions;
using System.Web;
using System.Globalization;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Web.SignalR;
using Whiskey.Web.Http;
using Whiskey.Web.Extensions;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models.Entities.Properties;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Class;
using System.Text.RegularExpressions;
using System.ComponentModel.Composition;
using System.IO;
using Whiskey.Utility.Logging;
using XKMath36;
using System.Diagnostics;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Factory;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{

    [Export(typeof(IProductContract))]
    public class ProductService : ServiceBase, IProductContract
    {
        #region ProductService

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ProductService));
        private readonly IRepository<Product, int> _productRepository;
        private readonly IRepository<Brand, int> _brandRepository;
        private readonly IRepository<Category, int> _categoryRepository;
        private readonly IRepository<Color, int> _colorRepository;
        private readonly IRepository<ProductImage, int> _imageRepository;
        private readonly IRepository<Season, int> _seasonRepository;
        private readonly IRepository<Size, int> _sizeRepository;
        private readonly IRepository<ProductAttribute, int> _productAttributeRepository;

        private readonly ICategoryContract _categoryContract;
        private readonly IRepository<MaintainExtend, int> _maintainRepository;
        private readonly IRepository<ProductOperationLog, int> _productlogRepository;

        private readonly IRepository<Inventory, int> _inventoryRepository;
        private readonly IRepository<Store, int> _storeRepository;

        private readonly IRepository<StoreRecommend, int> _storeRecommendRepository;

        private readonly IRepository<Storage, int> _storageRepository;
        private readonly IRepository<ProductOriginNumber, int> _productOriginNumberRepo;
        public ProductService(
            IRepository<Product, int> productRepository,
            IRepository<Brand, int> brandRepository,
            IRepository<Category, int> categoryRepository,
            IRepository<Color, int> colorRepository,
            IRepository<ProductImage, int> imageRepository,
            IRepository<Season, int> seasonRepository,
            IRepository<Size, int> sizeRepository,
            IRepository<ProductAttribute, int> productAttributeRepository,
            ICategoryContract categoryContract,
            IRepository<MaintainExtend, int> maintainRepository,
            IRepository<ProductOperationLog, int> productlogRepository,
            IRepository<Inventory, int> inventoryRepository,
            IRepository<Store, int> storeRepository,
            IRepository<StoreRecommend, int> storeRecommendRepository,
            IRepository<Storage, int> storageRepository,
            IRepository<ProductOriginNumber, int> productOriginNumberRepo
        )
            : base(productRepository.UnitOfWork)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _colorRepository = colorRepository;
            _imageRepository = imageRepository;
            _seasonRepository = seasonRepository;
            _sizeRepository = sizeRepository;
            _productAttributeRepository = productAttributeRepository;
            _categoryContract = categoryContract;
            _maintainRepository = maintainRepository;
            _productlogRepository = productlogRepository;
            _inventoryRepository = inventoryRepository;
            _storeRepository = storeRepository;
            _storeRecommendRepository = storeRecommendRepository;
            _storageRepository = storageRepository;
            _productOriginNumberRepo = productOriginNumberRepo;
        }

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Product View(int Id)
        {
            var entity = _productRepository.GetByKey(Id);
            return entity;
        }


        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ProductDto Edit(int Id)
        {
            var entity = _productRepository.GetByKey(Id);

            Mapper.CreateMap<Product, ProductDto>();

            var dto = Mapper.Map<Product, ProductDto>(entity);

            return dto;
        }


        /// <summary>
        /// 获取查询数据集
        /// </summary>
        public IQueryable<Product> Products { get { return _productRepository.Entities; } }


        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params ProductDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _productRepository.Insert(dtos,
                dto =>
                {
                },
                (dto, entity) =>
                {

                    if (dto.OriginalPath != null && dto.OriginalPath.Length > 0)
                    {
                        string exten = Path.GetExtension(dto.OriginalPath);
                        string imgtype = exten.Substring(1);
                        var thumbnailPath = EnvironmentHelper.ProductPath + "/p_s_" + DateTime.Now.ToString("HHmmssffff", DateTimeFormatInfo.InvariantInfo) + exten;
                        ImageHelper.MakeThumbnail(dto.OriginalPath, thumbnailPath, 204, 325, "W", imgtype);
                        entity.ThumbnailPath = thumbnailPath;
                    }

                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;

                    return entity;
                });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params ProductDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _productRepository.Update(dtos,
                    dto =>
                    {
                    },
                    (dto, entity) =>
                    {

                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;

                        //记录日志到数据库
                        entity.ProductOperationLogs.Add(new ProductOperationLog()
                        {
                            ProductNumber = entity.ProductNumber,
                            Description = "修改商品",
                            OperatorId = AuthorityHelper.OperatorId
                        });
                        return entity;
                    });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message, ex.ToString());
            }
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="products"></param>
        /// <param name="istrans">开启事务</param>
        /// <param name="optionDescr">操作日志描述</param>
        /// <returns></returns>
        public OperationResult Update(Product[] products, bool istrans, string optionDescr = null)
        {
            XKMath36.Math36 math = new Math36();
            products.Each(c =>
            {
                c.UpdatedTime = DateTime.Now;
                c.OperatorId = AuthorityHelper.OperatorId;

                c.ProductOperationLogs.Add(new ProductOperationLog()
                {
                    ProductNumber = c.ProductNumber,
                    ProductBarcode = "",
                    Description = optionDescr.IsNotNullAndEmpty() ? optionDescr : "修改商品",
                    OperatorId = AuthorityHelper.OperatorId
                });
            });
            _productRepository.UnitOfWork.TransactionEnabled = istrans;
            return _productRepository.Update(products);
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="ids">要移除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Remove(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _productRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    entity.ProductOperationLogs.Add(new ProductOperationLog()
                    {
                        ProductNumber = entity.ProductNumber,
                        ProductBarcode = "",
                        Description = "逻辑删除数据",
                        OperatorId = AuthorityHelper.OperatorId
                    });
                    _productRepository.Update(entity);

                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message, ex.ToString());
            }
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids">要删除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _productRepository.Entities.Where(m => ids.Contains(m.Id));
                var images = new List<ProductImage>();
                foreach (var entity in entities)
                {
                    entity.ProductOriginNumber.ProductAttributes.Clear();
                    if (entity.ProductImages != null)
                    {
                        foreach (var image in entity.ProductImages)
                        {
                            if (image != null)
                            {
                                images.Add(image);
                            }
                        }
                    }
                    _productlogRepository.Insert(new ProductOperationLog()
                    {
                        ProductNumber = entity.ProductNumber,
                        ProductBarcode = "",
                        Description = "物理删除数据",
                        OperatorId = AuthorityHelper.OperatorId

                    });
                    _imageRepository.Delete(images);
                    _productRepository.Delete(entity);
                }

                foreach (var image in images)
                {
                    FileHelper.Delete(image.OriginalPath);
                    FileHelper.Delete(image.ThumbnailLargePath);
                    FileHelper.Delete(image.ThumbnailSmallPath);
                }

                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "删除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message, ex.ToString());
            }

        }

        public void Update(IQueryable<Product> products)
        {
            try
            {
                List<Product> list = new List<Product>();
                list = products.ToList();
                foreach (var item in list)
                {
                    item.ProductOperationLogs.Add(new ProductOperationLog()
                    {
                        ProductNumber = item.ProductNumber,
                        ProductBarcode = "",
                        Description = "修改商品",
                        OperatorId = AuthorityHelper.OperatorId
                    });
                    item.ProductOriginNumber.AssistantNumberOfInt = Convert.ToInt32(item.ProductOriginNumber.AssistantNum);
                    _productRepository.Update(item);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="ids">要恢复的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _productRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    entity.ProductOperationLogs.Add(new ProductOperationLog()
                    {
                        ProductNumber = entity.ProductNumber,
                        ProductBarcode = "",
                        OperatorId = AuthorityHelper.OperatorId,
                        Description = "将商品档案从删除状态恢复"
                    });
                    _productRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="ids">要启用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Enable(params int[] ids)
        {

            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _productRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    entity.ProductOperationLogs.Add(new ProductOperationLog()
                    {
                        ProductNumber = entity.ProductNumber,
                        ProductBarcode = "",
                        OperatorId = AuthorityHelper.OperatorId,
                        Description = "将商品档案从禁用状态恢复"
                    });
                    _productRepository.Update(entity);
                }

                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }


        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="ids">要禁用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Disable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _productRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    entity.ProductOperationLogs.Add(new ProductOperationLog()
                    {
                        ProductNumber = entity.ProductNumber,
                        ProductBarcode = "",
                        OperatorId = AuthorityHelper.OperatorId,
                        Description = "将商品档案禁用"
                    });

                    _productRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }


        /// <summary>
        /// 审核数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public OperationResult Verify(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _productRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.ProductOriginNumber.IsVerified = CheckStatusFlag.通过;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;

                    entity.ProductOperationLogs.Add(new ProductOperationLog()
                    {
                        ProductNumber = entity.ProductNumber,
                        ProductBarcode = "",
                        OperatorId = AuthorityHelper.OperatorId,
                        Description = "商品档案审核通过"
                    });
                    _productRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "审核成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "审核失败！错误如下：" + ex.Message, ex.ToString());
            }
        }


        /// <summary>
        /// 驳回数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public OperationResult Reject(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _productRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.ProductOriginNumber.IsVerified = CheckStatusFlag.未通过;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _productRepository.Update(entity);
                    entity.ProductOperationLogs.Add(new ProductOperationLog()
                    {
                        ProductNumber = entity.ProductNumber,
                        ProductBarcode = "",
                        OperatorId = AuthorityHelper.OperatorId,
                        Description = "商品档案审核驳回"
                    });
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "驳回审核成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "驳回审核失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<Product, bool>> predicate, int id = 0)
        {
            return _productRepository.ExistsCheck(predicate, id);
        }

        //yxk 2015-10-改写
        /// <summary>
        /// 获取商品货号
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        //public string GetProductNumber(ProductDto dto)
        //{
        //    var result = string.Empty;
        //    if (dto != null)
        //    {

        //        var brandNum = _brandRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == dto.BrandId).Select(c => c.BrandCode).FirstOrDefault();

        //        var categoryNum = _categoryRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == dto.CategoryId).Select(c => c.CategoryCode).FirstOrDefault();
        //        if (categoryNum.Length < 3)
        //        {
        //            categoryNum = new string('0', 3 - categoryNum.Length) + categoryNum;

        //        }

        //        var size = _sizeRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == dto.SizeId).Select(c => c.SizeName).FirstOrDefault();

        //        string sizeNum = GetShowSizeNum(size);
        //        var seasonNum = _seasonRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == dto.SeasonId).Select(c => c.SeasonCode).FirstOrDefault();


        //        //result = brandNum + categoryNum + seasonNum + sizeNum;

        //    }
        //    //var category = _categoryContract.FullCode(dto.CategoryId, 2);
        //    //var brand = _brandRepository.GetByKey(dto.BrandId);
        //    //var size = _sizeRepository.GetByKey(dto.SizeId);
        //    //var season = _seasonRepository.GetByKey(dto.SeasonId);
        //    //var categoryCode = category != null && category.Length > 0 ? category : "品类编码错误";
        //    //var brandCode = brand != null && brand.BrandCode.Length > 0 ? brand.BrandCode : "品牌编码错误";
        //    //var sizeCode = size != null && size.SizeCode.Length > 0 ? size.SizeCode : "尺码编码错误";
        //    //var seasonCode = season != null && season.SeasonCode.Length > 0 ? season.SeasonCode : "季节编码错误";
        //    //var product = _productRepository.Entities.Where(m => m.CategoryId == dto.CategoryId && m.BrandId == dto.BrandId && m.SizeId == dto.SizeId && m.SeasonId == dto.SeasonId);
        //    //var counter = 0;
        //    //if (product != null)
        //    //{
        //    //    counter = product.Count() + 1;
        //    //}
        //    //result = (categoryCode + brandCode + seasonCode + sizeCode + counter.ToString().PadLeft(2, '0')).ToUpper();
        //    //00+00+0000+0+00+00

        //    return null;
        //}

        private string GetShowSizeNum(string size)
        {
            if (!string.IsNullOrEmpty(size))
            {

                string res = "";
                size = size.Trim();

                if (size.EndsWith("+"))
                {
                    res = size.Substring(0, size.Length - 1) + "P";
                }
                else if (size.EndsWith("-"))
                {
                    res = size.Substring(0, size.Length - 1) + "C";
                }
                else
                    res = size;
                return res.ToUpper();
            }
            return "";
        }

        #endregion

        /// <summary>
        /// 网址链接
        /// </summary>
        private readonly string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");
        //yxk 2015-9-10
        /// <summary>
        /// 查询商品信息
        /// </summary>
        /// <param name="title"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public List<Values<string, string>> Selectlist(string title, Expression<Func<Product, bool>> exp)
        {
            var list = (_productRepository.Entities.Where(exp).Select(c => new Values<string, string> { Key = c.Id.ToString(), Value = c.ProductName, IsEnabled = c.IsEnabled, IsDeleted = c.IsDeleted })).ToList();
            if (list.Count != 0 && title != null && title != "")
            {
                list.Insert(0, new Values<string, string> { Key = "", Value = title });

            }
            return list;

        }

        //yxk 2015-12-15
        /// <summary>
        /// 根据商品编号或者ID获取商品信息
        /// </summary>
        /// <param name="IdOrNum"></param>
        /// <returns></returns>
        public ProductDto GetProductByNumber(string IdOrNum)
        {
            ProductDto prodto = null;
            Regex rg = new Regex(@"^\d+$");
            if (rg.IsMatch(IdOrNum))
            {
                int _id = Convert.ToInt32(IdOrNum);
                prodto = Edit(_id);
            }
            else
            {
                var entity = _productRepository.Entities.Where(c => c.ProductNumber == IdOrNum && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
                if (entity != null)
                {
                    var dto = Mapper.Map<Product, ProductDto>(entity);

                    prodto = dto;
                }

            }
            return prodto;
        }


        #region 获取同货号下的商品
        /// <summary>
        /// 获取指定款号的颜色和尺码
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="bigProdNum"></param>
        /// <returns></returns>
        public OperationResult GetProductColorSize(string bigProdNum)
        {
            try
            {
                var productQuery = _productRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled)
                                                              .Where(x => x.ProductOriginNumber.BigProdNum == bigProdNum)
                                                              .Where(x => x.ProductOriginNumber.IsRecommend.Value == true);
                if (!productQuery.Any())
                {
                    return OperationResult.Error("没有数据");
                }

                // 按颜色分组,展示每个颜色都有那些尺码

                var res = productQuery.Select(p => new
                {
                    p.Id,
                    p.ProductNumber,
                    p.Color.IconPath,
                    p.Color.ColorName,
                    p.Size.SizeName,
                    ThumbnailPath = p.ThumbnailPath ?? p.ProductOriginNumber.ThumbnailPath
                }).ToList()
                .Select(p => new
                {
                    ProductId = p.Id,
                    ProductNumber = p.ProductNumber,
                    ColorName = p.ColorName,
                    ColorPath = strWebUrl + p.IconPath,
                    SizeName = p.SizeName,
                    ThumbnailPath = string.IsNullOrEmpty(p.ThumbnailPath) ? string.Empty : strWebUrl + p.ThumbnailPath
                }).ToList();


                return new OperationResult(OperationResultType.Success, string.Empty, res);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试");
            }
        }
        #endregion
        /// <summary>
        /// 商品条码打印
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        public OperationResult PrintProductCode(params Product[] products)
        {
            products.Each(c =>
            {
                c.UpdatedTime = DateTime.Now;
                c.OperatorId = AuthorityHelper.OperatorId;
                //if (c.ProductLogFlag == "")
                //    c.ProductLogFlag = Guid.NewGuid().ToString().Replace("-", "");
                c.BarcodePrintCount = c.ProductBarcodeDetails.Count;
                // c.AssistantNumberOfInt = Convert.ToInt32(c.AssistantNumber);
                c.ProductOperationLogs.Add(new ProductOperationLog()
                {
                    ProductNumber = c.ProductNumber,
                    ProductBarcode = "",
                    Description = "打印商品条码",
                    OperatorId = AuthorityHelper.OperatorId

                });
            });
            return _productRepository.Update(products);
        }

        /// <summary>
        /// 根据商品折扣更新商品价格
        /// </summary>
        /// <param name="discount"></param>
        /// <param name="productIds"></param>
        /// <param name="isTrans"></param>
        /// <returns></returns>
        public OperationResult UpdatePriceByDiscount(ProductDiscount discount, int[] productIds, bool isTrans)
        {
            var products =
                _productRepository.Entities.Where(c => productIds.Contains(c.Id) && c.IsEnabled && !c.IsDeleted);
            if (products.Any())
            {
                products.Each(c =>
                {
                    c.ProductOriginNumber.WholesalePrice = c.ProductOriginNumber.TagPrice * discount.WholesaleDiscount / 10;
                    c.ProductOriginNumber.PurchasePrice = c.ProductOriginNumber.TagPrice * discount.PurchaseDiscount / 10;
                    c.ProductOperationLogs.Add(new ProductOperationLog()
                    {
                        CreatedTime = DateTime.Now,
                        Description = "根据折扣更新商品价格",
                        OperatorId = AuthorityHelper.OperatorId,
                        IsDeleted = false,
                        IsEnabled = true,
                        LogFlag = Guid.NewGuid().ToString().Replace("-", "")

                    });
                });
            }
            _productRepository.UnitOfWork.TransactionEnabled = isTrans;
            return _productRepository.Update(products.ToArray());
        }

        #region Image 重命名
        public OperationResult Rename(int PageIndex, int PageSize)
        {
            try
            {
                //UnitOfWork.TransactionEnabled = true;
                IQueryable<Product> listPro = _productRepository.Entities.OrderBy(x => x.CreatedTime);
                List<Product> list = listPro.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
                int skipCount = (PageIndex - 1) * PageSize;
                int totalCount = listPro.Count();
                if (skipCount >= totalCount)
                {
                    return new OperationResult(OperationResultType.QueryNull);
                }
                DateTime current = DateTime.Now;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                foreach (Product pro in list)
                {
                    pro.OriginalPath = FileHelper.Rename(pro.OriginalPath);
                    pro.ThumbnailPath = FileHelper.Rename(pro.ThumbnailPath);
                    pro.ProductCollocationImg = FileHelper.Rename(pro.ProductCollocationImg);
                    pro.UpdatedTime = current;
                    foreach (ProductImage image in pro.ProductImages)
                    {
                        image.OriginalPath = FileHelper.Rename(image.OriginalPath);
                        image.ThumbnailSmallPath = FileHelper.Rename(image.ThumbnailSmallPath);
                        image.ThumbnailMediumPath = FileHelper.Rename(image.ThumbnailMediumPath);
                        image.ThumbnailLargePath = FileHelper.Rename(image.ThumbnailLargePath);
                        image.UpdatedTime = current;
                    }
                }
                stopwatch.Stop();
                TimeSpan timespan = stopwatch.Elapsed;
                double d = timespan.TotalSeconds;
                _productRepository.Update(list);
                int count = 0;
                if (count > 0)
                {
                    return new OperationResult(OperationResultType.Success);
                }
                else
                {
                    return new OperationResult(OperationResultType.Error);
                }
            }
            catch (Exception)
            {
                throw;
            }

        }


        #endregion
        public OperationResult Publish(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _productRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.ProductOriginNumber.IsPublish = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;

                    entity.ProductOperationLogs.Add(new ProductOperationLog()
                    {
                        ProductNumber = entity.ProductNumber,
                        ProductBarcode = "",
                        OperatorId = AuthorityHelper.OperatorId,
                        Description = "商品档案已发布,等待审核"
                    });
                    _productRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "发布成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "发布失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        /// <summary>
        /// 获取商城搭配素材(API用)
        /// </summary>
        /// <param name="PageIndex">页码</param>
        /// <param name="PageSize">分页大小</param>
        /// <returns></returns>
        public List<ColloProductInfo> GetStoreCollocationMaterials(int PageIndex = 1, int PageSize = 10)
        {
            var listProduct = _productRepository.Entities
                .Where(x => x.IsEnabled && !x.IsDeleted)
                .Where(x => x.ProductOriginNumber.IsRecommend.Value == true)
                .OrderByDescending(x => x.UpdatedTime)
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize);
            var list = listProduct.Select(x => new ColloProductInfo
            {
                ProductId = x.Id,
                CategoryName = x.ProductOriginNumber.Category.CategoryName,
                Price = x.ProductOriginNumber.TagPrice,
                SeasonName = x.ProductOriginNumber.Season.SeasonName,
                CoverImagePath = strWebUrl + (x.ProductCollocationImg ?? x.ThumbnailPath)
            }).ToList();
            return list;
        }
    }
}
