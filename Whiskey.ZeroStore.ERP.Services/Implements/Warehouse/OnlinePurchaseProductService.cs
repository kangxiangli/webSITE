using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Web.SignalR;
using Whiskey.Web.Http;
using Whiskey.Web.Extensions;
using Whiskey.Utility;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Logging;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Warehouses;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class OnlinePurchaseProductService : ServiceBase, IOnlinePurchaseProductContract
    {
        #region 声明数据层对象

        private readonly IRepository<OnlinePurchaseProduct, int> _onlinePurchaseProductRepository;

        private readonly IRepository<OnlinePurchaseProductItem, int> _oppItemRepository;

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(OnlinePurchaseProductService));

        public OnlinePurchaseProductService(IRepository<OnlinePurchaseProduct, int> onlinePurchaseProductRepository,
            IRepository<OnlinePurchaseProductItem, int> oppItemRepository)
            : base(onlinePurchaseProductRepository.UnitOfWork)
        {
            _onlinePurchaseProductRepository = onlinePurchaseProductRepository;
            _oppItemRepository = oppItemRepository;
        }
        #endregion

        #region 查看数据

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public OnlinePurchaseProduct View(int Id)
        {
            var entity = _onlinePurchaseProductRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑数据对象

        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public OnlinePurchaseProductDto Edit(int Id)
        {
            var entity = _onlinePurchaseProductRepository.GetByKey(Id);
            Mapper.CreateMap<OnlinePurchaseProduct, OnlinePurchaseProductDto>();
            var dto = Mapper.Map<OnlinePurchaseProduct, OnlinePurchaseProductDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集

        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<OnlinePurchaseProduct> OnlinePurchaseProducts { get { return _onlinePurchaseProductRepository.Entities; } }
        #endregion

        public IQueryable<OnlinePurchaseProductItem> OnlinePurchaseProductItems { get { return _oppItemRepository.Entities; } }

        #region 按条件检查数据是否存在

        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<OnlinePurchaseProduct, bool>> predicate, int id = 0)
        {
            return _onlinePurchaseProductRepository.ExistsCheck(predicate, id);
        }
        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params OnlinePurchaseProductDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                foreach (OnlinePurchaseProductDto dto in dtos)
                {
                    dto.NoticeQuantity = 0;
                    dto.UniqueCode = DateTime.Now.ToString("yyyyMMddhhmmss") + RandomHelper.GetRandomCode(4);
                }
                OperationResult result = _onlinePurchaseProductRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                return result;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 深拷贝优惠券详细信息
        /// <summary>
        /// 深拷贝优惠券详细信息
        /// </summary>
        /// <param name="couponItem"></param>
        /// <returns></returns>
        //private CouponItem DeepClone(CouponItemDto dto)
        //{
        //    using (Stream stream = new MemoryStream())
        //    {
        //        IFormatter formatter = new BinaryFormatter();
        //        formatter.Serialize(stream, dto);
        //        stream.Seek(0, SeekOrigin.Begin);
        //        CouponItemDto entityDto =formatter.Deserialize(stream) as CouponItemDto;
        //        Mapper.CreateMap<CouponItemDto, CouponItem>();
        //        var entity = Mapper.Map<CouponItemDto, CouponItem>(entityDto);
        //        return entity;
        //    }
        //}
        #endregion

        #region 更新数据

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params OnlinePurchaseProductDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _onlinePurchaseProductRepository.Update(dtos,
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
                return new OperationResult(OperationResultType.Error, "修改失败！错误如下：" + ex.Message);
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
                var entities = _onlinePurchaseProductRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _onlinePurchaseProductRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
            }
        }


        public OperationResult Remove(string UniqueCode, string[] BigProdNums)
        {
            try
            {
                UnitOfWork.TransactionEnabled = true;
                var entities = _onlinePurchaseProductRepository.Entities.FirstOrDefault(m => m.UniqueCode == UniqueCode)
                    .OnlinePurchaseProductItems.Where(x => BigProdNums.Contains(x.BigProdNum));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _oppItemRepository.Update(entity);
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
                var entities = _onlinePurchaseProductRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _onlinePurchaseProductRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 删除数据

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
                OperationResult result = _onlinePurchaseProductRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
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
                var entities = _onlinePurchaseProductRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _onlinePurchaseProductRepository.Update(entity);
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
                var entities = _onlinePurchaseProductRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _onlinePurchaseProductRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 重载--添加可采购商品详细
        /// <summary>
        /// 重载--添加可采购商品详细
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public OperationResult Insert(string UniqueCode, CheckResultEdo edo)
        {
            List<OnlinePurchaseProductItem> listEntity = new List<OnlinePurchaseProductItem>();
            OperationResult oper = new OperationResult(OperationResultType.Error);
            OnlinePurchaseProduct parent = this.OnlinePurchaseProducts.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.UniqueCode == UniqueCode);
            if (parent == null)
            {
                oper.Message = "采购单不存在";
            }
            else
            {
                List<OnlinePurchaseProductItem> oppItems = _oppItemRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.OnlinePurchaseProductId == parent.Id).ToList();
                List<string> list = edo.Valids;
                bool isHave = oppItems.Any(x => list.Contains(x.BigProdNum));
                //OnlinePurchaseProductItem item =
                foreach (string num in list)
                {
                    if (string.IsNullOrEmpty(num))
                    {
                        edo.InvalidCount += 1;
                    }
                    else
                    {
                        isHave = listEntity.Any(x => x.BigProdNum == num);
                        if (isHave == true)
                        {
                            edo.InvalidCount += 1;
                        }
                        else
                        {
                            //判断是否添加到数据库
                            isHave = oppItems.Any(x => list.Contains(x.BigProdNum));
                            if (isHave == true)
                            {
                                edo.InvalidCount += 1;
                            }
                            else
                            {
                                edo.ValidCount += 1;
                                OnlinePurchaseProductItem temp = new OnlinePurchaseProductItem()
                                {
                                    OnlinePurchaseProductId = parent.Id,
                                    BigProdNum = num,
                                };
                                listEntity.Add(temp);
                            }
                        }
                    }
                }
                if (listEntity.Count() > 0)
                {
                    int resultCount = _oppItemRepository.Insert(listEntity.AsEnumerable());
                    if (resultCount > 0)
                    {
                        oper.ResultType = OperationResultType.Success;
                    }
                    else
                    {
                        oper.Message = "添加失败";
                    }
                }
                else
                {
                    oper.ResultType = OperationResultType.Success;
                }
            }
            return oper;
        }

        /// <summary>
        /// 深复制
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private OnlinePurchaseProductItem DeepCopy(OnlinePurchaseProductItem entity)
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, entity);
                objectStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(objectStream) as OnlinePurchaseProductItem;
            }
        }

        #endregion


        /// <summary>
        /// 获取时间配置
        /// </summary>
        /// <returns></returns>
        public BigProdNumStateConfigEntry GetConfig()
        {
            var entry = RedisCacheHelper.Get<BigProdNumStateConfigEntry>(RedisCacheHelper.kEY_ONLINEPURCHASE_CONFIG);
            if (entry != null)
            {
                return entry;
            }

            // 配置初始化
            entry = new BigProdNumStateConfigEntry()
            {
                NewProductTime = DateTime.Now.Date,
                ClassicProductTime = DateTime.Now.Date.AddDays(-1)
            };

            RedisCacheHelper.Set(RedisCacheHelper.kEY_ONLINEPURCHASE_CONFIG, entry);
            return entry;

        }



        /// <summary>
        /// 更新配置
        /// </summary>
        /// <param name="entry">配置对象</param>
        /// <returns></returns>
        public OperationResult UpdateConfig(BigProdNumStateConfigEntry entry)
        {

            if (entry.NewProductTime <= entry.ClassicProductTime)
            {
                return OperationResult.Error("新品时间必须在经典时间以后");
            }
            entry.ModifyTime = DateTime.Now;
            entry.OperatorId = AuthorityHelper.OperatorId.Value;
            var key = RedisCacheHelper.kEY_ONLINEPURCHASE_CONFIG;
            var res = RedisCacheHelper.Set(key, entry);

            // 重置选购商品列表的新品状态缓存
            RedisCacheHelper.Remove(RedisCacheHelper.KEY_ONLINEPURCHASE_BIGPRODNUM_STATE);
            GetOnlinePurchaseBigProdNumStateDict(true);

            return OperationResult.OK();


        }


        /// <summary>
        /// 获取选货单新品状态
        /// </summary>
        /// <param name="createdTime">选货单创建时间</param>
        /// <param name="config">配置对象,由外部传入,批量操作时,注意避免在循环中反复从缓存中读取此配置</param>
        /// <returns>新品状态</returns>
        public BigProdNumStateEnum GetOnlinePurchaseState(DateTime createdTime, BigProdNumStateConfigEntry config)
        {

            if (config == null)
            {
                throw new Exception("配置不可为空");
            }
            if (createdTime <= config.ClassicProductTime)
            {
                return BigProdNumStateEnum.经典;
            }
            else if (createdTime >= config.NewProductTime)
            {
                return BigProdNumStateEnum.新品;

            }
            else
            {
                return BigProdNumStateEnum.普通;
            }
        }


        /// <summary>
        /// 批量根据款号查询新品状态
        /// </summary>
        /// <param name="bigProdNums">款号</param>
        /// <returns>新品状态</returns>
        public Dictionary<string, BigProdNumStateEnum> GetOnlinePurchaseBigProdNumState(BigProdNumStateConfigEntry config, params string[] bigProdNums)
        {
            if (config == null)
            {
                throw new Exception("配置不可为空");
            }
            if (bigProdNums == null)
            {
                throw new Exception("款号不可为空");

            }
            var dict = GetOnlinePurchaseBigProdNumStateDict();
            var resDict = new Dictionary<string, BigProdNumStateEnum>();
            var val = BigProdNumStateEnum.普通;
            for (int i = 0; i < bigProdNums.Length; i++)
            {
                if (resDict.ContainsKey(bigProdNums[i]))
                {
                    val = dict.ContainsKey(bigProdNums[i]) ? dict[bigProdNums[i]] : BigProdNumStateEnum.普通;
                    resDict.Add(bigProdNums[i], val);
                }
            }
            return dict;
        }



        /// <summary>
        /// 获取所有选货单内款号新品状态
        /// </summary>
        /// <returns>新品状态</returns>
        public Dictionary<string, BigProdNumStateEnum> GetOnlinePurchaseBigProdNumState()
        {
           
            var dict = GetOnlinePurchaseBigProdNumStateDict();
            return dict;
            
        }

        private Dictionary<string, BigProdNumStateEnum> GetOnlinePurchaseBigProdNumStateDict(bool resetCache = false)
        {
            var key = RedisCacheHelper.KEY_ONLINEPURCHASE_BIGPRODNUM_STATE;
            var dict = RedisCacheHelper.Get<Dictionary<string, BigProdNumStateEnum>>(key);
            if (dict == null)
            {
                dict = GenerateOnlinePurchaseBigProdNumStateDict();
                RedisCacheHelper.Set(key, dict, TimeSpan.FromHours(1));
            }
            return dict;
        }

        private Dictionary<string, BigProdNumStateEnum> GenerateOnlinePurchaseBigProdNumStateDict()
        {

            // 获取所有的选货单
            var purchaseIds = _onlinePurchaseProductRepository.Entities
                .Where(e => !e.IsDeleted && e.IsEnabled)
                .Select(e => e.Id);
            var bigProdNumDict = _oppItemRepository.Entities.Where(e => !e.IsDeleted && e.IsEnabled)
                                       .Where(e => purchaseIds.Contains(e.OnlinePurchaseProductId.Value))
                                       .GroupBy(e => e.BigProdNum)
                                       .Select(g => new
                                       {
                                           BigProdNum = g.Key,

                                           // 多个选货单有同一个款号的请款下, 选择最新创建的选货单
                                           LatestCreatedTime = g.OrderByDescending(i => i.OnlinePurchaseProduct.CreatedTime)
                                                                  .FirstOrDefault()       // 这里返回的是item,要的是父级的时间
                                                                  .OnlinePurchaseProduct.CreatedTime
                                       })
                                       .ToDictionary(s => s.BigProdNum, s => s.LatestCreatedTime);

            var dict = new Dictionary<string, BigProdNumStateEnum>();
            var config = GetConfig();
            foreach (var item in bigProdNumDict)
            {
                if (!dict.ContainsKey(item.Key))
                {
                    var val = GetOnlinePurchaseState(item.Value, config);
                    dict.Add(item.Key, val);
                }
            }
            return dict;



        }

    }
}
