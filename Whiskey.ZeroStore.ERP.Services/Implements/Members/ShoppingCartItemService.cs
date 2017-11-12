using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ShoppingCartItemService : ServiceBase, IShoppingCartItemContract
    {
        IRepository<ShoppingCartItem, int> _repo;
        protected readonly IMemberContract _memberContract;
        protected readonly IProductContract _productContract;
        protected readonly IBrandContract _brandContract;
        protected readonly ISalesCampaignContract _salesCampaignContract;
        public ShoppingCartItemService(IMemberContract memberContract,
            IProductContract productContract,
            IBrandContract brandContract,
            ISalesCampaignContract salesCampaignContract,
            IRepository<ShoppingCartItem, int> repo) : base(repo.UnitOfWork)
        {
            _memberContract = memberContract;
            _productContract = productContract;
            _brandContract = brandContract;
            _salesCampaignContract = salesCampaignContract;
            _repo = repo;
        }
        public IQueryable<ShoppingCartItem> Entities => _repo.Entities.Where(e => !e.IsDeleted && e.IsEnabled);

        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                var cnt = _repo.Delete(s => ids.Contains(s.Id));

                return cnt > 0 ? new OperationResult(OperationResultType.Success, "删除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public List<ShoppingCartEntry> GetItems(int memberId)
        {
            var memberQuery = _memberContract.Members.Where(m => !m.IsDeleted && m.IsEnabled && m.Id == memberId);
            if (!memberQuery.Any())
            {
                throw new Exception("会员不存在");
            }
            var storeId = memberQuery.Select(m => m.StoreId).FirstOrDefault();
            if (!storeId.HasValue)
            {
                storeId = 14;
            }
            var list = _repo.Entities.Where(s => s.MemberId == memberId)
                                                         .Select(s => new
                                                         {
                                                             s.Id,
                                                             s.ProductId,
                                                             s.ProductNumber,
                                                             s.Quantity,
                                                             ThumbnailPath = WebUrl + s.Product.ThumbnailPath ?? s.Product.ProductOriginNumber.ThumbnailPath,
                                                             s.CreatedTime,
                                                             s.Product.ProductOriginNumber.TagPrice,
                                                             s.Product.ProductOriginNumber.Brand.BrandName,
                                                             s.Product.ProductOriginNumber.Category.CategoryName,
                                                             s.Product.Color.ColorName,
                                                             s.Product.Size.SizeName
                                                         }).ToList()
                                                         .Select(s => new ShoppingCartEntry
                                                         {
                                                             Id = s.Id,
                                                             ProductId = s.ProductId,
                                                             ProductNumber = s.ProductNumber,
                                                             Quantity = s.Quantity,
                                                             ThumbnailPath = s.ThumbnailPath,
                                                             TagPrice = (decimal)s.TagPrice,
                                                             RetailPrice = (decimal)s.TagPrice,
                                                             BrandName = s.BrandName,
                                                             ColorName = s.ColorName,
                                                             SizeName = s.SizeName,
                                                             CategoryName = s.CategoryName,
                                                             CreatedTime = s.CreatedTime.ToUnixTime().ToString()
                                                         }).ToList();
            // 计算零售价
            var priceDict = GetRetailPrice(storeId.Value, list.Select(p => p.ProductNumber).ToArray());
            list.ForEach(s =>
            {
                s.RetailPrice = priceDict[s.ProductNumber].RetailPrice;
                s.SaleCampId = priceDict[s.ProductNumber].SaleCampId;
                s.BrandDiscountId = priceDict[s.ProductNumber].BrandDiscountId;
            });
            return list;
        }
        private IQueryable<SalesCampaign> GetAvailableSalesCampaignsByStore(int storeId)
        {
            if (storeId <= 0)
            {
                throw new Exception("参数异常");
            }
            var date = DateTime.Now;
            return _salesCampaignContract.SalesCampaigns.Where(s => !s.IsDeleted && s.IsEnabled)
                                                        .Where(s => s.SalesCampaignType == SalesCampaignType.EveryOne || s.SalesCampaignType == SalesCampaignType.MemberOnly) //参与类型
                                                        .Where(s => s.CampaignStartTime <= date && s.CampaignEndTime >= date) // 有效期过滤
                                                        .Where(s => s.StoresIds.Contains(storeId.ToString()));                // 参与店铺过滤


        }

        /// <summary>
        /// 获取店铺下商品零售价(针对会员)
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="productNumbers"></param>
        /// <returns></returns>
        private Dictionary<string, ProdutDictEntry> GetRetailPrice(int storeId, params string[] productNumbers)
        {
            try
            {
                // 去除重复
                var productNumberList = productNumbers.Distinct().ToList();

                // 商品品牌折扣字典
                var brandDiscountDic = _brandContract.Brands.Where(b => !b.IsDeleted && b.IsEnabled)
                                                            .Select(b => new { b.BrandName, b.DefaultDiscount, b.Id })
                                                            .ToDictionary(b => b.BrandName, b => new
                                                            {
                                                                b.Id,
                                                                b.DefaultDiscount
                                                            });

                // 商品吊牌价字典
                var products = _productContract.Products.Where(p => !p.IsDeleted && p.IsEnabled && productNumbers.Contains(p.ProductNumber))

                    .Select(p => new
                    {
                        p.ProductNumber,
                        p.ProductOriginNumber.TagPrice,
                        p.ProductOriginNumber.BigProdNum,
                        p.ProductOriginNumber.Brand.BrandName,
                    }).ToList()
                    .Select(p => new ProdutDictEntry
                    {
                        ProductNumber = p.ProductNumber,
                        TagPrice = (decimal)p.TagPrice,
                        RetailPrice = (decimal)p.TagPrice,
                        BigProdNum = p.BigProdNum,
                        BrandName = p.BrandName,
                    }).ToDictionary(p => p.ProductNumber);

                if (productNumbers.Except(products.Keys).Any())
                {
                    throw new Exception($"未找到以下商品信息{string.Join(",", productNumbers.Intersect(products.Keys))}");
                }

                // 获取店铺下所有商品活动
                var storeSalesCampaignList = GetAvailableSalesCampaignsByStore(storeId).Select(s => new
                {
                    s.Id,
                    s.CampaignName,
                    s.MemberDiscount,
                    BigProdNums = s.ProductOriginNumbers.Select(o => o.BigProdNum)
                }).ToList();


                // 商品零售价字典
                var retailPriceDict = new Dictionary<string, decimal>();


                // 计算商品零售价 规则：优先级： 店铺活动优惠>品牌折扣>吊牌价
                foreach (var productItem in products)
                {

                    // 根据款号查找是否有商品活动,有则找到折扣力度最大的那条
                    var saleCamp = storeSalesCampaignList.Where(s => s.BigProdNums.Contains(productItem.Value.BigProdNum)).OrderByDescending(s => s.MemberDiscount).FirstOrDefault();

                    if (saleCamp != null)
                    {
                        if (saleCamp.MemberDiscount < 0)
                        {
                            throw new Exception($"商品折扣异常productNumber:{productItem.Key},discount:{saleCamp.MemberDiscount}");
                        }
                        var retailPrice = productItem.Value.TagPrice * (decimal)(saleCamp.MemberDiscount / 10);
                        productItem.Value.RetailPrice = retailPrice;
                        productItem.Value.SaleCampId = saleCamp.Id;
                    }
                    else if (brandDiscountDic.ContainsKey(productItem.Value.BrandName))   //无活动,则查找品牌折扣
                    {

                        {
                            var brandDiscount = brandDiscountDic[productItem.Value.BrandName];
                            if (brandDiscount.DefaultDiscount < 0)
                            {
                                throw new Exception($"品牌折扣异常brandName:{productItem.Value.BrandName},discount:{brandDiscount.DefaultDiscount}");
                            }
                            var retailPrice = productItem.Value.TagPrice * (decimal)brandDiscount.DefaultDiscount;
                            productItem.Value.RetailPrice = retailPrice;
                            productItem.Value.BrandDiscountId = brandDiscount.Id;
                        }
                    }


                }


                return products.ToDictionary(p => p.Key, p => p.Value);
            }
            catch (Exception)
            {

                throw;
            }




        }
        public OperationResult Insert(params ShoppingCartItem[] entities)
        {
            try
            {

                return _repo.Insert(entities.AsEnumerable()) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
            }
            catch (Exception ex)
            {

                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }

        public OperationResult Replace(int memberId, ShoppingCartUpdateDto[] dtos)
        {

            using (var transaction = _repo.GetTransaction())
            {

                try
                {
                    var historyItems = _repo.Entities.Where(r => r.MemberId == memberId).ToList();
                    if (historyItems.Any())
                    {
                        _repo.Delete(historyItems);
                    }
                    if (dtos.Any())
                    {
                        var entities = dtos.Select(i => new ShoppingCartItem()
                        {
                            ProductId = i.ProductId,
                            ProductNumber = i.ProductNumber,
                            MemberId = memberId,
                            Quantity = i.Quantity
                        });
                        _repo.Insert(entities);
                    }

                    transaction.Commit();
                    return OperationResult.OK();

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
                }

            }




        }


        public OperationResult UpdateQuantity(int memberId, string productNumber, int quantity)
        {
            using (var transaction = _repo.GetTransaction())
            {

                try
                {
                    var historyItems = _repo.Entities.Where(r => r.MemberId == memberId && r.ProductNumber == productNumber).ToList();

                    historyItems.ForEach(i => i.Quantity = quantity);

                    _repo.Update(historyItems);

                    transaction.Commit();
                    return OperationResult.OK();

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
                }

            }
        }

        public OperationResult AddItem(int memberId, params ShoppingCartUpdateDto[] dtos)
        {
            if (dtos == null || dtos.Length <= 0)
            {
                return new OperationResult(OperationResultType.Error, "数据不能为空");
            }
            using (var transaction = _repo.GetTransaction())
            {
                try
                {

                    if (dtos.GroupBy(r => r.ProductNumber).Any(g => g.Count() > 1))
                    {
                        return new OperationResult(OperationResultType.Error, "数据有重复项");
                    }
                    var productNumbers = dtos.Select(d => d.ProductNumber).ToList(); ;
                    var historyItems = _repo.Entities.Where(r => r.MemberId == memberId && productNumbers.Contains(r.ProductNumber)).ToList();
                    if (historyItems.Any())
                    {
                        //之前已添加过,直接修改数量

                        historyItems.ForEach(i => i.Quantity += dtos.FirstOrDefault(d => d.ProductNumber == i.ProductNumber).Quantity);
                        _repo.Update(historyItems);
                    }
                    else //未添加过
                    {
                        var notExistNumbers = productNumbers.Except(historyItems.Select(s => s.ProductNumber));
                        var entities = dtos.Where(d => notExistNumbers.Contains(d.ProductNumber)).Select(i => new ShoppingCartItem()
                        {
                            MemberId = memberId,
                            ProductId = i.ProductId,
                            ProductNumber = i.ProductNumber,
                            Quantity = i.Quantity
                        });
                        _repo.Insert(entities);
                    }
                    transaction.Commit();
                    return OperationResult.OK();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
                }
            }
        }

        public OperationResult DelItem(int memberId, params string[] productNumber)
        {
            try
            {
                var historyItems = _repo.Entities.Where(r => r.MemberId == memberId && productNumber.Contains(r.ProductNumber));
                if (!historyItems.Any())
                {
                    return OperationResult.OK();
                }
                _repo.Delete(historyItems.ToArray());
                return OperationResult.OK();

            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
            }
        }

        public OperationResult ClearItem(int memberId)
        {
            try
            {
                var items = _repo.Entities.Where(s => s.MemberId == memberId).ToList();
                if (items == null || items.Count <= 0)
                {
                    return OperationResult.OK();
                }

                _repo.Delete(items);
                return OperationResult.OK();
            }
            catch (Exception e)
            {
                return OperationResult.Error(e.Message);
            }

        }

    }
}

