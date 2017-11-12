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





    public class RecommendMemberSingleProductService : ServiceBase, IRecommendMemberSingleProductContract
    {

        private readonly IRepository<RecommendMemberSingleProduct, int> _repo;
        private readonly IProductOrigNumberContract _productOrigNumberContract;

        public RecommendMemberSingleProductService(IRepository<RecommendMemberSingleProduct, int> repo,
            IProductOrigNumberContract productOrigNumberContract) : base(repo.UnitOfWork)
        {
            _repo = repo;
            _productOrigNumberContract = productOrigNumberContract;

        }
        public IQueryable<RecommendMemberSingleProduct> Entities => _repo.Entities;


        public OperationResult Insert(params RecommendMemberSingleProduct[] entities)
        {
            return _repo.Insert(entities, e =>
            {
                e.CreatedTime = DateTime.Now;
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }
        public OperationResult Delete(params RecommendMemberSingleProduct[] entities)
        {
            var count = _repo.Delete(entities);
            if (count > 0)
            {
                return OperationResult.OK();
            }
            return OperationResult.Error("删除失败");
        }
        public OperationResult Update(ICollection<RecommendMemberSingleProduct> entities)
        {
            return _repo.Update(entities, e =>
            {
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }

        public RecommendMemberSingleProduct Edit(int id)
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

        public RecommendMemberSingleProduct View(int Id)
        {
            return _repo.GetByKey(Id);
        }

        public OperationResult Update(params RecommendMemberSingleProduct[] entities)
        {
            return _repo.Update(entities, e =>
            {
                e.OperatorId = AuthorityHelper.OperatorId;
                e.UpdatedTime = DateTime.Now;
            });
        }

        public OperationResult SaveMemberId(string bigProdNumber, params SaveMemberRecommendEntry[] recommendMembers)
        {
            var originNumberId = _productOrigNumberContract.OrigNumbs
                .Where(p => !p.IsDeleted && p.IsEnabled && p.BigProdNum == bigProdNumber)
                .Select(p => p.Id).FirstOrDefault();

            var historyData = _repo.Entities.Where(r => r.BigProdNumber == bigProdNumber).ToList();
            if (recommendMembers == null || recommendMembers.Length <= 0)
            {
                //清空
                var count = _repo.Delete(historyData);
                return count > 0 ? OperationResult.OK() : OperationResult.Error("删除失败");
            }
            else
            {
                var count = _repo.Delete(historyData);
                var dataToAdd = recommendMembers.Select(r => new RecommendMemberSingleProduct()
                {
                    ProductOriginNumberId = originNumberId,
                    ColorId = r.ColorId,
                    BigProdNumber = bigProdNumber,
                    MemberId = r.MemberId
                }).ToList();

                var res = _repo.Insert(dataToAdd, e => { e.OperatorId = AuthorityHelper.OperatorId; e.UpdatedTime = DateTime.Now; });
                return res;
            }





        }
    }





}
