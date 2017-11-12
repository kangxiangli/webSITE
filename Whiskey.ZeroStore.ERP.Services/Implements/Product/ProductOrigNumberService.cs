using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Logs;
using Whiskey.Utility.Helper;
using XKMath36;
using Whiskey.Utility;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ProductOrigNumberService : ServiceBase, IProductOrigNumberContract
    {
        protected readonly IRepository<ProductOriginNumber, int> _productOrigNumRepository;
        protected readonly IRepository<ProductOperationLog, int> _operationLogRepository;
        public ProductOrigNumberService(IRepository<ProductOriginNumber, int> productOrigNumRepository, IRepository<ProductOperationLog, int> operationLogRepository)
            : base(productOrigNumRepository.UnitOfWork)
        {
            _productOrigNumRepository = productOrigNumRepository;
            _operationLogRepository = operationLogRepository;
        }
        public IQueryable<ProductOriginNumber> OrigNumbs
        {
            get
            {
                return _productOrigNumRepository.Entities;
            }

        }

        public Utility.Data.OperationResult Insert(params ProductOriginNumber[] produarr)
        {
            OperationResult result;
            try
            {
                produarr.CheckNotNull("produarr");
                foreach (var origNumber in produarr)
                {
                    origNumber.OperatorId = AuthorityHelper.OperatorId;
                    origNumber.CreatedTime = DateTime.Now;
                }

                int res = _productOrigNumRepository.Insert((IEnumerable<ProductOriginNumber>)produarr);
                return res > 0 ? result = new OperationResult(OperationResultType.Success, res + "条数据受影响") : new OperationResult(OperationResultType.Error);
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Update(params ProductOriginNumber[] orignum)
        {
            OperationResult result;
            try
            {
                orignum.CheckNotNull("orignum");
                orignum.Each(e =>
                {
                    e.UpdatedTime = DateTime.Now;
                    e.OperatorId = AuthorityHelper.OperatorId;
                });
                result = _productOrigNumRepository.Update(orignum);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败，错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Remove(bool isTran, params int[] ids)
        {
            OperationResult resul;
            try
            {
                _productOrigNumRepository.UnitOfWork.TransactionEnabled = isTran;
                var ents = _productOrigNumRepository.Entities.Where(c => ids.Contains(c.Id)).ToList();
                foreach (var en in ents)
                {
                    en.IsDeleted = true;
                    en.UpdatedTime = DateTime.Now;
                    en.OperatorId = AuthorityHelper.OperatorId;

                }
                resul = _productOrigNumRepository.Update(ents);

            }
            catch (Exception ex)
            {

                resul = new OperationResult(OperationResultType.Error, ex.Message);
            }

            return resul;
        }


        public OperationResult Delete(bool isTrans, int[] ids)
        {
            _productOrigNumRepository.UnitOfWork.TransactionEnabled = isTrans;

            _operationLogRepository.Insert(new ProductOperationLog()
            {
                Description = "删除原始货号数据,对应ID为：" + string.Join(",", ids),
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now,
                OperatorId = AuthorityHelper.OperatorId
            });
            return _productOrigNumRepository.Delete(ids);

        }


        public bool isHave(string SampleId)
        {
            var entities = _productOrigNumRepository.Entities.Where(m => SampleId == (m.OriginNumber));

            string dd = null;

            foreach (var entity in entities)
            {
                dd = entity.Id.ToString();
            }

            if (dd == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string GetOrigNumId(string SampleId)
        {
            var entities = _productOrigNumRepository.Entities.Where(m => SampleId == (m.OriginNumber));

            string dd = null;

            foreach (var entity in entities)
            {
                dd = entity.Id.ToString();
            }

            return dd;
        }

        public OperationResult UpdatePriceByDiscount(Models.ProductDiscount discount, int[] origIds, bool isTrans)
        {
            var orignums = _productOrigNumRepository.Entities.Where(c => origIds.Contains(c.Id) && c.IsEnabled && !c.IsDeleted);
            orignums.Each(c =>
            {
                c.WholesalePrice = c.TagPrice * discount.WholesaleDiscount / 10;
                c.PurchasePrice = c.TagPrice * discount.PurchaseDiscount / 10;
            });
            _productOrigNumRepository.UnitOfWork.TransactionEnabled = isTrans;
            return _productOrigNumRepository.Update(orignums.ToArray());

        }

        public OperationResult SetRecommand(bool IsRecommend, params string[] bigProNums)
        {
            try
            {
                bigProNums.CheckNotNull("bigProNums");
                UnitOfWork.TransactionEnabled = true;
                var entities = _productOrigNumRepository.Entities.Where(m => bigProNums.Contains(m.BigProdNum));

                foreach (var entity in entities)
                {
                    entity.IsRecommend = IsRecommend;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    //_productOrigNumRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "设置成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "设置失败！错误如下：" + ex.Message, ex.ToString());
            }
        }


        public ProductOriginNumber View(int Id)
        {
            var entity = _productOrigNumRepository.GetByKey(Id);
            return entity;
        }

        public OperationResult Verify(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _productOrigNumRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsVerified = CheckStatusFlag.通过;
                    entity.RefuseReason = null;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    entity.VerifiedMembId = AuthorityHelper.OperatorId;
                    _productOrigNumRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "审核成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "审核失败！错误如下：" + ex.Message, ex.ToString());
            }
        }
        public OperationResult VerifyRefuse(int Id, string RefuseReason)
        {
            try
            {
                var entity = _productOrigNumRepository.Entities.FirstOrDefault(m => m.Id == Id);
                entity.IsVerified = CheckStatusFlag.未通过;
                entity.RefuseReason = RefuseReason;
                entity.UpdatedTime = DateTime.Now;
                entity.OperatorId = AuthorityHelper.OperatorId;
                entity.VerifiedMembId = AuthorityHelper.OperatorId;

                return _productOrigNumRepository.Update(entity) > 0 ? new OperationResult(OperationResultType.Success, "审核成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "审核失败！错误如下：" + ex.Message, ex.ToString());
            }
        }
    }
}
