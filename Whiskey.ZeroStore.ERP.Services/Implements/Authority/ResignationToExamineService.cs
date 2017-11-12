using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements.Authority
{
    public class ResignationToExamineService : ServiceBase, IResignationToExamineContract
    {
        private readonly IRepository<ResignationToExamine, int> _entryToExamineRepository;

        public ResignationToExamineService(IRepository<ResignationToExamine, int> entryToExamineRepository) : base(entryToExamineRepository.UnitOfWork)
        {
            _entryToExamineRepository = entryToExamineRepository;
        }

        public IQueryable<ResignationToExamine> EntryToExamines
        {
            get { return _entryToExamineRepository.Entities; }
        }

        public OperationResult Insert(params ResignationToExamine[] dtos)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            foreach (var rule in dtos)
            {
                resul = _entryToExamineRepository.Insert(rule) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
            }
            return resul;
        }
    }
}
