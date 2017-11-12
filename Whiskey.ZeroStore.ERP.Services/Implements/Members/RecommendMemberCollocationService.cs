using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using System.Web.Mvc;
namespace Whiskey.ZeroStore.ERP.Services.Implements
{


    public class RecommendMemberCollocationService : ServiceBase, IRecommendMemberCollocationContract
    {

        private readonly IRepository<RecommendMemberCollocation, int> _repo;

        public RecommendMemberCollocationService(IRepository<RecommendMemberCollocation, int> repo) : base(repo.UnitOfWork)
        {
            _repo = repo;

        }
        public IQueryable<RecommendMemberCollocation> Entities => _repo.Entities;


        public OperationResult Insert(params RecommendMemberCollocation[] entities)
        {
            return _repo.Insert(entities, e =>
            {
                e.CreatedTime = DateTime.Now;
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }
        public OperationResult Delete(params RecommendMemberCollocation[] entities)
        {
            var count = _repo.Delete(entities);
            if (count > 0)
            {
                return OperationResult.OK();
            }
            return OperationResult.Error("删除失败");
        }
        public OperationResult Update(ICollection<RecommendMemberCollocation> entities)
        {
            return _repo.Update(entities, e =>
            {
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }

        public RecommendMemberCollocation Edit(int id)
        {
            return _repo.GetByKey(id);
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
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
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public RecommendMemberCollocation View(int Id)
        {
            return _repo.GetByKey(Id);
        }

        public OperationResult Update(params RecommendMemberCollocation[] entities)
        {
            return _repo.Update(entities, e =>
            {
                e.OperatorId = AuthorityHelper.OperatorId;
                e.UpdatedTime = DateTime.Now;
            });
        }
    }


}
