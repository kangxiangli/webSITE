using System;
using System.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ProdcutOrigNumberProductDetailService : ServiceBase, IProdcutOrigNumberProductDetailContract
    {
        private readonly IRepository<ProdcutOrigNumberProductDetail, int> _ProdcutOrigNumberProductDetailRepository;
        public ProdcutOrigNumberProductDetailService(
            IRepository<ProdcutOrigNumberProductDetail, int> _ProdcutOrigNumberProductDetailRepository
            ) : base(_ProdcutOrigNumberProductDetailRepository.UnitOfWork)
        {
            this._ProdcutOrigNumberProductDetailRepository = _ProdcutOrigNumberProductDetailRepository;
        }

        public IQueryable<ProdcutOrigNumberProductDetail> Entities
        {
            get
            {
                return _ProdcutOrigNumberProductDetailRepository.Entities;
            }
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _ProdcutOrigNumberProductDetailRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _ProdcutOrigNumberProductDetailRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params ProdcutOrigNumberProductDetail[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _ProdcutOrigNumberProductDetailRepository.Insert(entities,
                entity =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                return result;
            }, Operation.Add);
        }

        public OperationResult Update(params ProdcutOrigNumberProductDetail[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _ProdcutOrigNumberProductDetailRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public ProdcutOrigNumberProductDetail View(int Id)
        {
            return _ProdcutOrigNumberProductDetailRepository.GetByKey(Id);
        }
    }
}
