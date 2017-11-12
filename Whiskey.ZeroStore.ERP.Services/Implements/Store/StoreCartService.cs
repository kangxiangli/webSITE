using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility;
using Whiskey.Web.Helper;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Stores;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class StoreCartService : ServiceBase, IStoreCartContract
    {
        #region 声明数据层操作对象

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(StoreCartService));

        protected readonly IRepository<StoreCart, int> _storeCartRepository;

        protected readonly IRepository<StoreCartItem, int> _storeCartItemRepository;

        protected readonly IRepository<Product, int> _productRepository;

        protected readonly IRepository<Purchase, int> _purchaseRepository;

        protected readonly IRepository<Storage, int> _storageRepository;
        protected readonly IRepository<Store, int> _storeRepository;
        protected readonly IRepository<StoreLevel, int> _storeLevelRepository;
        protected readonly IRepository<StoreDeposit, int> _storeDepositRepository;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IStorageContract _storageContract;
        public StoreCartService(IRepository<StoreCart, int> storeCartRepository,
            IRepository<StoreCartItem, int> storeCartItemRepository,
            IRepository<Product, int> productRepository,
            IRepository<Purchase, int> purchaseRepository,
            IRepository<Store, int> _storeRepository,
            IRepository<StoreLevel, int> _storeLevelRepository,
            IRepository<StoreDeposit, int> _storeDepositRepository,
            IAdministratorContract _administratorContract,
            IStorageContract _storageContract,
            IRepository<Storage, int> storageRepository)
            : base(storeCartRepository.UnitOfWork)
		{
            _storeCartRepository = storeCartRepository;
            _storeCartItemRepository = storeCartItemRepository;
            _productRepository = productRepository;
            _purchaseRepository = purchaseRepository;
            _storageRepository = storageRepository;
            this._storeRepository = _storeRepository;
            this._storeLevelRepository = _storeLevelRepository;
            this._storeDepositRepository = _storeDepositRepository;
            this._administratorContract = _administratorContract;
            this._storageContract = _storageContract;

        }
        #endregion

        #region 查看数据

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public StoreCart View(int Id)
        {
            var entity = _storeCartRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑对象

        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public StoreCartDto Edit(int Id)
        {
            var entity = _storeCartRepository.GetByKey(Id);
            Mapper.CreateMap<StoreCart, StoreCartDto>();
            var dto = Mapper.Map<StoreCart, StoreCartDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集

        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<StoreCart> StoreCarts { get { return _storeCartRepository.Entities; } }
        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params StoreCartDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");                
                UnitOfWork.TransactionEnabled = true;
                IQueryable<StoreCart> listStoreCart = this.StoreCarts.Where(x => x.IsDeleted == false && x.IsEnabled == true);                
                
                OperationResult result = _storeCartRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "添加成功") : new OperationResult(OperationResultType.Error, "添加失败");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 更新数据

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params StoreCartDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _storeCartRepository.Update(dtos,
                    dto =>
                    {

                    },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 移除数据

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
                var entities = _storeCartRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _storeCartRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 恢复数据

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
                var entities = _storeCartRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _storeCartRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 启用数据

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
                var entities = _storeCartRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _storeCartRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 禁用数据

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
                var entities = _storeCartRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _storeCartRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 添加到购物车
        /// <summary>
        /// 添加到购物车
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        public OperationResult AddCart(int? StoreCartId, params StoreCartItemDto[] infos)
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            return AddCart(adminId, StoreCartId, infos);
        }

        public OperationResult AddCart(int AdminId, int? StoreCartId, params StoreCartItemDto[] infos)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            var query = this.StoreCarts.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsOrder == false);
            StoreCart storeCart = null;
            if (StoreCartId.HasValue)
            {
                storeCart = query.FirstOrDefault(f => f.Id == StoreCartId);
            }
            else
            {
                storeCart = query.FirstOrDefault(x => x.PurchaserId == AdminId);
            }

            if (storeCart == null)
            {
                oper.Message = "订单不存在";
            }
            else
            {
                UnitOfWork.TransactionEnabled = true;
                List<StoreCartItem> listStoreCartItem = new List<StoreCartItem>();
                //引用类型
                GetStoreCartItem(infos, listStoreCartItem);
                List<StoreCartItem> updateEntity = new List<StoreCartItem>();
                List<StoreCartItem> addEntity = new List<StoreCartItem>();
                List<StoreCartItem> listEntity = storeCart.StoreCartItems.Where(x => x.IsEnabled == true && x.IsDeleted == false).ToList();
                List<StoreCartItem> children = storeCart.StoreCartItems.Where(x => x.IsEnabled == true && x.IsDeleted == false && x.ParentId != null).ToList();
                List<String> listNumber = infos.Select(x => x.BigProdNum).ToList();
                List<Product> listProduct = _productRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && listNumber.Any(k => k == x.BigProdNum) == true).ToList();
                foreach (StoreCartItemDto dto in infos)
                {
                    StoreCartItem parent = listEntity.FirstOrDefault(x => x.BigProdNum == dto.BigProdNum);
                    Product product = listProduct.FirstOrDefault(x => x.BigProdNum == dto.BigProdNum && x.ColorId == dto.ColorId && x.SizeId == dto.SizeId);
                    if (product.IsNotNull())
                    {
                        if (parent.IsNull())
                        {
                            parent = new StoreCartItem()
                            {
                                BigProdNum = dto.BigProdNum,
                                OperatorId = AdminId,
                                StoreCartId = storeCart.Id,
                            };
                            parent.StoreCartItems.Add(new StoreCartItem()
                            {
                                ProductId = product.Id,
                                IsEnabled = true,
                                IsDeleted = false,
                                Quantity = dto.Quantity > 0 ? dto.Quantity : 1,
                                StoreCartId = storeCart.Id,
                            });
                            addEntity.Add(parent);
                            listEntity.Add(parent);
                        }
                        else
                        {
                            StoreCartItem child = children.FirstOrDefault(x => x.Product.ColorId == dto.ColorId && x.Product.SizeId == dto.SizeId && x.ParentId == parent.Id);
                            if (child == null)
                            {
                                child = new StoreCartItem()
                                {
                                    ProductId = product.Id,
                                    IsEnabled = true,
                                    IsDeleted = false,
                                    ParentId = parent.Id,
                                    Quantity = dto.Quantity > 0 ? dto.Quantity : 1,
                                    StoreCartId = storeCart.Id,
                                };
                                addEntity.Add(child);
                            }
                            else
                            {
                                child.Quantity += dto.Quantity;
                                updateEntity.Add(child);
                            }
                        }
                    }
                }
                int quantity = 0;
                if (addEntity.Count() > 0)
                {
                    quantity += 1;
                    _storeCartItemRepository.Insert(addEntity.AsEnumerable());
                }
                if (updateEntity.Count() > 0)
                {
                    quantity += 1;
                    _storeCartItemRepository.Update(addEntity);
                }
                if (quantity > 0)
                {
                    int resultCount = UnitOfWork.SaveChanges();
                    if (resultCount > 0)
                    {
                        oper.ResultType = OperationResultType.Success;
                    }
                    else
                    {
                        oper.Message = "对应的尺码或颜色数据不存在";
                    }
                }
                else
                {
                    oper.Message = "对应的尺码或颜色数据不存在";
                }
            }
            return oper;
        }
        /// <summary>
        /// 加入购物车，自动创建订单
        /// </summary>
        /// <param name="AdminId"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        public OperationResult AddCartAuto(int AdminId, StoreCartDto dtoCart, params StoreCartItemDto[] infos)
        {
            List<StoreCartItem> listStoreCartItem = new List<StoreCartItem>();
            var query = this.StoreCarts.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsOrder == false && x.OriginFlag == dtoCart.OriginFlag);
            var purquery = _purchaseRepository.Entities.Where(f => f.IsEnabled && !f.IsDeleted && f.OriginFlag == dtoCart.OriginFlag && f.PurchaseStatus != (int)PurchaseStatusFlag.Purchased && f.PurchaseStatus != (int)PurchaseStatusFlag.RefusePurchase);
            StoreCart storeCart = null;
            Purchase purchase = null;//判断是否有未处理的采购单
            if (dtoCart.OriginFlag != StoreCardOriginFlag.工厂)
            {
                if (dtoCart.OriginFlag == StoreCardOriginFlag.个人)
                {
                    storeCart = query.FirstOrDefault(f => f.PurchaserId == AdminId);
                    purchase = purquery.FirstOrDefault(f => f.StoreCart.PurchaserId == AdminId);
                }
                else
                {
                    storeCart = query.FirstOrDefault(x => x.Phone == dtoCart.Phone);
                    purchase = purquery.FirstOrDefault(f => f.StoreCart.Phone == dtoCart.Phone);
                }
            }
            else
            {
                storeCart = query.FirstOrDefault(f => f.FactoryId == dtoCart.FactoryId);
                purchase = purquery.FirstOrDefault(f => f.StoreCart.FactoryId == dtoCart.FactoryId);
            }

            #region 校验采购单中是否存在未处理完成的订单

            if (purchase.IsNotNull())
            {
                var strError = $"加入购物车失败,需要等待采购单: {purchase.PurchaseNumber} 处理完成";
                return new OperationResult(OperationResultType.Error, strError);
            }

            #endregion

            OperationResult oper = new OperationResult(OperationResultType.Success);
            if (storeCart == null)
            {
                DateTime current = DateTime.Now;
                string num = Guid.NewGuid().ToString("N").MD5Hash();
                storeCart = new StoreCart()
                {
                    StoreCartNum = num,
                    OriginFlag = dtoCart.OriginFlag,
                    OperatorId = AdminId,
                    CaptainId = AdminId,
                    CreatedTime = DateTime.Now,
                    StoreCartItems = listStoreCartItem,
                };
                if (dtoCart.OriginFlag != StoreCardOriginFlag.工厂)
                {
                    if (dtoCart.OriginFlag == StoreCardOriginFlag.个人)
                    {
                        storeCart.PurchaserId = AdminId;
                    }
                    else
                    {
                        storeCart.Phone = dtoCart.Phone;
                        storeCart.Address = dtoCart.Address;
                        storeCart.Name = dtoCart.Name;
                    }
                }
                else
                {
                    storeCart.FactoryId = dtoCart.FactoryId;
                }
                if (infos.IsNotNullOrEmptyThis())
                {
                    storeCart.StoreCartItems = this.GetStoreCartItem(infos.Select(s => s.BigProdNum).ToArray(), listStoreCartItem);
                }
                oper = this.Insert(storeCart);
                if (oper.ResultType != OperationResultType.Success)
                {
                    return oper;
                }
            }
            else
            {
                if (dtoCart.OriginFlag == StoreCardOriginFlag.临时)
                {
                    if (storeCart.Phone != dtoCart.Phone|| storeCart.Address != dtoCart.Address|| storeCart.Name != dtoCart.Name)
                    {
                        var dto = storeCart.MapperTo<StoreCartDto>();
                        this.Update(dto);
                    }
                }
            }
            if (infos.IsNotNullOrEmptyThis())
            {
                oper = AddCart(AdminId, storeCart.Id, infos);
            }
            return oper;
        }

        /// <summary>
        /// 获取可以添加的数据集
        /// </summary>
        /// <param name="listNumber">编号</param>
        /// <param name="listEntity">已添加的集合</param>
        /// <param name="StoreCartId">选购车ID</param>
        /// <returns></returns>
        private List<StoreCartItem> GetStoreCartItem(string[] arrNumber, List<StoreCartItem> listEntity, int StoreCartId = 0)
        {
            //去重复
            IEnumerable<string> numbers= arrNumber.Distinct();
            List<StoreCartItem> list = new List<StoreCartItem>();
            foreach (string number in numbers)
            {
                if (!string.IsNullOrEmpty(number))
                {
                    bool isHave = listEntity.Where(x => x.IsEnabled == true && x.IsDeleted == false && x.ParentId == null)
                            .Any(x => x.BigProdNum == number);
                    if (isHave != true)
                    {
                        list.Add(new StoreCartItem()
                        {
                            BigProdNum = number,
                            Quantity = 0,
                            OperatorId = AuthorityHelper.OperatorId,
                            StoreCartId = StoreCartId,
                        });
                    }
                }
            }
            return list;
        }



        /// <summary>
        /// 添加和更新购物车详情
        /// </summary>
        /// <param name="addEntity">需要添加的数据</param>
        /// <param name="updateEntity">需要更新的数据</param>
        /// <returns></returns>
        private OperationResult AddAndUpdate(List<StoreCartItemDto> addEntity, List<StoreCartItem> updateEntity)
        {
            OperationResult oper = new OperationResult(OperationResultType.Success);
            try
            {                
                UnitOfWork.TransactionEnabled = true;

                if (addEntity.Count>0)
                {
                    OperationResult result = _storeCartItemRepository.Insert(addEntity,
                    dto =>
                    {
                    
                    },
                    (dto, entity) =>
                    {
                        entity.CreatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                }
                if (updateEntity.Count>0)
                {
                    _storeCartItemRepository.Update(updateEntity);    
                }                
                int count = UnitOfWork.SaveChanges();
                if (count==0)
                {
                    oper.ResultType = OperationResultType.Error;
                    oper.Message = "添加到选购车失败";
                }                
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.ResultType = OperationResultType.Error;
                oper.Message = "服务器忙，请稍后重试";
            }
            return oper;
        }

        /// <summary>
        /// 将dto集合转换成entity集合
        /// </summary>
        /// <param name="infos"></param>
        /// <param name="listStoreCartItem"></param>
        private void GetStoreCartItem(StoreCartItemDto[] infos, List<StoreCartItem> listStoreCartItem)
        {            
            List<string> listBigNums=  infos.Select(x => x.BigProdNum).ToList();
            IQueryable<Product> listPro = _productRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true)
                .Where(x => listBigNums.Where(k => k == x.BigProdNum).Count() > 0);
            foreach (StoreCartItemDto item in infos)
            {
                Mapper.CreateMap<StoreCartItemDto, StoreCartItem>();
                StoreCartItem entity = Mapper.Map<StoreCartItemDto, StoreCartItem>(item);
                Product pro = listPro.FirstOrDefault(x => x.BigProdNum == item.BigProdNum && x.ColorId == item.ColorId && x.SizeId == item.SizeId);
                if (pro!=null)
                {
                    entity.ProductId = pro.Id;
                }
                listStoreCartItem.Add(entity);
            }            
        }
        
        #endregion

        #region 添加数据-重载
        /// <summary>
        /// 添加数据-重载
        /// </summary>
        /// <param name="storeCartDto"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        public OperationResult Insert(params StoreCart[] entities)
        {
            OperationResult result = new OperationResult(OperationResultType.Error);
            try
            {
                int resCount = _storeCartRepository.Insert(entities.AsEnumerable());
                if (resCount>0)
                {
                    result.ResultType = OperationResultType.Success;
                }
                else
                {
                    result.Message = "添加失败";
                    result.ResultType = OperationResultType.Error;
                }
            }
            catch (Exception ex)
            {
                result.Message = "服务器忙，请稍后重试";
                _Logger.Error<string>(ex.ToString());
            }
            return result;            
        }
        #endregion

        #region 一件下单
        /// <summary>
        /// 订购仓库（并生成采购单和配货单）
        /// </summary>
        /// <param name="Storage">订购仓库</param>
        /// <returns></returns>        
        public OperationResult AddPurchase(int ReceiverStorageId, int ReceiverStoreId, bool WithoutMoney = false)
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            return AddPurchase(adminId, ReceiverStorageId, ReceiverStoreId, WithoutMoney);
        }

        public OperationResult AddPurchase(int AdminId,int ReceiverStorageId, int ReceiverStoreId, bool WithoutMoney = false)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                UnitOfWork.TransactionEnabled = true;
                //拿到当前用户选购的商品
                StoreCart storeCart = this.StoreCarts.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsOrder == false)
                    .FirstOrDefault(x => x.PurchaserId == AdminId);
                if (storeCart != null)
                {
                    if (storeCart.IsOrder == true)
                    {
                        oper.Message = "已经下单";
                        return oper;
                    }
                    //该数据标记为订购
                    storeCart.IsOrder = true;
                    List<StoreCartItem> listStoreCartItem = storeCart.StoreCartItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId != null).ToList();
                    if (listStoreCartItem == null || listStoreCartItem.Count() == 0)
                    {
                        oper.Message = "请选择商品";
                        return oper;
                    }
                    string strNum = null;
                    //校验随机号码是否存在
                    List<string> listNum = _purchaseRepository.Entities.Select(x => x.PurchaseNumber).ToList();
                    while (true)
                    {
                        strNum = RandomHelper.GetRandomNum(8);
                        if (listNum != null && listNum.Count() > 0)
                        {
                            int count = listNum.Where(x => x == strNum).Count();
                            if (count > 0)
                            {
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    //拿到采购仓库
                    Storage storage = _storageRepository.Entities.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.IsOrderStorage == true);
                    int? storageId = null;
                    if (storage != null)
                    {
                        storageId = storage.Id;
                    }
                    Purchase purchase = new Purchase()
                    {
                        StorageId = storageId,
                        ReceiverId = ReceiverStoreId,
                        ReceiverStorageId = ReceiverStorageId,
                        PurchaseNumber = strNum,
                        PurchaseStatus = (int)PurchaseStatusFlag.Purchasing,
                        OriginFlag = storeCart.OriginFlag,
                        OperatorId = AuthorityHelper.OperatorId
                    };
                    List<PurchaseItem> listPurchaseItem = new List<PurchaseItem>();
                    float AllPrice = 0;//需要支付总价格
                    foreach (StoreCartItem item in listStoreCartItem)
                    {
                        PurchaseItem entityItem = new PurchaseItem();
                        entityItem.ProductId = item.ProductId ?? 0;
                        entityItem.TagPrice = item.Product.ProductOriginNumber == null ? 0 : item.Product.ProductOriginNumber.TagPrice;
                        entityItem.WholesalePrice = item.Product.ProductOriginNumber == null ? 0 : item.Product.ProductOriginNumber.WholesalePrice;
                        entityItem.PurchasePrice = item.Product.ProductOriginNumber == null ? 0 : item.Product.ProductOriginNumber.PurchasePrice;
                        entityItem.Quantity = item.Quantity;
                        entityItem.OperatorId = AuthorityHelper.OperatorId;
                        listPurchaseItem.Add(entityItem);

                        AllPrice += entityItem.Quantity * entityItem.PurchasePrice;
                    }

                    purchase.OrgPrice = AllPrice;

                    var canPurchase = true;//能否下单

                    #region 判断是否需要扣款

                    if (!WithoutMoney)
                    {
                        canPurchase = false;
                        var modStore = _storeRepository.Entities.FirstOrDefault(f => f.Id == ReceiverStoreId && f.IsEnabled && !f.IsDeleted);
                        if (modStore.IsNotNull())
                        {
                            //var storeRate = GetStoreDepositDiscount(modStore.Id);
                            var storeRate = modStore.StoreDiscount;

                            purchase.DespoitPrice = AllPrice * storeRate;

                            var remainPrice = modStore.Balance - purchase.DespoitPrice;
                            if (remainPrice >= 0)
                            {
                                modStore.Balance = remainPrice;
                                _storeRepository.Update(modStore);
                                canPurchase = true;
                            }
                            else
                            {
                                oper.Message = "下单失败,店铺余额不足";
                            }
                        }
                        else
                        {
                            oper.Message = "下单失败,店铺不存在";
                        }
                    }

                    #endregion

                    if (canPurchase)
                    {
                        purchase.PurchaseItems = listPurchaseItem;
                        _storeCartRepository.Update(storeCart);
                        _purchaseRepository.Insert(purchase);
                        int index = UnitOfWork.SaveChanges();
                        if (index > 0)
                        {
                            oper.ResultType = OperationResultType.Success;
                        }
                        else
                        {
                            oper.Message = "下单失败";
                        }
                    }
                }
                else {
                    oper.Message = "订单不存在";
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后重试";
            }
            return oper;
        }

        public OperationResult AddPurchaseDirect(int? AdminId, int? CartId)
        {
            if (!AdminId.HasValue)
            {
                AdminId = AuthorityHelper.OperatorId ?? 0;
            }

            var data = OperationHelper.Try((opera) =>
            {
                OperationResult oper = new OperationResult(OperationResultType.Error);

                UnitOfWork.TransactionEnabled = true;
                StoreCart storeCart = null;
                var query = this.StoreCarts.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsOrder == false);

                if (CartId.HasValue)
                {
                    storeCart = query.FirstOrDefault(x => x.Id == CartId);
                }
                else
                {
                    storeCart = query.FirstOrDefault(x => x.PurchaserId == AdminId);
                }

                if (storeCart.IsNotNull())
                {
                    if (storeCart.IsOrder == true)
                    {
                        oper.Message = "已经下单";
                        return oper;
                    }

                    //该数据标记为订购
                    storeCart.IsOrder = true;
                    List<StoreCartItem> listStoreCartItem = storeCart.StoreCartItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId != null).ToList();
                    if (listStoreCartItem == null || listStoreCartItem.Count() == 0)
                    {
                        oper.Message = "请选择商品";
                        return oper;
                    }
                    string strNum = null;
                    //校验随机号码是否存在
                    List<string> listNum = _purchaseRepository.Entities.Select(x => x.PurchaseNumber).ToList();
                    while (true)
                    {
                        strNum = RandomHelper.GetRandomNum(8);
                        if (listNum != null && listNum.Count() > 0)
                        {
                            int count = listNum.Where(x => x == strNum).Count();
                            if (count > 0)
                            {
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    //拿到采购仓库
                    Storage storage = null;
                    //临时仓库【收货仓库】
                    Storage storageTemp = null;
                    if (storeCart.OriginFlag != StoreCardOriginFlag.工厂)
                    {
                        storage = _storageRepository.Entities.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.IsOrderStorage == true);
                        if (storeCart.OriginFlag == StoreCardOriginFlag.个人)
                        {
                            storageTemp = CacheAccess.GetManagedStorage(_storageContract, _administratorContract).FirstOrDefault();//默认可以管理到的仓库中的第一个
                            if (storageTemp.IsNull())
                            {
                                oper.Message = "没有可管理的采购仓库";
                                return oper;
                            }
                        }
                        else
                        {
                            storageTemp = _storageRepository.Entities.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.IsTempStorage == true);
                            if (storageTemp.IsNull())
                            {
                                oper.Message = "缺少临时仓库配置";
                                return oper;
                            }
                        }
                    }
                    else
                    {
                        storage = storeCart.Factory.Storage;
                        storageTemp = _storageRepository.Entities.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.IsForAddInventory == true);
                        if (storageTemp.IsNull())
                        {
                            oper.Message = "分仓管理-缺少入库仓库配置";
                            return oper;
                        }
                    }

                    Purchase purchase = new Purchase()
                    {
                        StorageId = storage?.Id,
                        ReceiverId = storageTemp?.StoreId,
                        ReceiverStorageId = storageTemp?.Id,
                        PurchaseNumber = strNum,
                        PurchaseStatus = (int)PurchaseStatusFlag.Purchasing,
                        StoreCartId = storeCart.Id,
                        OriginFlag = storeCart.OriginFlag,
                        OperatorId = AuthorityHelper.OperatorId
                    };

                    List<PurchaseItem> listPurchaseItem = new List<PurchaseItem>();
                    foreach (StoreCartItem item in listStoreCartItem)
                    {
                        PurchaseItem entityItem = new PurchaseItem();
                        entityItem.ProductId = item.ProductId ?? 0;
                        entityItem.TagPrice = item.Product.ProductOriginNumber == null ? 0 : item.Product.ProductOriginNumber.TagPrice;
                        entityItem.WholesalePrice = item.Product.ProductOriginNumber == null ? 0 : item.Product.ProductOriginNumber.WholesalePrice;
                        entityItem.PurchasePrice = item.Product.ProductOriginNumber == null ? 0 : item.Product.ProductOriginNumber.PurchasePrice;
                        entityItem.Quantity = item.Quantity;
                        entityItem.OperatorId = AuthorityHelper.OperatorId;
                        listPurchaseItem.Add(entityItem);
                    }

                    purchase.PurchaseItems = listPurchaseItem;
                    _storeCartRepository.Update(storeCart);
                    _purchaseRepository.Insert(purchase);
                    int index = UnitOfWork.SaveChanges();
                    if (index > 0)
                    {
                        oper.ResultType = OperationResultType.Success;
                        oper.Message = "操作成功";
                    }
                    else
                    {
                        oper.Message = "下单失败";
                    }
                }
                else
                {
                    oper.Message = "订单不存在";
                }

                return oper;
            }, "一键下单");
            return data;
        }

        #endregion

        #region 移除用户的所有采购数据
        public OperationResult RemoveAll()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            return RemoveAll(adminId);
        }

        public OperationResult RemoveAll(int AdminId)
        {
            StoreCart storeCart = this.StoreCarts.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.PurchaserId == AdminId && x.IsOrder == false);
            OperationResult oper = new OperationResult(OperationResultType.Error, "数据不存在");
            if (storeCart != null)
            {
                storeCart.UpdatedTime = DateTime.Now;
                storeCart.IsDeleted = true;
                int resultCount = _storeCartRepository.Update(storeCart);
                if (resultCount > 0)
                {
                    oper.ResultType = OperationResultType.Success;
                    oper.Message = "清空成功";
                }
                else
                {
                    oper.Message = "更新失败";
                }
            }
            return oper;
        }

        #endregion

        /// <summary>
        /// 获取店铺可享受折扣比率(等级折扣)
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public float GetStoreDepositDiscount(int storeId)
        {
            var allStoreDepPrice = _storeDepositRepository.Entities.Where(w => w.StoreId == storeId && w.IsEnabled && !w.IsDeleted).Select(s=>s.Price).ToList().Sum();//当前店铺历史付款金额
            var storeLevel = _storeLevelRepository.Entities.Where(f => allStoreDepPrice >= f.UpgradeCondition && f.IsEnabled && !f.IsDeleted).OrderByDescending(o => o.UpgradeCondition).FirstOrDefault();
            return storeLevel?.Discount ?? 1;
        }
    }
}
