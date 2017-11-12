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

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class StoreCartItemService: ServiceBase, IStoreCartItemContract
    {
        #region 声明数据层操作对象

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(StoreCartItemService));

        protected readonly IRepository<StoreCart, int> _storeCartRepository;

        protected readonly IRepository<StoreCartItem, int> _storeCartItemRepository;
        public StoreCartItemService(IRepository<StoreCart, int> storeCartRepository,
            IRepository<StoreCartItem, int> storeCartItemRepository)
            : base(storeCartRepository.UnitOfWork)
		{
            _storeCartRepository = storeCartRepository;
            _storeCartItemRepository = storeCartItemRepository;
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
                var entities = _storeCartItemRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _storeCartItemRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 更新数据
        public OperationResult Update(params StoreCartItem[] StoreCartItems)
        {
            try
            {                
                OperationResult result = _storeCartItemRepository.Update(StoreCartItems) ;
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
            }
        }
        #endregion
    }
}
