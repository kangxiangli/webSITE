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
    public class StoreNoRecommentService : ServiceBase, IStoreNoRecommendContract
    {
        IRepository<StoreNoRecommend, int> _repo;
        protected readonly IProductOrigNumberContract _productOrignNumberContract;
        protected readonly IStoreContract _storeContract;
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(StoreRecommentService));
        public StoreNoRecommentService(IRepository<StoreNoRecommend, int> repo,
                IProductOrigNumberContract productOrigNumberContract,
                IStoreContract storeContract
            ) : base(repo.UnitOfWork)
        {
            _repo = repo;
            _productOrignNumberContract = productOrigNumberContract;
            _storeContract = storeContract;
        }
        public IQueryable<StoreNoRecommend> StoreNoRecommends
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
                    _repo.Delete(entity);
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

        public OperationResult Insert(params StoreNoRecommend[] entities)
        {
            return _repo.Insert((IEnumerable<StoreNoRecommend>)entities) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
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

        public OperationResult Update(params StoreNoRecommend[] entities)
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

        



    }
}
