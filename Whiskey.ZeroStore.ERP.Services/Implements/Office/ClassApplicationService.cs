using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Implements.Office
{
  public  class ClassApplicationService : ServiceBase, IClassApplicationContract
    {
        private readonly IRepository<ClassApplication, int> _classApplicationRepository;

        public ClassApplicationService(IRepository<ClassApplication, int> classApplicationRepository) : base(classApplicationRepository.UnitOfWork)
        {
            _classApplicationRepository = classApplicationRepository;
        }

        public IQueryable<ClassApplication> ClassApplications
        {
            get { return _classApplicationRepository.Entities; }
        }

        public OperationResult Insert(params ClassApplication[] dtos)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            foreach (var rule in dtos)
            {

                resul = _classApplicationRepository.Insert(rule) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
            }
            return resul;
        }
        public OperationResult Update(params ClassApplicationDto[] dtos)
        {
            var resul = _classApplicationRepository.Update(dtos, null, (dto, ent) =>
            {
                ent = AutoMapper.Mapper.Map<ClassApplication>(dto);
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                return ent;
            });
            return resul;
        }

        public OperationResult Remove(bool isdelate,params int[] ids)
        {
            var entys = _classApplicationRepository.Entities.Where(c => ids.Contains(c.Id));
            entys.Each(c =>
            {
                c.IsDeleted = isdelate;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _classApplicationRepository.Update(entys.ToList());
        }
        public OperationResult ToExamine(int ToExamineStatues, params int[] ids)
        {
            var enty = _classApplicationRepository.Entities.Where(c => ids.Contains(c.Id));
            enty.Each(c =>
            {
                c.ToExamineResult = ToExamineStatues;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _classApplicationRepository.Update(enty.ToList());
        }
    }
}
