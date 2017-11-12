using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Utility.Data;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
   public class EntryToExamineServices : ServiceBase, IEntryToExamineContract
    {
        private readonly IRepository<EntryToExamine, int> _entryToExamineRepository;

        public EntryToExamineServices(IRepository<EntryToExamine, int> entryToExamineRepository) : base(entryToExamineRepository.UnitOfWork)
        {
            _entryToExamineRepository = entryToExamineRepository;
        }

        public IQueryable<EntryToExamine> EntryToExamines
        {
            get { return _entryToExamineRepository.Entities; }
        }

        public OperationResult Insert(params EntryToExamine[] dtos)
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
