using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using System.Web.Mvc;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Utility.Data;
using AutoMapper;

namespace Whiskey.ZeroStore.ERP.Services
{
    public class EntryService : ServiceBase, IEntryContract
    {
        private readonly IRepository<Entry, int> _entryRepository;

        public EntryService(IRepository<Entry, int> entryRepository) : base(entryRepository.UnitOfWork)
        {
            _entryRepository = entryRepository;
        }

        public IQueryable<Entry> Entrys
        {
            get { return _entryRepository.Entities; }
        }

        public OperationResult Insert(params Entry[] dtos)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            foreach (var rule in dtos)
            {

                resul = _entryRepository.Insert(rule) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
            }
            return resul;
        }

        public OperationResult Update(params EntryDto[] dtos)
        {
            var resul = _entryRepository.Update(dtos, null, (dto, ent) =>
            {
                ent = AutoMapper.Mapper.Map<Entry>(dto);
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                return ent;
            });
            return resul;
        }

        public OperationResult Update(params Entry[] entities)
        {
            var resul = _entryRepository.Update(entities, ent =>
            {
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
            });
            return resul;
        }

        public OperationResult ToExamine(int ToExamineStatues, params int[] ids)
        {
            var enty = _entryRepository.Entities.Where(c => ids.Contains(c.Id));
            enty.Each(c =>
            {
                c.ToExamineResult = ToExamineStatues;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _entryRepository.Update(enty.ToList());
        }
        public OperationResult RoleJurisdiction(string JurisdictionStr, string InterviewEvaluation,params int[] ids)
        {
            var enty = _entryRepository.Entities.Where(c => ids.Contains(c.Id));
            enty.Each(c =>
            {
                c.RoleJurisdiction = JurisdictionStr;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.InterviewEvaluation = InterviewEvaluation;
                c.UpdatedTime = DateTime.Now;
            });
            return _entryRepository.Update(enty.ToList());
        }
    }
}
