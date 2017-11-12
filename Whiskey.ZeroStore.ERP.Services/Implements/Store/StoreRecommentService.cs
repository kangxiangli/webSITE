using AutoMapper;
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
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class StoreRecommentService : ServiceBase, IStoreRecommendContract
    {
        IRepository<StoreRecommend, int> _repo;
        protected readonly IProductOrigNumberContract _productOrignNumberContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IStoreNoRecommendContract _storeNoRecommendContract;
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(StoreRecommentService));
        public StoreRecommentService(IRepository<StoreRecommend, int> repo,
                IProductOrigNumberContract productOrigNumberContract,
                IStoreContract storeContract,
                IStoreNoRecommendContract storeNoRecommendContract
            ) : base(repo.UnitOfWork)
        {
            _repo = repo;
            _productOrignNumberContract = productOrigNumberContract;
            _storeContract = storeContract;
            _storeNoRecommendContract = storeNoRecommendContract;
        }
        public IQueryable<StoreRecommend> StoreRecommends
        {
            get
            {
                return _repo.Entities;
            }
        }

        public OperationResult Delete(int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _repo.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "删除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Disable(int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id)).ToList();
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Enable(int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _repo.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Insert(params StoreRecommend[] entities)
        {
            return _repo.Insert((IEnumerable<StoreRecommend>)entities) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params StoreRecommendDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _repo.Insert(dtos,
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
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }

        public OperationResult Recovery(int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _repo.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Update(params StoreRecommend[] entities)
        {
            try
            {
                return _repo.Update(entities);
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "设置失败！错误如下：" + ex.Message, ex.ToString());
            }

        }


        public OperationResult SaveRecommend(string numbers)
        {
            if (string.IsNullOrEmpty(numbers))
            {
                return new OperationResult(OperationResultType.Error, "参数错误");
            }
            var recommendStoreIds = _storeContract.Stores.Where(s => !s.IsDeleted && s.IsEnabled && s.StoreTypeId == 1).Select(s => s.Id).ToList();
            var numberArr = numbers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var querySource = _productOrignNumberContract.OrigNumbs.Where(o => !o.IsDeleted && o.IsEnabled && numberArr.Contains(o.BigProdNum));
            if (querySource.Count() != numberArr.Count)
            {
                return new OperationResult(OperationResultType.Error, "数量不一致");
            }
            if (querySource.Any(o => o.IsRecommend.Value == true))
            {
                return new OperationResult(OperationResultType.Error, "提交数据中有已经设为推荐的数据");
            }
            var list = querySource.ToList();
            list.Each(item =>
            {
                item.IsRecommend = true;
                item.RecommendTime = DateTime.Now;
                item.RecommendStoreIds = string.Join(",", recommendStoreIds);
            });
            var res = _productOrignNumberContract.Update(list.ToArray());
            if (res.ResultType != OperationResultType.Success)
            {
                return new OperationResult(OperationResultType.Error, "保存失败");
            }
            ResetBigProdNumStateCache();
            return new OperationResult(OperationResultType.Success, string.Empty);
        }


        public OperationResult UpdateRecommendStoreId(string number, string storeIds)
        {
            if (string.IsNullOrEmpty(number))
            {
                return new OperationResult(OperationResultType.Error, "参数错误");
            }
            var numberEntity = _productOrignNumberContract.OrigNumbs.Where(o => !o.IsDeleted && o.IsEnabled && o.BigProdNum == number && o.IsRecommend == true).FirstOrDefault();
            if (numberEntity == null)
            {
                return new OperationResult(OperationResultType.Error, "款号不存在");
            }

            var storeIdFromUser = string.IsNullOrEmpty(storeIds) ? new int[0] : storeIds.Split(",", true).Select(s => int.Parse(s)).ToArray();

            if (storeIdFromUser.Length == 0)
            {
                //取消款号的推荐
                numberEntity.IsRecommend = false;
                numberEntity.RecommendStoreIds = string.Empty;
            }
            else
            {
                numberEntity.RecommendStoreIds = string.Join(",", storeIdFromUser);
            }

            var res = _productOrignNumberContract.Update(numberEntity);
            if (res.ResultType != OperationResultType.Success)
            {
                return new OperationResult(OperationResultType.Error, "保存失败");
            }
            return new OperationResult(OperationResultType.Success, string.Empty);

        }


        public OperationResult DeleteRecommend(string number)
        {
            if (string.IsNullOrEmpty(number))
            {
                return new OperationResult(OperationResultType.Error, "参数错误");
            }
            var entity = _productOrignNumberContract.OrigNumbs.Where(o => !o.IsDeleted && o.IsEnabled && o.BigProdNum == number).FirstOrDefault();
            if (entity == null)
            {
                return new OperationResult(OperationResultType.Error, "款号不存在");
            }
            if (entity.IsRecommend == false)
            {
                return new OperationResult(OperationResultType.Success, string.Empty);
            }


            entity.IsRecommend = false;
            var res = _productOrignNumberContract.Update(entity);
            if (res.ResultType != OperationResultType.Success)
            {
                return new OperationResult(OperationResultType.Error, "保存失败");
            }
            ResetBigProdNumStateCache();
            return new OperationResult(OperationResultType.Success, string.Empty);
        }





        /// <summary>
        /// 获取时间配置
        /// </summary>
        /// <returns></returns>
        public BigProdNumStateConfigEntry GetConfig()
        {
            var entry = RedisCacheHelper.Get<BigProdNumStateConfigEntry>(RedisCacheHelper.kEY_ONLINESTOREE_CONFIG);
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

            RedisCacheHelper.Set(RedisCacheHelper.kEY_ONLINESTOREE_CONFIG, entry);
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
            var key = RedisCacheHelper.kEY_ONLINESTOREE_CONFIG;
            var res = RedisCacheHelper.Set(key, entry);

            ResetBigProdNumStateCache();

            return OperationResult.OK();


        }

        /// <summary>
        /// 重置商城列表的新品状态缓存
        /// </summary>
        private void ResetBigProdNumStateCache()
        {
            RedisCacheHelper.Remove(RedisCacheHelper.KEY_ONLINESTORE_BIGPRODNUM_STATE);
            GetOnlineStoreBigProdNumStateDict(true);
        }





        /// <summary>
        /// 批量根据商城款号查询新品状态
        /// </summary>
        /// <param name="bigProdNums">款号</param>
        /// <returns>新品状态</returns>
        public Dictionary<string, BigProdNumStateEnum> GetOnlineStoreBigProdNumState(BigProdNumStateConfigEntry config, params string[] bigProdNums)
        {
            if (config == null)
            {
                throw new Exception("配置不可为空");
            }
            if (bigProdNums == null)
            {
                throw new Exception("款号不可为空");

            }
            var dict = GetOnlineStoreBigProdNumStateDict();
            var resDict = new Dictionary<string, BigProdNumStateEnum>();
            var defaultVal = BigProdNumStateEnum.普通;
            for (int i = 0; i < bigProdNums.Length; i++)
            {
                if (resDict.ContainsKey(bigProdNums[i]))
                {
                    defaultVal = dict.ContainsKey(bigProdNums[i]) ? dict[bigProdNums[i]] : BigProdNumStateEnum.普通;
                    resDict.Add(bigProdNums[i], defaultVal);
                }
            }
            return dict;
        }



        /// <summary>
        /// 获取所有商城款号新品状态
        /// </summary>
        /// <returns>新品状态</returns>
        public Dictionary<string, BigProdNumStateEnum> GetOnlineStoreBigProdNumState()
        {

            var dict = GetOnlineStoreBigProdNumStateDict();
            return dict;

        }




        /// <summary>
        /// 计算新品状态
        /// </summary>
        /// <param name="recommendTime">推荐时间</param>
        /// <param name="config">配置对象,由外部传入,批量操作时,注意避免在循环中反复从缓存中读取此配置</param>
        /// <returns>新品状态</returns>
        private BigProdNumStateEnum GetSingleBigProdNumState(DateTime recommendTime, BigProdNumStateConfigEntry config)
        {

            if (config == null)
            {
                throw new Exception("配置不可为空");
            }
            if (recommendTime <= config.ClassicProductTime)
            {
                return BigProdNumStateEnum.经典;
            }
            else if (recommendTime >= config.NewProductTime)
            {
                return BigProdNumStateEnum.新品;

            }
            else
            {
                return BigProdNumStateEnum.普通;
            }
        }
        private Dictionary<string, BigProdNumStateEnum> GetOnlineStoreBigProdNumStateDict(bool resetCache = false)
        {
            var key = RedisCacheHelper.KEY_ONLINESTORE_BIGPRODNUM_STATE;
            var dict = RedisCacheHelper.Get<Dictionary<string, BigProdNumStateEnum>>(key);
            if (dict == null)
            {
                dict = GenerateOnlineStoreBigProdNumStateDict();
                RedisCacheHelper.Set(key, dict, TimeSpan.FromHours(1));
            }
            return dict;
        }

        private Dictionary<string, BigProdNumStateEnum> GenerateOnlineStoreBigProdNumStateDict()
        {

            // 获取所有的商城款号和加入商城时间
            var bigProdNumDict = _productOrignNumberContract.OrigNumbs.Where(e => !e.IsDeleted && e.IsEnabled)
                                       .Where(e => e.IsRecommend == true)
                                       .ToDictionary(s => s.BigProdNum, s => s.RecommendTime ?? DateTime.MinValue);

            var dict = new Dictionary<string, BigProdNumStateEnum>();
            var config = GetConfig();
            foreach (var item in bigProdNumDict)
            {
                if (!dict.ContainsKey(item.Key))
                {
                    var val = GetSingleBigProdNumState(item.Value, config);
                    dict.Add(item.Key, val);
                }
            }
            return dict;



        }
    }
}
